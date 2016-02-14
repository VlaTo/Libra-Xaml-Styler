using System.Diagnostics;
using System.Text;
using LibraProgramming.Xaml.Visitors;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using XamlParser = LibraProgramming.Xaml.XamlParser;

namespace UnitTests.CorrectXaml
{
    [TestClass]
    public class SingleNode
    {
        [TestMethod]
        public void Simple()
        {
            const string tag = "Application";
            var node = XamlParser.Parse('<' + tag + '>');

            Assert.IsNotNull(node);
            Assert.AreEqual(tag, node.Name);
            Assert.AreEqual(0, node.Children.Count);
            Assert.AreEqual(0, node.Attributes.Count);
        }

        [TestMethod]
        public void TwoNodes()
        {
            const string tag = "Application";
            const string grid = "Grid";

            var node = XamlParser.Parse('<' + tag + '>' + '<' + grid + '>');

            Assert.IsNotNull(node);
            Assert.AreEqual(tag, node.Name);
            Assert.AreEqual(1, node.Children.Count);
            Assert.AreEqual(0, node.Attributes.Count);
        }

        [TestMethod]
        public void TestMethod2()
        {
            var text =
                @"<Application x:Class = ""LibraProgramming.Sample.App"" xmlns = ""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" xmlns:x = ""http://schemas.microsoft.com/winfx/2006/xaml"" xmlns:vm = ""using:LibraProgramming.Sample.ViewModels"" xmlns:xaml = ""using:LibraProgramming.Sample.UI.Xaml"" RequestedTheme = ""Dark"">" +
                "    <Application.Resources>" +
                "    </Application.Resources>" +
                "</Application>";
            var builder = new StringBuilder();
            var visitor = new StylerNodeVisitor(builder);

            var node = XamlParser.Parse(text);

            visitor.Visit(node);

            Debug.WriteLine(builder.ToString());
        }
    }
}
