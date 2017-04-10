using System;

namespace LibraProgramming.Parsing.Xaml
{
    public class XamlName : IEquatable<XamlName>
    {
        private string name;
        private int localNameHash;

        public string Prefix
        {
            get;
        }

        public string LocalName
        {
            get;
        }

        public string Name
        {
            get
            {
                if (null == name)
                {
                    if (0 < Prefix.Length)
                    {
                        if (0 < LocalName.Length)
                        {
                            var n = String.Concat(Prefix, ':', LocalName);

                            lock (Document.NameTable)
                            {
                                if (name == null)
                                {
                                    name = Document.NameTable.Add(n);
                                }
                            }
                        }
                        else
                        {
                            name = Prefix;
                        }
                    }
                    else
                    {
                        name = LocalName;
                    }
                }

                return name;
            }
        }

        public string NamespaceURI
        {
            get;
        }

        public int HashCode
        {
            get;
        }

        internal int LocalNameHashCode
        {
            get
            {
                if (0 == localNameHash)
                {
                    localNameHash = LocalName.GetHashCode();
                }

                return localNameHash;
            }
        }

        internal XamlDocument Document
        {
            get;
        }

        internal XamlName Next
        {
            get;
        }

        internal XamlName(string prefix, string localName, string ns, int hashCode, XamlDocument document, XamlName next)
        {
            Prefix = prefix;
            LocalName = localName;
            NamespaceURI = ns;
            HashCode = hashCode;
            Document = document;
            Next = next;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return GetType() == obj.GetType() && Equals((XamlName) obj);
        }

        public bool Equals(XamlName other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return AreSame(Prefix, other.Prefix)
                   && String.Equals(Name, other.Name, StringComparison.Ordinal)
                   && AreSame(NamespaceURI, other.NamespaceURI);
        }

        public override int GetHashCode()
        {
            return HashCode;
/*
                        unchecked
                        {
                            var hashCode = NamespaceURI?.GetHashCode() ?? 0;
            
                            hashCode = (hashCode * 0x18D) ^ (Prefix?.GetHashCode() ?? 0);
                            hashCode = (hashCode * 0x18D) ^ (Name?.GetHashCode() ?? 0);
            
                            return hashCode;
                        }
            */
        }

        public int 

        public static bool operator ==(XamlName left, XamlName right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(XamlName left, XamlName right)
        {
            return false == Equals(left, right);
        }

        private static bool AreSame(string one, string two)
        {
            if (null == one)
            {
                return null == two;
            }

            return String.Equals(one, two, StringComparison.Ordinal);
        }
    }
}