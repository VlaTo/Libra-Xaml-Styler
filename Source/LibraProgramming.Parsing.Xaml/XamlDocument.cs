using System;
using System.IO;
using System.Threading.Tasks;
using XamlParser;

namespace LibraProgramming.Parsing.Xaml
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class XamlDocument
    {
        /// <summary>
        /// 
        /// </summary>
        public IXamlNode Root
        {
            get;
        }

        private XamlDocument()
        {
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
            var context = new XamlParsingContext
            {
            };

            using (var tokenizer = new XamlTokenizer(reader, 1024))
            {
                var parser = new XamlParser(tokenizer, document);
                await parser.ParseAsync();
            }

            return document;
        }
    }
}
