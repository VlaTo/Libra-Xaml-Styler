using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;

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
        Document
    }

    /// <summary>
    /// 
    /// </summary>
    public sealed class XamlAttributesCollection : Collection<XamlAttribute>
    {
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

        public virtual XamlNode LastChild => LastNode;

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
            Attributes = new XamlAttributesCollection();
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

        public string GetNamespaceOfPrefix(string ns)
        {
            throw new NotImplementedException();
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