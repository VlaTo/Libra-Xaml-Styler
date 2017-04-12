using System;
using System.Collections.Generic;
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
            string name = null;
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
                        var temp = name;

                        state = await ParseOpenBracketAsync(text =>
                        {
                            name = BeginName(temp, text);
                        });
                        break;
                    }

                    case ParserState.OpeningTagBegin:
                    {
                        state = await ParseTagOpenAsync(() =>
                        {
                            // nothing to do here
                        });
                        break;
                    }

                    case ParserState.OpeningTagNamespaceColon:
                    {
                        var temp = name;

                        state = await ParseOpeningTagNamespaceColonAsync(text =>
                        {
                            name = SetNamespaceSeparator(temp) + AppendName(null, text);
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
                        var temp = name;

                        state = await ParseOpeningTagNameDotAsync(text =>
                        {
                            name = AppendName(temp, text);
                        });

                        break;
                    }

                    case ParserState.OpeningTagNameTrailing:
                    {
                        var temp = name;

                        state = await ParseOpeningTagNameTrailingAsync(() =>
                        {
                            element = document.CreateElement(temp);
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
                        element = document.CreateElement(name, true);
                        state = ParserState.OpeningTagClosed;
                        name = null;

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
                        var temp = name;
                           
                        state = await ParseOpeningTagTrailingAsync(text =>
                        {
                            name = BeginName(temp, text);
                        });

                        break;
                    }

                    case ParserState.OpeningTagAttributeName:
                    {
                        state = await ParseOpeningTagAttributeNameAsync();
                        break;
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
                        var attribute = document.CreateAttribute(name);

                        if (null == element)
                        {
                            throw new XamlParsingException();
                        }

                        name = null;
                        element.Attributes.Append(attribute);
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
                        state = ParseClosingTagNameClose(nodes, name);
                        break;
                    }

                    default:
                    {
                        throw new XamlParsingException();
                    }
                }
            }
        }

        private async Task<ParserState> ParseOpeningTagAttributeNameAsync()
        {
            var token = await tokenizer.GetTokenAsync();

            if (token.IsColon())
            {
                return ParserState.OpeningTagAttributeNamespaceColon;
            }

            if (token.IsDot())
            {
                return ParserState.OpeningTagAttributeNameDot;
            }

            if (token.IsEqual())
            {
                return ParserState.OpeningTagAttributeNameEnd;
            }

            if (token.IsWhitespace())
            {
                return ParserState.OpeningTagAttributeName;
            }

            throw new XamlParsingException();
        }

        private async Task<ParserState> ParseOpeningTagTrailingAsync(Action<string> callback)
        {
            var token = await tokenizer.GetTokenAsync();

            if (token.IsWhitespace())
            {
                return ParserState.OpeningTagTrailing;
            }

            string text;

            if (token.IsString(out text))
            {
                callback.Invoke(text);
                return ParserState.OpeningTagAttributeName;
            }

            if (token.IsSlash())
            {
                return ParserState.OpeningTagNameSlash;
            }

            throw new XamlParsingException();
        }

        private static ParserState ParseOpeningTagClosed(Stack<XamlNode> nodes, XamlElement element)
        {
            var parent = nodes.Peek();

            parent.AppendChild(element);

            if (element.IsInlined == false)
            {
                nodes.Push(element);
            }

            return ParserState.Begin;
        }

        private async Task<ParserState> ParseOpeningTagNameTrailingAsync(Action callback)
        {
            var token = await tokenizer.GetTokenAsync();

            if (token.IsDot())
            {
                return ParserState.OpeningTagNameDot;
            }

            if (token.IsWhitespace())
            {
                callback.Invoke();
                return ParserState.OpeningTagTrailing;
            }

            if (token.IsSlash())
            {
                return ParserState.OpeningTagNameSlash;
            }

            if (token.IsCloseBracket())
            {
                callback.Invoke();
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

        private static ParserState ParseClosingTagNameClose(Stack<XamlNode> nodes, string name)
        {
            var last = nodes.Peek() as XamlElement;

            if (null == last)
            {
                throw new XamlParsingException();
            }

            if (last.XamlName.Match(name))
            {
                throw new XamlParsingException();
            }

            nodes.Pop();

            return ParserState.Begin;
        }

        private static string BeginName(string name, string str)
        {
            if (false == String.IsNullOrEmpty(name))
            {
                throw new Exception();
            }

            return str;
        }

        private static string SetNamespaceSeparator(string name)
        {
            var position = name.IndexOf(':');

            if (-1 < position)
            {
                throw new Exception();
            }

            return String.Concat(name, ':');
        }

        private static string AppendName(string name, string part)
        {
            if (false == String.IsNullOrEmpty(name))
            {
                return String.Concat(name, '.', part);
            }

            return part;
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