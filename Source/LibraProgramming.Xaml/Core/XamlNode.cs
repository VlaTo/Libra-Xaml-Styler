using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace LibraProgramming.Xaml.Core
{
    internal class XamlNode : IXamlNode
    {
        private XamlNode parent;

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

        public string Name
        {
            get;
        }

        public IReadOnlyCollection<string> NameSegments
        {
            get;
        }

        public XamlNode First => Children.FirstOrDefault();

        public XamlNode Last => Children.LastOrDefault();

        IReadOnlyCollection<IXamlAttribute> IXamlNode.Attributes => Attributes;

        IReadOnlyCollection<IXamlNode> IXamlNode.Children => Children;
        
        IXamlNode IXamlNode.Parent => Parent;

        public XamlNode(IReadOnlyCollection<string> nameSegments)
        {
            if (null == nameSegments)
            {
                throw new ArgumentNullException(nameof(nameSegments));
            }

            Attributes = new Collection<XamlAttribute>();
            Children = new Collection<XamlNode>();
            NameSegments = nameSegments;
            Name = String.Join('.'.ToString(), nameSegments);
        }
    }
}