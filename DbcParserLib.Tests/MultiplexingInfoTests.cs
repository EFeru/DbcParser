using System.Collections.Generic;
using System.Linq;
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
            multiplexing = string.Empty,
            Name = "Test",
            extendedMultiplex = null
        };

        var multiplexing = new MultiplexingInfo(sig);
        Assert.AreEqual(MultiplexingRole.None, multiplexing.Role);
        Assert.AreEqual(MultiplexingMode.None, multiplexing.Mode);
        Assert.IsNull(multiplexing.ExtendedMultiplex);
        Assert.IsNull(multiplexing.SimpleMultiplex);
    }

    [Test]
    public void MultiplexorTest()
    {
        var sig = new Signal
        {
            multiplexing = "M",
            Name = "Test",
            extendedMultiplex = null
        };

        var multiplexing = new MultiplexingInfo(sig);
        Assert.AreEqual(MultiplexingRole.Multiplexor, multiplexing.Role);
        Assert.AreEqual(MultiplexingMode.Simple, multiplexing.Mode);
        Assert.IsNull(multiplexing.ExtendedMultiplex);
        Assert.IsNull(multiplexing.SimpleMultiplex);
    }

    [Test]
    public void MultiplexedSingleDigitTest()
    {
        var sig = new Signal
        {
            multiplexing = "m3",
            Name = "Test",
            extendedMultiplex = null
        };

        var multiplexing = new MultiplexingInfo(sig);
        Assert.AreEqual(MultiplexingRole.Multiplexed, multiplexing.Role);
        Assert.AreEqual(MultiplexingMode.Simple, multiplexing.Mode);
        Assert.IsNull(multiplexing.ExtendedMultiplex);
        Assert.AreEqual(3, multiplexing.SimpleMultiplex!.MultiplexorValue);
    }

    [Test]
    public void ExtendedMultiplexingUsedInvalid()
    {
        var sig = new Signal
        {
            multiplexing = "m3M",
            Name = "Test",
            extendedMultiplex = null
        };

        var multiplexing = new MultiplexingInfo(sig);
        
        Assert.AreEqual(MultiplexingRole.Unknown, multiplexing.Role);
        Assert.AreEqual(MultiplexingMode.Simple, multiplexing.Mode);
        Assert.IsNull(multiplexing.ExtendedMultiplex);
        Assert.IsNull(multiplexing.SimpleMultiplex);
    }

    [Test]
    public void MultiplexedDoubleDigitTest()
    {
        var sig = new Signal
        {
            multiplexing = "m12",
            Name = "Test",
            extendedMultiplex = null
        };

        var multiplexing = new MultiplexingInfo(sig);
        
        Assert.AreEqual(MultiplexingRole.Multiplexed, multiplexing.Role);
        Assert.AreEqual(MultiplexingMode.Simple, multiplexing.Mode);
        Assert.IsNull(multiplexing.ExtendedMultiplex);
        Assert.AreEqual(12, multiplexing.SimpleMultiplex!.MultiplexorValue);
    }
    
    [Test]
    public void ExtendedMultiplexing()
    {
        var sig = new Signal
        {
            multiplexing = "m3M",
            Name = "Test",
            extendedMultiplex = new ExtendedMultiplex
            {
                MultiplexorSignal = "Multiplexor",
                MultiplexorRanges = new List<MultiplexorRange> { new MultiplexorRange{ Lower = 3, Upper = 3} }
            }
        };

        var multiplexing = new MultiplexingInfo(sig);
        
        Assert.AreEqual(MultiplexingRole.MultiplexedMultiplexor, multiplexing.Role);
        Assert.AreEqual(MultiplexingMode.Extended, multiplexing.Mode);
        Assert.IsNull(multiplexing.SimpleMultiplex);
        Assert.AreEqual("Multiplexor", multiplexing.ExtendedMultiplex!.MultiplexorSignal);
        Assert.AreEqual(1, multiplexing.ExtendedMultiplex!.MultiplexorRanges.Count);
        Assert.AreEqual(3, multiplexing.ExtendedMultiplex!.MultiplexorRanges.First().Lower);
        Assert.AreEqual(3, multiplexing.ExtendedMultiplex!.MultiplexorRanges.First().Upper);
    }
}