using System;
using System.Collections.Generic;
using System.Linq;

namespace LibraProgramming.Parsing.Xaml
{
    public sealed class XamlNodeName : IEquatable<XamlNodeName>
    {
        public string Alias
        {
            get;
        }

        public string Name
        {
            get;
        }

        public IEnumerable<string> PathSegments
        {
            get;
        }

        public XamlNodeName(string name)
        {
            if (null == name)
            {
                throw new ArgumentNullException(nameof(name));
            }

            Alias = null;
            Name = name;
            PathSegments = Enumerable.Empty<string>();
        }

        public XamlNodeName(string alias, string name)
        {
            if (null == alias)
            {
                throw new ArgumentNullException(nameof(alias));
            }

            if (null == name)
            {
                throw new ArgumentNullException(nameof(name));
            }

            Alias = alias;
            Name = name;
            PathSegments = Enumerable.Empty<string>();
        }

        public XamlNodeName(string alias, string name, IList<string> segments)
        {
            if (null == alias)
            {
                throw new ArgumentNullException(nameof(alias));
            }

            if (null == name)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (null == segments)
            {
                throw new ArgumentNullException(nameof(segments));
            }

            Alias = alias;
            Name = name;
            PathSegments = segments.ToArray();
        }

        public XamlNodeName(string name, IList<string> segments)
        {
            if (null == name)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (null == segments)
            {
                throw new ArgumentNullException(nameof(segments));
            }

            Alias = null;
            Name = name;
            PathSegments = segments.ToArray();
        }

        public bool Equals(XamlNodeName other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return String.Equals(Alias, other.Alias) && String.Equals(Name, other.Name) &&
                   Equals(PathSegments, other.PathSegments);
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

            var other = obj as XamlNodeName;

            return other != null && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Alias?.GetHashCode() ?? 0;

                hashCode = (hashCode * 397) ^ (Name?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (PathSegments?.GetHashCode() ?? 0);

                return hashCode;
            }
        }

        public static bool operator ==(XamlNodeName left, XamlNodeName right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(XamlNodeName left, XamlNodeName right)
        {
            return !Equals(left, right);
        }
    }
}