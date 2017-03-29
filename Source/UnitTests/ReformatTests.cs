using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using LibraProgramming.Parsing.Xaml;
using LibraProgramming.Parsing.Xaml.Visitors;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class ReformatTests
    {
        [TestMethod]
        public void TestMethod1()
        {
            var text = new StringBuilder();
            var document = new XamlDocument();

            document.AppendChild(
                new XamlElement(document,
                    XamlName.Create("uwp", "Node.Property.Path", "using: Test.Sample.Windows"))
            );

            using (var writer = new StringWriter(text))
            {
                var settings = new DocumentReformatSettings
                {
                    SpacesBeforeEmptyNodeClose = 1
                };
                var visitor = new ReformatXamlVisitor(writer, settings);

                visitor.Visit(document);
            }

            Debug.WriteLine(text.ToString());
        }
    }
}
