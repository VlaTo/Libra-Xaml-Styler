using System;
using System.IO;

namespace LibraProgramming.Parsing.Xaml
{
    /// <summary>
    /// 
    /// </summary>
    internal enum XamlTokenType
    {
        Terminal,
        EndOfStream
    }

    /// <summary>
    /// 
    /// </summary>
    internal class XamlToken
    {
        public XamlTokenType TokenType
        {
            get;
        }

        public string Token
        {
            get;
        }

        public uint LineNumber
        {
            get;
        }

        public uint Position
        {
            get;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public XamlToken(XamlTokenType tokenType, uint lineNumber, uint position)
            : this(tokenType, null, lineNumber, position)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public XamlToken(XamlTokenType tokenType, string token, uint lineNumber, uint position)
        {
            TokenType = tokenType;
            Token = token;
            LineNumber = lineNumber;
            Position = position;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    internal class XamlTokenizer : IDisposable
    {
        private readonly TextReader reader;
        private bool disposed;
        private uint lineNumber;
        private uint charPosition;
        private TokenizerState state;

        public XamlTokenizer(TextReader reader)
        {
            this.reader = reader;

            lineNumber = 1;
            charPosition = 1;
            state = TokenizerState.NotStarted;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public XamlToken ReadNextToken()
        {
            var on = true;

            while (on)
            {
                switch (state)
                {
                    case TokenizerState.EndOfStream:
                    {
                        return new XamlToken(XamlTokenType.EndOfStream, lineNumber, charPosition);
                    }

                    case TokenizerState.NotStarted:
                    {
                        var current = ReadNextChar();

                        if (-1 == current)
                        {
                            state = TokenizerState.EndOfStream;
                            continue;
                        }

                        if (Char.IsWhiteSpace((char) current))
                        {
                            state = TokenizerState.HeadingWhitespaces;
                            continue;
                        }


                        switch (current)
                        {
                            case '=':
                            case '<':
                            case '>':
                            {
                                state = TokenizerState.Terminal;
                                return new XamlToken(XamlTokenType.Terminal, ((char) current).ToString(), lineNumber, charPosition);
                            }
                        }

                        break;
                    }
                }
            }

            return new XamlToken(
                XamlTokenType.Terminal,
                '<'.ToString(),
                lineNumber,
                charPosition);
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

        private int ReadNextChar()
        {
            var current = reader.Read();

            if (-1 != current)
            {
                if ('\n' == (char) current)
                {
                    lineNumber++;
                    charPosition = 1;
                }
                else if ('\r' != (char) current)
                {
                    charPosition++;
                }
            }

            return current;
        }

/*
        public XamlTerminal GetAlphaNumericString(out string str)
        {
            var name = new StringBuilder();

            while (true)
            {
                var current = ReadNextChar();

                if (-1 == current)
                {
                    str = name.ToString();
                    break;
                }

                if (Char.IsDigit((char) current))
                {
                    if (0 == name.Length)
                    {
                        throw new TokenizerException(this, "");
                    }

                    name.Append((char) current);

                    continue;
                }

                if (Char.IsLetter((char) current) || '_' == current)
                {
                    name.Append((char) current);
                    continue;
                }

                XamlTerminal term;

                str = name.ToString();

                if (ClassifyTerminal(current, out term))
                {
                    return term;
                }

                throw new TokenizerException(this, "");
            }

            return XamlTerminal.EOF;
        }
*/

        /// <summary>
        /// The <see cref="XamlTokenizer" /> internal states.
        /// </summary>
        private enum TokenizerState
        {
            EndOfStream = -1,
            NotStarted,
            HeadingWhitespaces,
            Terminal
        }
    }
}