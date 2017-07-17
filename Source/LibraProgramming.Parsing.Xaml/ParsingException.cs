using System;

namespace LibraProgramming.Parsing.Xaml
{
    /// <summary>
    /// 
    /// </summary>
    public class ParsingException : Exception
    {
        /// <summary>
        /// 
        /// </summary>
        public ParsingException()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public ParsingException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}