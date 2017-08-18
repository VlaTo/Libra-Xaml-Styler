using System;
using System.Collections;
using System.Collections.Generic;

namespace LibraProgramming.Parsing.Xaml
{
    internal sealed class XamlNameTable:IEnumerable<XamlName>
    {
        private const int DefaultNamesSize = 64;

        private readonly StringComparer comparer;
        private readonly XamlDocument document;
        private XamlName[] names;
        private int count;
        private int mask;

        public XamlNameTable(XamlDocument document)
        {
            this.document = document;
            comparer = StringComparer.Ordinal;
            names = new XamlName[DefaultNamesSize];
            count = 0;
            mask = DefaultNamesSize - 1;
        }

        public XamlName GetName(string prefix, string localName, string ns)
        {
            if (null == prefix)
            {
                prefix = String.Empty;
            }

            if (null == ns)
            {
                ns = String.Empty;
            }

            var hash = XamlName.GetHashCode(localName);

            for (var entry = names[hash & mask]; null != entry; entry = entry.Next)
            {
                if (entry.HashCode != hash || false == IsSameName(entry, prefix, localName, ns))
                {
                    continue;
                }

                return entry;
            }

            return null;
        }

        public XamlName AddName(string prefix, string localName, string ns)
        {
            if (null == prefix)
            {
                prefix = String.Empty;
            }

            if (null == ns)
            {
                ns = String.Empty;
            }

            var hash = XamlName.GetHashCode(localName);

            for (var entry = names[hash & mask]; null != entry; entry = entry.Next)
            {
                if (entry.HashCode != hash || false == IsSameName(entry, prefix, localName, ns))
                {
                    continue;
                }

                return entry;
            }

            var index = hash & mask;
            var name = new XamlName(prefix, localName, ns, hash, document, names[index]);

            names[index] = name;

            if (count++ == mask)
            {
                GrowNames();
            }

            return name;
        }

        public IEnumerator<XamlName> GetEnumerator()
        {
            return new Enumerator(names, count);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private bool IsSameName(XamlName name, string prefix, string localName, string ns)
        {
            return comparer.Equals(name.LocalName, localName) &&
                   comparer.Equals(name.Prefix, prefix) &&
                   comparer.Equals(name.NamespaceURI, ns);
        }

        private void GrowNames()
        {
            var temp = mask << 1 + 1;
            var array = new XamlName[temp + 1];

            for (var index = 0; index < names.Length; index++)
            {
                var name = names[index];

                while (null != name)
                {
                    var position = name.HashCode & temp;
                    var next = name.Next;

                    name.Next = array[position];
                    array[position] = name;
                    name = next;
                }
            }

            names = array;
            mask = temp;
        }

        /// <summary>
        /// 
        /// </summary>
        private class Enumerator : IEnumerator<XamlName>
        {
            private readonly XamlName[] names;
            private bool disposed;
            private int position;

            public XamlName Current
            {
                get
                {
                    if (disposed)
                    {
                        throw new InvalidOperationException();
                    }

                    return names[position];
                }
            }

            object IEnumerator.Current => Current;

            public Enumerator(XamlName[] names, int count)
            {
                position = -1;
                this.names = CreateCopy(names, count);
            }

            public void Dispose()
            {
                Dispose(true);
            }

            public bool MoveNext()
            {
                EnsureNotDisposed();

                if (-1 == position)
                {
                    if (0 == names.Length)
                    {
                        return false;
                    }

                    position = 0;

                    return true;
                }

                if ((names.Length - 1) <= position)
                {
                    return false;
                }

                position++;

                return true;
            }

            public void Reset()
            {
                EnsureNotDisposed();
                position = -1;
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
                        
                    }
                }
                finally
                {
                    disposed = true;
                }
            }

            private void EnsureNotDisposed()
            {
                if (disposed)
                {
                    throw new InvalidOperationException();
                }
            }

            private XamlName[] CreateCopy(XamlName[] source, int count)
            {
                var list = new List<XamlName>(count);

                for (var index = 0; index < source.Length; index++)
                {
                    var name = source[index];

                    if (null == name)
                    {
                        continue;
                    }

                    list.Add(name);
                }

                return list.ToArray();
            }
        }
    }
}