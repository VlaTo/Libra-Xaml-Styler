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

        public async Task<XamlToken> GetTermAsync()
        {
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
                        var input = await ReadNextCharAsync(true);

                        if (Eof == input)
                        {
                            state = TokenizerState.EndOfDocument;
                            return XamlToken.End;
                        }

                        var current = (char) input;

                        if (Char.IsWhiteSpace(current))
                        {
                            return XamlToken.Terminal(XamlTerminal.Whitespace);
                        }

                        var term = GetTermFromChar(current);

                        if (XamlTerminal.Unknown != term)
                        {
                            return XamlToken.Terminal(term);
                        }

                        state = TokenizerState.Unknown;

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

        public async Task<XamlToken> GetAlphaNumericTokenAsync()
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

                            await ReadNextCharAsync(true);

                            if (0 < text.Length)
                            {
                                return XamlToken.String(text.ToString());
                            }

                            return XamlToken.End;
                        }

                        var current = (char) input;

                        if (CanAcceptSymbol(text.Length, current))
                        {
                            text.Append((char) await ReadNextCharAsync(true));
                            break;
                        }

                        return XamlToken.String(text.ToString());
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

        private static bool CanAcceptSymbol(int length, char ch)
        {
            return 0 == length ? Char.IsLetter(ch) : Char.IsLetterOrDigit(ch);
        }

        private static XamlTerminal GetTermFromChar(char ch)
        {
            char[] terminals =
            {
                '<', ':', '.', '=', '\\', '>'
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

        /*public XamlTerminal GetTerminal()
        {
            XamlTerminal term;
            var current = ReadNextCharAsync();

            if (ClassifyTerminal(current, out term))
            {
                return term;
            }

            throw new Exception();
        }*/

        /*
                private static bool ClassifyTerminal(int current, out XamlTerminal term)
                {
                    switch (current)
                    {
                        case '<':
                        {
                            term = XamlTerminal.OpenAngleBracket;
                            return true;
                        }

                        case '=':
                        {
                            term = XamlTerminal.Equal;
                            return true;
                        }

                        case '>':
                        {
                            term = XamlTerminal.CloseAngleBracket;
                            return true;
                        }

                        case '/':
                        {
                            term = XamlTerminal.Slash;
                            return true;
                        }

                        case '.':
                        {
                            term = XamlTerminal.Dot;
                            return true;
                        }

                        case ':':
                        {
                            term = XamlTerminal.Colon;
                            return true;
                        }
                    }

                    if (Char.IsWhiteSpace((char) current) || Char.IsControl((char) current))
                    {
                        term = XamlTerminal.Whitespace;
                        return true;
                    }

                    term = XamlTerminal.EOF;

                    return false;
                }
        */

        /*
                public XamlTerminal GetAlphaNumericString(out string str)
                {
                    var name = new StringBuilder();

                    while (true)
                    {
                        var current = ReadNextCharAsync();

                        if (-1 == current)
                        {
                            str = null;
                            break;
                        }

                        if (Char.IsDigit((char) current))
                        {
                            if (0 == name.Length)
                            {
                                throw new Exception();
                            }
                        }
                        else
                        if (!Char.IsLetter((char) current) && '_' != current)
                        {
                            XamlTerminal term;

                            str = name.ToString();

                            if (!ClassifyTerminal(current, out term))
                            {
                                throw new Exception();
                            }
                        }

                        name.Append((char) current);
                    }

                    return XamlTerminal.EOF;
                }
        */

        /*while (true)
        {
            if (-1 == current)
            {
                return null;
            }

            if (!Char.IsWhiteSpace((char)current))
            {
                break;
            }

            current = ReadNextChar();
        }

        var @namespace = String.Empty;
        var name = new StringBuilder();

        while (true)
        {
            if (':' == current)
            {
                if (0 == name.Length)
                {
                    throw new Exception();
                }

                if (0 < @namespace.Length)
                {
                    throw new Exception();
                }

                @namespace = name.ToString();
                name.Clear();

                continue;
            }

            if (Char.IsDigit((char) current))
            {
                if (0 == name.Length)
                {
                    throw new Exception();
                }
            }
            else
            if (!Char.IsLetter((char) current) && '_' != current)
            {
                // rollback one char
                break;
            }

            name.Append((char) current);

            current = ReadNextChar();
        }

        return new NodeName(@namespace, name.ToString());*/

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