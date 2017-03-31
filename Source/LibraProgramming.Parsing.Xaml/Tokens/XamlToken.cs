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
        public static readonly XamlToken Unknown;

        public XamlTokenType TokenType
        {
            get;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="term"></param>
        /// <returns></returns>
        public static XamlTerminalToken Terminal(char term)
        {
            return new XamlTerminalToken(term);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
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
            Unknown = new XamlToken(XamlTokenType.Unknown);
        }
    }
}