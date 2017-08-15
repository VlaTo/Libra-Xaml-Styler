using System;

namespace LibraProgramming.Parsing.Xaml.Visitors
{
    public class ReformatXamlVisitor : XamlNodeVisitor
    {
        private readonly XamlWriter writer;

        public ReformatXamlVisitor(XamlWriter writer)
        {
            this.writer = writer;
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
            var ns = element.GetNamespaceOfPrefix(element.Prefix);

            writer.WriteStartElement(element.Prefix, element.LocalName, ns);

            base.VisitElement(element);
            
            if (element.IsEmpty && false == String.IsNullOrEmpty(element.Value))
            {
                var content = new XamlTextContent(element.Value);
                content.WriteTo(writer);
            }

            writer.WriteEndElement();
        }
    }
}