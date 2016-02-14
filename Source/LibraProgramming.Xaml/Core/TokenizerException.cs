using System;

namespace LibraProgramming.Xaml.Core
{
    public class TokenizerException : Exception
    {
         public int LineNumber
         {
             get;
         }

        public int Positiion
        {
            get;
        }

        /// <summary>
        /// Выполняет инициализацию нового экземпляра класса <see cref="T:System.Exception"/>, используя указанное сообщение об ошибке.
        /// </summary>
        public TokenizerException(int lineNumber, int positiion)
        {
            LineNumber = lineNumber;
            Positiion = positiion;
        }
    }
}