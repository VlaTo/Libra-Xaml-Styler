using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraProgramming.Parsing.Xaml.Tokens;

namespace LibraProgramming.Parsing.Xaml
{
    internal sealed class XamlParser
    {
        private readonly StringBuilder trace;
        private readonly XamlTokenizer tokenizer;
        private ParserState state;

        private ParserState State
        {
            get
            {
                return state;
            }
            set
            {
                if (state == value)
                {
                    return;
                }

                trace.Append(' ').Append(value);

                state = value;
            }
        }

        public XamlParser(XamlTokenizer tokenizer)
        {
            state = ParserState.Begin;
            this.tokenizer = tokenizer;
            trace = new StringBuilder(state.ToString());
        }

        public async Task ParseAsync(XamlDocument document)
        {
            if (null == document)
            {
                throw new ArgumentNullException(nameof(document));
            }

            try
            {
                var levels = new Stack<XamlNode>();

                levels.Push(document.Root);

                await ParseInternalAsync(document, levels);

                if (1 != levels.Count)
                {
                    throw new XamlParsingException();
                }

                var last = levels.Pop();
            }
            catch (Exception)
            {
                Debug.Write("[XamlParser] State trace: ");
                Debug.Write(trace.ToString());

                throw new XamlParsingException();
            }
        }

        private async Task ParseInternalAsync(XamlDocument document, Stack<XamlNode> nodes)
        {
            var nameBuilder = new XamlNameBuilder();
            var valueBuilder = new XamlValueBuilder();
            var done = false;

            while (false == done)
            {
                switch (state)
                {
                    case ParserState.Begin:
                    {
                        XamlToken token;

                        while (true)
                        {
                            token = await tokenizer.GetTokenAsync();

                            if (token.IsWhitespace())
                            {
                                // add empty nodes
                                continue;
                            }

                            break;
                        }

                        if (token.IsEnd())
                        {
                            done = true;
                            break;
                        }

                        if (token.IsOpenBracket())
                        {
                            State = ParserState.OpenBracket;

                            break;
                        }

                        throw new Exception();
                    }

                    case ParserState.OpenBracket:
                    {
                        var token = await tokenizer.GetTokenAsync();

                        if (token.IsSlash())
                        {
                            State = ParserState.ClosingTagBegin;
                            break;
                        }

                        string text;

                        if (false == token.IsString(out text))
                        {
                            throw new XamlParsingException();
                        }

                        nameBuilder.Clear();
                        nameBuilder.Name = text;

                        State = ParserState.OpeningTagBegin;

                        break;
                    }

                    case ParserState.OpeningTagBegin:
                    {
                        var token = await tokenizer.GetTokenAsync();

                        if (token.IsColon())
                        {
                            var text = nameBuilder.Name;

                            nameBuilder.Prefix = text;
                            nameBuilder.Name = null;
                            State = ParserState.OpeningTagNamespaceColon;

                            break;
                        }

                        if (token.IsDot())
                        {
                            State = ParserState.OpeningTagNameDot;
                            break;
                        }

                        if (token.IsWhitespace())
                        {
                            State = ParserState.OpeningTagTrailing;
                            break;
                        }

                        if (token.IsSlash())
                        {
                            State = ParserState.OpeningTagNameSlash;
                            break;
                        }

                        if (token.IsCloseBracket())
                        {
                            State = ParserState.OpeningTagClose;
                            break;
                        }

                        throw new XamlParsingException();
                    }

                    case ParserState.OpeningTagNamespaceColon:
                    {
                        var token = await tokenizer.GetTokenAsync();

                        string text;

                        if (token.IsString(out text))
                        {
                            nameBuilder.Name = text;
                            State = ParserState.OpeningTagName;

                            break;
                        }

                        throw new XamlParsingException();
                    }

                    case ParserState.OpeningTagName:
                    {
                        var token = await tokenizer.GetTokenAsync();

                        if (token.IsDot())
                        {
                            State = ParserState.OpeningTagNameDot;
                            break;
                        }

                        if (token.IsWhitespace())
                        {
                            State = ParserState.OpeningTagTrailing;
                            break;
                        }

                        if (token.IsSlash())
                        {
                            State = ParserState.OpeningTagNameSlash;
                            break;
                        }

                        if (token.IsCloseBracket())
                        {
                            State = ParserState.OpeningTagClose;
                            break;
                        }

                        throw new XamlParsingException();
                    }

                    case ParserState.OpeningTagNameDot:
                    {
                        var token = await tokenizer.GetTokenAsync();

                        string text;

                        if (false == token.IsString(out text))
                        {
                            throw new XamlParsingException();
                        }

                        nameBuilder.AccumulateName(text);
                        State = ParserState.OpeningTagNameTrailing;

                        break;
                    }

                    case ParserState.OpeningTagNameTrailing:
                    {
                        var token = await tokenizer.GetTokenAsync();

                        if (token.IsDot())
                        {
                            State = ParserState.OpeningTagNameDot;
                            break;
                        }

                        if (token.IsWhitespace())
                        {
                            var element = new XamlElement(document, nameBuilder.ToXamlName());
                                
                            State = ParserState.OpeningTagTrailing;

                            break;
                        }

                        if (token.IsSlash())
                        {
                            State = ParserState.OpeningTagNameSlash;
                            break;
                        }

                        if (token.IsCloseBracket())
                        {
                            State = ParserState.OpeningTagClose;
                            break;
                        }

                        throw new XamlParsingException();
                    }

                    case ParserState.OpeningTagNameSlash:
                    {
                        var token = await tokenizer.GetTokenAsync();

                        if (token.IsCloseBracket())
                        {
                            State = ParserState.OpeningTagInline;
                            break;
                        }

                        throw new XamlParsingException();
                    }

                    case ParserState.OpeningTagClose:
                    {
                        AppendNewElement(nameBuilder, document, nodes);
                        State = ParserState.OpeningTagClosed;

                        break;
                    }

                    case ParserState.OpeningTagInline:
                    {
                        State = ParserState.OpeningTagClosed;
                        break;
                    }

                    case ParserState.OpeningTagClosed:
                    {
                        State = ParserState.Begin;
                        break;
                    }

                    case ParserState.OpeningTagTrailing:
                    {
                        var token = await tokenizer.GetTokenAsync();

                        if (token.IsWhitespace())
                        {
                            break;
                        }

                        string text;

                        if (token.IsString(out text))
                        {
                            State = ParserState.OpeningTagAttributeName;
                            break;
                        }

                        if (token.IsSlash())
                        {
                            State = ParserState.OpeningTagNameSlash;
                            break;
                        }

                        throw new XamlParsingException();
                    }

                    case ParserState.OpeningTagAttributeName:
                    {
                        var token = await tokenizer.GetTokenAsync();

                        if (token.IsColon())
                        {
                            State = ParserState.OpeningTagAttributeNamespaceColon;
                            break;
                        }

                        if (token.IsDot())
                        {
                            State = ParserState.OpeningTagAttributeNameDot;
                            break;
                        }

                        if (token.IsEqual())
                        {
                            State = ParserState.OpeningTagAttributeNameEnd;
                            break;
                        }

                        if (token.IsWhitespace())
                        {
                            break;
                        }

                        throw new XamlParsingException();
                    }

                    case ParserState.OpeningTagAttributeNameEnd:
                    {
                        var token = await tokenizer.GetTokenAsync();

                        if (token.IsDoubleQuote())
                        {
                            State = ParserState.OpeningTagAttributeQuotedValueBegin;
                            break;
                        }

                        if (token.IsSingleQuote())
                        {
                            State = ParserState.OpeningTagAttributeValueBegin;
                            break;
                        }

                        if (token.IsWhitespace())
                        {
                            break;
                        }

                        throw new XamlParsingException();
                    }

                    case ParserState.OpeningTagAttributeNamespaceColon:
                    {
                        throw new NotImplementedException();
                    }

                    case ParserState.OpeningTagAttributeNameDot:
                    {
                        throw new NotImplementedException();
                    }

                    case ParserState.OpeningTagAttributeNameTrailing:
                    {
                        throw new NotImplementedException();
                    }

                    case ParserState.OpeningTagAttributeQuotedValueBegin:
                    {
                        State = ParserState.OpeningTagAttributeQuotedValue;
                        break;
                    }

                    case ParserState.OpeningTagAttributeValueBegin:
                    {
                        State = ParserState.OpeningTagAttributeValue;
                        break;
                    }

                    case ParserState.OpeningTagAttributeValue:
                    {
                        var token = await tokenizer.GetTokenAsync();

                        if (token.IsSingleQuote())
                        {
                            State = ParserState.OpeningTagAttributeValueEnd;
                            break;
                        }


                        break;
                    }

                    case ParserState.OpeningTagAttributeQuotedValue:
                    {
                        var token = await tokenizer.GetTokenAsync();

                        if (token.IsDoubleQuote())
                        {
                            State = ParserState.OpeningTagAttributeQuotedValueEnd;
                            break;
                        }

                        valueBuilder.AccumulateToken(token);

                        break;
                    }

                    case ParserState.OpeningTagAttributeQuotedValueEnd:
                    {
                        State = ParserState.OpeningTagAttributeValueEnd;
                        break;
                    }

                    case ParserState.OpeningTagAttributeValueEnd:
                    {
                        var token = await tokenizer.GetTokenAsync();

                        if (token.IsWhitespace())
                        {
                            State = ParserState.OpeningTagTrailing;
                            break;
                        }

                        if (token.IsSlash())
                        {
                            State = ParserState.OpeningTagNameSlash;
                            break;
                        }

                        if (token.IsCloseBracket())
                        {
                            State = ParserState.OpeningTagClose;
                            break;
                        }

                        throw new XamlParsingException();
                    }

                    case ParserState.ClosingTagBegin:
                    {
                        string text;
                        var token = await tokenizer.GetTokenAsync();

                        if (false == token.IsString(out text))
                        {
                            throw new XamlParsingException();
                        }

                        token = await tokenizer.GetTokenAsync();

                        if (token.IsColon())
                        {
//                            prefix = text;
//                            fullName = null;
                            State = ParserState.ClosingTagNamespaceColon;

                            break;
                        }

//                        prefix = null;
//                        fullName = text;

                        if (token.IsDot())
                        {
                            State = ParserState.ClosingTagNameDot;
                            break;
                        }

                        if (token.IsCloseBracket())
                        {
                            State = ParserState.ClosingTagNameClose;
                            break;
                        }

                        throw new XamlParsingException();
                    }

                    case ParserState.ClosingTagNamespaceColon:
                    {
                        string text;
                        var token = await tokenizer.GetTokenAsync();

                        if (false == token.IsString(out text))
                        {
                            throw new XamlParsingException();
                        }

//                        fullName = text;
                        token = await tokenizer.GetTokenAsync();

                        if (token.IsDot())
                        {
                            State = ParserState.ClosingTagNameDot;
                            break;
                        }

                        if (token.IsCloseBracket())
                        {
                            State = ParserState.ClosingTagNameClose;
                            break;
                        }

                        throw new XamlParsingException();
                    }

                    case ParserState.ClosingTagNameDot:
                    {
                        string text;
                        var token = await tokenizer.GetTokenAsync();

                        if (false == token.IsString(out text))
                        {
                            throw new XamlParsingException();
                        }

//                        fullName = new StringBuilder(fullName).Append('.').Append(text).ToString();
                        token = await tokenizer.GetTokenAsync();

                        if (token.IsDot())
                        {
                            break;
                        }

                        if (token.IsCloseBracket())
                        {
                            State = ParserState.ClosingTagNameClose;
                            break;
                        }

                        throw new XamlParsingException();
                    }

                    case ParserState.ClosingTagNameClose:
                    {
                        var node = nodes.Peek();

                        if (null == node)
                        {
                            throw new XamlParsingException();
                        }

//                        var temp = XamlName.Create(prefix, fullName, null);
                        var temp = nameBuilder.ToXamlName();

                        if (XamlNodeType.Element == node.NodeType)
                        {
                            var name = ((XamlElement) node).XamlName;

                            if (temp != name)
                            {
                                throw new XamlParsingException();
                            }

                            nodes.Pop();

                            State = ParserState.Begin;

                            break;
                        }

                        throw new XamlParsingException();
                    }
                }
            }
        }

        private static XamlElement AppendNewElement(XamlNameBuilder nameBuilder, XamlDocument document, Stack<XamlNode> nodes)
        {
            var parent = nodes.Peek();

            if (null == parent)
            {
                throw new XamlParsingException();
            }

            var name = nameBuilder.ToXamlName();
            var element = new XamlElement(document, name);

            parent.AppendChild(element);
            nodes.Push(element);
            nameBuilder.Clear();

            return element;
        }

        /// <summary>
        /// 
        /// </summary>
        private enum ParserState
        {
            Begin,
            //
            OpenBracket,
            OpeningTagBegin,
            OpeningTagName,
            OpeningTagNamespaceColon,
            OpeningTagNameDot,
            OpeningTagNameTrailing,
            OpeningTagNameSlash,
            OpeningTagClose,
            OpeningTagTrailing,
            OpeningTagInline,
            OpeningTagClosed,
            //
            OpeningTagAttributeName,
            OpeningTagAttributeNamespaceColon,
            OpeningTagAttributeNameDot,
            OpeningTagAttributeNameTrailing,
            OpeningTagAttributeNameEnd,
            OpeningTagAttributeQuotedValueBegin,
            OpeningTagAttributeValue,
            OpeningTagAttributeQuotedValueEnd,
            //
            ClosingTagBegin,
            ClosingTagNamespaceColon,
            ClosingTagNameDot,
            ClosingTagNameClose,
            OpeningTagAttributeValueBegin,
            OpeningTagAttributeQuotedValue,
            OpeningTagAttributeValueEnd
        }
    }
}