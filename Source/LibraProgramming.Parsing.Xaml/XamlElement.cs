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

        public XamlElement this[string n]
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

                var element = ChildNodes.FirstOrDefault(
                    child => comparer.Equals(child.Name, n) && comparer.Equals(child.Prefix, ns)
                );

                return (XamlElement) element;
            }
        }

        public XamlElement(XamlDocument document, XamlName name)
            : this(XamlNodeType.Element, document)
        {
            this.name = name;
        }

        protected XamlElement(XamlNodeType nodeType, XamlDocument document)
            : base(nodeType, document, null)
        {
        }
    }
}