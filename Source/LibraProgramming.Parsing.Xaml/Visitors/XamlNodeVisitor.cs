using System;

namespace LibraProgramming.Parsing.Xaml.Visitors
{
    public class XamlNodeVisitor
    {
        public virtual void Visit(XamlDocument document)
        {
            if (null == document)
            {
                throw new ArgumentNullException(nameof(document));
            }

            VisitRoot(document.Root);
        }

        protected virtual void VisitRoot(XamlNode element)
        {
            foreach (XamlNode child in element.ChildNodes)
            {
                VisitNode(child);
            }
        }

        protected virtual void VisitAttribute(XamlAttribute attribute)
        {
        }

        protected virtual void VisitComment(XamlComment comment)
        {
        }

        protected virtual void VisitElement(XamlElement element)
        {
            foreach (XamlAttribute attribute in element.Attributes)
            {
                VisitAttribute(attribute);
            }

            foreach (XamlNode child in element.ChildNodes)
            {
                VisitNode(child);
            }
        }

        protected virtual void VisitUnknown(XamlNode node)
        {
            throw new ParsingException();
        }

        private void VisitNode(XamlNode node)
        {
            switch (node.NodeType)
            {
                case XamlNodeType.Comment:
                {
                    VisitComment((XamlComment) node);
                    break;
                }

                case XamlNodeType.Element:
                {
                    VisitElement((XamlElement) node);
                    break;
                }

                default:
                {
                    VisitUnknown(node);
                    break;
                }
            }
        }
    }
}