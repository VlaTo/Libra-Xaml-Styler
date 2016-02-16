using System;

namespace LibraProgramming.Xaml.Core
{
    public class XamlParsingException : Exception
    {
        public int LineNumber
        {
            get;
        }

        public int Position
        {
            get;
        }

        public override string Message
        {
            get
            {
                if (!String.IsNullOrEmpty(base.Message))
                {
                    return base.Message;
                }

                var message = String.Format("General parsing error at line: {0} position: {1}", LineNumber, Position);

                return message;
            }
        }

        /// <summary>
        /// Выполняет инициализацию нового экземпляра класса <see cref="T:System.Exception"/>, используя указанное сообщение об ошибке.
        /// </summary>
        /// <param name="message">Сообщение, описывающее ошибку. </param>
        public XamlParsingException(int lineNumber, int position, string message)
            : base(message)
        {
            LineNumber = lineNumber;
            Position = position;
        }

        /// <summary>
        /// Выполняет инициализацию нового экземпляра класса <see cref="T:System.Exception"/>, используя указанное сообщение об ошибке.
        /// </summary>
        /// <param name="message">Сообщение, описывающее ошибку. </param>
        public XamlParsingException(int lineNumber, int position, string message, Exception innerException)
            : base(message, innerException)
        {
            LineNumber = lineNumber;
            Position = position;
        }
    }
}