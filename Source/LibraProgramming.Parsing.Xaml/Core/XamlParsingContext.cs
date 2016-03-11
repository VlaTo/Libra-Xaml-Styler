namespace LibraProgramming.Parsing.Xaml.Core
{
    internal class XamlParsingContext
    {
        private XamlNode root;

        public XamlNode DocumentRoot
        {
            get
            {
                return root;
            }
        }

        public XamlParsingContext()
        {
            root = new XamlRootNode();
        }

        public void ValidateDocument(XamlDocumentValidator validator)
        {
            
        }
    }
}