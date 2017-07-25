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

        protected override void VisitAttribute(XamlAttribute attribute)
        {
            writer.WriteAttributeBegin(attribute.Prefix, attribute.LocalName, attribute.NamespaceURI);

            var content = new XamlAttributeContent(attribute.Value);

            writer.WriteAttributeContent(content);
            writer.WriteAttributeEnd();
        }

        protected override void VisitElement(XamlElement element)
        {
            writer.WriteElementBegin(element.Prefix, element.LocalName, element.NamespaceURI);

            base.VisitElement(element);

            var empty = false == element.HasChildNodes && false == String.IsNullOrEmpty(element.Value);

            if (empty)
            {
                var content = new XamlTextContent(element.Value);
                writer.WriteElementContent(content);
            }

            writer.WriteElementEnd(empty);
        }
    }
}