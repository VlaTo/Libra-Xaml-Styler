using System;
using System.Linq;

namespace LibraProgramming.Parsing.Xaml.Core
{
    internal sealed class XamlNamespaceResolver
    {
        private readonly XamlNode node;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="T:System.Object"/>.
        /// </summary>
        public XamlNamespaceResolver(XamlNode node)
        {
            this.node = node;
        }

        public string GetUri(string prefix)
        {
            if (null == prefix)
            {
                throw new ArgumentNullException(nameof(prefix));
            }

            var comparer = StringComparer.Ordinal;
            var temp = node;

            while (null != temp)
            {
                var attribute = temp.Attributes
                    .FirstOrDefault(
                        attr => comparer.Equals(attr.Prefix, "xmlns") && comparer.Equals(attr.Name, prefix)
                    );

                if (null != attribute)
                {
                    return attribute.Value;
                }

                temp = temp.Parent;
            }

            return String.Empty;
        }
    }
}