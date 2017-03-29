namespace LibraProgramming.Parsing.Xaml
{
    public class XamlName
    {
        private string prefix;
        private string name;
        private string ns;
        private string localName;

        public string Prefix
        {
            get
            {
                return prefix;
            }
        }

        public string Name
        {
            get
            {
                return name;
            }
        }

        public string LocalName
        {
            get
            {
                return localName;
            }
        }

        public static XamlName Create(string prefix, string localName, string ns)
        {
            return new XamlName(prefix, localName, ns);
        }

        internal XamlName(string prefix, string localName, string ns)
        {
            this.prefix = prefix;
            this.localName = localName;
            this.ns = ns;
        }
    }
}