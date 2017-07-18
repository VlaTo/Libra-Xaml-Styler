using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using LibraProgramming.Parsing.Xaml.Tokens;

namespace LibraProgramming.Parsing.Xaml
{
    internal sealed class XamlTokenizer : IDisposable
    {
        private const int EndOfStream = -1;

        private readonly TextReader reader;
        private bool disposed;
        private TokenizerState state;
        private int input;

        public TextPosition TextPosition
        {
            get;
            private set;
        }

        public XamlTokenizer(TextReader reader)
        {
            this.reader = reader;
            state = TokenizerState.Unknown;
            TextPosition = TextPosition.Empty;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public Task<XamlToken> GetTokenAsync()
        {
            if (disposed)
            {
                throw new ObjectDisposedException("tokenizer");
            }

            var text = new StringBuilder();

            while (true)
            {
                switch (state)
                {
                    case TokenizerState.Unknown:
                    {
                        input = reader.Read();

                        if (EndOfStream != input)
                        {
                            TextPosition = TextPosition.Begin();
                            state = TokenizerState.Reading;

                            break;
                        }

                        state = TokenizerState.EndOfDocument;

                        break;
                    }

                    case TokenizerState.EndOfDocument:
                    {
                        return Task.FromResult(XamlToken.End);
                    }

                    case TokenizerState.Reading:
                    {
                        if (EndOfStream == input)
                        {
                            state = TokenizerState.EndOfDocument;
                            break;
                        }

                        var current = (char) input;

                        if (IsTerm(current))
                        {
                            if (0 < text.Length)
                            {
                                return Task.FromResult<XamlToken>(XamlToken.String(text.ToString()));
                            }

                            input = reader.Read();

                            return Task.FromResult<XamlToken>(XamlToken.Terminal(current));
                        }

                        text.Append(current);

                        input = reader.Read();

                        break;
                    }

                    default:
                    {
                        throw new Exception();
                    }
                }
            }
        }

        private static bool IsTerm(char ch)
        {
            char[] terminals =
            {
                XamlTerminals.OpenAngleBracket,
                XamlTerminals.CloseAngleBracket,
                XamlTerminals.Colon,
                XamlTerminals.Dot,
                XamlTerminals.Equal,
                XamlTerminals.DoubleQuote,
                XamlTerminals.SingleQuote,
                XamlTerminals.Slash,
                XamlTerminals.Whitespace,
                '\t',
                '\r',
                '\n',
                '!',
                '@',
                '#',
                '$',
                '%',
                '&',
                '*',
                '(',
                ')',
                '-',
                '_',
                '=',
                '+',
                '[',
                ']',
                '{',
                '}',
                ':',
                ';',
                '\\',
                '/'
            };

            return 0 <= Array.IndexOf(terminals, ch);
        }

        private void Dispose(bool dispose)
        {
            if (disposed)
            {
                return;
            }

            try
            {
                if (dispose)
                {
                    reader.Dispose();
                }
            }
            finally
            {
                disposed = true;
            }
        }

        private enum TokenizerState
        {
            Unknown,
            Reading,
            EndOfDocument
        }
    }
}