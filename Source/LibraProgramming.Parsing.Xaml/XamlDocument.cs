﻿using System;
using System.IO;
using System.Threading.Tasks;

namespace LibraProgramming.Parsing.Xaml
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class XamlDocument : XamlNode
    {
        private const string xmlns = "xmlns";

        public override string LocalName => Name;

        public override string Name => "#document";

        public override XamlNode ParentNode => null;

        public XamlNode Root
        {
            get;
        }

        internal XamlNameTable NameTable
        {
            get;
        }

        public XamlDocument()
            : base(XamlNodeType.Document)
        {
            Root = new XamlRootElement(this);
            NameTable = new XamlNameTable(this);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public XamlAttribute CreateAttribute(string name)
        {
            string prefix;
            string localName;
            var ns = String.Empty;

            SplitName(name, out prefix, out localName);
            SetDefaultNamespace(prefix, localName, ref ns);

            return CreateAttribute(prefix, localName, ns);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="prefix"></param>
        /// <param name="localName"></param>
        /// <param name="ns"></param>
        /// <returns></returns>
        public XamlAttribute CreateAttribute(string prefix, string localName, string ns)
        {
            return new XamlAttribute(this, CreateAttributeName(prefix, localName, ns));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static Task<XamlDocument> ParseAsync(string text)
        {
            if (null == text)
            {
                throw new ArgumentNullException(nameof(text));
            }

            using (var reader = new StringReader(text))
            {
                return ParseAsync(reader);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static async Task<XamlDocument> ParseAsync(TextReader reader)
        {
            if (null == reader)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            var document = new XamlDocument();

            using (var tokenizer = new XamlTokenizer(reader, 1024))
            {
                var parser = new XamlParser(tokenizer);
                await parser.ParseAsync(document);
            }

            return document;
        }

        private XamlName CreateAttributeName(string prefix, string localName, string ns)
        {
            return NameTable.AddName(prefix, localName, ns);
        }

        private static void SetDefaultNamespace(string prefix, string localName, ref string ns)
        {
            if (prefix.Equals(xmlns, StringComparison.Ordinal) ||
                (0 == prefix.Length && localName.Equals(xmlns, StringComparison.Ordinal)))
            {
                ns = XamlReservedNamespaces.XamlNs;
            }
            else if (prefix == "xml")
            {
                ns = XamlReservedNamespaces.XNs;
            }
        }
    }
}
