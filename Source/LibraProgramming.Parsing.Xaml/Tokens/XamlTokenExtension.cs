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
            return IsTerm(token, XamlTerminal.End);
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

            return CheckTerm(token, XamlTerminal.Whitespace);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static bool IsOpenBracket(this XamlToken token)
        {
            return IsTerm(token, XamlTerminal.OpenAngleBracket);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="token"></param>
        /// <param name="term"></param>
        /// <returns></returns>
        public static bool IsColon(this XamlToken token)
        {
            return IsTerm(token, XamlTerminal.Colon);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="token"></param>
        /// <param name="term"></param>
        /// <returns></returns>
        public static bool IsDot(this XamlToken token)
        {
            return IsTerm(token, XamlTerminal.Dot);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="token"></param>
        /// <param name="term"></param>
        /// <returns></returns>
        public static bool IsEqual(this XamlToken token)
        {
            return IsTerm(token, XamlTerminal.Equal);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="token"></param>
        /// <param name="term"></param>
        /// <returns></returns>
        public static bool IsSlash(this XamlToken token)
        {
            return IsTerm(token, XamlTerminal.Slash);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="token"></param>
        /// <param name="term"></param>
        /// <returns></returns>
        public static bool IsCloseBracket(this XamlToken token)
        {
            return IsTerm(token, XamlTerminal.CloseAngleBracket);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="token"></param>
        /// <param name="term"></param>
        /// <returns></returns>
        public static bool IsTerm(this XamlToken token, XamlTerminal term)
        {
            if (null == token)
            {
                throw new ArgumentNullException(nameof(token));
            }

            return CheckTerm(token, term);
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

        private static bool CheckTerm(XamlToken token, XamlTerminal term)
        {
            if (XamlTokenType.Terminal != token.TokenType)
            {
                return false;
            }

            return term == ((XamlTerminalToken)token).Term;
        }
    }
}