using System;
using System.Collections.Generic;

namespace LibraProgramming.Xaml.Core
{
    internal sealed class XamlAttribute : IXamlAttribute
    {
        private XamlNode node;

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
                    throw new SourceXamlParsingException();
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
            get;
        }

        public string Value
        {
            get;
            internal set;
        }

        public IReadOnlyCollection<string> NameSegments
        {
            get;
        }

        IXamlNode IXamlAttribute.Node
        {
            get;
        }

        public XamlAttribute(IReadOnlyCollection<string> nameSegments)
        {
            NameSegments = nameSegments;
            Name = String.Join('.'.ToString(), nameSegments);
        }
    }
}