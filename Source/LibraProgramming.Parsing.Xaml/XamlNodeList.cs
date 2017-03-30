using System;
using System.Collections;

namespace LibraProgramming.Parsing.Xaml
{
    public abstract class XamlNodeList : IEnumerable, IDisposable
    {
        public abstract int Count
        {
            get;
        }

        public virtual XamlNode this[int index] => GetNode(index);

        public abstract IEnumerator GetEnumerator();

        public abstract XamlNode GetNode(int index);

        public void Dispose()
        {
            Dispose(true);
        }

        protected abstract void Dispose(bool dispose);
    }
}