using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace LibraProgramming.Parsing.Xaml
{
    internal class XamlNode : IXamlNode
    {
        private XamlNode parent;
        private XamlNodeName name;

        private Collection<XamlAttribute> Attributes
        {
            get;
        }

        public Collection<XamlNode> Children
        {
            get;
        }

        public XamlNode Parent
        {
            get
            {
                return parent;
            }
            set
            {
                if (null != parent)
                {
                    throw new Exception();
                }

                if (null == value)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                parent = value;
                parent.Children.Add(this);
            }
        }

        public string BaseURI
        {
            get
            {
                /*if (String.IsNullOrEmpty(Prefix))
                {
                    return String.Empty;
                }

                var resolver = new XamlNamespaceResolver(this);

                return resolver.GetUri(Prefix);*/

                throw new NotImplementedException();
            }
        }

/*
        public string Prefix
        {
            get
            {
                return prefix;
            }
            set
            {
                if (null == value)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                if (String.Equals(prefix, value))
                {
                    return;
                }

                prefix = value;
            }
        }
*/

        public XamlNodeName Name
        {
            get
            {
                return name;
            }
            set
            {
                if (null == value)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                name = value;
            }
        }

        public XamlNode FirstChild => Children.FirstOrDefault();

        public XamlNode LastChild => Children.LastOrDefault();

        IReadOnlyCollection<IXamlAttribute> IXamlNode.Attributes => Attributes;

        IReadOnlyCollection<IXamlNode> IXamlNode.Children => Children;
        
        IXamlNode IXamlNode.Parent => Parent;

        public XamlNode()
        {
            Attributes = new Collection<XamlAttribute>();
            Children = new Collection<XamlNode>();
        }
    }
}