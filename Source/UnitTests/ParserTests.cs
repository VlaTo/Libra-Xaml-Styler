using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
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

        [DataRow("<:test/>", DisplayName = "Colon name")]
        [DataRow("<t::test/>", DisplayName = "Prefix colon colon name")]
        [DataRow("<t:t:est/>", DisplayName = "Multi-prefixed name")]
        [DataRow("<.test/>", DisplayName = "Dot name")]
        [DataRow("<t:.test/>", DisplayName = "Prefix colon dot name")]
        [DataRow("<t.:test/>", DisplayName = "Prefix dot colon name")]
        [DataRow("<t.t:test/>", DisplayName = "Prefix dot name colon name")]
        [TestMethod]
        public async Task BrokenInput(string input)
        {
            await Assert.ThrowsExceptionAsync<XamlParserException>(() => XamlDocument.ParseAsync(input));
        }

        [DataRow(null, "test", "test", DisplayName = "Stripped inlined tag")]
        [DataRow(null, "test", "test ", DisplayName = "Stripped inlined with trailing whitespace tag")]
        [DataRow("t", "test", "t:test", DisplayName = "Prefixed inlined tag")]
        [DataRow("t", "test", "t:test ", DisplayName = "Prefixed inlined with trailing whitespace tag")]
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

        [DataRow("test", DisplayName = "Stripped name")]
        [DataRow("t:test", DisplayName = "Prefixed name")]
        [DataRow("test.name.attribute", DisplayName = "Prefixed name")]
        [DataRow("t:test.name.attribute", DisplayName = "Prefixed complex name")]
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

            var attr = element.Attributes[0];

            Assert.IsNotNull(attr);
            Assert.AreEqual(attribute, attr.Name);
        }

        [TestMethod]
        public async Task CheckNodeWithChild()
        {
            const string test = "<Page><Grid></Grid></Page>";

            var document = await XamlDocument.ParseAsync(test);

            Assert.IsNotNull(document);
            Assert.IsNotNull(document.Root);
            Assert.IsTrue(document.Root.HasChildNodes);
            Assert.AreEqual(1, document.Root.ChildNodes.Count);
            Assert.IsInstanceOfType(document.Root.FirstChild, typeof(XamlElement));

            var element = (XamlElement) document.Root.FirstChild;

            Assert.AreEqual(1, element.ChildNodes.Count);
        }

        [DataRow("< Page />", DisplayName = "Test1")]
        [DataRow("<-Page />", DisplayName = "Test2")]
        [DataRow("<!Page />", DisplayName = "Test3")]
        [DataRow("<@Page />", DisplayName = "Test4")]
        [DataRow("<#Page />", DisplayName = "Test5")]
        [DataRow("<$Page />", DisplayName = "Test6")]
        [TestMethod]
        public async Task InvalidTagName(string text)
        {
            await Assert.ThrowsExceptionAsync<XamlParserException>(() => XamlDocument.ParseAsync(text));
        }

        [TestMethod]
        public async Task ValidTagInnerValue()
        {
            const string content = "Inner Content";
            var test = "<Page>" + content + "</Page>";

            var document = await XamlDocument.ParseAsync(test);

            Assert.IsNotNull(document);
            Assert.IsNotNull(document.Root);
            Assert.IsTrue(document.Root.HasChildNodes);
            Assert.AreEqual(1, document.Root.ChildNodes.Count);

            var element = document.Root.ChildNodes[0];

            Assert.AreEqual(content, element.Value);
        }

        [TestMethod]
        public async Task Test1()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var stream = assembly.GetManifestResourceStream("UnitTests.Samples.Test1.Sample.xaml");

            try
            {
                using (var reader = new StreamReader(stream))
                {
                    var document = await XamlDocument.ParseAsync(reader);

                    Assert.IsNotNull(document);
                    Assert.IsNotNull(document.Root);
                }
            }
            catch (XamlParserException exception)
            {
                throw exception;
            }
        }
    }
}
