using System;
using System.Threading.Tasks;
using LibraProgramming.Parsing.Xaml.Tokens;

namespace LibraProgramming.Parsing.Xaml
{
    internal sealed class XamlParser
    {
        private readonly XamlTokenizer tokenizer;
        private readonly XamlDocument document;
        private ParserState state;

        public XamlParser(XamlTokenizer tokenizer, XamlDocument document)
        {
            state = ParserState.Begin;
            this.tokenizer = tokenizer;
            this.document = document;
        }

        public async Task ParseAsync()
        {
            var done = false;
//            XamlNodeBuilder nodeBuilder;
            XamlNodeNameBuilder nameBuilder = null;

            while (false == done)
            {
                switch (state)
                {
                    case ParserState.Begin:
                    {
                        XamlToken token;

                        while (true)
                        {
                            token = await tokenizer.GetTermAsync();

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
                            nameBuilder = new XamlNodeNameBuilder();
                            state = ParserState.TagOpenBracket;

                            break;
                        }

                        throw new Exception();
                    }

                    case ParserState.TagOpenBracket:
                    {
                        string text;
                        var token = await tokenizer.GetAlphaNumericTokenAsync();

                        if (false == token.IsString(out text))
                        {
                            throw new Exception();
                        }

                        token = await tokenizer.GetTermAsync();

                        if (token.IsColon())
                        {
                            nameBuilder.SetNamespaceAlias(text);
                            state = ParserState.TagOpenNameColon;

                            break;
                        }

                        if (token.IsDot())
                        {
                            nameBuilder.SetName(text);
                            state = ParserState.TagOpenNameDot;

                            break;
                        }

                        if (token.IsEqual())
                        {
                            nameBuilder.SetName(text);
                            state = ParserState.TagOpenNameEqual;

                            break;
                        }

                        if (token.IsSlash())
                        {
                            nameBuilder.SetName(text);
                            state = ParserState.TagOpenNameSlash;

                            break;
                        }

                        if (token.IsCloseBracket())
                        {
                            nameBuilder.SetName(text);
                            state = ParserState.TagCloseBracket;

                            break;
                        }

                        throw new Exception();
                    }

                    case ParserState.TagOpenNameColon:
                    {
                        string text;
                        var token = await tokenizer.GetAlphaNumericTokenAsync();

                        if (false == token.IsString(out text))
                        {
                            throw new Exception();
                        }

                        token = await tokenizer.GetTermAsync();

                        if (token.IsDot())
                        {
                            nameBuilder.SetName(text);
                            state = ParserState.TagOpenNameDot;

                            break;
                        }

                        if (token.IsEqual())
                        {
                            nameBuilder.SetName(text);
                            state = ParserState.TagOpenNameEqual;

                            break;
                        }

                        if (token.IsSlash())
                        {
                            nameBuilder.SetName(text);
                            state = ParserState.TagOpenNameSlash;

                            break;
                        }

                        if (token.IsCloseBracket())
                        {
                            nameBuilder.SetName(text);
                            state = ParserState.TagCloseBracket;

                            break;
                        }

                        throw new Exception();
                    }

                    case ParserState.TagOpenNameDot:
                    {
                        string text;
                        var token = await tokenizer.GetAlphaNumericTokenAsync();

                        if (false == token.IsString(out text))
                        {
                            throw new Exception();
                        }

                        token = await tokenizer.GetTermAsync();

                        if (token.IsDot())
                        {
                            nameBuilder.AddPropertyPath(text);
                            break;
                        }

                        if (token.IsEqual())
                        {
                            nameBuilder.SetName(text);
                            state = ParserState.TagOpenNameEqual;

                            break;
                        }

                        if (token.IsSlash())
                        {
                            nameBuilder.SetName(text);
                            state = ParserState.TagOpenNameSlash;

                            break;
                        }

                        if (token.IsCloseBracket())
                        {
                            nameBuilder.SetName(text);
                            state = ParserState.TagCloseBracket;

                            break;
                        }

                        throw new Exception();
                    }

                    case ParserState.TagOpenNameEqual:
                    {
                        throw new Exception();
                    }

                    case ParserState.TagOpenNameSlash:
                    {
                        var token = await tokenizer.GetTermAsync();

                        if (token.IsCloseBracket())
                        {
                            state = ParserState.TagInlineClose;
                            break;
                        }

                        throw new Exception();
                    }

                    case ParserState.TagCloseBracket:
                    {
                        state = ParserState.TagClosed;
                        break;
                    }

                    case ParserState.TagInlineClose:
                    {
                        state = ParserState.TagClosed;
                        break;
                    }

                    case ParserState.TagClosed:
                    {
                        done = true;
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
            TagOpenBracket,
            TagOpenNameColon,
            TagOpenNameDot,
            TagOpenNameEqual,
            TagOpenNameSlash,
            TagCloseBracket,
            TagInlineClose,
            TagClosed
        }
    }
}