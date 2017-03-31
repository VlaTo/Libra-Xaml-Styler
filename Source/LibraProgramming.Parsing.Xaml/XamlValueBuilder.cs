using System;
using System.Text;
using LibraProgramming.Parsing.Xaml.Tokens;

namespace LibraProgramming.Parsing.Xaml
{
    internal sealed class XamlValueBuilder
    {
        private readonly StringBuilder buffer;

        public string Value => buffer.ToString();

        public XamlValueBuilder()
        {
            buffer = new StringBuilder();
        }

        public XamlValueBuilder AccumulateToken(XamlToken token)
        {
            var term = XamlTerminals.Whitespace;
            string text;

            if (token.IsTerminal(ref term))
            {
                buffer.Append(term);
            }
            else if(token.IsString(out text))
            {
                buffer.Append(text);
            }
            else
            {
                throw new Exception();
            }

            return this;
        }
    }
}