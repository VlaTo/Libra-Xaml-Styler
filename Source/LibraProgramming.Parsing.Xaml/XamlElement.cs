using System;
using System.Linq;

namespace LibraProgramming.Parsing.Xaml
{
    /// <summary>
    /// 
    /// </summary>
    public class XamlElement : XamlLinkedNode
    {
        private static readonly StringComparer comparer = StringComparer.Ordinal;
        private readonly XamlName name;

        public override string Name => name.Name;

        public override string Prefix => name.Prefix;

        public override string LocalName => name.LocalName;

        public XamlElement this[int index]
        {
            get
            {
                if (0 > index || index >= ChildNodes.Count)
                {
                    throw new IndexOutOfRangeException();
                }

                return (XamlElement) ChildNodes[index];
            }
        }

        public XamlElement this[string name]
        {
            get
            {
                if (String.IsNullOrEmpty(name))
                {
                    throw new ArgumentException("", nameof(name));
                }

                var element = ChildNodes.FirstOrDefault(child => comparer.Equals(child.Name, name));

                return (XamlElement) element;
            }
        }

        public XamlElement this[string name, string ns]
        {
            get
            {
                if (String.IsNullOrEmpty(name))
                {
                    throw new ArgumentException("", nameof(name));
                }

                if (null == ns)
                {
                    throw new ArgumentNullException(nameof(ns));
                }

                var element = ChildNodes.FirstOrDefault(
                    child => comparer.Equals(child.Name, name) && comparer.Equals(child.Prefix, ns)
                );

                return (XamlElement) element;
            }
        }

        public XamlElement(XamlDocument document, XamlName name)
            : base(XamlNodeType.Element, document, null)
        {
            this.name = name;
        }

        protected XamlElement(XamlNodeType nodeType)
            : base(nodeType)
        {
        }
    }
}