using System;
using System.Collections.Generic;

namespace LibraProgramming.Parsing.Xaml
{
    internal sealed class XamlNodeNameBuilder
    {
        private readonly IList<string> path;
        private string alias;
        private string name;

        public bool IsEmpty => null == alias && null == name && 0 == path.Count;

        public XamlNodeNameBuilder()
        {
            path = new List<string>();
        }

        public XamlNodeNameBuilder SetAlias(string value)
        {
            if (null == value)
            {
                throw new ArgumentNullException(nameof(value));
            }

            alias = value;

            return this;
        }

        public XamlNodeNameBuilder SetName(string value)
        {
            if (null == value)
            {
                throw new ArgumentNullException(nameof(value));
            }

            name = value;

            return this;
        }

        public XamlNodeNameBuilder AddPropertyPath(string value)
        {
            if (null == value)
            {
                throw new ArgumentNullException(nameof(value));
            }

            path.Add(value);

            return this;
        }

        public XamlNodeName ToName()
        {
            return new XamlNodeName(alias ?? String.Empty, name, path);
        }

        public void Reset()
        {
            alias = null;
            name = null;
            path.Clear();
        }
    }
}