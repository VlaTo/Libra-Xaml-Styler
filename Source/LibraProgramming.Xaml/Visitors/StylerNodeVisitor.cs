using System;
using System.Text;
using LibraProgramming.Xaml.Core;

namespace LibraProgramming.Xaml.Visitors
{
    public class StylerNodeVisitor : XamlNodeVisitor
    {
        private readonly StringBuilder builder;

        public StylerNodeVisitor(StringBuilder builder)
        {
            this.builder = builder;
        }

        protected override void VisitOpenTag(IXamlNode node)
        {
            builder.Append('<');

            if (!String.IsNullOrEmpty(node.Prefix))
            {
                builder.Append(node.Prefix).Append(':');
            }

            builder.Append(node.Name);

            base.VisitOpenTag(node);

            if (0 == node.Children.Count)
            {
                builder.Append('/');
            }

            builder.Append('>');
        }

        protected override void VisitNodeAttribute(IXamlAttribute attribute)
        {
            builder.Append(' ');

            if (!String.IsNullOrEmpty(attribute.Prefix))
            {
                builder.Append(attribute.Prefix).Append(':');
            }

            builder.Append(attribute.Name);

            if (!String.IsNullOrEmpty(attribute.Value))
            {
                builder
                    .Append('=')
                    .Append('\"')
                    .Append(attribute.Value)
                    .Append('\"');
            }

            base.VisitNodeAttribute(attribute);
        }

        protected override void VisitCloseTag(IXamlNode node)
        {
            base.VisitCloseTag(node);

            if (0 == node.Children.Count)
            {
                return;
            }

            builder.Append('<').Append('/');

            if (!String.IsNullOrEmpty(node.Prefix))
            {
                builder.Append(node.Prefix).Append(':');
            }

            builder.Append(node.Name).Append('>');
        }
    }
}