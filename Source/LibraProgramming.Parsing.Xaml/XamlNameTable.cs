using System;

namespace LibraProgramming.Parsing.Xaml
{
    internal sealed class XamlNameTable
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
    }
}