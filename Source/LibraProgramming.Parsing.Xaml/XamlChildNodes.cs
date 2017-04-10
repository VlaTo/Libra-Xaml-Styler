using System.Collections;

namespace LibraProgramming.Parsing.Xaml
{
    public sealed class XamlChildNodes : XamlNodeList
    {
        private readonly XamlNode container;

        public override int Count
        {
            get
            {
                var count = 0;

                for (var node = container.FirstChild; null != node; node = node.NextSubling)
                {
                    count++;
                }

                return count;
            }
        }

        internal XamlChildNodes(XamlNode container)
        {
            this.container = container;
        }

        public override IEnumerator GetEnumerator()
        {
            return new XamlNodeEnumerator(container);
        }

        public override XamlNode GetNode(int index)
        {
            if (0 > index)
            {
                return null;
            }

            for (var node = container.FirstChild; null != node; node = node.NextSubling)
            {
                if (0 == index)
                {
                    return node;
                }

                index--;
            }

            return null;
        }

        protected override void Dispose(bool dispose)
        {
        }
    }
}