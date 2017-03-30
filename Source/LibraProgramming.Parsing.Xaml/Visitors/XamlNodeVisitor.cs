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

            VisitDocument(document);
        }

        protected virtual void VisitDocument(XamlDocument document)
        {
            foreach (XamlNode child in document.ChildNodes)
            {
                ProcessChild(child);
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
            foreach (var attribute in element.Attributes)
            {
                VisitAttribute(attribute);
            }

            foreach (XamlNode child in element.ChildNodes)
            {
                ProcessChild(child);
            }
        }

        protected virtual void VisitUnknown(XamlNode node)
        {
            throw new XamlParsingException();
        }

        private void ProcessChild(XamlNode node)
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