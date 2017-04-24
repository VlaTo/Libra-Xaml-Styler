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
                var hash = XamlName.GetHashCode(name);

                for (var index = 0; index < attributes.Count; index++)
                {
                    var attribute = (XamlAttribute) attributes[index];

                    if (hash == attribute.LocalNameHash &&
                        name.Equals(attribute.LocalName, StringComparison.Ordinal))
                    {
                        return attribute;
                    }
                }

                return null;
            }
        }

        public XamlAttribute this[string name, string ns]
        {
            get
            {
                var hash = XamlName.GetHashCode(name);

                for (var index = 0; index < attributes.Count; index++)
                {
                    var attribute = (XamlAttribute) attributes[index];

                    if (hash == attribute.LocalNameHash &&
                        name.Equals(attribute.LocalName, StringComparison.Ordinal) &&
                        ns.Equals(attribute.NamespaceURI, StringComparison.Ordinal))
                    {
                        return attribute;
                    }
                }

                return null;
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

        public XamlNode Append(XamlAttribute attribute)
        {
            var doc = attribute.OwnerDocument;

            if (null != doc && doc != owner.OwnerDocument)
            {
                throw new Exception();
            }

//            if (attribute.OwnerDocument != null)
            if (null != attribute.ParentNode)
            {
                DetachAttribute(attribute);
            }

            attributes.Add(attribute);

            return attribute;
        }

        public XamlAttribute Remove(XamlAttribute attribute)
        {
            for (var index = 0; index < attributes.Count;)
            {
                if (attributes[index] == attribute)
                {
                    return RemoveAt(index);
                }
            }

            return null;
        }

        public XamlAttribute RemoveAt(int index)
        {
            var attribute = (XamlAttribute) attributes[index];

            attributes.RemoveAt(index);
            attribute.SetParent(null);

            return attribute;
        }

        internal static void DetachAttribute(XamlAttribute attribute)
        {
            var parent = attribute.ParentNode;
            parent.Attributes.Remove(attribute);
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