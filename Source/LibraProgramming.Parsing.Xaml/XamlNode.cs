using System;
using System.Collections;

namespace LibraProgramming.Parsing.Xaml
{
    /// <summary>
    /// 
    /// </summary>
    public enum XamlNodeType
    {
        /// <summary>
        /// 
        /// </summary>
        Attribute,

        /// <summary>
        /// 
        /// </summary>
        Element,

        /// <summary>
        /// 
        /// </summary>
        Comment,

        /// <summary>
        /// 
        /// </summary>
        Document,

        /// <summary>
        /// 
        /// </summary>
        Root
    }

    /// <summary>
    /// 
    /// </summary>
    public abstract class XamlNode : IEnumerable
    {
        private XamlNode parent;

        public virtual string BaseURI
        {
            get;
        }

        public virtual XamlAttributesCollection Attributes
        {
            get;
        }

        public virtual XamlNodeList ChildNodes => new XamlChildNodes(this);

        /// <summary>
        /// 
        /// </summary>
        public virtual XamlNode FirstChild => LastNode?.Next;

        /// <summary>
        /// 
        /// </summary>
        public virtual bool HasChildNodes => null != LastChild;

        public virtual XamlNode PreviousSubling
        {
            get;
        }

        public virtual XamlNode NextSubling
        {
            get;
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual XamlNode ParentNode
        {
            get
            {
                if (XamlNodeType.Document != parent.NodeType)
                {
                    return parent;
                }

                var first = parent.FirstChild as XamlLinkedNode;

                if (null != first)
                {
                    var node = first;

                    do
                    {
                        if (this == node)
                        {
                            return parent;
                        }

                        node = node.Next;

                    } while (null != node && first != node);
                }

                return null;
            }
        }

        public abstract string Name
        {
            get;
        }

        public virtual string NamespaceURI
        {
            get;
        }

        public virtual XamlNode LastChild
        {
            get
            {
                return LastNode;
            }
            internal set
            {
//                LastChild = value;
                LastNode = (XamlLinkedNode) value;
            }
        }

        public virtual XamlDocument OwnerDocument
        {
            get
            {
                if (null == parent)
                {
                    throw new InvalidOperationException();
                }

                if (XamlNodeType.Document == parent.NodeType)
                {
                    return (XamlDocument) parent;
                }

                return parent.OwnerDocument;
            }
        }

        public virtual string Prefix
        {
            get;
        }

        public XamlNodeType NodeType
        {
            get;
        }

        public virtual string LocalName
        {
            get;
        }

        public virtual string Value
        {
            get;
            set;
        }

        internal virtual XamlLinkedNode LastNode
        {
            get
            {
                return null;
            }
            set
            {
            }
        }

        protected internal virtual bool CanAcceptChilren => true;

        internal XamlNode(XamlNodeType nodeType)
        {
            Attributes = new XamlAttributesCollection(this);
            NodeType = nodeType;
        }

        protected XamlNode(XamlNodeType nodeType, XamlDocument document)
            : this(nodeType)
        {
            SetParent(document);
        }

        public IEnumerator GetEnumerator()
        {
            return new XamlNodeEnumerator(this);
        }

        public virtual XamlNode AppendChild(XamlNode node)
        {
            if (null == node)
            {
                throw new ArgumentNullException(nameof(node));
            }

            if (false == CanAcceptChilren)
            {
                throw new InvalidOperationException();
            }

            if (ReferenceEquals(this, node) || IsDescendantOf(node))
            {
                throw new ArgumentException("", nameof(node));
            }

            node.ParentNode?.RemoveChild(node);

            var last = LastNode;
            var appendee = (XamlLinkedNode) node;

            if (null == last)
            {
                appendee.Next = appendee;
            }
            else
            {
                appendee.Next = last.Next;
                last.Next = appendee;
            }

            LastNode = appendee;
            appendee.SetParent(this);

            return node;
        }

        public string GetNamespaceOfPrefix(string prefix)
        {
            return GetNamespaceOfPrefixInternal(prefix) ?? String.Empty;
        }

        public string GetPrefixOfNamespace(string ns)
        {
            return GetPrefixOfNamespaceInternal(ns) ?? String.Empty;
        }

        public virtual XamlNode RemoveChild(XamlNode node)
        {
            if (null == node)
            {
                throw new ArgumentNullException(nameof(node));
            }

            if (false == CanAcceptChilren)
            {
                throw new InvalidOperationException();
            }

            if (false == ReferenceEquals(node.ParentNode, this))
            {
                throw new InvalidOperationException();
            }

            var removee = (XamlLinkedNode) node;

            if (removee == FirstChild)
            {
                if (removee == LastNode)
                {
                    LastNode = null;
                }
                else
                {
                    LastNode.Next = removee.Next;
                }

                removee.Next = null;
                removee.SetParent(null);
            }
            else
            {
                var previous = (XamlLinkedNode) removee.PreviousSubling;

                if (removee == LastNode)
                {
                    LastNode = previous;
                }

                previous.Next = removee.Next;
                removee.Next = null;
                removee.SetParent(null);
            }

            return node;
        }

        public virtual void RemoveAll()
        {
            var child = FirstChild;

            while (null != child)
            {
                var subling = child.NextSubling;

                RemoveChild(child);
                child = subling;
            }
        }

        internal static void SplitName(string name, out string prefix, out string localName)
        {
            var position = name.IndexOf(':');

            if (-1 == position || 0 == position || name.Length - 1 == position)
            {
                prefix = String.Empty;
                localName = name;
            }
            else
            {
                prefix = name.Substring(0, position);
                localName = name.Substring(position + 1);
            }
        }

        internal string GetNamespaceOfPrefixInternal(string prefix)
        {
            var doc = OwnerDocument;

            if (null != doc)
            {
                if (null == prefix)
                {
                    return null;
                }

                var comparer = StringComparer.Ordinal;

                for (var node = this; null != node;)
                {
                    switch (node.NodeType)
                    {
                        case XamlNodeType.Element:
                        {
                            var element = (XamlElement) node;

                            if (0 < element.Attributes.Count)
                            {
                                var attributes = element.Attributes;

                                if (0 == prefix.Length)
                                {
                                    for (var index = 0; index < attributes.Count; index++)
                                    {
                                        var attr = attributes[index];

                                        if (0 == attr.Prefix.Length && comparer.Equals("xmlns", attr.LocalName))
                                        {
                                            return attr.Value;
                                        }
                                    }
                                }
                                else
                                {
                                    for (var index = 0; index < attributes.Count; index++)
                                    {
                                        var attr = attributes[index];

                                        if (comparer.Equals("xmlns", attr.Prefix))
                                        {
                                            if (comparer.Equals(attr.LocalName, prefix))
                                            {
                                                return attr.Value;
                                            }
                                        }
                                        else if (comparer.Equals(attr.Prefix, prefix))
                                        {
                                            return attr.NamespaceURI;
                                        }
                                    }
                                }
                            }

                            if (comparer.Equals(node.Prefix, prefix))
                            {
                                return node.NamespaceURI;
                            }

                            node = node.ParentNode;

                            break;
                        }

                        case XamlNodeType.Attribute:
                        {
                            node = ((XamlAttribute) node).ParentNode;
                            break;
                        }

                        default:
                        {
                            node = node.ParentNode;
                            break;
                        }
                    }
                }
            }

            return null;
        }

        internal string GetPrefixOfNamespaceInternal(string ns)
        {
            var doc = OwnerDocument;

            if (null != doc)
            {
                var comparer = StringComparer.Ordinal;

                for (var node = this; null != node;)
                {
                    switch (node.NodeType)
                    {
                        case XamlNodeType.Element:
                        {
                            var element = (XamlElement) node;

                            if (0 <= element.Attributes.Count)
                            {
                                var attributes = element.Attributes;

                                for (var index = 0; index < attributes.Count; index++)
                                {
                                    var attr = attributes[index];

                                    if (0 == attr.Prefix.Length)
                                    {
                                        if (comparer.Equals("xmlns", attr.LocalName) && attr.Value == ns)
                                        {
                                            return String.Empty;
                                        }
                                    }
                                    else if (comparer.Equals("xmlns", attr.Prefix))
                                    {
                                        if (attr.Value == ns)
                                        {
                                            return attr.LocalName;
                                        }
                                    }
                                    else if (comparer.Equals(ns, attr.NamespaceURI))
                                    {
                                        return attr.Prefix;
                                    }
                                }
                            }

                            if (comparer.Equals(ns, node.NamespaceURI))
                            {
                                return node.Prefix;
                            }

                            node = node.ParentNode;

                            break;
                        }

                        case XamlNodeType.Attribute:
                        {
                            node = ((XamlAttribute) node).ParentNode;
                            break;
                        }

                        default:
                        {
                            node = node.ParentNode;
                            break;
                        }
                    }
                }

                if (comparer.Equals(ns, "xml"))
                {
                    return "xml";
                }

                if (comparer.Equals(ns, "xmlns"))
                {
                    return "xmlns";
                }
            }

            return null;
        }

        protected internal void SetParent(XamlNode value)
        {
            parent = value ?? OwnerDocument;
        }

        protected bool IsDescendantOf(XamlNode node)
        {
            var temp = ParentNode;

            while (null != temp && ReferenceEquals(temp, this))
            {
                if (ReferenceEquals(temp, node))
                {
                    return true;
                }

                temp = temp.ParentNode;
            }

            return false;
        }
    }
}