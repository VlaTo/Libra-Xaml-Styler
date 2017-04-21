namespace LibraProgramming.Parsing.Xaml
{
    public sealed class XamlAttribute : XamlNode
    {
        public override string Name => XamlName.Name;

        public override string Prefix => XamlName.Prefix;

        public override string NamespaceURI => XamlName.NamespaceURI;

/*
        public XamlNode Node
        {
            get
            {
                return node;
            }
            set
            {
                if (null != node)
                {
                    throw new Exception();
                }

                if (null == value)
                {
                    throw new ArgumentNullException(nameof(node));
                }

                node = value;
                node.Attributes.Add(this);
            }
        }
*/

        internal int LocalNameHash
        {
            get;
        }

        public override string Value
        {
            get;
            set;
        }

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