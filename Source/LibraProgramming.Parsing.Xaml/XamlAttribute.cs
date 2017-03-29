namespace LibraProgramming.Parsing.Xaml
{
    public sealed class XamlAttribute : XamlNode
    {
        private readonly XamlName name;

        public override string Name => name.Name;

        public override string Prefix => name.Prefix;

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

        public override string Value
        {
            get
            {
                return null;
            }
        }

        public XamlAttribute(XamlDocument document, XamlName name)
            : base(XamlNodeType.Attribute, document)
        {
            this.name = name;
        }
    }
}