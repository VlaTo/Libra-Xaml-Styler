using System;
using System.Collections.Generic;

namespace LibraProgramming.Parsing.Xaml
{
    internal sealed class XamlNodeNameBuilder
    {
        private string namespaceAlias;
        private string name;
        private IList<string> path;

        public XamlNodeNameBuilder()
        {
            path = new List<string>();
        }

        public XamlNodeNameBuilder SetNamespaceAlias(string value)
        {
            if (null == value)
            {
                throw new ArgumentNullException(nameof(value));
            }

            namespaceAlias = value;

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
    }
}