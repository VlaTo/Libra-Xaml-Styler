using System;
using System.Collections;

namespace LibraProgramming.Parsing.Xaml
{
    /// <summary>
    /// 
    /// </summary>
    public sealed partial class XamlAttributesCollection : IEnumerable
    {
        private const int NotFound = -1;

        private readonly XamlNode owner;
        private XamlAttributesArray attributes;

        public int Count => attributes.Count;

        public XamlAttribute this[int index]
        {
            get
            {
                try
                {
                    return (XamlAttribute) attributes[index];
                }
                catch (ArgumentOutOfRangeException)
                {
                    throw new IndexOutOfRangeException();
                }
            }
        }

        public XamlAttribute this[string name]
        {
            get
            {
                
            }
        }

        public XamlAttributesCollection(XamlNode owner)
        {
            this.owner = owner;
            attributes = new XamlAttributesArray();
        }

        public IEnumerator GetEnumerator()
        {
            return attributes.GetEnumerator();
        }

        public XamlNode AddNode(XamlNode node)
        {
            ((XamlAttribute)node).ParentNode
        }

        private int FindIndex(string name)
        {
            for (var index = 0; index < Count; index++)
            {
                var attribute = (XamlAttribute) attributes[index];

                if (name == attribute.Name)
                {
                    return index;
                }
            }

            return NotFound;
        }

        private int FindIndex(string name, string namespaceURI)
        {
            for (var index = 0; index < Count; index++)
            {
                var attribute = (XamlAttribute)attributes[index];

                if (name == attribute.Name && namespaceURI == attribute.NamespaceURI)
                {
                    return index;
                }
            }

            return NotFound;
        }
    }
}