namespace LibraProgramming.Parsing.Xaml
{
    public class XamlName
    {
        private string ns;

        public string Prefix
        {
            get;
        }

        public string Name
        {
            get;
        }

        public string LocalName
        {
            get
            {
                var position = Name.IndexOf('.');
                return 0 > position ? Name : Name.Substring(0, position);
            }
        }

        public static XamlName Create(string prefix, string name, string ns = null)
        {
            return new XamlName(prefix, name, ns);
        }

        internal XamlName(string prefix, string name, string ns)
        {
            Prefix = prefix;
            Name = name;
            this.ns = ns;
        }
    }
}