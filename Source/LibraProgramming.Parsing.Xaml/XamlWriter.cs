using System;
using System.IO;

namespace LibraProgramming.Parsing.Xaml
{
    public sealed class XamlWriter : IDisposable
    {
        private readonly TextWriter writer;
        private bool disposed;

        public XamlWriter(TextWriter writer)
        {
            this.writer = writer;
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
            }
            finally
            {
                disposed = true;
            }
        }

        public void WriteStartAttribute(string prefix, string localName, string namespaceURI)
        {
            throw new NotImplementedException();
        }

        public void WriteAttributeContent(XamlContent content)
        {
            throw new NotImplementedException();
        }

        public void WriteEndAttribute()
        {
            throw new NotImplementedException();
        }
    }
}