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

        protected override void VisitNode(IXamlNode node)
        {
            builder.Append("node");
            base.VisitNode(node);
        }
    }
}