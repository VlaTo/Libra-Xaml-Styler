using System;
using System.IO;
using System.Text;

namespace LibraProgramming.Parsing.Xaml
{
    // http://referencesource.microsoft.com/#System.Xml/System/Xml/Core/XmlTextWriter.cs,789e9b2b28f9b93e
    public sealed class XamlWriter
    {
        private State currentState;
        private Token lastToken;
        private TagInfo[] stack;
        private int stackIndex;
        private readonly TextWriter writer;

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

        public void WriteAttributeEnd()
        {
        }

        public void WriteElementBegin(string prefix, string localName, string namespaceURI)
        {
            var name = CreateTagName(prefix, localName, namespaceURI);
            var info = LastInfo;

            if (null != info)
            {
                info.IsEmpty = false;
                //writer.WriteElementEnd(false);
            }

            stack.Push(new TagInfo(name));

            writer.WriteElementBegin(false);
            writer.WriteElementName(name);
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

        public void WriteElementEnd(bool forceInline)
        {
            var info = stack.Pop();

            if (forceInline && info.IsEmpty)
            {
                writer.WriteElementEnd(true);
                return;
            }

            writer.WriteElementBegin(true);
            writer.WriteElementName(info.Name);
            //writer.WriteElementEnd(false);
        }

        public void Flush()
        {
            writer.Flush();
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

            public string DefaultNs
            {
                get;
                set;
            }

            public void Init()
            {
                Name = null;
                Prefix = null;
                DefaultNs = String.Empty;
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
            Failed = -1,
            Start,
            Prolog,
            Element,
            Attribute,
            Content,
            Epilog,
            Closed
        }
    }
}