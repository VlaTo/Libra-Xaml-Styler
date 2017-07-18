using System;
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

            var text = new StringBuilder();
            var settings = new DocumentReformatSettings
            {
                SpacesBeforeEmptyNodeClose = 1
            };

            using (var writer = new XamlWriter(new StringWriter(text)))
            {
                var visitor = new ReformatXamlVisitor(writer, settings);
                visitor.Visit(document);
            }

            Debug.WriteLine(text.ToString());
        }
    }
}
