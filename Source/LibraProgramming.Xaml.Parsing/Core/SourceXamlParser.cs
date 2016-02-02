namespace LibraProgramming.Xaml.Parsing.Core
{
    internal class SourceXamlParser
    {
        private readonly SourceXamlTokenizer tokenizer;

        public SourceXamlParser(SourceXamlTokenizer tokenizer)
        {
            this.tokenizer = tokenizer;
        }

        public void Parse(XamlDocument document)
        {
            while (true)
            {
                var term = tokenizer.GetTerminal();

                if (XamlTerminal.Whitespace == term)
                {
                    continue;
                }

                if (XamlTerminal.OpenAngleBracket == term)
                {
                    ParseNodeName();
                }
            }
        }

        private void ParseNodeName()
        {
            string name;

            var term = tokenizer.GetAlphaNumericString(out name);

            if (XamlTerminal.Colon == term)
            {
                
            }
        }
    }
}