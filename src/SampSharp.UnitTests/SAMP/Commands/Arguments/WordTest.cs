﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using SampSharp.GameMode.SAMP.Commands.Arguments;

namespace SampSharp.UnitTests.SAMP.Commands.Arguments
{
    [TestClass]
    public class WordTest : ArgumentTest<WordCommandParameterType>
    {
        [TestMethod]
        public void NoTextTest()
        {
            Test("", false);
        }

        [TestMethod]
        public void RegularTest()
        {
            Test("some text", "some", "text");
        }
        [TestMethod]
        public void OneWordTest()
        {
            Test("one-word", "one-word", "");
        }
    }
}