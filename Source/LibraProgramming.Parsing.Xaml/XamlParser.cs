using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.AccessControl;
using System.Threading.Tasks;
using LibraProgramming.Parsing.Xaml.Tokens;

namespace LibraProgramming.Parsing.Xaml
{
    internal sealed class XamlParser
    {
        private readonly XamlTokenizer tokenizer;

        public XamlParser(XamlTokenizer tokenizer)
        {
            this.tokenizer = tokenizer;
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
                throw new XamlParsingException();
            }
        }

        private async Task ParseInternalAsync(XamlDocument document, Stack<XamlNode> nodes)
        {
            var state = ParserState.Unknown;
            var nameBuilder = new XamlNameBuilder();
            var valueBuilder = new XamlValueBuilder();
            XamlElement element = null;

            while (ParserState.Done != state)
            {
                switch (state)
                {
                    case ParserState.Unknown:
                    {
                        state = ParserState.Begin;
                        break;
                    }

                    case ParserState.Begin:
                    {
                        state = await ParseBeginAsync();
                        break;
                    }

                    case ParserState.OpenBracket:
                    {
                        state = await ParseOpenBracketAsync(text =>
                        {
                            nameBuilder.Clear();
                            nameBuilder.Name = text;
                        });
                        break;
                    }

                    case ParserState.OpeningTagBegin:
                    {
                        state = await ParseTagOpenAsync(() =>
                        {
                            var text = nameBuilder.Name;

                            nameBuilder.Clear();
                            nameBuilder.Prefix = text;
                        });
                        break;
                    }

                    case ParserState.OpeningTagNamespaceColon:
                    {
                        state = await ParseOpeningTagNamespaceColonAsync(text =>
                        {
                            nameBuilder.Name = text;
                        });
                        break;
                    }

                    case ParserState.OpeningTagName:
                    {
                        state = await ParseOpeningTagNameAsync();
                        break;
                    }

                    case ParserState.OpeningTagNameDot:
                    {
                        state = await ParseOpeningTagNameDotAsync(nameBuilder.AccumulateName);
                        break;
                    }

                    case ParserState.OpeningTagNameTrailing:
                    {
                        state = await ParseOpeningTagNameTrailingAsync(() =>
                        {
                            element = new XamlElement(document, nameBuilder.ToXamlName());
                        });
                        break;
                    }

                    case ParserState.OpeningTagNameSlash:
                    {
                        var token = await tokenizer.GetTokenAsync();

                        if (token.IsCloseBracket())
                        {
                            state = ParserState.OpeningTagInline;
                            break;
                        }

                        throw new XamlParsingException();
                    }

                    case ParserState.OpeningTagClose:
                    {
                        state = ParserState.OpeningTagClosed;
                        break;
                    }

                    case ParserState.OpeningTagInline:
                    {
                        element = new XamlElement(document, nameBuilder.ToXamlName(), true);
                        state = ParserState.OpeningTagClosed;

                        break;
                    }

                    case ParserState.OpeningTagClosed:
                    {
                        if (null == element)
                        {
                            throw new XamlParsingException();
                        }

                        state = ParseOpeningTagClosed(nodes, element);

                        if (ParserState.Begin == state)
                        {
                            element = null;
                        }

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
                            state = ParserState.OpeningTagAttributeName;
                            break;
                        }

                        if (token.IsSlash())
                        {
                            state = ParserState.OpeningTagNameSlash;
                            break;
                        }

                        throw new XamlParsingException();
                    }

                    case ParserState.OpeningTagAttributeName:
                    {
                        var token = await tokenizer.GetTokenAsync();

                        if (token.IsColon())
                        {
                            state = ParserState.OpeningTagAttributeNamespaceColon;
                            break;
                        }

                        if (token.IsDot())
                        {
                            state = ParserState.OpeningTagAttributeNameDot;
                            break;
                        }

                        if (token.IsEqual())
                        {
                            state = ParserState.OpeningTagAttributeNameEnd;
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
                        state = await ParseOpeningTagAttributeNameEndAsync();
                        break;
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
                        state = ParserState.OpeningTagAttributeQuotedValue;
                        break;
                    }

                    case ParserState.OpeningTagAttributeValueBegin:
                    {
                        state = ParserState.OpeningTagAttributeValue;
                        break;
                    }

                    case ParserState.OpeningTagAttributeValue:
                    {
                        var token = await tokenizer.GetTokenAsync();

                        if (token.IsSingleQuote())
                        {
                            state = ParserState.OpeningTagAttributeValueEnd;
                            break;
                        }


                        break;
                    }

                    case ParserState.OpeningTagAttributeQuotedValue:
                    {
                        var token = await tokenizer.GetTokenAsync();

                        if (token.IsDoubleQuote())
                        {
                            state = ParserState.OpeningTagAttributeQuotedValueEnd;
                            break;
                        }

                        valueBuilder.AccumulateToken(token);

                        break;
                    }

                    case ParserState.OpeningTagAttributeQuotedValueEnd:
                    {
                        state = ParserState.OpeningTagAttributeValueEnd;
                        break;
                    }

                    case ParserState.OpeningTagAttributeValueEnd:
                    {
                        var token = await tokenizer.GetTokenAsync();

                        if (token.IsWhitespace())
                        {
                            state = ParserState.OpeningTagTrailing;
                            break;
                        }

                        if (token.IsSlash())
                        {
                            state = ParserState.OpeningTagNameSlash;
                            break;
                        }

                        if (token.IsCloseBracket())
                        {
                            state = ParserState.OpeningTagClose;
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
                            state = ParserState.ClosingTagNamespaceColon;

                            break;
                        }

//                        prefix = null;
//                        fullName = text;

                        if (token.IsDot())
                        {
                            state = ParserState.ClosingTagNameDot;
                            break;
                        }

                        if (token.IsCloseBracket())
                        {
                            state = ParserState.ClosingTagNameClose;
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
                            state = ParserState.ClosingTagNameDot;
                            break;
                        }

                        if (token.IsCloseBracket())
                        {
                            state = ParserState.ClosingTagNameClose;
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
                            state = ParserState.ClosingTagNameClose;
                            break;
                        }

                        throw new XamlParsingException();
                    }

                    case ParserState.ClosingTagNameClose:
                    {
                        state = ParseClosingTagNameClose(nodes, nameBuilder);
                        break;
                    }

                    default:
                    {
                        throw new XamlParsingException();
                    }
                }
            }
        }

        private static ParserState ParseOpeningTagClosed(Stack<XamlNode> nodes, XamlElement element)
        {
            var parent = nodes.Peek();

            parent.AppendChild(element);
            nodes.Push(element);

            return ParserState.Begin;
        }

        private async Task<ParserState> ParseOpeningTagNameTrailingAsync(Action createElementCallback)
        {
            var token = await tokenizer.GetTokenAsync();

            if (token.IsDot())
            {
                return ParserState.OpeningTagNameDot;
            }

            if (token.IsWhitespace())
            {
                createElementCallback.Invoke();
                //element = new XamlElement(document, nameBuilder.ToXamlName());
                return ParserState.OpeningTagTrailing;
            }

            if (token.IsSlash())
            {
                return ParserState.OpeningTagNameSlash;
            }

            if (token.IsCloseBracket())
            {
                createElementCallback.Invoke();
                //element = new XamlElement(document, nameBuilder.ToXamlName());
                return ParserState.OpeningTagClose;
            }

            return ParserState.Failed;
        }

        private async Task<ParserState> ParseOpeningTagNameDotAsync(Action<string> callback)
        {
            var token = await tokenizer.GetTokenAsync();

            string text;

            if (false == token.IsString(out text))
            {
                throw new XamlParsingException();
            }

            callback.Invoke(text);

            return ParserState.OpeningTagNameTrailing;
        }

        private async Task<ParserState> ParseOpeningTagNamespaceColonAsync(Action<string> callback)
        {
            var token = await tokenizer.GetTokenAsync();

            string text;

            if (token.IsString(out text))
            {
                callback.Invoke(text);
                return ParserState.OpeningTagName;
            }

            throw new XamlParsingException();
        }

        private async Task<ParserState> ParseOpeningTagAttributeNameEndAsync()
        {
            var token = await tokenizer.GetTokenAsync();

            if (token.IsDoubleQuote())
            {
                return ParserState.OpeningTagAttributeQuotedValueBegin;
            }

            if (token.IsSingleQuote())
            {
                return ParserState.OpeningTagAttributeValueBegin;
            }

            if (token.IsWhitespace())
            {
                return ParserState.OpeningTagAttributeNameEnd;
            }

            throw new XamlParsingException();
        }

        private async Task<ParserState> ParseOpeningTagNameAsync()
        {
            var token = await tokenizer.GetTokenAsync();

            if (token.IsDot())
            {
                return ParserState.OpeningTagNameDot;
            }

            if (token.IsWhitespace())
            {
                return ParserState.OpeningTagTrailing;
            }

            if (token.IsSlash())
            {
                return ParserState.OpeningTagNameSlash;
            }

            if (token.IsCloseBracket())
            {
                return ParserState.OpeningTagClose;
            }

            throw new XamlParsingException();
        }

        private async Task<ParserState> ParseTagOpenAsync(Action callback)
        {
            var token = await tokenizer.GetTokenAsync();

            if (token.IsColon())
            {
                callback.Invoke();

                return ParserState.OpeningTagNamespaceColon;
            }

            if (token.IsDot())
            {
                return ParserState.OpeningTagNameDot;
            }

            if (token.IsWhitespace())
            {
                return ParserState.OpeningTagTrailing;
            }

            if (token.IsSlash())
            {
                return ParserState.OpeningTagNameSlash;
            }

            if (token.IsCloseBracket())
            {
                return ParserState.OpeningTagClose;
            }

            throw new XamlParsingException();
        }

        private async Task<ParserState> ParseBeginAsync()
        {
//            bool done;
//            ParserState state;
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
                return ParserState.Done;
            }

            if (token.IsOpenBracket())
            {
                return ParserState.OpenBracket;
            }

            throw new XamlParsingException();
        }

        private async Task<ParserState> ParseOpenBracketAsync(Action<string> callback)
        {
            var token = await tokenizer.GetTokenAsync();

            if (token.IsSlash())
            {
                return ParserState.ClosingTagBegin;
            }

            string text;

            if (false == token.IsString(out text))
            {
                throw new XamlParsingException();
            }

            callback.Invoke(text);

            return ParserState.OpeningTagBegin;
        }

        private static ParserState ParseClosingTagNameClose(Stack<XamlNode> nodes, XamlNameBuilder nameBuilder)
        {
            var last = nodes.Peek() as XamlElement;

            if (null == last)
            {
                throw new XamlParsingException();
            }

            var name = nameBuilder.ToXamlName();

            if (last.XamlName != name)
            {
                throw new XamlParsingException();
            }

            nodes.Pop();

            return ParserState.Begin;
        }

/*
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
                        */

        /// <summary>
        /// 
        /// </summary>
        internal enum ParserState
        {
            Failed = -1,
            Unknown,
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
            OpeningTagAttributeValueEnd,
            Done
        }
    }
}