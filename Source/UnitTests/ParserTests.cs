using System;
using System.Threading.Tasks;
using LibraProgramming.Parsing.Xaml;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class ParserTests
    {
        [TestMethod]
        [TestCategory("Empty")]
        public async Task EmptyInput()
        {
            const string test = "";
            var document = await XamlDocument.ParseAsync(test);

            Assert.IsNotNull(document);
            Assert.IsNotNull(document.Root);
            Assert.IsFalse(document.Root.HasChildNodes);
        }

        [TestMethod]
        [TestCategory("Empty")]
        public async Task ThreeWhitespacesInput()
        {
            var test = new String(' ', 3);
            var document = await XamlDocument.ParseAsync(test);

            Assert.IsNotNull(document);
            Assert.IsNotNull(document.Root);
            Assert.IsFalse(document.Root.HasChildNodes);
        }

        [TestMethod]
        [TestCategory("Empty")]
        public async Task SimpleInlinedPrefixedTag()
        {
            const string prefix = "app";
            const string name = "Application.Property.Attribute";
            const string test = "<" + prefix + ":" + name + "/>";
            var document = await XamlDocument.ParseAsync(test);

            Assert.IsNotNull(document);
            Assert.IsNotNull(document.Root);
            Assert.IsTrue(document.Root.HasChildNodes);
            Assert.AreEqual(1, document.Root.ChildNodes.Count);

            var node = document.Root.FirstChild;

            Assert.AreEqual(prefix, node.Prefix);
            Assert.AreEqual(name, node.LocalName);
        }

        [TestMethod]
        [TestCategory("Empty")]
        public async Task SimpleInlinedNonPrefixedTag()
        {
            const string name = "Application.Property.Attribute";
            const string test = "<" + name + "/>";
            var document = await XamlDocument.ParseAsync(test);

            Assert.IsNotNull(document);
            Assert.IsNotNull(document.Root);
            Assert.IsTrue(document.Root.HasChildNodes);
            Assert.AreEqual(1, document.Root.ChildNodes.Count);

            var node = document.Root.FirstChild;

            Assert.AreEqual(String.Empty, node.Prefix);
            Assert.AreEqual(name, node.LocalName);
        }

        [TestMethod]
        [TestCategory("Empty")]
        public async Task TestMethod5()
        {
            const string attribute = "Attribute";
            const string value = "Lorem Ipsum";
            const string test = "<p " + attribute + "=\"" + value + "\"/>";
            var document = await XamlDocument.ParseAsync(test);

            Assert.IsNotNull(document);
            Assert.IsNotNull(document.Root);
            Assert.IsTrue(document.Root.HasChildNodes);
            Assert.AreEqual(1, document.Root.ChildNodes.Count);
            Assert.IsInstanceOfType(document.Root.FirstChild, typeof(XamlElement));

            var element = (XamlElement) document.Root.FirstChild;

            Assert.AreEqual(1, element.Attributes.Count);

            var attr = element.Attributes[attribute];

            Assert.IsNotNull(attr);
            Assert.AreEqual(attribute, attr.LocalName);
        }
    }
}
