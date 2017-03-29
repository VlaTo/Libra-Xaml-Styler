namespace LibraProgramming.Parsing.Xaml
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class XamlLinkedNode : XamlNode
    {
        /// <summary>
        /// 
        /// </summary>
        public XamlLinkedNode PreviousSubling
        {
            get
            {
                var parent = ParentNode;

                if (null != parent)
                {
                    var node = parent.FirstChild as XamlLinkedNode;

                    while (null != node)
                    {
                        var nextSubling = node.NextSubling;

                        if (this == nextSubling)
                        {
                            return nextSubling;
                        }

                        node = nextSubling;
                    }
                }

                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public XamlLinkedNode NextSubling
        {
            get
            {
                var parent = ParentNode;
                return null != parent && parent.FirstChild != Next ? Next : null;
            }
        }

        internal XamlLinkedNode Next
        {
            get;
        }

        protected XamlLinkedNode(XamlNodeType nodeType, XamlDocument document, XamlLinkedNode next)
            : base(nodeType, document)
        {
            Next = next;
        }
    }
}