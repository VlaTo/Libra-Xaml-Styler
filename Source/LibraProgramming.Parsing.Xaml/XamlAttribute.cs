using System;

namespace LibraProgramming.Parsing.Xaml
{
    internal sealed class XamlAttribute : IXamlAttribute
    {
        private XamlNode node;
        private string prefix;
        private string name;

        public XamlNode Node
        {
            get
            {
                return node;
            }
            set
            {
                if (null != node)
                {
                    throw new Exception();
                }

                if (null == value)
                {
                    throw new ArgumentNullException(nameof(node));
                }

                node = value;
                node.Attributes.Add(this);
            }
        }

        public string Name
        {
            get
            {
                return name;
            }
            internal set
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

        public string Value
        {
            get;
            internal set;
        }

        IXamlNode IXamlAttribute.Node => Node;
    }
}