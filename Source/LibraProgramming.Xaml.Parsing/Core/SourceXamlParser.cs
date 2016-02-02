using System;
using System.Collections.ObjectModel;

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
                    ParseNode();
                }

                if (XamlTerminal.EOF == term)
                {
                    break;
                }
            }
        }

        private XamlTerminal ParseNode()
        {
            var term = ParseNodeName();

            if (XamlTerminal.Whitespace == term)
            {
                term = ParseNodeName();
            }

            while (true)
            {
                if (XamlTerminal.Whitespace == term)
                {
                    term = ParseNodeName();
                }
                else if(XamlTerminal.)
                {
                    
                }
            }

        }

        private XamlTerminal ParseNodeName()
        {
            string @namespace = null;
            var names = new Collection<string>();

            while (true)
            {
                string str;

                var term = tokenizer.GetAlphaNumericString(out str);

                switch (term)
                {
                    case XamlTerminal.Colon:
                        if (null == @namespace && 0 < str.Length)
                        {
                            @namespace = str;
                            continue;
                        }

                        throw new Exception();

                    case XamlTerminal.Dot:
                        names.Add(str);
                        break;

                    case XamlTerminal.Whitespace:
                    case XamlTerminal.Equal:
                    case XamlTerminal.Slash:
                    case XamlTerminal.CloseAngleBracket:
                        return term;

                    default:
                        throw new Exception();
                }
            }
        }
    }
}