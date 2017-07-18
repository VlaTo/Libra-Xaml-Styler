using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LibraProgramming.Parsing.Xaml.Tokens;

namespace LibraProgramming.Parsing.Xaml
{
    internal sealed class XamlParser
    {
        private readonly XamlTokenizer tokenizer;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tokenizer"></param>
        public XamlParser(XamlTokenizer tokenizer)
        {
            this.tokenizer = tokenizer;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="document"></param>
        /// <returns></returns>
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
                    throw new ParsingException();
                }
            }
            catch (ParsingException)
            {
                throw;
            }
            catch (Exception exception)
            {
                throw new ParsingException("", exception);
            }
        }

        private async Task ParseInternalAsync(XamlDocument document, Stack<XamlNode> nodes)
        {
            var state = ParserState.Unknown;
            string prefix = null;
            string name = null;
            string value = null;
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
                        state = await ParseBeginAsync(text =>
                        {
                            value = text;

                        }).ConfigureAwait(false);

                        break;
                    }

                    case ParserState.OpenBracket:
                    {
                        state = await ParseOpenBracketAsync(text =>
                        {
                            if (null != name)
                            {
                                throw new ParsingException();
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
                                throw new ParsingException();
                            }

                            prefix = name;
                            name = text;

                        }).ConfigureAwait(false);

                        break;
                    }

                    case ParserState.OpeningTagName:
                    {
                        state = await ParseOpeningTagNameAsync(() =>
                        {
                            element = document.CreateElement(prefix, name, null);
                            prefix = null;
                            name = null;

                        }).ConfigureAwait(false);

                        break;
                    }

                    case ParserState.OpeningTagNameDot:
                    {
                        state = await ParseOpeningTagNameDotAsync(text =>
                        {
                            if (null == name)
                            {
                                throw new ParsingException();
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

                    case ParserState.OpeningTagSlash:
                    {
                        if (null == element)
                        {
                            state = ParserState.Failed;
                            break;
                        }

                        var token = await tokenizer.GetTokenAsync().ConfigureAwait(false);

                        if (token.IsCloseBracket())
                        {
                            state = ParserState.OpeningTagInline;
                            break;
                        }

                        state = ParserState.Failed;

                        break;
                    }

                    case ParserState.OpeningTagClose:
                    {
                        state = ParserState.OpeningTagClosed;
                        break;
                    }

                    case ParserState.OpeningTagInline:
                    {
                        if (null == element)
                        {
                            throw new ParsingException();
                        }

                        element.IsEmpty = true;
                        state = ParserState.OpeningTagClosed;

                        break;
                    }

                    case ParserState.OpeningTagClosed:
                    {
                        if (null == element)
                        {
                            throw new ParsingException();
                        }

                        var parent = nodes.Peek();

                        if (null == parent)
                        {
                            state = ParserState.Failed;
                            break;
                        }

                        if (null != parent.Value)
                        {
                            state = ParserState.Failed;
                            break;
                        }

                        parent.AppendChild(element);

                        if (false == element.IsEmpty)
                        {
                            nodes.Push(element);
                        }

                        state = ParserState.Begin;
                        element = null;

                        break;
                    }

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
                        state = await ParseOpeningTagAttributeNameAsync().ConfigureAwait(false);
                        break;
                    }

                    case ParserState.OpeningTagAttributeNameEnd:
                    {
                        state = await ParseOpeningTagAttributeNameEndAsync(() =>
                        {
                            var attribute = document.CreateAttribute(prefix, name, null);

                            element.Attributes.Append(attribute);

                            prefix = null;
                            name = null;

                        }).ConfigureAwait(false);

                        break;
                    }

                    case ParserState.OpeningTagAttributeNamespaceColon:
                    {
                        state = await ParseOpeningTagAttributeNamespaceColonAsync(text =>
                        {
                            if (null != prefix)
                            {
                                throw new ParsingException();
                            }

                            prefix = name;
                            name = text;

                        }).ConfigureAwait(false);

                        break;
                    }

                    case ParserState.OpeningTagAttributeNameDot:
                    {
                        state = await ParseOpeningTagAttributeNameDotAsync(text =>
                        {
                            name += ('.' + text);

                        }).ConfigureAwait(false);

                        break;
                    }

                    case ParserState.OpeningTagAttributeNameTrailing:
                    {
                        state = await ParseOpeningTagAttributeNameTrailingAsync().ConfigureAwait(false);
                        break;
                    }

                    case ParserState.OpeningTagAttributeNameWhitespaceTrailing:
                    {
                        var token = await tokenizer.GetTokenAsync().ConfigureAwait(false);

                        if (token.IsWhitespace())
                        {
                            state = ParserState.OpeningTagAttributeNameWhitespaceTrailing;
                        }
                        else if (token.IsEqual())
                        {
                            state = ParserState.OpeningTagAttributeNameEnd;
                        }
                        else
                        {
                            state = ParserState.Failed;
                        }

                        break;
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
                            value += text;

                        }).ConfigureAwait(false);

                        break;
                    }

                    case ParserState.OpeningTagAttributeQuotedValueEnd:
                    {
                        var index = element.Attributes.Count - 1;
                        var attribute = element.Attributes[index];

                        attribute.Value = value;

                        value = null;
                        state = ParserState.OpeningTagAttributeValueEnd;

                        break;
                    }

                    case ParserState.OpeningTagAttributeValueEnd:
                    {
                        state = await ParseOpeningTagAttributeValueEndAsync().ConfigureAwait(false);
                        break;
                    }

                    case ParserState.ClosingTagBegin:
                    {
                        state = await ParseClosingTagBeginAsync(text =>
                        {
                            name = text;

                        }).ConfigureAwait(false);

                        break;
                    }

                    case ParserState.ClosingTagName:
                    {
                        state = await ParseClosingTagNameAsync().ConfigureAwait(false);
                        break;
                    }

                    case ParserState.ClosingTagNamespaceColon:
                    {
                        state = await ParseClosingTagNamespaceColonAsync(text =>
                        {
                            prefix = name;
                            name = text;

                        }).ConfigureAwait(false);

                        break;
                    }

                    case ParserState.ClosingTagNameDot:
                    {
                        state = await ParseClosingTagNameDotAsync(text =>
                        {
                            name += ('.' + text);

                        }).ConfigureAwait(false);

                        break;
                    }

                    case ParserState.ClosingTagNameTrailing:
                    {
                        state = await ParseClosingTagNameTrailingAsync(text =>
                        {
                            name += text;

                        }).ConfigureAwait(false);

                        break;
                    }

                    case ParserState.ClosingTagNameClose:
                    {
                        var tagname = document.CreateElementName(prefix, name, null);

                        if (CheckClosingTag(nodes, tagname))
                        {
                            prefix = null;
                            name = null;
                            state = ParserState.Begin;
                        }
                        else
                        {
                            state = ParserState.Failed;
                        }

                        break;
                    }

                    case ParserState.TagInnerValue:
                    {
                        state = await ParseTagInnerValueAsync(text =>
                        {
                            value += text;

                        }).ConfigureAwait(false);

                        break;
                    }

                    case ParserState.TagInnerValueEnd:
                    {
                        var node = nodes.Peek();

                        if (null == node)
                        {
                            state = ParserState.Failed;
                            break;
                        }

                        node.Value = value;
                        state = ParserState.OpenBracket;
                        value = null;

                        break;
                    }

                    default:
                    {
                        throw new ParsingException();
                    }
                }
            }
        }

        private async Task<ParserState> ParseClosingTagBeginAsync(Action<string> callback)
        {
            var token = await tokenizer.GetTokenAsync().ConfigureAwait(false);

            if (false == token.IsString(out string text))
            {
                return ParserState.Failed;
            }

            callback.Invoke(text);

            return ParserState.ClosingTagName;
        }

        private async Task<ParserState> ParseClosingTagNameDotAsync(Action<string> callback)
        {
            var token = await tokenizer.GetTokenAsync().ConfigureAwait(false);

            if (false == token.IsString(out string text))
            {
                return ParserState.Failed;
            }

            callback.Invoke(text);

            return ParserState.ClosingTagNameTrailing;
        }

        /*token = await tokenizer.GetTokenAsync();

            if (token.IsDot())
            {
                break;
            }

            if (token.IsCloseBracket())
            {
                state = ParserState.ClosingTagNameClose;
                break;
            }

            state = ParserState.Failed;
        }*/

        private async Task<ParserState> ParseOpeningTagAttributeValueEndAsync()
        {
            var token = await tokenizer.GetTokenAsync().ConfigureAwait(false);

            if (token.IsWhitespace())
            {
//                return ParserState.OpeningTagTrailing;
                return ParserState.OpeningTagAttributeCheck;
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

        private async Task<ParserState> ParseOpeningTagAttributeQuotedValueAsync(Action<string> callback)
        {
            var token = await tokenizer.GetTokenAsync().ConfigureAwait(false);

            if (token.IsDoubleQuote())
            {
                return ParserState.OpeningTagAttributeQuotedValueEnd;
            }

            if (token.IsString(out string text))
            {
                callback.Invoke(text);
                return ParserState.OpeningTagAttributeQuotedValue;
            }

            if (token.IsWhitespace())
            {
                callback.Invoke(new String(' ', 1));
                return ParserState.OpeningTagAttributeQuotedValue;
            }
            else
            {
                var ch = '\0';

                if (token.IsTerminal(ref ch))
                {
                    callback.Invoke(new string(ch, 1));
                    return ParserState.OpeningTagAttributeQuotedValue;
                }
            }

            return ParserState.Failed;
        }

        private async Task<ParserState> ParseOpeningTagAttributeNameAsync()
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
                return ParserState.OpeningTagAttributeNameEnd;
            }

            if (token.IsWhitespace())
            {
                return ParserState.OpeningTagAttributeName;
            }

            return ParserState.Failed;
        }

        private async Task<ParserState> ParseOpeningTagAttributeNamespaceColonAsync(Action<string> callback)
        {
            var token = await tokenizer.GetTokenAsync().ConfigureAwait(false);

            if (false == token.IsString(out string text))
            {
                return ParserState.Failed;
            }

            callback.Invoke(text);

            return ParserState.OpeningTagAttributeNameTrailing;
        }

        private async Task<ParserState> ParseOpeningTagAttributeNameTrailingAsync()
        {
            var token = await tokenizer.GetTokenAsync().ConfigureAwait(false);

            if (token.IsDot())
            {
                return ParserState.OpeningTagAttributeNameDot;
            }

            if (token.IsWhitespace())
            {
                return ParserState.OpeningTagAttributeNameWhitespaceTrailing;
            }

            if (token.IsEqual())
            {
                return ParserState.OpeningTagAttributeNameEnd;
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

            if (token.IsString(out string text))
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

        private async Task<ParserState> ParseOpeningTagAttributeNameDotAsync(Action<string> callback)
        {
            var token = await tokenizer.GetTokenAsync().ConfigureAwait(false);

            if (token.IsString(out string text))
            {
                callback.Invoke(text);
                return ParserState.OpeningTagAttributeName;
            }

            return ParserState.Failed;
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
                return ParserState.OpeningTagSlash;
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

            if (false == token.IsString(out string text))
            {
                return ParserState.Failed;
            }

            callback.Invoke(text);

            return ParserState.OpeningTagNameTrailing;
        }

        private async Task<ParserState> ParseOpeningTagNamespaceColonAsync(Action<string> callback)
        {
            var token = await tokenizer.GetTokenAsync().ConfigureAwait(false);

            if (false == token.IsString(out string text))
            {
                return ParserState.Failed;
            }

            callback.Invoke(text);

            return ParserState.OpeningTagName;
        }

        private async Task<ParserState> ParseOpeningTagAttributeNameEndAsync(Action callback)
        {
            var token = await tokenizer.GetTokenAsync().ConfigureAwait(false);

            if (token.IsDoubleQuote())
            {
                callback.Invoke();
                return ParserState.OpeningTagAttributeQuotedValueBegin;
            }

            if (token.IsSingleQuote())
            {
                callback.Invoke();
                return ParserState.OpeningTagAttributeValueBegin;
            }

            if (token.IsWhitespace())
            {
                callback.Invoke();
                return ParserState.OpeningTagAttributeNameEnd;
            }

            return ParserState.Failed;
        }

        private async Task<ParserState> ParseOpeningTagNameAsync(Action callback)
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
                return ParserState.OpeningTagSlash;
            }

            if (token.IsCloseBracket())
            {
                callback.Invoke();
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
                callback.Invoke();
                return ParserState.OpeningTagSlash;
            }

            if (token.IsCloseBracket())
            {
                callback.Invoke();
                return ParserState.OpeningTagClose;
            }

            return ParserState.Failed;
        }

        private async Task<ParserState> ParseBeginAsync(Action<string> callback)
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

            if (token.IsString(out string text))
            {
                callback.Invoke(text);
                return ParserState.TagInnerValue;
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

            if (false == token.IsString(out string text))
            {
                return ParserState.Failed;
            }

            callback.Invoke(text);

            return ParserState.OpeningTagBegin;
        }

        private async Task<ParserState> ParseClosingTagNameAsync()
        {
            var token = await tokenizer.GetTokenAsync().ConfigureAwait(false);

            if (token.IsColon())
            {
                return ParserState.ClosingTagNamespaceColon;
            }

            if (token.IsDot())
            {
                return ParserState.ClosingTagNameDot;
            }

            if (token.IsCloseBracket())
            {
                return ParserState.ClosingTagNameClose;
            }

            return ParserState.Failed;
        }

        private async Task<ParserState> ParseClosingTagNameTrailingAsync(Action<string> callback)
        {
            var token = await tokenizer.GetTokenAsync().ConfigureAwait(false);

            if (token.IsString(out string text))
            {
                callback.Invoke(text);
                return ParserState.ClosingTagNameTrailing;
            }

            if (token.IsDot())
            {
                return ParserState.ClosingTagNameDot;
            }

            if (token.IsCloseBracket())
            {
                return ParserState.ClosingTagNameClose;
            }

            return ParserState.Failed;
        }

        private async Task<ParserState> ParseClosingTagNamespaceColonAsync(Action<string> callback)
        {
            var token = await tokenizer.GetTokenAsync().ConfigureAwait(false);

            if (false == token.IsString(out string text))
            {
                return ParserState.Failed;
            }

            callback.Invoke(text);

            return ParserState.ClosingTagNameTrailing;
        }

        private async Task<ParserState> ParseTagInnerValueAsync(Action<string> callback)
        {
            var token = await tokenizer.GetTokenAsync().ConfigureAwait(false);

            if (token.IsOpenBracket())
            {
                return ParserState.TagInnerValueEnd;
            }

            if (token.IsString(out string text))
            {
                callback.Invoke(text);
                return ParserState.TagInnerValue;
            }

            if (token.IsWhitespace())
            {
                callback.Invoke(((XamlTerminalToken) token).Term.ToString());
                return ParserState.TagInnerValue;
            }

            return ParserState.Failed;
        }

        private static bool CheckClosingTag(Stack<XamlNode> nodes, XamlName name)
        {
            var last = nodes.Peek() as XamlElement;

            if (null == last)
            {
                throw new ParsingException();
            }

            if (false == last.XamlName.Equals(name))
            {
                return false;
            }

            nodes.Pop();

            return true;
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
            OpeningTagClose,
//            OpeningTagTrailing,
            OpeningTagInline,
            OpeningTagClosed,
            //
            OpeningTagAttributeCheck,
            OpeningTagAttributeName,
            OpeningTagAttributeNamespaceColon,
            OpeningTagAttributeNameDot,
            OpeningTagAttributeNameTrailing,
            OpeningTagAttributeNameEnd,
            OpeningTagAttributeNameWhitespaceTrailing,
            OpeningTagAttributeQuotedValueBegin,
            OpeningTagAttributeValue,
            OpeningTagAttributeValueBegin,
            OpeningTagAttributeValueEnd,
            OpeningTagAttributeQuotedValue,
            OpeningTagAttributeQuotedValueEnd,
            OpeningTagSlash,
            //
            ClosingTagBegin,
            ClosingTagName,
            ClosingTagNamespaceColon,
            ClosingTagNameDot,
            ClosingTagNameTrailing,
            ClosingTagNameClose,
            //
            TagInnerValue,
            TagInnerValueEnd,
            //
            Done
        }
    }
}