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
            catch (TokenizerException exception)
            {
                var position = exception.Tokenizer.GetSourcePosition();
                throw new XamlParsingException(position.LineNumber, position.CharPosition, "", exception);
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
                        var position = tokenizer.GetSourcePosition();
                        throw new XamlParsingException(position.LineNumber, position.CharPosition, "");
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
                switch (term)
                {
                    case XamlTerminal.Slash:
                    {
                        term = ParseNodeName(out nodeName);

                        if (XamlTerminal.CloseAngleBracket == term)
                        {
                            EsureClosingNode(queue, nodeName);
                            return term;
                        }

                        break;
                    }

                    case XamlTerminal.Exclamation:
                    {
                        term = ParseComment();

                        return term;
                    }
                }

                /*if (XamlTerminal.Slash != term)
                {
                    var position = tokenizer.GetSourcePosition();
                    throw new XamlParsingException(position.LineNumber, position.CharPosition, "");
                }

                term = ParseNodeName(out nodeName);

                if (XamlTerminal.CloseAngleBracket != term)
                {
                    var position = tokenizer.GetSourcePosition();
                    throw new XamlParsingException(position.LineNumber, position.CharPosition, "");
                }

                EsureClosingNode(queue, nodeName);

                return term;*/
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
                            var position = tokenizer.GetSourcePosition();
                            throw new XamlParsingException(position.LineNumber, position.CharPosition, "");
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
                            var position = tokenizer.GetSourcePosition();
                            throw new XamlParsingException(position.LineNumber, position.CharPosition, "");
                        }

                        node.IsInline = true;

                        return term;
                    }

                    default:
                    {
                        var position = tokenizer.GetSourcePosition();
                        throw new XamlParsingException(position.LineNumber, position.CharPosition, "");
                    }
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
                            var position = tokenizer.GetSourcePosition();
                            throw new XamlParsingException(position.LineNumber, position.CharPosition, "");
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

                        var position = tokenizer.GetSourcePosition();

                        throw new XamlParsingException(position.LineNumber, position.CharPosition, "");

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

                    case XamlTerminal.Exclamation:
                        return term;

                    default:
                        throw new Exception();
                }
            }
        }

        private XamlTerminal ParseComment(ICollection<string> lines)
        {
            var term = tokenizer.GetTerminal();

            if (XamlTerminal.Dash != term)
            {
                return term;
            }

            term = tokenizer.GetTerminal();

            if (XamlTerminal.Dash != term)
            {
                return term;
            }

            while (true)
            {
                var current
            }
        }

        private void EsureClosingNode(Stack<XamlNode> queue, NodeName nodeName)
        {
            if (0 >= queue.Count)
            {
                var position = tokenizer.GetSourcePosition();
                throw new XamlParsingException(position.LineNumber, position.CharPosition, "");
            }

            var node = queue.Peek();

            if (comparer.Equals(node.Prefix, nodeName.Prefix) && comparer.Equals(node.Name, nodeName.Name))
            {
                queue.Pop();
                return;
            }

            var p = tokenizer.GetSourcePosition();

            throw new XamlParsingException(p.LineNumber, p.CharPosition, "");
        }
    }
}