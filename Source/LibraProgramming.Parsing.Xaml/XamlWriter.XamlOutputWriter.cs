using System;
using System.IO;

namespace LibraProgramming.Parsing.Xaml
{
    public partial class XamlWriter
    {
        private class XamlOutputWriter : IXamlOutputWriter, IDisposable
        {
            private bool disposed;
            private TextWriter writer;

            public XamlOutputWriter(TextWriter writer)
            {
                this.writer = writer;
            }

            public void Dispose()
            {
                if (disposed)
                {
                    throw new ObjectDisposedException(GetType().ToString());
                }

                Dispose(true);
            }

            public void WriteElementBegin(bool forceClosing)
            {
                writer.WriteLine();
                writer.Write('<');

                if (forceClosing)
                {
                    writer.Write('/');
                }
            }

            public void WriteElementName(string name)
            {
                writer.Write(name);
            }

            public void WriteElementEnd(bool forceInline)
            {
                if (forceInline)
                {
                    writer.Write('/');
                }

                writer.Write('>');
            }

            public void WriteElementValue(string value)
            {
                writer.Write(value);
            }

            public void WriteAttributeBegin()
            {
                writer.Write(' ');
            }

            public void WriteAttributeName(string name)
            {
                writer.Write(name);
            }

            public void WriteAttributeValue(string value)
            {
                writer.Write('=');
                writer.Write('\"');
                writer.Write(value);
                writer.Write('\"');
            }

            public void WriteAttributeEnd()
            {
            }

            private void Dispose(bool dispose)
            {
                if (disposed)
                {
                    return;
                }

                try
                {
                    if (dispose)
                    {
                        writer.Dispose();
                        writer = null;
                    }
                }
                finally
                {
                    disposed = true;
                }
            }
        }
    }
}