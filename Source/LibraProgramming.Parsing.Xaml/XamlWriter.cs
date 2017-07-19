using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LibraProgramming.Parsing.Xaml
{
    public sealed partial class XamlWriter : IDisposable
    {
        private XamlOutputWriter writer;
        private bool disposed;
        private Stack<ElementWriteInfo> infos;

        private ElementWriteInfo LastInfo
        {
            get
            {
                if (0 == infos.Count)
                {
                    return null;
                }

                return infos.Peek();
            }
        }

        public XamlWriter(TextWriter writer)
            : this()
        {
            if (null == writer)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            this.writer = new XamlOutputWriter(writer);
        }

        public XamlWriter(Stream stream)
            : this()
        {
            if (null == stream)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            var streamWriter = new StreamWriter(stream);

            writer = new XamlOutputWriter(streamWriter);
        }

        private XamlWriter()
        {
            infos = new Stack<ElementWriteInfo>();
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
                infos = null;
            }
            finally
            {
                disposed = true;
            }
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
                writer.WriteElementEnd(false);
            }

            infos.Push(new ElementWriteInfo(name));

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
            var info = infos.Pop();

            if (forceInline && info.IsEmpty)
            {
                writer.WriteElementEnd(true);
                return;
            }

            writer.WriteElementBegin(true);
            writer.WriteElementName(info.Name);
            //writer.WriteElementEnd(false);
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

        /// <summary>
        /// 
        /// </summary>
        private class ElementWriteInfo
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

            public ElementWriteInfo(string name)
            {
                Name = name;
                IsEmpty = true;
            }
        }
    }
}