using System.Collections.Generic;
using NUnit.Framework;
using DbcParserLib.Model;

namespace DbcParserLib.Tests
{
    [TestFixture]
    public class ExtensionMethodsTests
    {
        [Test]
        public void MotorolaTest()
        {
            var sig = new Signal
            {
                ByteOrder = 0, // 0 = Big Endian (Motorola), 1 = Little Endian (Intel)
            };

            Assert.IsTrue(sig.Motorola());
            Assert.IsTrue(sig.Msb());
            Assert.IsFalse(sig.Intel());
            Assert.IsFalse(sig.Lsb());
        }

        [Test]
        public void IntelTest()
        {
            var sig = new Signal
            {
                ByteOrder = 1, // 0 = Big Endian (Motorola), 1 = Little Endian (Intel)
            };

            Assert.IsFalse(sig.Motorola());
            Assert.IsFalse(sig.Msb());
            Assert.IsTrue(sig.Intel());
            Assert.IsTrue(sig.Lsb());
        }

        [TestCase("1 \"First\" 2 \"Second\" 3 \"Third\"")]
        [TestCase("1 \"First with spaces\" 2 \" Second \" 3 \"T h i r d\"")]
        [TestCase("1 \"First with spaces\" 2 \" \" 3 \"\"")]
        [TestCase("1 \"1\" 2 \" 2 \" 3 \" 3\"")]
        public void FsmNoErrorTest(string text)
        {
            var operation = text.TryParseToDict(out _);
            Assert.IsTrue(operation);
        }

        [TestCase("1 \"First 2 \"Second\" 3 \"Third\"")]
        [TestCase("1 First 2 \"Second\" 3 \"Third\"")]
        [TestCase("1 \"First\" 2 Second\" 3 \"Third\"")]
        [TestCase("One \"First with spaces\" 2 \" Second \"")]
        [TestCase("1 \"First\" 2 Second\" 3 \"Third\" 4")]
        [TestCase("1 \"First\" 2 Second\" 3 \"Third")]
        [TestCase("1 \"First\", 2 Second\", 3 \"Third")]
        public void FsmWithErrorTest(string text)
        {
            var operation = text.TryParseToDict(out _);
            Assert.IsFalse(operation);
        }

        [Test]
        public void FsmNoSpacesParsedTest()
        {
            var text = "1 \"First\" 2 \"Second\" 3 \"Third\"";
            var operation = text.TryParseToDict(out var dict);
            var expectedDict = new Dictionary<int, string>()
            {
                { 1, "First" },
                { 2, "Second" },
                { 3, "Third" }
            };

            Assert.IsTrue(operation);
            Assert.AreEqual(expectedDict, dict);
        }

        [Test]
        public void FsmWithSpacesParsedTest()
        {
            var text = "1 \"First with spaces\" 2 \" Second \" 3 \" T h i r d \"";
            var operation = text.TryParseToDict(out var dict);
            var expectedDict = new Dictionary<int, string>()
            {
                { 1, "First with spaces" },
                { 2, " Second " },
                { 3, " T h i r d " }
            };

            Assert.IsTrue(operation);
            Assert.AreEqual(expectedDict, dict);
        }

        [Test]
        public void FsmWithEmptyStringParsedTest()
        {
            var text = "1 \"\" 2 \" \"";
            var operation = text.TryParseToDict(out var dict);
            var expectedDict = new Dictionary<int, string>()
            {
                { 1, "" },
                { 2, " " }
            };

            Assert.IsTrue(operation);
            Assert.AreEqual(expectedDict, dict);
        }

        [Test]
        public void FsmWithIntegerStringParsedTest()
        {
            var text = "1 \"1\" 2 \"2\"";
            var operation = text.TryParseToDict(out var dict);
            var expectedDict = new Dictionary<int, string>()
            {
                { 1, "1" },
                { 2, "2" }
            };

            Assert.IsTrue(operation);
            Assert.AreEqual(expectedDict, dict);
        }

        [Test]
        public void FsmErrorTest()
        {
            var text = "1 \"First with spaces\" 2 \" Second \" 3 T h i r d \"";
            var operation = text.TryParseToDict(out var dict);

            Assert.IsFalse(operation);
            Assert.IsNull(dict);
        }
    }
}