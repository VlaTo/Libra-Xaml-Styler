using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using LibraProgramming.Xaml.Core;

namespace LibraProgramming.Xaml
{
    public sealed class XamlParser
    {
        private readonly XamlTokenizer tokenizer;
        private readonly StringComparer comparer;

        private XamlParser(XamlTokenizer tokenizer)
        {
            this.tokenizer = tokenizer;
            comparer = StringComparer.Ordinal;
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

            try
            {
                using (var tokenizer = new XamlTokenizer(reader))
                {
                    var parser = new XamlParser(tokenizer);
                    var node = parser.Parse();

                    return node;
                }
            }
            catch (Exception exception)
            {
                throw new XamlParsingException("", exception);
            }
        }

        private XamlNode Parse()
        {
            var root = new XamlRootNode();
            var queue = new Stack<XamlNode>();
            var done = false;

            queue.Push(root);

            while (!done)
            {
                var term = tokenizer.GetTerminal();

                switch (term)
                {
                    case XamlTerminal.Whitespace:
                        continue;

                    case XamlTerminal.OpenAngleBracket:
                    {
                        var temp = ParseNode(queue);
                        break;
                    }

                    case XamlTerminal.EOF:
                        done = true;
                        break;

                    default:
                        throw new XamlParsingException("");
                }
            }
           
            return root.First;
        }

        private XamlTerminal ParseNode(Stack<XamlNode> queue)
        {
            NodeName nodeName;

            var term = ParseNodeName(out nodeName);

            if (null == nodeName)
            {
                if (XamlTerminal.Slash != term)
                {
                    throw new XamlParsingException("");
                }

                term = ParseNodeName(out nodeName);

                if (XamlTerminal.CloseAngleBracket != term)
                {
                    throw new XamlParsingException("");
                }

                EsureClosingNode(queue, nodeName);

                return term;
            }

            var node = new XamlNode
            {
                Parent = queue.Peek(),
                Prefix = nodeName.Prefix,
                Name = nodeName.Name
            };

            queue.Push(node);

            while (true)
            {
                switch (term)
                {
                    case XamlTerminal.Whitespace:
                    {
                        term = ParseAttribute(node);

                        if (XamlTerminal.Quote != term)
                        {
                            throw new XamlParsingException("");
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
                            throw new XamlParsingException("");
                        }

                        node.IsInline = true;

                        return term;
                    }

                    default:
                        throw new XamlParsingException("");
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
            var attribute = new XamlAttribute
            {
                Node = node,
                Prefix = nodeName.Prefix,
                Name = nodeName.Name
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
                            throw new XamlParsingException("");
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
        }

        private XamlTerminal ParseNodeName(out NodeName nodeName)
        {
            var prefix = String.Empty;
            var name = new StringBuilder();

            nodeName = null;

            while (true)
            {
                string str;

                var term = tokenizer.GetAlphaNumericString(out str);

                switch (term)
                {
                    case XamlTerminal.Colon:
                        if (String.IsNullOrEmpty(prefix) && 0 < str.Length)
                        {
                            prefix = str;
                            continue;
                        }

                        throw new XamlParsingException("");

                    case XamlTerminal.Dot:
                        name.Append(str).Append('.');
                        break;

                    case XamlTerminal.Slash:
                    {
                        if (String.IsNullOrEmpty(str))
                        {
                            return term;
                        }

                        var temp = tokenizer.GetTerminal();

                        if (XamlTerminal.CloseAngleBracket != temp)
                        {
                            throw new Exception();
                        }

                        name.Append(str);

                        nodeName = new NodeName(prefix, name.ToString());

                        return temp;
                    }

                    case XamlTerminal.Whitespace:
                        if (String.IsNullOrEmpty(str))
                        {
                            return term;
                        }

                        name.Append(str);

                        nodeName = new NodeName(prefix, name.ToString());

                        return term;

                    case XamlTerminal.Equal:
                    case XamlTerminal.CloseAngleBracket:
                        name.Append(str);

                        nodeName = new NodeName(prefix, name.ToString());

                        return term;

                    default:
                        throw new Exception();
                }
            }
        }

        private void EsureClosingNode(Stack<XamlNode> queue, NodeName nodeName)
        {
            if (0 >= queue.Count)
            {
                throw new XamlParsingException("");
            }

            var node = queue.Peek();

            if (comparer.Equals(node.Prefix, nodeName.Prefix) && comparer.Equals(node.Name, nodeName.Name))
            {
                queue.Pop();
                return;
            }

            throw new XamlParsingException("");
        }
    }
}