using System;

namespace LibraProgramming.Xaml.Core
{
    internal class TokenizerException : Exception
    {
        public XamlTokenizer Tokenizer
        {
            get;
        }

        /// <summary>
        /// Выполняет инициализацию нового экземпляра класса <see cref="T:System.Exception"/>, используя указанное сообщение об ошибке.
        /// </summary>
        public TokenizerException(XamlTokenizer tokenizer, string message)
            : base(message)
        {
            Tokenizer = tokenizer;
        }
    }
}