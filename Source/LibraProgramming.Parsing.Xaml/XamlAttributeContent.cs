using System.IO;

namespace LibraProgramming.Parsing.Xaml
{
    internal class XamlAttributeContent : XamlContent
    {
        private readonly string text;

        public XamlAttributeContent(string text)
        {
            this.text = text;
        }

        public override void WriteTo(TextWriter writer)
        {
            writer.Write('\"');
            writer.Write(text);
            writer.Write('\"');
        }
    }
}