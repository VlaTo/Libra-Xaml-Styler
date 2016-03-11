using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using LibraProgramming.Parsing.Xaml.Core;

namespace LibraProgramming.Parsing.Xaml
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class XamlParser
    {
        private readonly XamlTokenizer tokenizer;
        private readonly StringComparer comparer;

        private XamlParser(XamlTokenizer tokenizer)
        {
            this.tokenizer = tokenizer;
            comparer = StringComparer.Ordinal;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static IXamlNode Parse(string text)
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static IXamlNode Parse(TextReader reader)
        {
            if (null == reader)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            using (var tokenizer = new XamlTokenizer(reader))
            {
                var context = new XamlParsingContext();
//                var root = new XamlRootNode();
//                var stack = new Stack<XamlNode>();

//                stack.Push(root);

                try
                {
                    var parser = new XamlParser(tokenizer);

                    parser.Parse(context);
                    context.ValidateDocument(new XamlDocumentValidator());
                }
                catch (Exception exception)
                {
//                    var position = tokenizer.GetSourcePosition();
                    throw new XamlParsingException();
                }

                return context.DocumentRoot;
            }
        }

        private void Parse(XamlParsingContext context)
        {
            string prefix = null;
            string name = null;

            var state = ParserState.Begin;
            var on = true;

            while (on)
            {
                switch (state)
                {
                    case ParserState.Begin:
                        break;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private enum ParserState
        {
            Begin,
        }
    }
}