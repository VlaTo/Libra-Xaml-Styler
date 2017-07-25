using System;

namespace LibraProgramming.Parsing.Xaml
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class XamlParserException : Exception
    {
        /// <summary>
        /// 
        /// </summary>
        public TextPosition TextPosition
        {
            get;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="position"></param>
        public XamlParserException(TextPosition position)
        {
            TextPosition = position;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="position"></param>
        /// <param name="message"></param>
        public XamlParserException(TextPosition position, string message)
            : base(message)
        {
            TextPosition = position;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="position"></param>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public XamlParserException(TextPosition position, string message, Exception innerException)
            : base(message, innerException)
        {
            TextPosition = position;
        }
    }
}