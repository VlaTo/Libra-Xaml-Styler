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
        }

        [TestMethod]
        [TestCategory("Empty")]
        public async Task ThreeWhitespacesInput()
        {
            var test = new String(' ', 3);
            var document = await XamlDocument.ParseAsync(test);

            Assert.IsNotNull(document);
        }

        [TestMethod]
        [TestCategory("Empty")]
        public async Task TestMethod2()
        {
            const string test = " <Property>";
            var document = await XamlDocument.ParseAsync(test);

            Assert.IsNotNull(document);
        }

        [TestMethod]
        [TestCategory("Empty")]
        public async Task TestMethod3()
        {
            const string test = "<app:Application.Property.Attribute> </app:Application.Property.Attribute>";
            var document = await XamlDocument.ParseAsync(test);

            Assert.IsNotNull(document);
        }

        [TestMethod]
        [TestCategory("Empty")]
        public async Task TestMethod4()
        {
            const string test = "<app:Application.Property.Attribute test=\"test string with 'single quoted' text\"> </app:Application.Property.Attribute>";
            var document = await XamlDocument.ParseAsync(test);

            Assert.IsNotNull(document);
        }
    }
}
