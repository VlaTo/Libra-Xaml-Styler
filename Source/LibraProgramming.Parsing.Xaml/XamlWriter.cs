using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LibraProgramming.Parsing.Xaml
{
    public sealed class XamlWriter : IDisposable
    {
        private TextWriter writer;
        private bool disposed;
        private Stack<ElementRec> tags;

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
            tags = new Stack<ElementRec>();
        }

        public void Dispose()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(GetType().ToString());
            }

            try
            {
                writer.Dispose();
                writer = null;
                tags = null;
            }
            finally
            {
                disposed = true;
            }
        }

        public void WriteAttributeBegin(string prefix, string localName, string namespaceURI)
        {
            var name = CreateTagName(prefix, localName, namespaceURI);

            writer.Write(' ');
            writer.Write(name);
        }

        public void WriteAttributeContent(XamlContent content)
        {
            writer.Write('=');
            content.WriteTo(writer);
        }

        public void WriteAttributeEnd()
        {
        }

        public void WriteElementBegin(string prefix, string localName, string namespaceURI)
        {
            var name = CreateTagName(prefix, localName, namespaceURI);

            if (0 < tags.Count)
            {
                var rec = tags.Peek();
                rec.IsEmpty = false;
                writer.Write('>');
            }

            tags.Push(new ElementRec(name));

            writer.Write('<');
            writer.Write(name);
        }

        public void WriteElementContent(XamlContent content)
        {
            var rec = tags.Peek();

            rec.IsEmpty = false;

            writer.Write('>');
            content.WriteTo(writer);
        }

        public void WriteElementEnd(bool forceInline)
        {
            var rec = tags.Pop();
            var canInline = forceInline && rec.IsEmpty;

            if (canInline)
            {
                writer.Write(' ');
                writer.Write('/');
                writer.Write('>');
            }
            else
            {
                writer.Write('<');
                writer.Write('/');
                writer.Write(rec.Name);
                writer.Write('>');
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

        private class ElementRec
        {
            public string Name
            {
                get;
            }

            public bool IsEmpty
            {
                get;
                set;
            }

            public ElementRec(string name)
            {
                Name = name;
                IsEmpty = true;
            }
        }
    }
}