using System;
using System.IO;
using System.Text;

namespace LibraProgramming.Parsing.Xaml.Visitors
{
    public class ReformatXamlVisitor : XamlNodeVisitor
    {
        private readonly TextWriter writer;
        private readonly DocumentReformatSettings settings;

        public ReformatXamlVisitor(TextWriter writer, DocumentReformatSettings settings)
        {
            this.writer = writer;
            this.settings = settings;
        }

        protected override void VisitDocument(XamlDocument document)
        {
            base.VisitDocument(document);
        }

        protected override void VisitAttribute(XamlAttribute attribute)
        {
            writer.Write(CreateNameString(attribute));
            writer.Write('=');
        }

        protected override void VisitComment(XamlComment comment)
        {
            base.VisitComment(comment);
        }

        protected override void VisitElement(XamlElement element)
        {
            var name = CreateNameString(element);

            writer.Write('<');
            writer.Write(name);

            if (false == element.HasChildNodes)
            {
                for (var count = 0; count < settings.SpacesBeforeEmptyNodeClose; count++)
                {
                    writer.Write(' ');
                }

                writer.Write('/');
            }

            writer.Write('>');

            if (element.HasChildNodes)
            {
                base.VisitElement(element);

                writer.Write('<');
                writer.Write('/');
                writer.Write(name);
                writer.Write('>');
            }
        }

        private string CreateNameString(XamlNode node)
        {
            var name = new StringBuilder();

            if (false == String.IsNullOrEmpty(node.Prefix))
            {
                name.Append(node.Prefix);
                name.Append(':');
            }

            return name.Append(node.Name).ToString();
        }
    }
}