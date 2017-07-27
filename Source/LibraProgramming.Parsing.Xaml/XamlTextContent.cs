using System.IO;

namespace LibraProgramming.Parsing.Xaml
{
    internal class XamlTextContent : XamlContent
    {
        private readonly string text;

        public XamlTextContent(string text)
        {
            this.text = text;
        }

        public override void WriteTo(XamlWriter writer)
        {
            writer.WriteString(text);
        }
    }
}