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
            writer.WriteStartDocument();
            base.Visit(document);
            writer.WriteEndDocument();
        }

        protected override void VisitAttribute(XamlAttribute attribute)
        {
            writer.WriteStartAttribute(attribute.Prefix, attribute.LocalName, attribute.NamespaceURI);

            var content = new XamlAttributeContent(attribute.Value);

            content.WriteTo(writer);

            writer.WriteEndAttribute();
        }

        protected override void VisitElement(XamlElement element)
        {
            writer.WriteStartElement(element.Prefix, element.LocalName, element.NamespaceURI);

            base.VisitElement(element);

            var empty = false == element.HasChildNodes && false == String.IsNullOrEmpty(element.Value);

            if (empty)
            {
                var content = new XamlTextContent(element.Value);
                content.WriteTo(writer);
            }

            writer.WriteEndElement();
        }
    }
}