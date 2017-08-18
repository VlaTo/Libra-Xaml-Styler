using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using LibraProgramming.Parsing.Xaml;
using LibraProgramming.Parsing.Xaml.Visitors;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class ReformatTests
    {
        [TestMethod]
        public async Task TestMethod1()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var stream = assembly.GetManifestResourceStream("UnitTests.Samples.Test1.Sample.xaml");
            XamlDocument document;

            using (var reader = new StreamReader(stream))
            {
                document = await XamlDocument.ParseAsync(reader);

                Assert.IsNotNull(document);
                Assert.IsNotNull(document.Root);
            }

            ReformatPrefixes(document);

            var text = new StringBuilder();
            var settings = new DocumentReformatSettings
            {
                SpacesBeforeEmptyNodeClose = 1
            };

            using (var writer = new XamlWriter(new StringWriter(text)))
            {
                var visitor = new ReformatXamlVisitor(writer);

                writer.NewLineBeforeAttribute = true;
                writer.FormattingMode = FormattingMode.Indent;
                writer.Indentation = 4;

                visitor.Visit(document);
            }

            Debug.WriteLine(text.ToString());
        }

        private static void ReformatPrefixes(XamlDocument document)
        {
            var comparer = StringComparer.Ordinal;
            var names = new Dictionary<string, XamlName>(comparer);

            foreach (var name in document.NameTable)
            {
                if (false == comparer.Equals("xmlns", name.Prefix))
                {
                    continue;
                }

                names.Add(name.LocalName, name);
            }

            var length = names.Count;

            foreach (var kvp in names)
            {
                var key = kvp.Key.ToLowerInvariant();

                if (key.Length >= length)
                {
                    var ns = document.GetNamespaceOfPrefix(key);
                    Debug.WriteLine("{0} => {1}", key, ns);
                }
            }
        }
    }
}
