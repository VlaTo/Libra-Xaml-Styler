using System.IO;
using System.Threading.Tasks;
using LibraProgramming.Parsing.Xaml;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
/*
    [TestClass]
    public class TokenizerTestFixture
    {
        [TestMethod]
        public async Task TestMethod1()
        {
            using (var reader = new StringReader("ABC"))
            {
                var tokenizer = new XamlTokenizer(reader, 2);

                Assert.AreEqual('A', await tokenizer.ReadCurrentChar(true));
                Assert.AreEqual('B', await tokenizer.ReadCurrentChar(true));
                Assert.AreEqual('C', await tokenizer.ReadCurrentChar(true));
                Assert.AreEqual(-1, await tokenizer.ReadCurrentChar(true));
            }
        }

        [TestMethod]
        public async Task TestMethod2()
        {
            using (var reader = new StringReader("AB"))
            {
                var tokenizer = new XamlTokenizer(reader, 2);

                Assert.AreEqual('A', await tokenizer.ReadCurrentChar(false));
                Assert.AreEqual('A', await tokenizer.ReadCurrentChar(true));
                Assert.AreEqual('B', await tokenizer.ReadCurrentChar(true));
                Assert.AreEqual(-1, await tokenizer.ReadCurrentChar(true));
            }
        }

        [TestMethod]
        public async Task TestMethod3()
        {
            using (var reader = new StringReader("ABCD"))
            {
                var tokenizer = new XamlTokenizer(reader, 2);

                Assert.AreEqual('A', await tokenizer.ReadCurrentChar(false));
                Assert.AreEqual('A', await tokenizer.ReadCurrentChar(true));
                Assert.AreEqual('B', await tokenizer.ReadCurrentChar(true));
                Assert.AreEqual('C', await tokenizer.ReadCurrentChar(false));
                Assert.AreEqual('C', await tokenizer.ReadCurrentChar(true));
                Assert.AreEqual('D', await tokenizer.ReadCurrentChar(true));
                Assert.AreEqual(-1, await tokenizer.ReadCurrentChar(true));
            }
        }

        [TestMethod]
        public async Task TestMethod4()
        {
            using (var reader = new StringReader("AB CD"))
            {
                var tokenizer = new XamlTokenizer(reader, 2);

                Assert.AreEqual('A', await tokenizer.ReadCurrentChar(false));
                Assert.AreEqual('A', await tokenizer.ReadCurrentChar(true));
                Assert.AreEqual('B', await tokenizer.ReadCurrentChar(true));
                Assert.AreEqual('C', await tokenizer.ReadCurrentChar(false));
                Assert.AreEqual('C', await tokenizer.ReadCurrentChar(true));
                Assert.AreEqual('D', await tokenizer.ReadCurrentChar(true));
                Assert.AreEqual(-1, await tokenizer.ReadCurrentChar(true));
            }
        }
    }
*/
}
