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

        [Test]
        public void NoMultiplexingTest()
        {
            var sig = new Signal
            {
                Multiplexing = null,
            };

            var multiplexing = sig.MultiplexingInfo();
            Assert.AreEqual(MultiplexingRole.None, multiplexing.Role);
            Assert.AreEqual(0, multiplexing.Group);
        }

        [Test]
        public void MultiplexorTest()
        {
            var sig = new Signal
            {
                Multiplexing = "M",
            };

            var multiplexing = sig.MultiplexingInfo();
            Assert.AreEqual(MultiplexingRole.Multiplexor, multiplexing.Role);
            Assert.AreEqual(0, multiplexing.Group);
        }

        [Test]
        public void MultiplexedSingleDigitTest()
        {
            var sig = new Signal
            {
                Multiplexing = "m3",
            };

            var multiplexing = sig.MultiplexingInfo();
            Assert.AreEqual(MultiplexingRole.Multiplexed, multiplexing.Role);
            Assert.AreEqual(3, multiplexing.Group);
        }

        [Test]
        public void ExtendedMultiplexingIsPartiallySupportedTest()
        {
            var sig = new Signal
            {
                Multiplexing = "m3M",
            };

            var multiplexing = sig.MultiplexingInfo();
            Assert.AreEqual(MultiplexingRole.Multiplexed, multiplexing.Role);
            Assert.AreEqual(3, multiplexing.Group);
        }

        [Test]
        public void MultiplexedDoubleDigitTest()
        {
            var sig = new Signal
            {
                Multiplexing = "m12",
            };

            var multiplexing = sig.MultiplexingInfo();
            Assert.AreEqual(MultiplexingRole.Multiplexed, multiplexing.Role);
            Assert.AreEqual(12, multiplexing.Group);
        }

        [Test]
        public void MultiplexedMessageTest()
        {
            var sig = new Signal
            {
                Multiplexing = "M",
            };

            var message = new Message();
            message.Signals.Add(sig);

            Assert.IsTrue(message.IsMultiplexed());
        }

        [Test]
        public void MessageWithNoMutiplexorIsNotMultiplexedTest()
        {
            var sig = new Signal
            {
                Multiplexing = null,
            };

            var message = new Message();
            message.Signals.Add(sig);

            Assert.IsFalse(message.IsMultiplexed());
        }

        [Test]
        public void EmptyMessageIsNotMultiplexedTest()
        {
            var message = new Message();
            Assert.IsFalse(message.IsMultiplexed());
        }

        [TestCase("1 \"First\" 2 \"Second\" 3 \"Third\"")]
        [TestCase("1 \"First with spaces\" 2 \" Second \" 3 \"T h i r d\"")]
        [TestCase("1 \"First with spaces\" 2 \" \" 3 \"\"")]
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
        public void FsmErrorTest()
        {
            var text = "1 \"First with spaces\" 2 \" Second \" 3 T h i r d \"";
            var operation = text.TryParseToDict(out var dict);
            var expectedDict = new Dictionary<int, string>()
            {
                { 1, "First with spaces" },
                { 2, " Second " }
            };

            Assert.IsFalse(operation);
            Assert.AreEqual(expectedDict, dict);
        }
    }
}