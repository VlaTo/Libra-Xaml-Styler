using System;
using LibraProgramming.Xaml.Core;

namespace LibraProgramming.Xaml.Visitors
{
    public class XamlNodeVisitor
    {
        protected XamlNodeVisitor()
        {
            
        }

        protected virtual void VisitNode(IXamlNode node)
        {
            
        }

        public void Visit(IXamlNode node)
        {
            if (null == node)
            {
                throw new ArgumentNullException(nameof(node));
            }


        } 
    }
}