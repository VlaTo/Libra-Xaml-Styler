using System;
using LibraProgramming.Xaml;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.Incorrect
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
//            var node = XamlParser.Parse(" <test />");
            var node = XamlParser.Parse(" </test >");

            Assert.IsNotNull(node);
        }
    }
}
