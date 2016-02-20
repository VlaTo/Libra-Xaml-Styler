﻿using System;
using System.Diagnostics;
using System.Linq;
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
            const string tag = "test";

            var samples = new[]
            {
                "<" + tag + ">",
                "<" + tag + " " + ">",
                " " + "<" + tag + ">",
                " " + "<" + tag + " " + ">"
            };

            foreach (var sample in samples)
            {
                var root = XamlParser.Parse(sample);

                Assert.IsNotNull(root);
                Assert.AreEqual(1, root.Children.Count, $"Sample: \'{sample}\'");

                var node = root.Children.First();

                Assert.AreEqual(tag, node.Name);
                Assert.IsFalse(node.IsInline);
                Assert.AreEqual(String.Empty, node.Prefix);
                Assert.AreEqual(0, root.Attributes.Count);
            }
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
                "<Application x:Class = \"LibraProgramming.Sample.App\" xmlns = \"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" xmlns:x = \"http://schemas.microsoft.com/winfx/2006/xaml\" xmlns:vm = \"using:LibraProgramming.Sample.ViewModels\" xmlns:xaml = \"using:LibraProgramming.Sample.UI.Xaml\" RequestedTheme = \"Dark\">\n" +
                "    <Application.Resources>\n" +
                "        some text" +
                "    </Application.Resources>\n" +
                "</Application>";
            var builder = new StringBuilder();
            var visitor = new StylerNodeVisitor(builder);

            var node = XamlParser.Parse(text);

            visitor.Visit(node);

            Debug.WriteLine(builder.ToString());
        }
    }
}
