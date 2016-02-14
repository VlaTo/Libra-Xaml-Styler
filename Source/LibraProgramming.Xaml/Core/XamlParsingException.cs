using System;

namespace LibraProgramming.Xaml.Core
{
    public class XamlParsingException : Exception
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="T:System.Exception"/> указанным сообщением об ошибке и ссылкой на внутреннее исключение, которое стало причиной данного исключения.
        /// </summary>
        /// <param name="message">Сообщение об ошибке с объяснением причин исключения. </param><param name="innerException">Исключение, вызвавшее текущее исключение, или указатель null (Nothing в Visual Basic), если внутреннее исключение не задано. </param>
        public XamlParsingException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Выполняет инициализацию нового экземпляра класса <see cref="T:System.Exception"/>, используя указанное сообщение об ошибке.
        /// </summary>
        /// <param name="message">Сообщение, описывающее ошибку. </param>
        public XamlParsingException(string message)
            : base(message)
        {
        }
    }
}