using System;

namespace LibraProgramming.Parsing.Xaml
{
    /// <summary>
    /// 
    /// </summary>
    public class XamlParsingException : Exception
    {
        public XamlParsingException()
        {
        }

        public XamlParsingException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}