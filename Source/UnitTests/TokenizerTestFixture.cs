using System.IO;
using System.Threading.Tasks;
using LibraProgramming.Parsing.Xaml;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class TokenizerTestFixture
    {
        [TestMethod]
        public async Task TestMethod1()
        {
            using (var reader = new StringReader("ABC"))
            {
                var tokenizer = new XamlTokenizer(reader, 2);

                Assert.AreEqual('A', await tokenizer.ReadNextCharAsync(true));
                Assert.AreEqual('B', await tokenizer.ReadNextCharAsync(true));
                Assert.AreEqual('C', await tokenizer.ReadNextCharAsync(true));
                Assert.AreEqual(-1, await tokenizer.ReadNextCharAsync(true));
            }
        }

        [TestMethod]
        public async Task TestMethod2()
        {
            using (var reader = new StringReader("AB"))
            {
                var tokenizer = new XamlTokenizer(reader, 2);

                Assert.AreEqual('A', await tokenizer.ReadNextCharAsync(false));
                Assert.AreEqual('A', await tokenizer.ReadNextCharAsync(true));
                Assert.AreEqual('B', await tokenizer.ReadNextCharAsync(true));
                Assert.AreEqual(-1, await tokenizer.ReadNextCharAsync(true));
            }
        }

        [TestMethod]
        public async Task TestMethod3()
        {
            using (var reader = new StringReader("ABCD"))
            {
                var tokenizer = new XamlTokenizer(reader, 2);

                Assert.AreEqual('A', await tokenizer.ReadNextCharAsync(false));
                Assert.AreEqual('A', await tokenizer.ReadNextCharAsync(true));
                Assert.AreEqual('B', await tokenizer.ReadNextCharAsync(true));
                Assert.AreEqual('C', await tokenizer.ReadNextCharAsync(false));
                Assert.AreEqual('C', await tokenizer.ReadNextCharAsync(true));
                Assert.AreEqual('D', await tokenizer.ReadNextCharAsync(true));
                Assert.AreEqual(-1, await tokenizer.ReadNextCharAsync(true));
            }
        }
    }
}
