using System;

namespace LibraProgramming.Parsing.Xaml
{
    public class XamlName : IEquatable<XamlName>
    {
        private string ns;

        public string Prefix
        {
            get;
        }

        public string Name
        {
            get;
        }

        public string LocalName
        {
            get
            {
                var position = Name.IndexOf('.');
                return 0 > position ? Name : Name.Substring(0, position);
            }
        }

        public static XamlName Create(string prefix, string name, string ns)
        {
            return new XamlName(prefix, name, ns);
        }

        public static XamlName Create(string name, string ns = null)
        {
            return Create(null, name, ns);
        }

        internal XamlName(string prefix, string name, string ns)
        {
            Prefix = prefix;
            Name = name;
            this.ns = ns;
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
                   && AreSame(ns, other.ns);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = ns?.GetHashCode() ?? 0;

                hashCode = (hashCode * 0x18D) ^ (Prefix?.GetHashCode() ?? 0);
                hashCode = (hashCode * 0x18D) ^ (Name?.GetHashCode() ?? 0);

                return hashCode;
            }
        }

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