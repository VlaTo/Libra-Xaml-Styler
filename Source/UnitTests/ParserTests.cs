using System;
using System.Threading.Tasks;
using LibraProgramming.Parsing.Xaml;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    [TestCategory("ParserSimple")]
    public class ParserTests
    {
        [DataRow("", DisplayName = "Empty String")]
        [DataRow("  ", DisplayName = "Whitespaced String")]
        [TestMethod]
        public async Task EmptyInput(string empty)
        {
            var document = await XamlDocument.ParseAsync(empty);

            Assert.IsNotNull(document);
            Assert.IsNotNull(document.Root);
            Assert.IsFalse(document.Root.HasChildNodes);
        }

        /*[TestMethod, Ignore]
        public async Task ThreeWhitespacesInput()
        {
            var test = new String(' ', 3);
            var document = await XamlDocument.ParseAsync(test);

            Assert.IsNotNull(document);
            Assert.IsNotNull(document.Root);
            Assert.IsFalse(document.Root.HasChildNodes);
        }*/

        [DataRow(null, "test", "test", DisplayName = "Stripped inlined tag")]
        [DataRow("t", "test", "t:test", DisplayName = "Prefixed inlined tag")]
        [DataRow("t", "test.element.node", "t:test.element.node", DisplayName = "Complex inlined tag")]
        [TestMethod]
        public async Task SimpleInlinedPrefixedTag(string prefix, string name, string tag)
        {
            var test = "<" + tag + "/>";
            var document = await XamlDocument.ParseAsync(test);

            Assert.IsNotNull(document);
            Assert.IsNotNull(document.Root);
            Assert.IsTrue(document.Root.HasChildNodes);
            Assert.AreEqual(1, document.Root.ChildNodes.Count);

            var node = document.Root.FirstChild;

            if (false == String.IsNullOrEmpty(prefix))
            {
                Assert.AreEqual(prefix, node.Prefix);
            }

            Assert.AreEqual(name, node.LocalName);
        }

        /*[TestMethod, Ignore]
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
        }*/

//        [DataRow("test", DisplayName = "Stripped name")]
        [DataRow("t:test", DisplayName = "Prefixed name")]
//        [DataRow("test.name.attribute", DisplayName = "Prefixed name")]
//        [DataRow("t:test.name.attribute", DisplayName = "Prefixed complex name")]
        [TestMethod]
        public async Task CheckAttributeNameParsingAsync(string attribute)
        {
            const string value = "Lorem Ipsum";
            var test = "<p " + attribute + "=\"" + value + "\"/>";
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
