using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using LibraProgramming.Parsing.Xaml.Tokens;

namespace LibraProgramming.Parsing.Xaml
{
    internal sealed class XamlTokenizer : IDisposable
    {
        private const int Eof = -1;

        private readonly TextReader reader;
        private readonly char[] buffer;
        private bool disposed;
        private TokenizerState state;
        private int bufferCount;
        private int bufferPosition;
        private bool ended;

        public XamlTokenizer(TextReader reader, int bufferSize)
        {
            this.reader = reader;
            state = TokenizerState.Normal;
            buffer = new char[bufferSize];
            bufferCount = 0;
            bufferPosition = 0;
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
                    case TokenizerState.EndOfDocument:
                    {
                        return XamlToken.End;
                    }

                    case TokenizerState.Normal:
                    {
                        var input = await ReadNextCharAsync(false);

                        if (Eof == input)
                        {
                            state = TokenizerState.EndOfDocument;

                            if (0 < text.Length)
                            {
                                return XamlToken.String(text.ToString());
                            }

                            return XamlToken.End;
                        }

                        var current = (char) input;

                        if (Char.IsWhiteSpace(current))
                        {
                            await ReadNextCharAsync(true);
                            return XamlToken.Terminal(XamlTerminal.Whitespace);
                        }

                        var term = GetTermFromChar(current);

                        if (XamlTerminal.Unknown == term)
                        {
                            text.Append((char) await ReadNextCharAsync(true));
                        }
                        else if (0 < text.Length)
                        {
                            return XamlToken.String(text.ToString());
                        }
                        else
                        {
                            await ReadNextCharAsync(true);
                            return XamlToken.Terminal(term);
                        }

                        break;
                    }

                    case TokenizerState.Unknown:
                    {
                        throw new Exception();
                    }

                    default:
                    {
                        throw new Exception();
                    }
                }
            }
        }

        private static XamlTerminal GetTermFromChar(char ch)
        {
            char[] terminals =
            {
                '<', ':', '.', '=', '/', '>'
            };
            XamlTerminal[] values =
            {
                XamlTerminal.OpenAngleBracket,
                XamlTerminal.Colon,
                XamlTerminal.Dot,
                XamlTerminal.Equal,
                XamlTerminal.Slash,
                XamlTerminal.CloseAngleBracket
            };
            var index = Array.IndexOf(terminals, ch);

            if (-1 == index)
            {
                return XamlTerminal.Unknown;
            }

            return values[index];
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

        internal async Task<int> ReadNextCharAsync(bool advance)
        {
            if (ended)
            {
                return Eof;
            }

            if (bufferPosition == bufferCount)
            {
                var count = await reader.ReadBlockAsync(buffer, 0, buffer.Length);

                if (0 == count)
                {
                    ended = true;
                    return Eof;
                }

                bufferCount = count;
                bufferPosition = 0;
            }

            var position = bufferPosition;

            if (advance)
            {
                bufferPosition++;
            }

            return buffer[position];
        }

        private enum TokenizerState
        {
            Failed = -1,
            Unknown,
            Normal,
            NodeBeginTerminal,
            TagOrAliasBegin,

            MultilineCommentProbe1,

            EndOfDocument
        }
    }
}