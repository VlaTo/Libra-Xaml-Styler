using System;
using System.IO;
using System.Text;

namespace LibraProgramming.Xaml.Core
{
    /// <summary>
    /// 
    /// </summary>
    internal enum XamlTokenType
    {
        Terminal
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
            state = TokenizerState.Begin;
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
                var current = ReadNextChar();

                switch (state)
                {
                    case TokenizerState.Begin:
                    {
                        on = false;
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

        private enum TokenizerState
        {
            Begin
        }
    }
}