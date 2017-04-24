namespace LibraProgramming.Parsing.Xaml
{
    public sealed class XamlAttribute : XamlNode
    {
        public override string Name => XamlName.Name;

        public override string Prefix => XamlName.Prefix;

        public override string NamespaceURI => XamlName.NamespaceURI;

        public override string LocalName => XamlName.LocalName;

        internal int LocalNameHash => XamlName.HashCode;

        /*public override string Value
        {
            get;
            set;
        }*/

        public override XamlNodeList ChildNodes => null;

        internal XamlName XamlName
        {
            get;
        }

        protected internal override bool CanAcceptChilren => false;

        public XamlAttribute(XamlDocument document, XamlName name)
            : base(XamlNodeType.Attribute, document)
        {
            XamlName = name;
        }
    }
}