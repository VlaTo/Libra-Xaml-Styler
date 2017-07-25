using System;

namespace LibraProgramming.Parsing.Xaml.Visitors
{
    public class ReformatXamlVisitor : XamlNodeVisitor
    {
        private readonly XamlWriter writer;
        private readonly DocumentReformatSettings settings;

        public ReformatXamlVisitor(XamlWriter writer, DocumentReformatSettings settings)
        {
            this.writer = writer;
            this.settings = settings;
        }

        public override void Visit(XamlDocument document)
        {
            base.Visit(document);
        }

        protected override void VisitRoot(XamlNode element)
        {
            base.VisitRoot(element);
        }

        protected override void VisitAttribute(XamlAttribute attribute)
        {
            writer.WriteAttributeBegin(attribute.Prefix, attribute.LocalName, attribute.NamespaceURI);

            var content = new XamlAttributeContent(attribute.Value);

            writer.WriteAttributeContent(content);
            writer.WriteAttributeEnd();
        }

        protected override void VisitComment(XamlComment comment)
        {
            base.VisitComment(comment);
        }

        protected override void VisitElement(XamlElement element)
        {
            writer.WriteElementBegin(element.Prefix, element.LocalName, element.NamespaceURI);

            base.VisitElement(element);

            if (false == element.HasChildNodes && false == String.IsNullOrEmpty(element.Value))
            {
                var content = new XamlTextContent(element.Value);
                writer.WriteElementContent(content);
            }

            writer.WriteElementEnd(true);
        }
    }
}