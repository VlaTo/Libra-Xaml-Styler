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
        private readonly char[] buffer;
        private bool disposed;
        private TokenizerState state;
        private int count;
        private int position;
        private bool eof;

        public XamlTokenizer(TextReader reader, int bufferSize)
        {
            this.reader = reader;
            state = TokenizerState.Unknown;
            buffer = new char[bufferSize];
            count = 0;
            position = 0;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public async Task<XamlToken> GetTokenAsync()
        {
            var text = new StringBuilder();

            while (true)
            {
                switch (state)
                {
                    case TokenizerState.Unknown:
                    {
                        var flag = await AdvancePosition();

                        if (flag)
                        {
                            state = TokenizerState.Reading;
                            break;
                        }

                        state = TokenizerState.EndOfDocument;

                        break;
                    }

                    case TokenizerState.EndOfDocument:
                    {
                        return XamlToken.End;
                    }

                    case TokenizerState.Reading:
                    {
                        var input = ReadCurrentChar();

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
                                return XamlToken.String(text.ToString());
                            }

                            await AdvancePosition();

                            return XamlToken.Terminal(current);
                        }

                        text.Append(current);

                        await AdvancePosition();

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
                '\n'
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

        internal int ReadCurrentChar()
        {
            return eof ? EndOfStream : buffer[position];
        }

        private async Task<bool> AdvancePosition()
        {
            if (eof)
            {
                return false;
            }

            if (position == count)
            {
                count = await reader.ReadBlockAsync(buffer, 0, buffer.Length);
                position = 0;

                if (0 == count)
                {
                    eof = true;
                    return false;
                }

                return true;
            }

            position++;

            return true;
        }

        private enum TokenizerState
        {
            Failed = -1,
            Unknown,
            Reading,
            EndOfDocument
        }
    }
}