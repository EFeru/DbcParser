using DbcParserLib.Model;
using NUnit.Framework;

namespace DbcParserLib.Tests;

internal class MultiplexingInfoTests
{
    [Test]
    public void NoMultiplexingTest()
    {
        var sig = new Signal
        {
            multiplexing = null,
            Name = "Test"
        };

        var multiplexing = new MultiplexingInfo(sig);
        Assert.AreEqual(MultiplexingRole.None, multiplexing.Role);
        Assert.AreEqual(0, multiplexing.Group);
    }

    [Test]
    public void MultiplexorTest()
    {
        var sig = new Signal
        {
            multiplexing = "M",
            Name = "Test"
        };

        var multiplexing = new MultiplexingInfo(sig);
        Assert.AreEqual(MultiplexingRole.Multiplexor, multiplexing.Role);
        Assert.AreEqual(0, multiplexing.Group);
    }

    [Test]
    public void MultiplexedSingleDigitTest()
    {
        var sig = new Signal
        {
            multiplexing = "m3",
            Name = "Test"
        };

        var multiplexing = new MultiplexingInfo(sig);
        Assert.AreEqual(MultiplexingRole.Multiplexed, multiplexing.Role);
        Assert.AreEqual(3, multiplexing.Group);
    }

    [Test]
    public void ExtendedMultiplexingIsPartiallySupportedTest()
    {
        var sig = new Signal
        {
            multiplexing = "m3M",
            Name = "Test"
        };

        var multiplexing = new MultiplexingInfo(sig);
        Assert.AreEqual(MultiplexingRole.Multiplexed, multiplexing.Role);
        Assert.AreEqual(3, multiplexing.Group);
    }

    [Test]
    public void MultiplexedDoubleDigitTest()
    {
        var sig = new Signal
        {
            multiplexing = "m12",
            Name = "Test"
        };

        var multiplexing = new MultiplexingInfo(sig);
        Assert.AreEqual(MultiplexingRole.Multiplexed, multiplexing.Role);
        Assert.AreEqual(12, multiplexing.Group);
    }
}