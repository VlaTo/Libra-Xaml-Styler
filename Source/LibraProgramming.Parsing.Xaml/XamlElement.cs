using System;

namespace LibraProgramming.Parsing.Xaml
{
    /// <summary>
    /// 
    /// </summary>
    public class XamlElement : XamlLinkedNode
    {
        private XamlLinkedNode lastChild;

        public override string Name => XamlName.Name;

        public override string Prefix => XamlName.Prefix;

        public override string LocalName => XamlName.LocalName;

        public bool IsEmpty
        {
            get
            {
                return this == lastChild;
            }
            set
            {
                if (value)
                {
                    if (this != lastChild)
                    {
                        RemoveAll();
                        lastChild = this;
                    }
                }
                else
                {
                    if (this == lastChild)
                    {
                        lastChild = null;
                    }
                }
            }
        }

        public XamlElement this[int index]
        {
            get
            {
                var nodes = ChildNodes;

                if (0 > index || index >= nodes.Count)
                {
                    throw new IndexOutOfRangeException();
                }

                return (XamlElement) nodes[index];
            }
        }

        internal override XamlLinkedNode LastNode
        {
            get
            {
                return this == lastChild ? null : lastChild;
            }
            set
            {
                lastChild = value;
            }
        }

        /*public XamlElement this[string n]
        {
            get
            {
                if (String.IsNullOrEmpty(n))
                {
                    throw new ArgumentException("", nameof(n));
                }

                var element = ChildNodes.FirstOrDefault(child => comparer.Equals(child.Name, n));

                return (XamlElement) element;
            }
        }

        public XamlElement this[string n, string ns]
        {
            get
            {
                if (String.IsNullOrEmpty(n))
                {
                    throw new ArgumentException("", nameof(n));
                }

                if (null == ns)
                {
                    throw new ArgumentNullException(nameof(ns));
                }

                var temp = XamlName.Create(n, ns);

                for (XamlElement node = FirstChild; null != node; node = node.NextSubling)
                {
                    if(node.XamlName==)
                }
                var element = ChildNodes.FirstOrDefault(
                    child => comparer.Equals(child.Name, n) && comparer.Equals(child.Prefix, ns)
                );

                return (XamlElement) element;
            }
        }*/

        internal XamlName XamlName
        {
            get;
        }

        public XamlElement(XamlDocument document, XamlName name, bool empty = false)
            : this(XamlNodeType.Element, document, empty)
        {
            XamlName = name;
        }

        protected XamlElement(XamlNodeType nodeType, XamlDocument document, bool empty = false)
            : base(nodeType, document, null)
        {
            if (empty)
            {
                lastChild = this;
            }
        }
    }
}