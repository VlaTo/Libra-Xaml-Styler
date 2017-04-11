namespace LibraProgramming.Parsing.Xaml
{
    public sealed class XamlRootElement : XamlElement
    {
        public XamlRootElement(XamlDocument document)
            : base(document, new XamlName(null, null, null, 0, document, null))
        {
        }
    }
}