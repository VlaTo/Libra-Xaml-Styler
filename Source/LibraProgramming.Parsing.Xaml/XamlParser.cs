using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
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
                var nodes = new Stack<XamlNode>();

                nodes.Push(document.Root);

                await ParseInternalAsync(document, nodes).ConfigureAwait(false);

                if (1 != nodes.Count)
                {
                    throw new XamlParsingException();
                }
            }
            catch (Exception exception)
            {
                Debugger.Break();
                throw new XamlParsingException();
            }
        }

        private async Task ParseInternalAsync(XamlDocument document, Stack<XamlNode> nodes)
        {
            var state = ParserState.Unknown;
            string prefix = null;
            string name = null;
            string value = null;
            XamlElement element = null;
            XamlAttribute attribute = null;

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
                        state = await ParseBeginAsync().ConfigureAwait(false);
                        break;
                    }

                    case ParserState.OpenBracket:
                    {
                        state = await ParseOpenBracketAsync(text =>
                        {
                            if (null != name)
                            {
                                throw new XamlParsingException();
                            }

                            name = text;

                        }).ConfigureAwait(false);

                        break;
                    }

                    case ParserState.OpeningTagBegin:
                    {
                        state = await ParseTagOpenAsync(() =>
                        {
                            element = document.CreateElement(null, name, null);
                            name = null;
                        }).ConfigureAwait(false);

                        break;
                    }

                    case ParserState.OpeningTagNamespaceColon:
                    {
                        state = await ParseOpeningTagNamespaceColonAsync(text =>
                        {
                            if (null != prefix)
                            {
                                throw new XamlParsingException();
                            }

                            prefix = name;
                            name = text;

                        }).ConfigureAwait(false);

                        break;
                    }

                    case ParserState.OpeningTagName:
                    {
                        state = await ParseOpeningTagNameAsync().ConfigureAwait(false);
                        break;
                    }

                    case ParserState.OpeningTagNameDot:
                    {
                        state = await ParseOpeningTagNameDotAsync(text =>
                        {
                            if (null == name)
                            {
                                throw new XamlParsingException();
                            }

                            name += ('.' + text);

                        }).ConfigureAwait(false);

                        break;
                    }

                    case ParserState.OpeningTagNameTrailing:
                    {
                        state = await ParseOpeningTagNameTrailingAsync(() =>
                        {
                            element = document.CreateElement(prefix, name, null);
                            prefix = null;
                            name = null;

                        }).ConfigureAwait(false);

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
//                        element = document.CreateElement(text, true);
                        state = ParserState.OpeningTagClosed;
//                        text = null;

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

                    /*case ParserState.OpeningTagTrailing:
                    {
                        var temp = name;
                           
                        state = await ParseOpeningTagTrailingAsync(text =>
                        {
                            name = BeginName(temp, text);
                        });

                        break;
                    }*/

                    case ParserState.OpeningTagAttributeCheck:
                    {
                        state = await ParseOpeningTagAttributeCheckAsync(text =>
                        {
                            prefix = null;
                            name = text;
                        }).ConfigureAwait(false);

                        break;
                    }

                    case ParserState.OpeningTagAttributeName:
                    {
                        state = await ParseOpeningTagAttributeNameAsync(() =>
                        {
                            if (null == element)
                            {
                                throw new XamlParsingException();
                            }

                            attribute = document.CreateAttribute(prefix, name, null);
                            element.Attributes.Append(attribute);
                            prefix = null;
                            name = null;

                        }).ConfigureAwait(false);

                        break;
                    }

                    case ParserState.OpeningTagAttributeNameEnd:
                    {
                        state = await ParseOpeningTagAttributeNameEndAsync().ConfigureAwait(false);
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
                        var token = await tokenizer.GetTokenAsync().ConfigureAwait(false);

                        if (token.IsSingleQuote())
                        {
                            state = ParserState.OpeningTagAttributeValueEnd;
                            break;
                        }


                        break;
                    }

                    case ParserState.OpeningTagAttributeQuotedValue:
                    {
                        state = await ParseOpeningTagAttributeQuotedValueAsync(text =>
                        {
                            
                        }).ConfigureAwait(false);

                        break;
                    }

                    case ParserState.OpeningTagAttributeQuotedValueEnd:
                    {
                        /*var attribute = document.CreateAttribute(text);

                        if (null == element)
                        {
                            throw new XamlParsingException();
                        }

                        text = null;
                        element.Attributes.Append(attribute);*/
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

                        state = ParserState.Failed;

                        break;
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
                        state = ParseClosingTagNameClose(nodes, null);
                        break;
                    }

                    default:
                    {
                        throw new XamlParsingException();
                    }
                }
            }
        }

        private async Task<ParserState> ParseOpeningTagAttributeQuotedValueAsync(Action<string> callback)
        {
            var token = await tokenizer.GetTokenAsync().ConfigureAwait(false);

            if (token.IsDoubleQuote())
            {
                return ParserState.OpeningTagAttributeQuotedValueEnd;
            }

            string text;

            if (token.IsString(out text))
            {
                callback.Invoke(text);
            }
            else if (token.IsWhitespace())
            {
                callback.Invoke(new String(' ', 1));
            }

            return ParserState.OpeningTagAttributeQuotedValue;
        }

        private async Task<ParserState> ParseOpeningTagAttributeNameAsync(Action callback)
        {
            var token = await tokenizer.GetTokenAsync().ConfigureAwait(false);

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
                callback.Invoke();
                return ParserState.OpeningTagAttributeNameEnd;
            }

            if (token.IsWhitespace())
            {
                // staying in the same state
                return ParserState.OpeningTagAttributeName;
            }

            return ParserState.Failed;
        }

        private async Task<ParserState> ParseOpeningTagAttributeCheckAsync(Action<string> callback)
        {
            var token = await tokenizer.GetTokenAsync().ConfigureAwait(false);

            if (token.IsWhitespace())
            {
                return ParserState.OpeningTagAttributeCheck;
            }

            string text;

            if (token.IsString(out text))
            {
                callback.Invoke(text);
                return ParserState.OpeningTagAttributeName;
            }

            if (token.IsSlash())
            {
                return ParserState.OpeningTagSlash;
            }

            if (token.IsCloseBracket())
            {
                return ParserState.OpeningTagClose;
            }

            return ParserState.Failed;
        }

        private async Task<ParserState> ParseOpeningTagTrailingAsync(Action<string> callback)
        {
            var token = await tokenizer.GetTokenAsync().ConfigureAwait(false);

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
            var token = await tokenizer.GetTokenAsync().ConfigureAwait(false);

            if (token.IsDot())
            {
                return ParserState.OpeningTagNameDot;
            }

            if (token.IsWhitespace())
            {
                callback.Invoke();
                return ParserState.OpeningTagAttributeCheck;
            }

            if (token.IsSlash())
            {
                callback.Invoke();
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
            var token = await tokenizer.GetTokenAsync().ConfigureAwait(false);

            string text;

            if (false == token.IsString(out text))
            {
                return ParserState.Failed;
            }

            callback.Invoke(text);

            return ParserState.OpeningTagNameTrailing;
        }

        private async Task<ParserState> ParseOpeningTagNamespaceColonAsync(Action<string> callback)
        {
            var token = await tokenizer.GetTokenAsync().ConfigureAwait(false);

            string text;

            if (false == token.IsString(out text))
            {
                return ParserState.Failed;
            }

            callback.Invoke(text);

            return ParserState.OpeningTagName;
        }

        private async Task<ParserState> ParseOpeningTagAttributeNameEndAsync()
        {
            var token = await tokenizer.GetTokenAsync().ConfigureAwait(false);

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

            return ParserState.Failed;
        }

        private async Task<ParserState> ParseOpeningTagNameAsync()
        {
            var token = await tokenizer.GetTokenAsync().ConfigureAwait(false);

            if (token.IsDot())
            {
                return ParserState.OpeningTagNameDot;
            }

            if (token.IsWhitespace())
            {
                return ParserState.OpeningTagAttributeCheck;
            }

            if (token.IsSlash())
            {
                return ParserState.OpeningTagNameSlash;
            }

            if (token.IsCloseBracket())
            {
                return ParserState.OpeningTagClose;
            }

            return ParserState.Failed;
        }

        private async Task<ParserState> ParseTagOpenAsync(Action callback)
        {
            var token = await tokenizer.GetTokenAsync().ConfigureAwait(false);

            if (token.IsColon())
            {
                return ParserState.OpeningTagNamespaceColon;
            }

            if (token.IsDot())
            {
                return ParserState.OpeningTagNameDot;
            }

            if (token.IsWhitespace())
            {
                callback.Invoke();
                return ParserState.OpeningTagAttributeCheck;
            }

            if (token.IsSlash())
            {
                return ParserState.OpeningTagNameSlash;
            }

            if (token.IsCloseBracket())
            {
                return ParserState.OpeningTagClose;
            }

            return ParserState.Failed;
        }

        private async Task<ParserState> ParseBeginAsync()
        {
            XamlToken token;

            while (true)
            {
                token = await tokenizer.GetTokenAsync().ConfigureAwait(false);

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

            return ParserState.Failed;
        }

        private async Task<ParserState> ParseOpenBracketAsync(Action<string> callback)
        {
            var token = await tokenizer.GetTokenAsync().ConfigureAwait(false);

            if (token.IsSlash())
            {
                return ParserState.ClosingTagBegin;
            }

            string text;

            if (false == token.IsString(out text))
            {
                return ParserState.Failed;
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
            Done,
            OpeningTagAttributeCheck,
            OpeningTagSlash
        }
    }
}