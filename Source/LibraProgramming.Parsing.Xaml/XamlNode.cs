using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

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
    public sealed class XamlNodeList : Collection<XamlNode>
    {
    }    

    /// <summary>
    /// 
    /// </summary>
    public abstract class XamlNode : IEnumerable<XamlNode>
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

        public virtual XamlNodeList ChildNodes
        {
            get;
        }

        public virtual XamlNode FirstChild => ChildNodes.FirstOrDefault();

        public virtual bool HasChildNodes => 0 < ChildNodes.Count;

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

        public virtual XamlNode LastChild => ChildNodes.LastOrDefault();

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

        protected XamlNode(XamlNodeType nodeType, XamlDocument document)
        {
            Attributes = new XamlAttributesCollection();
            ChildNodes = new XamlNodeList();
            NodeType = nodeType;
            parent = document;
        }

        public IEnumerator<XamlNode> GetEnumerator()
        {
            return ChildNodes.GetEnumerator();
        }

        public XamlNode AppendChild(XamlNode node)
        {
            if (null == node)
            {
                throw new ArgumentNullException(nameof(node));
            }

            ChildNodes.Add(node);

            return node;
        }

        public string GetNamespaceOfPrefix(string ns)
        {
            throw new NotImplementedException();
        }

        public XamlNode RemoveChild(XamlNode node)
        {
            if (null == node)
            {
                throw new ArgumentNullException(nameof(node));
            }

            if (false == ChildNodes.Remove(node))
            {
                throw new ArgumentException("", nameof(node));
            }

            return node;
        }

        public void RemoveAll()
        {
            ChildNodes.Clear();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}