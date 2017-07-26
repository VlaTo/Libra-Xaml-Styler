using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace LibraProgramming.Parsing.Xaml
{
    // http://referencesource.microsoft.com/#System.Xml/System/Xml/Core/XmlTextWriter.cs,789e9b2b28f9b93e
    public sealed class XamlWriter
    {
        private readonly TextWriter writer;
        private State currentState;
        private State[] stateTransitions;
        private Token lastToken;
        private TagInfo[] stack;
        private Namespace[] namespaces;
        private bool indented;
        private int stackIndex;
        private int namespaceIndex;
        private bool hasNamespaces;

        public XamlWriter(TextWriter writer)
            : this()
        {
            if (null == writer)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            this.writer = writer;
        }

        public XamlWriter(Stream stream)
            : this()
        {
            if (null == stream)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            writer = new StreamWriter(stream);
        }

        private XamlWriter()
        {
            stack = new TagInfo[10];
            stackIndex = 0;
            stack[stackIndex].Init();
            stateTransitions = stateTransitionsDefault;
            currentState = State.Start;
            lastToken = Token.Empty;
        }

        public void WriteAttributeBegin(string prefix, string localName, string namespaceURI)
        {
            var name = CreateTagName(prefix, localName, namespaceURI);

            writer.WriteAttributeBegin();
            writer.WriteAttributeName(name);
        }

        public void WriteAttributeContent(XamlContent content)
        {
            var buffer = new StringBuilder();

            using (var temp = new StringWriter(buffer))
            {
                content.WriteTo(temp);
            }

            writer.WriteAttributeValue(buffer.ToString());
        }

        public void WriteEndAttribute()
        {
            try
            {
                AutoComplete(Token.EndAttribute);
            }
            catch
            {
                currentState = State.Failed;
                throw;
            }
        }

        public void WriteStartElement(string prefix, string localName, string namespaceURI)
        {
            try
            {
                AutoComplete(Token.StartElement);
                PushStack();

                writer.Write('<');

                if (hasNamespaces)
                {
                    stack[stackIndex].DefaultNs = stack[stackIndex - 1].DefaultNs;

                    if (NamespaceState.Uninitialized != stack[stackIndex - 1].DefaultNamespaceState)
                    {
                        stack[stackIndex - 1].DefaultNamespaceState = NamespaceState.NotDeclaredButInScope;
                    }

                    stack[stackIndex].Mixed = stack[stackIndex - 1].Mixed;

                    if (null == namespaceURI)
                    {
                        if (false == String.IsNullOrEmpty(prefix) && (-1 == LookupNamespace(prefix)))
                        {
                            throw new ArgumentException("", nameof(prefix));
                        }
                    }
                    else
                    {
                        if (null == prefix)
                        {
                            var str = FindPrefix(namespaceURI);

                            if (null != str)
                            {
                                prefix = str;
                            }
                            else
                            {
                                PushNamespace(null, namespaceURI, false);
                            }
                        }
                        else if (0 == prefix.Length)
                        {
                            PushNamespace(null, namespaceURI, false);
                        }
                        else
                        {
                            if (0 == namespaceURI.Length)
                            {
                                prefix = null;
                            }

                            VerifyPrefix(prefix, namespaceURI);
                            PushNamespace(prefix, namespaceURI, false);
                        }
                    }

                    stack[stackIndex].Prefix = null;

                    if (false == String.IsNullOrEmpty(prefix))
                    {
                        stack[stackIndex].Prefix = prefix;
                        writer.Write(prefix);
                        writer.Write(':');
                    }
                }
                else
                {
                    if (false == String.IsNullOrEmpty(namespaceURI) || false == String.IsNullOrEmpty(prefix))
                    {
                        throw new ArgumentException("", nameof(namespaceURI));
                    }
                }

                stack[stackIndex].Name = localName;
                writer.Write(localName);
            }
            catch
            {
                currentState = State.Failed;
                throw;
            }
        }

        private int LookupNamespace(string prefix)
        {
            throw new NotImplementedException();
        }

        private string FindPrefix(string namespaceUri)
        {
            throw new NotImplementedException();
        }

        public void WriteElementContent(XamlContent content)
        {
            LastInfo.IsEmpty = false;

            var buffer = new StringBuilder();

            using (var temp = new StringWriter(buffer))
            {
                content.WriteTo(temp);
            }

            writer.WriteElementValue(buffer.ToString());
        }

        public void WriteEndElement()
        {
            WriteEndElementInternal(false);
        }

        public void WriteName(string name)
        {
            try
            {
                AutoComplete(Token.Content);
                WriteNameInternal(name, false);
            }
            catch
            {
                currentState = State.Failed;
                throw;
            }
        }

        public void Close()
        {
            try
            {
                AutoCompleteAll();
            }
            catch
            {

                throw;
            }
        }

        public void Flush()
        {
            writer.Flush();
        }

        private void AutoComplete(Token token)
        {
            if (State.Closed == currentState)
            {
                throw new InvalidOperationException();
            }

            if (State.Failed == currentState)
            {
                throw new InvalidOperationException();
            }

            var state = stateTransitions[(int) token*7 + (int) currentState];

            if (State.Failed == state)
            {
                throw new InvalidOperationException();
            }

            switch (token)
            {
                case Token.StartElement:
                {
                    if (State.Attribute == currentState)
                    {
                        WriteEndAttributeQuote();
                        WriteEndStartTag(false);
                    }
                    else if (State.Element == currentState)
                    {
                        WriteEndStartTag(false);
                    }

                    if (this.indented && State.Start != currentState)
                    {
                        Indent(false);
                    }

                    break;
                }

                case Token.EndElement:
                case Token.LongEndElement:
                {
                    if (State.Attribute == currentState)
                    {
                        WriteEndAttributeQuote();
                    }

                    if (State.Content == currentState)
                    {
                        token = Token.LongEndElement;
                    }
                    else
                    {
                        WriteEndStartTag(Token.EndElement == token);
                    }

                    if (stateTransitionsDocument == stateTransitions && 1 == stackIndex)
                    {
                        state = State.Epilog;
                    }

                    break;
                }

                case Token.StartAttribute:
                {
                    if (State.Attribute == currentState)
                    {
                        WriteEndAttributeQuote();
                        writer.Write(' ');
                    }
                    else if (State.Element == currentState)
                    {
                        writer.Write(' ');
                    }

                    break;
                }

                case Token.EndAttribute:
                {
                    WriteEndAttributeQuote();
                    break;
                }

                case Token.Content:
                {
                    if (State.Element == currentState && Token.Content != lastToken)
                    {
                        WriteEndStartTag(false);
                    }

                    if (State.Content == state)
                    {
                        stack[stackIndex].Mixed = true;
                    }

                    break;
                }
            }

            currentState = state;
            lastToken = token;
        }

        private void AutoCompleteAll()
        {
            while (0 < stackIndex)
            {
                WriteEndElement();
            }
        }

        private void WriteEndElementInternal(bool forceLong)
        {
            try
            {
                if (0 >= stackIndex)
                {
                    throw new InvalidOperationException();
                }

                AutoComplete(forceLong ? Token.LongEndElement : Token.EndElement);

                if (Token.LongEndElement == lastToken)
                {
                    if (indented)
                    {
                        Ident(true);
                    }

                    writer.Write('<');
                    writer.Write('/');

                    if (null != stack[stackIndex].Prefix)
                    {
                        writer.Write(stack[stackIndex].Prefix);
                        writer.Write(':');
                    }

                    writer.Write(stack[stackIndex].Name);
                    writer.Write('>');
                }

                var prevNsIndex = stack[stackIndex].PreviosNsIndex;

//                if (useNsHashtable && prevNsTop < nsTop)
                if (prevNsIndex < namespaceIndex)
                {
                    PopNamespaces(prevNsIndex + 1, namespaceIndex);
                }

                namespaceIndex = prevNsIndex;

                stackIndex--;
            }
            catch
            {
                currentState = State.Failed;
                throw;
            }
        }

        private void WriteEndStartTag(bool empty)
        {
            for (var index = namespaceIndex; index > stack[stackIndex].PreviosNsIndex; index--)
            {
                if (false == namespaces[index].IsDeclared)
                {
                    writer.Write("xmlns");
                    writer.Write(':');
                    writer.Write(namespaces[index].Prefix);
                    writer.Write('=');
                    //textWriter.Write(this.quoteChar);
                    writer.Write('\"');
//                    xmlEncoder.Write(nsStack[i].ns);
                    writer.Write(namespaces[index].Name);
//                    textWriter.Write(this.quoteChar);
                    writer.Write('\"');
                }
            }
        }

        private static string CreateTagName(string prefix, string localName, string namespaceURI)
        {
            var name = new StringBuilder();

            if (false == String.IsNullOrEmpty(prefix))
            {
                name.Append(prefix).Append(':');
            }

            name.Append(localName);

            return name.ToString();
        }

        private void PushStack()
        {
            if (stackIndex == (stack.Length - 1))
            {
                var temp = new TagInfo[stack.Length + 10];

                if (stackIndex > 0)
                {
                    Array.Copy(stack, temp, stackIndex + 1);
                }

                stack = temp;
            }

            stackIndex++;

            stack[stackIndex].Init();
        }

        /// <summary>
        /// 
        /// </summary>
        private enum NamespaceState
        {
            Uninitialized,
            NotDeclaredButInScope,
            DeclaredButNotWrittenOut,
            DeclaredAndWrittenOut
        }

        /// <summary>
        /// 
        /// </summary>
        private struct TagInfo
        {
            public string Name
            {
                get;
                set;
            }

            public string Prefix
            {
                get;
                set;
            }

            public NamespaceState DefaultNamespaceState
            {
                get;
                set;
            }

            public string DefaultNs
            {
                get;
                set;
            }

            public bool Mixed
            {
                get;
                set;
            }

            public int PreviosNsIndex
            {
                get;
                set;
            }

            public void Init()
            {
                Name = null;
                Prefix = null;
                DefaultNs = String.Empty;
                Mixed = false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private struct Namespace
        {
            public string Prefix
            {
                get;
                set;
            }

            public string Name
            {
                get;
                set;
            }

            public bool IsDeclared
            {
                get;
                set;
            }

            public int PrevNsIndex
            {
                get;
                set;
            }

            internal void Set(string prefix, string ns, bool declared)
            {
                Prefix = prefix;
                Name = ns;
                IsDeclared = declared;
                PrevNsIndex = -1;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        private enum Token
        {
            StartElement,
            EndElement,
            LongEndElement,
            StartAttribute,
            EndAttribute,
            Content,
            Empty
        }

        /// <summary>
        /// 
        /// </summary>
        private enum State
        {
            Start,
            Prolog,
            Element,
            Attribute,
            Content,
            Epilog,
            Closed,
            Failed
        }

        private static readonly State[] stateTransitionsDefault =
        {
            //                         State.Start   State.Prolog  State.Element  State.Attribute State.Content State.Epilog State.Closed
            /* Token.StartElement   */ State.Prolog, State.Prolog, State.Content, State.Failed, State.Failed, 
            /* Token.EndElement     */ State.Prolog, State.Prolog, State.Content,
            /* Token.LongEndElement */ State.Prolog, State.Prolog, State.Content,
            /* Token.StartAttribute */ State.Prolog, State.Prolog, State.Content,
            /* Token.EndAttribute   */ State.Prolog, State.Prolog, State.Content,
            /* Token.Content        */ State.Prolog, State.Prolog, State.Content,
        };

        private static readonly State[] stateTransitionsDocument =
        {
            //                         State.Start   State.Prolog  State.Element  State.Attribute State.Content State.Epilog State.Closed
            /* Token.StartElement   */ State.Prolog, State.Prolog, State.Content, State.Failed, State.Failed, 
            /* Token.EndElement     */ State.Prolog, State.Prolog, State.Content,
            /* Token.LongEndElement */ State.Prolog, State.Prolog, State.Content,
            /* Token.StartAttribute */ State.Prolog, State.Prolog, State.Content,
            /* Token.EndAttribute   */ State.Prolog, State.Prolog, State.Content,
            /* Token.Content        */ State.Prolog, State.Prolog, State.Content,
        };
    }
}