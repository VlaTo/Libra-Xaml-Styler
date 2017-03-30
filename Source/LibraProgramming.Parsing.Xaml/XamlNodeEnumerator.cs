using System.Collections;

namespace LibraProgramming.Parsing.Xaml
{
    internal class XamlNodeEnumerator : IEnumerator
    {
        private readonly XamlNode container;

        internal XamlNode Current
        {
            get;
            private set;
        }

        object IEnumerator.Current => Current;

        public XamlNodeEnumerator(XamlNode container)
        {
            this.container = container;
            Current = null;
        }

        public bool MoveNext()
        {
            Current = null != Current ? Current.NextSubling : container.FirstChild;
            return null != Current;
        }

        public void Reset()
        {
            Current = null;
        }
    }
}