using System.Diagnostics;

namespace LibraProgramming.Parsing.Xaml.Tokens
{
    /// <summary>
    /// 
    /// </summary>
    internal enum XamlTokenType
    {
        Unknown = -1,
        End,
        Terminal,
        String
    }

    /// <summary>
    /// 
    /// </summary>
    [DebuggerDisplay("Type = {TokenType}")]
    internal class XamlToken
    {
        public static readonly XamlToken End;

        public XamlTokenType TokenType
        {
            get;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="term"></param>
        /// <returns></returns>
        public static XamlTerminalToken Terminal(XamlTerminal term)
        {
            return new XamlTerminalToken(term);
        }

        public static XamlStringToken String(string text)
        {
            return new XamlStringToken(text);
        }

        protected XamlToken(XamlTokenType tokenType)
        {
            TokenType = tokenType;
        }

        static XamlToken()
        {
            End = new XamlToken(XamlTokenType.End);
        }
    }
}