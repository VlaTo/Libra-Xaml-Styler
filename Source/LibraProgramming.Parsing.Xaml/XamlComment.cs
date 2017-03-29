namespace LibraProgramming.Parsing.Xaml
{
    public sealed class XamlComment : XamlNode
    {
        public override string Name => "#comment";

        public XamlComment(XamlDocument document)
            : base(XamlNodeType.Comment, document)
        {
        }
    }
}