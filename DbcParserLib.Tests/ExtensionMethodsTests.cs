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

            Assert.That(sig.Motorola(), Is.True);
            Assert.That(sig.Msb(), Is.True);
            Assert.That(sig.Intel(), Is.False);
            Assert.That(sig.Lsb(), Is.False);
        }

        [Test]
        public void IntelTest()
        {
            var sig = new Signal
            {
                ByteOrder = 1, // 0 = Big Endian (Motorola), 1 = Little Endian (Intel)
            };

            Assert.That(sig.Motorola(), Is.False);
            Assert.That(sig.Msb(), Is.False);
            Assert.That(sig.Intel(), Is.True);
            Assert.That(sig.Lsb(), Is.True);
        }

        [Test]
        public void NoMultiplexingTest()
        {
            var sig = new Signal
            {
                Multiplexing = null,
            };

            var multiplexing = sig.MultiplexingInfo();
            Assert.That(multiplexing.Role, Is.EqualTo(MultiplexingRole.None));
            Assert.That(multiplexing.Group, Is.EqualTo(0));
        }

        [Test]
        public void MultiplexorTest()
        {
            var sig = new Signal
            {
                Multiplexing = "M",
            };

            var multiplexing = sig.MultiplexingInfo();
            Assert.That(multiplexing.Role, Is.EqualTo(MultiplexingRole.Multiplexor));
            Assert.That(multiplexing.Group, Is.EqualTo(0));
        }

        [Test]
        public void MultiplexedSingleDigitTest()
        {
            var sig = new Signal
            {
                Multiplexing = "m3",
            };

            var multiplexing = sig.MultiplexingInfo();
            Assert.That(multiplexing.Role, Is.EqualTo(MultiplexingRole.Multiplexed));
            Assert.That(multiplexing.Group, Is.EqualTo(3));
        }

        [Test]
        public void ExtendedMultiplexingIsPartiallySupportedTest()
        {
            var sig = new Signal
            {
                Multiplexing = "m3M",
            };

            var multiplexing = sig.MultiplexingInfo();
            Assert.That(multiplexing.Role, Is.EqualTo(MultiplexingRole.Multiplexed));
            Assert.That(multiplexing.Group, Is.EqualTo(3));
        }

        [Test]
        public void MultiplexedDoubleDigitTest()
        {
            var sig = new Signal
            {
                Multiplexing = "m12",
            };

            var multiplexing = sig.MultiplexingInfo();
            Assert.That(multiplexing.Role, Is.EqualTo(MultiplexingRole.Multiplexed));
            Assert.That(multiplexing.Group, Is.EqualTo(12));
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

            Assert.That(message.IsMultiplexed(), Is.True);
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

            Assert.That(message.IsMultiplexed(), Is.False);
        }

        [Test]
        public void EmptyMessageIsNotMultiplexedTest()
        {
            var message = new Message();
            Assert.That(message.IsMultiplexed(), Is.False);
        }

        [TestCase("1 \"First\" 2 \"Second\" 3 \"Third\"")]
        [TestCase("1 \"First with spaces\" 2 \" Second \" 3 \"T h i r d\"")]
        [TestCase("1 \"First with spaces\" 2 \" \" 3 \"\"")]
        [TestCase("1 \"1\" 2 \" 2 \" 3 \" 3\"")]
        public void FsmNoErrorTest(string text)
        {
            var operation = text.TryParseToDict(out _);
            Assert.That(operation, Is.True);
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
            Assert.That(operation, Is.False);
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

            Assert.That(operation, Is.True);
            Assert.That(dict, Is.EqualTo(expectedDict));
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

            Assert.That(operation, Is.True);
            Assert.That(dict, Is.EqualTo(expectedDict));
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

            Assert.That(operation, Is.True);
            Assert.That(dict, Is.EqualTo(expectedDict));
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

            Assert.That(operation, Is.True);
            Assert.That(dict, Is.EqualTo(expectedDict));
        }

        [Test]
        public void FsmErrorTest()
        {
            var text = "1 \"First with spaces\" 2 \" Second \" 3 T h i r d \"";
            var operation = text.TryParseToDict(out var dict);

            Assert.That(operation, Is.False);
            Assert.That(dict, Is.Null);
        }
    }
}