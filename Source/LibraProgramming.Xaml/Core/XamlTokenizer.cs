using System;
using System.CodeDom;
using System.IO;
using System.Text;

namespace LibraProgramming.Xaml.Core
{
    internal struct SourcePosition
    {
        public int LineNumber
        {
            get;
        }

        public int CharPosition
        {
            get;
        }

        public SourcePosition(int lineNumber, int charPosition)
        {
            LineNumber = lineNumber;
            CharPosition = charPosition;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    internal enum XamlTerminal
    {
        EOF = -1,
        OpenAngleBracket,
        Whitespace,
        Equal,
        CloseAngleBracket,
        Slash,
        Dot,
        Colon,
        Quote,
        Unknown = Int32.MaxValue
    }

    /// <summary>
    /// 
    /// </summary>
    internal class XamlTokenizer : IDisposable
    {
        private readonly TextReader reader;
        private bool disposed;
        private int lineNumber;
        private int charPosition;

        public XamlTokenizer(TextReader reader)
        {
            this.reader = reader;

            lineNumber = 1;
            charPosition = 1;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public SourcePosition GetSourcePosition()
        {
            return new SourcePosition(lineNumber, charPosition);
        }

        public XamlTerminal GetTerminal()
        {
            XamlTerminal term;
            var current = ReadNextChar();

            if (ClassifyTerminal(current, out term))
            {
                return term;
            }

            throw new Exception();
        }

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
                        throw new Exception();
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

                var position = GetSourcePosition();

                throw new TokenizerException(position.LineNumber, position.CharPosition);
            }

            return XamlTerminal.EOF;
        }

        public XamlTerminal GetAttributeValueString(StringBuilder builder)
        {
            while (true)
            {
                var current = ReadNextChar();

                switch (current)
                {
                    case '\"':
                        return XamlTerminal.Quote;

                    case -1:
                        return XamlTerminal.EOF;

                    default:
                        builder.Append((char) current);
                        break;
                }
            }
        }

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

                case '\"':
                {
                    term = XamlTerminal.Quote;
                    return true;
                }

                case -1:
                {
                    term = XamlTerminal.EOF;
                    return true;
                }

                default:
                    if (Char.IsWhiteSpace((char) current) || Char.IsControl((char) current))
                    {
                        term = XamlTerminal.Whitespace;
                        return true;
                    }

                    term = XamlTerminal.Unknown;

                    break;
            }

            return false;
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
    }
}