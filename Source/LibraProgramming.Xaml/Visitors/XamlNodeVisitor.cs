using System;
using System.Collections.Generic;
using LibraProgramming.Xaml.Core;

namespace LibraProgramming.Xaml.Visitors
{
    public class XamlNodeVisitor
    {
        protected XamlNodeVisitor()
        {
        }

        public void Visit(IXamlNode node)
        {
            if (null == node)
            {
                throw new ArgumentNullException(nameof(node));
            }

            VisitNode(node);
        }

        protected virtual void VisitNode(IXamlNode node)
        {
            VisitOpenTag(node);
            VisitNodeChildren(node.Children);
            VisitCloseTag(node);
        }

        protected virtual void VisitNodeChildren(IReadOnlyCollection<IXamlNode> children)
        {
            foreach (var child in children)
            {
                VisitNode(child);
            }
        }

        protected virtual void VisitNodeAttributes(IReadOnlyCollection<IXamlAttribute> attributes)
        {
            foreach (var attribute in attributes)
            {
                VisitNodeAttribute(attribute);
            }
        }

        protected virtual void VisitNodeAttribute(IXamlAttribute attribute)
        {
        }

        protected virtual void VisitOpenTag(IXamlNode node)
        {
            VisitNodeAttributes(node.Attributes);
        }

        protected virtual void VisitCloseTag(IXamlNode node)
        {
        }
    }
}