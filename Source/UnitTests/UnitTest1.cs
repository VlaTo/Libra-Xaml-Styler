using Microsoft.VisualStudio.TestTools.UnitTesting;
using XamlParser = LibraProgramming.Xaml.XamlParser;

namespace UnitTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var text = "<Application xmlns = \"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" xmlns:x = \"http://schemas.microsoft.com/winfx/2006/xaml\" x:Class=\"Arkadium.Solitaire.App\">";
            var node = XamlParser.Parse(text);

            Assert.IsNotNull(node);
        }
    }
}
