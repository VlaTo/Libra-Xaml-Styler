using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using LibraProgramming.Parsing.Xaml.Tokens;

namespace LibraProgramming.Parsing.Xaml
{
    internal sealed class XamlParser
    {
        private readonly XamlTokenizer tokenizer;
        private ParserState state;

        public XamlParser(XamlTokenizer tokenizer)
        {
            state = ParserState.Begin;
            this.tokenizer = tokenizer;
        }

        public Task ParseAsync(XamlDocument document)
        {
            if (null == document)
            {
                throw new ArgumentNullException(nameof(document));
            }

            var nodes = new Stack<XamlNode>();

            nodes.Push(document);

            return ParseInternalAsync(document, nodes);
        }

        private async Task ParseInternalAsync(XamlDocument document, Stack<XamlNode> nodes)
        {
            string prefix = null;
            string fullName = null;
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
                            state = ParserState.OpenBracket;

                            break;
                        }

                        throw new Exception();
                    }

                    case ParserState.OpenBracket:
                    {
                        var token = await tokenizer.GetTokenAsync();

                        if (token.IsSlash())
                        {
                            state = ParserState.ClosingTagBegin;
                            break;
                        }

                        string text;

                        if (false == token.IsString(out text))
                        {
                            throw new XamlParsingException();
                        }

                        /*if (false == nameBuilder.IsEmpty)
                        {
                            throw new XamlParsingException();
                        }*/

                        token = await tokenizer.GetTokenAsync();

                        if (token.IsColon())
                        {
                            prefix = text;
                            fullName = null;
                            state = ParserState.OpeningTagNamespaceColon;

                            break;
                        }

                        prefix = null;
                        fullName = text;

                        if (token.IsDot())
                        {
                            state = ParserState.OpeningTagNameDot;
                            break;
                        }

                        if (token.IsWhitespace())
                        {
                            state = ParserState.OpeningTagNameWhitespace;
                            break;
                        }

                        if (token.IsSlash())
                        {
                            state = ParserState.OpeningTagNameSlash;
                            break;
                        }

                        if (token.IsCloseBracket())
                        {
                            state = ParserState.OpeningTagNameClose;
                            break;
                        }

                        throw new XamlParsingException();
                    }

                    case ParserState.OpeningTagNamespaceColon:
                    {
                        string text;
                        var token = await tokenizer.GetTokenAsync();

                        if (false == token.IsString(out text))
                        {
                            throw new XamlParsingException();
                        }

                        fullName = text;
                        token = await tokenizer.GetTokenAsync();

                        if (token.IsDot())
                        {
                            state = ParserState.OpeningTagNameDot;
                            break;
                        }

                        if (token.IsWhitespace())
                        {
                            state = ParserState.OpeningTagNameWhitespace;
                            break;
                        }

                        if (token.IsSlash())
                        {
                            state = ParserState.OpeningTagNameSlash;
                            break;
                        }

                        if (token.IsCloseBracket())
                        {
                            state = ParserState.OpeningTagNameClose;
                            break;
                        }

                        throw new XamlParsingException();
                    }

                    case ParserState.OpeningTagNameDot:
                    {
                        string text;
                        var token = await tokenizer.GetTokenAsync();

                        if (false == token.IsString(out text))
                        {
                            throw new XamlParsingException();
                        }

                        fullName = new StringBuilder(fullName).Append('.').Append(text).ToString();
                        token = await tokenizer.GetTokenAsync();

                        if (token.IsDot())
                        {
                            break;
                        }

                        if (token.IsWhitespace())
                        {
                            state = ParserState.OpeningTagNameWhitespace;
                            break;
                        }

                        if (token.IsSlash())
                        {
                            state = ParserState.OpeningTagNameSlash;
                            break;
                        }

                        if (token.IsCloseBracket())
                        {
                            state = ParserState.OpeningTagNameClose;
                            break;
                        }

                        throw new XamlParsingException();
                    }

                    case ParserState.OpeningTagNameSlash:
                    {
                        var token = await tokenizer.GetTokenAsync();

                        if (token.IsCloseBracket())
                        {
                            state = ParserState.OpeningTagInlined;
                            break;
                        }

                        throw new XamlParsingException();
                    }

                    case ParserState.OpeningTagNameClose:
                    {
                        var parent = nodes.Peek();

                        if (null == parent)
                        {
                            throw new XamlParsingException();
                        }

                        var node = new XamlElement(document, XamlName.Create(prefix, fullName));

                        parent.AppendChild(node);
                        nodes.Push(node);
                        state = ParserState.OpeningTagClosed;

                        break;
                    }

                    case ParserState.OpeningTagInlined:
                    {
                        state = ParserState.OpeningTagClosed;

                        break;
                    }

                    case ParserState.OpeningTagClosed:
                    {
                        state = ParserState.Begin;
                        break;
                    }

                    case ParserState.OpeningTagNameWhitespace:
                    {
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private enum ParserState
        {
            Begin,
            //
            OpenBracket,
            OpeningTagNamespaceColon,
            OpeningTagNameDot,
            OpeningTagNameSlash,
            OpeningTagNameClose,
            OpeningTagNameWhitespace,
            OpeningTagInlined,
            OpeningTagClosed,
            //
            ClosingTagBegin
        }
    }
}