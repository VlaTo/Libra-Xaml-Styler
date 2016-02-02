using System;
using LibraProgramming.Xaml.Parsing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
//            var text = "<Application x:Class=\"Arkadium.Solitaire.App\" xmlns = \"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" xmlns:x = \"http://schemas.microsoft.com/winfx/2006/xaml\" xmlns:controls = \"using:Arkadium.Win10.Xaml.Toolkit.Controls\" xmlns:controls1 = \"using:Arkadium.Win10.DailyChallenges.Controls\">";
            var text = "<std:Application>";
            var document = XamlDocument.Parse(text);

            Assert.IsNotNull(document);
        }
    }
}
