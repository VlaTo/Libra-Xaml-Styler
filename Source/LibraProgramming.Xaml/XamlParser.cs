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

                switch (term)
                {
                    case XamlTerminal.Whitespace:
                        continue;

                    case XamlTerminal.OpenAngleBracket:
                    {
                        var temp = ParseNode(root);
                        break;
                    }

                    case XamlTerminal.EOF:
                        return;

                    default:
                        throw new SourceXamlParsingException();
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
                    {
                        term = ParseAttribute(node);

                        if (XamlTerminal.Quote != term)
                        {
                            throw new SourceXamlParsingException();
                        }

                        term = tokenizer.GetTerminal();

                        break;
                    }

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

            var term = ParseNodeName(out nodeName);

            if (null == nodeName)
            {
                return term;
            }

            var hasequal = false;
            var attribute = new XamlAttribute(nodeName.Parts)
            {
                Node = node
            };

            while (true)
            {
                switch (term)
                {
                    case XamlTerminal.Whitespace:
                        break;

                    case XamlTerminal.Equal:
                        if (hasequal)
                        {
                            throw new SourceXamlParsingException();
                        }

                        hasequal = true;

                        break;

                    case XamlTerminal.Quote:
                        var builder = new StringBuilder();

                        term = tokenizer.GetAttributeValueString(builder);

                        if (XamlTerminal.Quote == term)
                        {
                            attribute.Value = builder.ToString();
                            return term;
                        }

                        break;
                }

                term = tokenizer.GetTerminal();

            }

            /*while (XamlTerminal.EOF != term && XamlTerminal.Whitespace == term)
            {
                term = tokenizer.GetTerminal();
            }

            if (XamlTerminal.Equal != term)
            {
                return term;
            }

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
            }*/

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
                        if (String.IsNullOrEmpty(str))
                        {
                            return term;
                        }

                        parts.Add(str);

                        nodeName = new NodeName(alias, parts);

                        return term;

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