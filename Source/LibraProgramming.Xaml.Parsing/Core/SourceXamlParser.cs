using System;
using System.Collections.ObjectModel;
using System.Text;

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
                    var temp = ParseNodeOpenTag();
                }
                else
                if (XamlTerminal.EOF == term)
                {
                    break;
                }
            }
        }

        private XamlTerminal ParseNodeOpenTag()
        {
            NodeName nodeName;
            var term = ParseNodeName(out nodeName);

            do
            {
                switch (term)
                {
                    case XamlTerminal.Whitespace:
                        term = ParseNodeName(out nodeName);
                        break;

                    case XamlTerminal.Equal:
                        term = ParseAttributeValue();
                        break;

                    case XamlTerminal.CloseAngleBracket:
                    case XamlTerminal.EOF:
                        return term;

                    default:
                        throw new SourceXamlParsingException();
                }

            } while (true);
        }

        private XamlTerminal ParseNodeName(out NodeName nodeName)
        {
            var @namespace = String.Empty;
            var parts =  new Collection<string>();

            nodeName = null;

            while (true)
            {
                string str;

                var term = tokenizer.GetAlphaNumericString(out str);

                switch (term)
                {
                    case XamlTerminal.Colon:
                        if (String.IsNullOrEmpty(@namespace) && 0 < str.Length)
                        {
                            @namespace = str;
                            continue;
                        }

                        throw new SourceXamlParsingException();

                    case XamlTerminal.Dot:
                        parts.Add(str);
                        break;

                    case XamlTerminal.Slash:
                    {
                        var temp = tokenizer.GetTerminal();

                        if (XamlTerminal.CloseAngleBracket != temp)
                        {
                                throw new Exception();
                        }

                        parts.Add(str);

                        nodeName = new NodeName(@namespace, parts);

                        return temp;
                    }

                    case XamlTerminal.Whitespace:
                    case XamlTerminal.Equal:
                        return term;

                    case XamlTerminal.CloseAngleBracket:
                        parts.Add(str);
                        nodeName = new NodeName(@namespace, parts);
                        return term;

                    default:
                        throw new Exception();
                }
            }
        }

        private XamlTerminal ParseAttributeValue()
        {
            var term = tokenizer.GetTerminal();

            while (XamlTerminal.Whitespace == term)
            {
                term = tokenizer.GetTerminal();
            }

            if (XamlTerminal.Quote != term)
            {
                throw new SourceXamlParsingException();
            }

//            var value = new StringBuilder();

            while (true)
            {
                term = tokenizer.GetTerminal();

                if (XamlTerminal.Quote == term)
                {
                    return term;
                }
            }
        }
    }
}