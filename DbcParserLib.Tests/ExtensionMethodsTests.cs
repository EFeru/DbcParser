using NUnit.Framework;
using System.Linq;
using DbcParserLib.Model;
using System.Collections.Generic;

namespace DbcParserLib.Tests
{
    public class ExtensionMethodsTests
    {
        [Test]
        public void MotorolaTest()
        {
            var sig = new EditableSignal
            {
                ByteOrder = 0, // 0 = Big Endian (Motorola), 1 = Little Endian (Intel)
            };

            Assert.IsTrue(sig.CreateSignal().Motorola());
            Assert.IsTrue(sig.CreateSignal().Msb());
            Assert.IsFalse(sig.CreateSignal().Intel());
            Assert.IsFalse(sig.CreateSignal().Lsb());
        }

        [Test]
        public void IntelTest()
        {
            var sig = new EditableSignal
            {
                ByteOrder = 1, // 0 = Big Endian (Motorola), 1 = Little Endian (Intel)
            };

            Assert.IsFalse(sig.CreateSignal().Motorola());
            Assert.IsFalse(sig.CreateSignal().Msb());
            Assert.IsTrue(sig.CreateSignal().Intel());
            Assert.IsTrue(sig.CreateSignal().Lsb());
        }

        [Test]
        public void NoMultiplexingTest()
        {
            var sig = new EditableSignal
            {
                Multiplexing = null,
            };

            var multiplexing = sig.CreateSignal().MultiplexingInfo();
            Assert.AreEqual(MultiplexingRole.None, multiplexing.Role);
            Assert.AreEqual(0, multiplexing.Group);
        }

        [Test]
        public void MultiplexorTest()
        {
            var sig = new EditableSignal
            {
                Multiplexing = "M",
            };

            var multiplexing = sig.CreateSignal().MultiplexingInfo();
            Assert.AreEqual(MultiplexingRole.Multiplexor, multiplexing.Role);
            Assert.AreEqual(0, multiplexing.Group);
        }

        [Test]
        public void MultiplexedSingleDigitTest()
        {
            var sig = new EditableSignal
            {
                Multiplexing = "m3",
            };

            var multiplexing = sig.CreateSignal().MultiplexingInfo();
            Assert.AreEqual(MultiplexingRole.Multiplexed, multiplexing.Role);
            Assert.AreEqual(3, multiplexing.Group);
        }

        [Test]
        public void ExtendedMultiplexingIsPartiallySupportedTest()
        {
            var sig = new EditableSignal
            {
                Multiplexing = "m3M",
            };

            var multiplexing = sig.CreateSignal().MultiplexingInfo();
            Assert.AreEqual(MultiplexingRole.Multiplexed, multiplexing.Role);
            Assert.AreEqual(3, multiplexing.Group);
        }

        [Test]
        public void MultiplexedDoubleDigitTest()
        {
            var sig = new EditableSignal
            {
                Multiplexing = "m12",
            };

            var multiplexing = sig.CreateSignal().MultiplexingInfo();
            Assert.AreEqual(MultiplexingRole.Multiplexed, multiplexing.Role);
            Assert.AreEqual(12, multiplexing.Group);
        }

        [Test]
        public void MultiplexedMessageTest()
        {
            var sig = new EditableSignal
            {
                Multiplexing = "M",
            };

            var message = new EditableMessage();
            message.Signals.Add(sig);

            Assert.IsTrue(message.CreateMessage().IsMultiplexed());
        }

        [Test]
        public void MessageWithNoMutiplexorIsNotMultiplexedTest()
        {
            var sig = new EditableSignal
            {
                Multiplexing = null,
            };

            var message = new EditableMessage();
            message.Signals.Add(sig);

            Assert.IsFalse(message.CreateMessage().IsMultiplexed());
        }

        [Test]
        public void EmptyMessageIsNotMultiplexedTest()
        {
            var message = new EditableMessage();
            Assert.IsFalse(message.CreateMessage().IsMultiplexed());
        }
    }
}