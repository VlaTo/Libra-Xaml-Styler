using System;
using System.IO;
using LibraProgramming.Xaml.Parsing.StateMachine;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    public enum XamlParsingState
    {
        LineBegin,
        BeginingWhitespaces,
        NodeOpenToken,
        NodeNameOrAlias,
    }

    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var machine = new ParsingState<XamlParsingState, char>();

            machine
                .Configure(XamlParsingState.LineBegin)
                .Permit('<', XamlParsingState.NodeOpenToken)
                .Permit(' ', XamlParsingState.BeginingWhitespaces)
                .Permit('\t', XamlParsingState.BeginingWhitespaces);
            machine
                .Configure(XamlParsingState.BeginingWhitespaces)
                .Permit('<', XamlParsingState.NodeOpenToken)
                .Ignore(' ')
                .Ignore('\t');
            machine
                .Configure(XamlParsingState.NodeOpenToken)
                .Permit('a', XamlParsingState.NodeNameOrAlias)
                .Permit('b', XamlParsingState.NodeNameOrAlias)
                .Permit('c', XamlParsingState.NodeNameOrAlias)
                .Permit('d', XamlParsingState.NodeNameOrAlias)
                .Permit('e', XamlParsingState.NodeNameOrAlias)
                .Permit('f', XamlParsingState.NodeNameOrAlias)
                .Permit('g', XamlParsingState.NodeNameOrAlias)
                .Permit('h', XamlParsingState.NodeNameOrAlias)
                .Permit('i', XamlParsingState.NodeNameOrAlias)
                .Permit('j', XamlParsingState.NodeNameOrAlias)
                .Permit('k', XamlParsingState.NodeNameOrAlias)
                .Permit('l', XamlParsingState.NodeNameOrAlias)
                .Permit('m', XamlParsingState.NodeNameOrAlias)
                .Permit('n', XamlParsingState.NodeNameOrAlias)
                .Permit('o', XamlParsingState.NodeNameOrAlias)
                .Permit('p', XamlParsingState.NodeNameOrAlias)
                .Permit('q', XamlParsingState.NodeNameOrAlias)
                .Permit('r', XamlParsingState.NodeNameOrAlias)
                .Permit('s', XamlParsingState.NodeNameOrAlias)
                .Permit('t', XamlParsingState.NodeNameOrAlias)
                .Permit('u', XamlParsingState.NodeNameOrAlias)
                .Permit('v', XamlParsingState.NodeNameOrAlias)
                .Permit('w', XamlParsingState.NodeNameOrAlias)
                .Permit('x', XamlParsingState.NodeNameOrAlias)
                .Permit('y', XamlParsingState.NodeNameOrAlias)
                .Permit('z', XamlParsingState.NodeNameOrAlias)
                .Permit('A', XamlParsingState.NodeNameOrAlias)
                .Permit('B', XamlParsingState.NodeNameOrAlias)
                .Permit('C', XamlParsingState.NodeNameOrAlias)
                .Permit('D', XamlParsingState.NodeNameOrAlias)
                .Permit('E', XamlParsingState.NodeNameOrAlias)
                .Permit('F', XamlParsingState.NodeNameOrAlias)
                .Permit('G', XamlParsingState.NodeNameOrAlias)
                .Permit('H', XamlParsingState.NodeNameOrAlias)
                .Permit('I', XamlParsingState.NodeNameOrAlias)
                .Permit('J', XamlParsingState.NodeNameOrAlias)
                .Permit('K', XamlParsingState.NodeNameOrAlias)
                .Permit('L', XamlParsingState.NodeNameOrAlias)
                .Permit('M', XamlParsingState.NodeNameOrAlias)
                .Permit('N', XamlParsingState.NodeNameOrAlias)
                .Permit('O', XamlParsingState.NodeNameOrAlias)
                .Permit('P', XamlParsingState.NodeNameOrAlias)
                .Permit('Q', XamlParsingState.NodeNameOrAlias)
                .Permit('R', XamlParsingState.NodeNameOrAlias)
                .Permit('S', XamlParsingState.NodeNameOrAlias)
                .Permit('T', XamlParsingState.NodeNameOrAlias)
                .Permit('U', XamlParsingState.NodeNameOrAlias)
                .Permit('V', XamlParsingState.NodeNameOrAlias)
                .Permit('W', XamlParsingState.NodeNameOrAlias)
                .Permit('X', XamlParsingState.NodeNameOrAlias)
                .Permit('Y', XamlParsingState.NodeNameOrAlias)
                .Permit('Z', XamlParsingState.NodeNameOrAlias)
                .Permit('0', XamlParsingState.NodeNameOrAlias)
                .Permit('1', XamlParsingState.NodeNameOrAlias)
                .Permit('2', XamlParsingState.NodeNameOrAlias)
                .Permit('3', XamlParsingState.NodeNameOrAlias)
                .Permit('4', XamlParsingState.NodeNameOrAlias)
                .Permit('5', XamlParsingState.NodeNameOrAlias)
                .Permit('6', XamlParsingState.NodeNameOrAlias)
                .Permit('7', XamlParsingState.NodeNameOrAlias)
                .Permit('8', XamlParsingState.NodeNameOrAlias)
                .Permit('9', XamlParsingState.NodeNameOrAlias)
                .Permit('_', XamlParsingState.NodeNameOrAlias)
                .Permit('');

            var sample = "  <test";

            using (var reader = new StringReader(sample))
            {
                while (true)
                {
                    var current = reader.Read();

                    if (-1 == current)
                    {
                        break;
                    }

                    machine.Fire((char) current);
                }
            }
        }
    }
}
