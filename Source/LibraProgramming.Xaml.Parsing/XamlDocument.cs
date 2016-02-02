using System;
using System.IO;
using LibraProgramming.Xaml.Parsing.Core;

namespace LibraProgramming.Xaml.Parsing
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class XamlDocument : IXamlNode
    {
        public XamlDocument()
        {
        }

        public static XamlDocument Parse(string text)
        {
            if (null == text)
            {
                throw new ArgumentNullException(nameof(text));
            }

            using (var reader = new StringReader(text))
            {
                return Parse(reader);
            }
        }

        public static XamlDocument Parse(TextReader reader)
        {
            if (null == reader)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            var document = new XamlDocument();
            var context = new SourceXamlParsingContext();

            using (var tokenizer = new SourceXamlTokenizer(reader, context))
            {
                var parser = new SourceXamlParser(tokenizer);
                parser.Parse(document);
            }

            return document;
        }
    }
}
