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

//            var context = new SourceXamlParsingContext();

            using (var tokenizer = new XamlTokenizer(reader))
            {
                var root = new XamlRootNode();
                var parser = new XamlParser(tokenizer);

                parser.Parse(root);

                return root.First;
            }
        }

        private void Parse(XamlNode root)
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
                    var temp = ParseNode(root);
                }
                else
                if (XamlTerminal.EOF == term)
                {
                    break;
                }
            }
        }

        private XamlTerminal ParseNode(XamlNode parent)
        {
            NodeName nodeName;

            var term = ParseNodeName(out nodeName);

            if (null == nodeName)
            {
                throw new SourceXamlParsingException();
            }

            var node = new XamlNode(nodeName.Parts)
            {
                Parent = parent
            };

            while (true)
            {
                switch (term)
                {
                    case XamlTerminal.Whitespace:
                        term = ParseAttribute(node);
                        break;

                    case XamlTerminal.CloseAngleBracket:
                    case XamlTerminal.EOF:
                        return term;

                    case XamlTerminal.Slash:
                    {
                        term = tokenizer.GetTerminal();

                        if (XamlTerminal.CloseAngleBracket != term)
                        {
                            throw new SourceXamlParsingException();
                        }

                        return term;
                    }

                    default:
                        throw new SourceXamlParsingException();
                }
            }
        }

        private XamlTerminal ParseAttribute(XamlNode node)
        {
            NodeName nodeName;

            for (var temp = tokenizer.PeekTerminal();
                XamlTerminal.EOF != temp && XamlTerminal.Whitespace == temp;
                temp = tokenizer.GetTerminal())
            {
            }

            var term = ParseNodeName(out nodeName);

            if (null == nodeName)
            {
                throw new SourceXamlParsingException();
            }

            var attribute = new XamlAttribute(nodeName.Parts)
            {
                Node = node
            };

            while (XamlTerminal.EOF != term && XamlTerminal.Whitespace == term)
            {
                term = tokenizer.GetTerminal();
            }

            if (XamlTerminal.Equal == term)
            {
                while (XamlTerminal.EOF != term && XamlTerminal.Whitespace == term)
                {
                    term = tokenizer.GetTerminal();
                }

                if (XamlTerminal.Quote != term)
                {
                    throw new SourceXamlParsingException();
                }

                var builder = new StringBuilder();

                term = tokenizer.GetAttributeValueString(builder);

                if (XamlTerminal.Quote == term)
                {
                    attribute.Value = builder.ToString();
                }
            }

            return term;
        }

        private XamlTerminal ParseNodeName(out NodeName nodeName)
        {
            var alias = String.Empty;
            var parts =  new Collection<string>();

            nodeName = null;

            while (true)
            {
                string str;

                var term = tokenizer.GetAlphaNumericString(out str);

                switch (term)
                {
                    case XamlTerminal.Colon:
                        if (String.IsNullOrEmpty(alias) && 0 < str.Length)
                        {
                            alias = str;
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

                        nodeName = new NodeName(alias, parts);

                        return temp;
                    }

                    case XamlTerminal.Whitespace:
                    case XamlTerminal.Equal:
                    case XamlTerminal.CloseAngleBracket:
                        parts.Add(str);
                        nodeName = new NodeName(alias, parts);
                        return term;

                    default:
                        throw new Exception();
                }
            }
        }

        private XamlTerminal ParseAttributeName(out NodeName nodeName)
        {
            var alias = String.Empty;
            var parts =  new Collection<string>();

            nodeName = null;

            while (true)
            {
                string str;

                var term = tokenizer.GetAlphaNumericString(out str);

                switch (term)
                {
                    case XamlTerminal.Colon:
                        if (String.IsNullOrEmpty(alias) && 0 < str.Length)
                        {
                            alias = str;
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

                        nodeName = new NodeName(alias, parts);

                        return temp;
                    }

                    case XamlTerminal.Whitespace:
                    case XamlTerminal.Equal:
                    case XamlTerminal.CloseAngleBracket:
                        parts.Add(str);
                        nodeName = new NodeName(alias, parts);
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