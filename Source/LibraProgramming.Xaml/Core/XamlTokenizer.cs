using System;
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
        Quote
    }

    /// <summary>
    /// 
    /// </summary>
    internal class XamlTokenizer : IDisposable
    {
        private readonly TextReader reader;
//        private readonly SourceXamlParsingContext context;
        private bool disposed;
        private int lineNumber;
        private int charPosition;
//        private TokenizerState state;
//        private char[] buffer;
//        private int bufferCount;
//        private int bufferPosition;

        public XamlTokenizer(TextReader reader)
        {
            this.reader = reader;

            lineNumber = 1;
            charPosition = 1;

//            this.context = context;

//            state = TokenizerState.DocumentBegin;
//            buffer = new char[128];
//            bufferCount = 0;
//            bufferPosition = 0;
        }

        public void Dispose()
        {
            Dispose(true);
        }

/*
        public IXamlToken GetNextToken()
        {
            while (true)
            {
                var input = ReadNextChar();
                var current = (char) input;
                var iseof = -1 == input;

                switch (state)
                {
                    case TokenizerState.DocumentBegin:
                        if (Char.IsWhiteSpace(current))
                        {
                            continue;
                        }

                        if ('<' == current)
                        {
                            state = TokenizerState.NodeBeginTerminal;
                        }

                        break;

                    case TokenizerState.NodeBeginTerminal:
                        if ('!' == current)
                        {
                            state = TokenizerState.MultilineCommentProbe1;
                            continue;
                        }

                        if (Char.IsLetter(current) || '_' == current)
                        {
                            state = TokenizerState.TagOrAliasBegin;
                        }

                        break;
                }
            }
        }
*/

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

        public XamlTerminal PeekTerminal()
        {
            XamlTerminal term;
            var current = PeekNextChar();

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

                throw new Exception();
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
            }

            if (Char.IsWhiteSpace((char) current) || Char.IsControl((char) current))
            {
                term = XamlTerminal.Whitespace;
                return true;
            }

            term = XamlTerminal.EOF;

            return false;
        }

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

        private int ReadNextChar()
        {
            /*if (bufferPosition == bufferCount)
            {
                var count = reader.Read(buffer, 0, buffer.Length);

                if (0 == count)
                {
                    return -1;
                }

                bufferCount = count;
                bufferPosition = 0;
            }

            return buffer[bufferPosition++];*/

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

        private int PeekNextChar()
        {
            return reader.Peek();
        }

        /*private enum TokenizerState
        {
            Error = -1,

            DocumentBegin,
            NodeBeginTerminal,
            TagOrAliasBegin,

            MultilineCommentProbe1,

            EndOfDocument
        }*/
    }
}