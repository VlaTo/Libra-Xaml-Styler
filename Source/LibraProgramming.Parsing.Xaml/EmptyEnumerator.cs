using System;
using System.Collections;

namespace LibraProgramming.Parsing.Xaml
{
    internal sealed class EmptyEnumerator : IEnumerator
    {
        public static readonly IEnumerator Instance;

        public object Current
        {
            get
            {
                throw new InvalidOperationException();
            }
        }

        static EmptyEnumerator()
        {
            Instance = new EmptyEnumerator();
        }

        public bool MoveNext()
        {
            return false;
        }

        public void Reset()
        {
        }
    }
}