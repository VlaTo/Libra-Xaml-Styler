using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using LibraProgramming.Xaml.Core;

namespace LibraProgramming.Xaml
{
    public sealed class XamlParser
    {
        private readonly XamlTokenizer tokenizer;

        private XamlParser(XamlTokenizer tokenizer)
        {
            this.tokenizer = tokenizer;
        }

        public static IXamlNode Parse(string text)
        {
            if (null == text)
            {
                throw new ArgumentNullException(nameof(text));
            }

            using (var reader = new StringReader(text))
            {
                return Parse(reader);
            }
        }

        public static IXamlNode Parse(TextReader reader)
        {
            if (null == reader)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            var context = new SourceXamlParsingContext();

            using (var tokenizer = new XamlTokenizer(reader, context))
            {
                var parser = new XamlParser(tokenizer);
                return parser.Parse();
            }
        }

        private IXamlNode Parse()
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
                    var node = new XamlNode();
                    var temp = ParseNode(node);
                }
                else
                if (XamlTerminal.EOF == term)
                {
                    break;
                }
            }

            return null;
        }

        private XamlTerminal ParseNode(XamlNode node)
        {
            NodeName nodeName;
            var term = ParseNodeName(out nodeName);

            if (null != nodeName)
            {
                node.Name=nodeName.
            }

            do
            {
                switch (term)
                {
                    case XamlTerminal.Whitespace:
                        term = ParseNodeName(out nodeName);
                        break;

                    case XamlTerminal.Equal:
                    {
                        string value;

                        term = ParseAttributeValue(out value);

                        break;
                    }

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
                    case XamlTerminal.CloseAngleBracket:
                        parts.Add(str);
                        nodeName = new NodeName(@namespace, parts);
                        return term;

                    default:
                        throw new Exception();
                }
            }
        }

        private XamlTerminal ParseAttributeValue(out string value)
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

            var builder = new StringBuilder();

            term = tokenizer.GetAttributeValueString(builder);

            if (XamlTerminal.Quote != term)
            {
                throw new SourceXamlParsingException();
            }

            value = builder.ToString();

            return term;
        }
    }
}