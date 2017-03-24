using System.Diagnostics;

namespace LibraProgramming.Parsing.Xaml.Tokens
{
    [DebuggerDisplay("String, Text = {Text}")]
    internal sealed class XamlStringToken : XamlToken
    {
        public string Text
        {
            get;
        }

        public XamlStringToken(string text)
            : base(XamlTokenType.String)
        {
            Text = text;
        }
    }
}