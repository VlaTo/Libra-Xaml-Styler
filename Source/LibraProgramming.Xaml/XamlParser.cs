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

            using (var tokenizer = new XamlTokenizer(reader))
            {
                var root = new XamlRootNode();
                var stack = new Stack<XamlNode>();

                stack.Push(root);

                try
                {
                    new XamlParser(tokenizer).Parse(stack);
                }
                catch (ParserException exception)
                {
                    var position = tokenizer.GetSourcePosition();
                    throw new XamlParsingException(position.LineNumber, position.CharPosition, "", exception);
                }

                return root;
            }
        }

        private enum ParserState
        {
            Begin,
            OpenAngleBracket,
            NodeTag,
            NodeName,
            FindTagEnd,
            InlinedTagEndFound,
            TagEndFound,
            End,
            Asterisk,
            Text,
            SearchAttribute,
            CloseNodeTag,
            CloseNodeName,
            CloseNodePrefix,
            FindCloseTagEnd,
            CloseTagEndFound,
            FindAttribute,
            AttributeFound
        }

        private void Parse(Stack<XamlNode> stack)
        {
            var buffer = new StringBuilder();
            string prefix = null;
            string name = null;
            string value = null;

            var state = ParserState.Begin;
            var on = true;

            while (on)
            {
                switch (state)
                {
                    case ParserState.Begin:
                    {
                        var current = tokenizer.ReadNextChar();

                        switch (current)
                        {
                            case '<':
                            {
                                state = ParserState.OpenAngleBracket;
                                buffer.Append((char) current);

                                continue;
                            }

                            case -1:
                            {
                                on = false;
                                continue;
                            }

                            default:
                            {
                                break;
                            }
                        }

                        break;
                    }

                    case ParserState.OpenAngleBracket:
                    {
                        var current = tokenizer.ReadNextChar();

                        switch (current)
                        {
                            case '!':
                            {
                                state = ParserState.Asterisk;
                                buffer.Clear();

                                continue;
                            }

                            case '/':
                            {
                                state = ParserState.CloseNodeTag;
                                buffer.Clear();

                                continue;
                            }

                            case -1:
                            {
                                on = false;
                                continue;
                            }

                            default:
                            {
                                if (Char.IsLetter((char) current) || '_' == current)
                                {
                                    state = ParserState.NodeTag;
                                    buffer.Clear();
                                    buffer.Append((char) current);

                                    continue;
                                }

                                state = ParserState.Text;

                                continue;
                            }
                        }
                    }

                    case ParserState.NodeTag:
                    {
                        var current = tokenizer.ReadNextChar();

                        if (-1 == current)
                        {
                            break;
                        }

                        var symbol = (char) current;

                        if (Char.IsLetter(symbol) || Char.IsDigit(symbol) || '_' == symbol)
                        {
                            buffer.Append(symbol);
                            continue;
                        }

                        if (Char.IsWhiteSpace(symbol))
                        {
                            state = ParserState.FindAttribute;
                            name = buffer.ToString();

                            buffer.Clear();
                        }
                        else
                        {
                            switch (current)
                            {
                                case ':':
                                {
                                    if (0 == buffer.Length || false == String.IsNullOrEmpty(prefix))
                                    {
                                        throw new ParserException();
                                    }

                                    prefix = buffer.ToString();
                                    buffer.Clear();

                                    continue;
                                }

                                case '/':
                                {
                                    if (0 == buffer.Length)
                                    {
                                        throw new ParserException();
                                    }

                                    state = ParserState.FindTagEnd;
                                    name = buffer.ToString();

                                    buffer.Clear();

                                    break;
                                }

                                case '>':
                                {
                                    if (0 == buffer.Length)
                                    {
                                        throw new ParserException();
                                    }

                                    state = ParserState.TagEndFound;
                                    name = buffer.ToString();

                                    buffer.Clear();

                                    break;
                                }
                            }
                        }

                        var node = new XamlNode
                        {
                            Parent = stack.Peek(),
                            Name = name
                        };

                        if (!String.IsNullOrEmpty(prefix))
                        {
                            node.Prefix = prefix;
                        }

                        break;
                    }

                    case ParserState.CloseNodeName:
                    {
                        var current = tokenizer.ReadNextChar();

                        if (Char.IsLetter((char) current) || '_' == current)
                        {
                            buffer.Append((char) current);
                            continue;
                        }

                        if (Char.IsDigit((char) current))
                        {
                            if (1 < buffer.Length)
                            {
                                throw new ParserException();
                            }

                            buffer.Append((char) current);

                            continue;
                        }

                        if (':' == current)
                        {
                            if (0 == buffer.Length || false == String.IsNullOrEmpty(prefix))
                            {
                                throw new ParserException();
                            }

                            prefix = buffer.ToString();
                            buffer.Clear();

                            continue;
                        }

                        name = buffer.ToString();
                        state = ParserState.FindCloseTagEnd;

                        break;
                    }

                    case ParserState.FindTagEnd:
                    {
                        var current = tokenizer.ReadNextChar();

                        if ('>' == current)
                        {
                            state = ParserState.InlinedTagEndFound;
                            continue;
                        }

                        throw new ParserException();
                    }

                    case ParserState.InlinedTagEndFound:
                    {
                        var node = new XamlNode
                        {
                            Parent = stack.Peek(),
                            Name = name,
                            IsInline = true
                        };

                        if (!String.IsNullOrEmpty(prefix))
                        {
                            node.Prefix = prefix;
                        }

                        state = ParserState.Begin;

                        continue;
                    }

                    case ParserState.TagEndFound:
                    {
                        var node = new XamlNode
                        {
                            Parent = stack.Peek(),
                            Name = name
                        };

                        if (!String.IsNullOrEmpty(prefix))
                        {
                            node.Prefix = prefix;
                        }

                        stack.Push(node);

                        state = ParserState.Begin;

                        continue;
                    }

                    case ParserState.FindAttribute:
                    {
                        var current = tokenizer.ReadNextChar();

                        switch (current)
                        {
                            case '/':
                            {
                                state = ParserState.FindTagEnd;
                                continue;
                            }

                            case '>':
                            {
                                state = ParserState.TagEndFound;
                                continue;
                            }

                            default:
                            {
                                var symbol = (char)current;

                                if (Char.IsLetter(symbol) || '_' == symbol)
                                {
                                    state = ParserState.AttributeFound;
                                    continue;
                                }

                                if (Char.IsWhiteSpace(symbol))
                                {
                                    continue;
                                }

                                throw new ParserException();
                            }
                        }
                    }

                    /*case ParserState.CloseNodeTag:
                    {
                        var current = tokenizer.ReadNextChar();

                        if (Char.IsWhiteSpace((char) current))
                        {
                            continue;
                        }

                        if (Char.IsLetter((char) current) || '_' == current)
                        {
                            state = ParserState.CloseNodeName;
                            buffer.Clear();
                            buffer.Append((char) current);

                            continue;
                        }

                        var position = tokenizer.GetSourcePosition();
                        throw new XamlParsingException(position.LineNumber, position.CharPosition, "");
                    }*/



                    case ParserState.FindCloseTagEnd:
                    {
                        var current = tokenizer.ReadNextChar();

                        if (Char.IsWhiteSpace((char) current))
                        {
                            continue;
                        }

                        if ('>' == current)
                        {
                            state = ParserState.CloseTagEndFound;
                            continue;
                        }

                        throw new ParserException();
                    }

                    case ParserState.CloseTagEndFound:
                    {
                        if (String.IsNullOrEmpty(name))
                        {
                            throw new ParserException();
                        }

                        var node = stack.Peek();

                        if (String.Equals(node.Prefix, prefix) && String.Equals(node.Name, name))
                        {
                            stack.Pop();
                            state = ParserState.Begin;

                            continue;
                        }

                        throw new ParserException();
                    }

                    default:
                    {
                        throw new ParserException();
                    }
                }
            }
        }

        private XamlTerminal ParseNode(Stack<XamlNode> queue)
        {
            NodeName nodeName;

            var term = ParseNodeName(out nodeName);

            if (null == nodeName)
            {
                var prefix = new StringBuilder();

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

                        prefix.Append('/');

                        break;
                    }

                    /*case XamlTerminal.Exclamation:
                    {
                        term = ParseComment();

                        return term;
                    }*/
                }

                /*if (XamlTerminal.FindTagEnd != term)
                {
                    var position = tokenizer.GetSourcePosition();
                    throw new XamlParsingException(position.LineNumber, position.CharPosition, "");
                }

                term = ParseNodeName(out nodeName);

                if (XamlTerminal.TagEndFound != term)
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

            /*while (true)
            {
                var current
            }*/

            throw new NotImplementedException();
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

        /*private static class Throw
        {
            internal static void ParsingError()
            {
                throw new XamlParsingException();
            }
        }*/
    }
}