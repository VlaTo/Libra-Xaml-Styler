using System;

namespace LibraProgramming.Parsing.Xaml.Tokens
{
    /// <summary>
    /// 
    /// </summary>
    internal static class XamlTokenExtension
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static bool IsEnd(this XamlToken token)
        {
            if (null == token)
            {
                throw new ArgumentNullException(nameof(token));
            }

            return XamlTokenType.End == token.TokenType;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static bool IsWhitespace(this XamlToken token)
        {
            if (null == token)
            {
                throw new ArgumentNullException(nameof(token));
            }

            if (XamlTokenType.Terminal != token.TokenType)
            {
                return false;
            }

            var term = ((XamlTerminalToken) token).Term;

            return term == XamlTerminals.Whitespace || term == '\t' || term == '\r' || term == '\n';
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static bool IsOpenBracket(this XamlToken token)
        {
            return CheckIsTerminal(token, XamlTerminals.OpenAngleBracket);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static bool IsColon(this XamlToken token)
        {
            return CheckIsTerminal(token, XamlTerminals.Colon);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static bool IsDot(this XamlToken token)
        {
            return CheckIsTerminal(token, XamlTerminals.Dot);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static bool IsDoubleQuote(this XamlToken token)
        {
            return CheckIsTerminal(token, XamlTerminals.DoubleQuote);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static bool IsSingleQuote(this XamlToken token)
        {
            return CheckIsTerminal(token, XamlTerminals.SingleQuote);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static bool IsEqual(this XamlToken token)
        {
            return CheckIsTerminal(token, XamlTerminals.Equal);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static bool IsSlash(this XamlToken token)
        {
            return CheckIsTerminal(token, XamlTerminals.Slash);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static bool IsCloseBracket(this XamlToken token)
        {
            return CheckIsTerminal(token, XamlTerminals.CloseAngleBracket);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="token"></param>
        /// <param name="term"></param>
        /// <returns></returns>
        public static bool IsTerminal(this XamlToken token, ref char term)
        {
            if (null == token)
            {
                throw new ArgumentNullException(nameof(token));
            }

            if (XamlTokenType.Terminal == token.TokenType)
            {
                term = ((XamlTerminalToken) token).Term;
                return true;
            }

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="token"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public static bool IsString(this XamlToken token, out string text)
        {
            if (null == token)
            {
                throw new ArgumentNullException(nameof(token));
            }

            if (XamlTokenType.String == token.TokenType)
            {
                text = ((XamlStringToken) token).Text;
                return true;
            }

            text = null;

            return false;
        }

        private static bool CheckIsTerminal(XamlToken token, char term)
        {
            if (null == token)
            {
                throw new ArgumentNullException(nameof(token));
            }

            if (XamlTokenType.Terminal != token.TokenType)
            {
                return false;
            }

            return term == ((XamlTerminalToken)token).Term;
        }
    }
}