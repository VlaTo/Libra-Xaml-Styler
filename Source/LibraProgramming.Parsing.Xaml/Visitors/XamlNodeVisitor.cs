using System;

namespace LibraProgramming.Parsing.Xaml.Visitors
{
    public class XamlNodeVisitor
    {
        protected XamlNodeVisitor()
        {
        }

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
            foreach (var child in document.ChildNodes)
            {
                ProcessChildNode(child);
            }
        }

        protected virtual void VisitNode(XamlNode node)
        {
            foreach (var attribute in node.Attributes)
            {
                VisitAttribute(attribute);
            }

            foreach (var child in node.ChildNodes)
            {
                ProcessChildNode(child);
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
        }

        private void ProcessChildNode(XamlNode node)
        {
            var comment = node as XamlComment;

            if (null != comment)
            {
                VisitComment(comment);
            }
            else
            {
                ProcessChildElement(node);
            }
        }

        private void ProcessChildElement(XamlNode node)
        {
            var element = node as XamlElement;

            if (null != element)
            {
                VisitElement(element);
            }
            else
            {
                VisitNode(node);
            }
        }
    }
}