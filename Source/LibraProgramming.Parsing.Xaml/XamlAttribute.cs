namespace LibraProgramming.Parsing.Xaml
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class XamlAttribute : XamlNode
    {
        /// <summary>
        /// 
        /// </summary>
        public override string Name => XamlName.Name;

        /// <summary>
        /// 
        /// </summary>
        public override string Prefix => XamlName.Prefix;

        /// <summary>
        /// 
        /// </summary>
        public override string NamespaceURI => XamlName.NamespaceURI;

        /// <summary>
        /// 
        /// </summary>
        public override string LocalName => XamlName.LocalName;

        /// <summary>
        /// 
        /// </summary>
        public override string Value
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public override XamlNodeList ChildNodes => null;

        /// <summary>
        /// 
        /// </summary>
        internal XamlName XamlName
        {
            get;
        }

        /// <summary>
        /// 
        /// </summary>
        internal int LocalNameHash => XamlName.HashCode;

        /// <summary>
        /// 
        /// </summary>
        protected internal override bool CanAcceptChilren => false;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="document"></param>
        /// <param name="name"></param>
        public XamlAttribute(XamlDocument document, XamlName name)
            : base(XamlNodeType.Attribute, document)
        {
            XamlName = name;
        }
    }
}