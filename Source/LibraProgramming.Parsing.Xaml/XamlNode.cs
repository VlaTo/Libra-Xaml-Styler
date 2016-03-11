using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace LibraProgramming.Parsing.Xaml
{
    internal class XamlNode : IXamlNode
    {
        private XamlNode parent;
        private string prefix;
        private string name;

        public Collection<XamlAttribute> Attributes
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
                if (String.IsNullOrEmpty(Prefix))
                {
                    return String.Empty;
                }

                var resolver = new XamlNamespaceResolver(this);

                return resolver.GetUri(Prefix);
            }
        }

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

        public string Name
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

                if (String.Equals(name, value))
                {
                    return;
                }

                name = value;
            }
        }

        public string LocalName
        {
            get
            {
                for (int index = name.Length - 1; index >= 0; index--)
                {
                    if ('.' == name[index])
                    {
                        return name.Substring(index);
                    }
                }

                return name;
            }
        }

        public bool IsInline
        {
            get;
            set;
        }

        public XamlNode FirstChild => Children.FirstOrDefault();

        public XamlNode LastChild => Children.LastOrDefault();

        IReadOnlyCollection<IXamlAttribute> IXamlNode.Attributes => Attributes;

        IReadOnlyCollection<IXamlNode> IXamlNode.Children => Children;
        
        IXamlNode IXamlNode.Parent => Parent;

        public XamlNode()
        {
            prefix = String.Empty;
            Attributes = new Collection<XamlAttribute>();
            Children = new Collection<XamlNode>();
        }
    }
}