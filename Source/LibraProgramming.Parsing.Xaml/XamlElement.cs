using System;
using System.Linq;

namespace LibraProgramming.Parsing.Xaml
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class XamlElement : XamlNode
    {
        private static readonly StringComparer comparer = StringComparer.Ordinal;

        public override string Name
        {
            get;
        }

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

        public XamlElement()
            : base(XamlNodeType.Element)
        {
        }
    }
}