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
            m_multiplexing = string.Empty,
            Name = "Test",
            m_extendedMultiplex = null
        };

        var message = new Message
        {
            m_signals = new Dictionary<string, Signal>
            {
                { "Test", sig }
            }
        };

        var multiplexing = new MultiplexingInfo(sig, message, false);
        Assert.AreEqual(MultiplexingRole.None, multiplexing.Role);
        Assert.IsNotNull(multiplexing.Multiplexing);
        Assert.AreEqual(string.Empty, multiplexing.Multiplexing.MultiplexorSignal);
        Assert.AreEqual(0, multiplexing.Multiplexing.MultiplexorRanges.Count);
    }

    [Test]
    public void MultiplexorTest()
    {
        var sig = new Signal
        {
            m_multiplexing = "M",
            Name = "Test",
            m_extendedMultiplex = null
        };
        
        var message = new Message
        {
            m_signals = new Dictionary<string, Signal>
            {
                { "Test", sig }
            }
        };

        var multiplexing = new MultiplexingInfo(sig, message, false);
        Assert.AreEqual(MultiplexingRole.Multiplexor, multiplexing.Role);
        Assert.IsNotNull(multiplexing.Multiplexing);
        Assert.AreEqual(string.Empty, multiplexing.Multiplexing.MultiplexorSignal);
        Assert.AreEqual(0, multiplexing.Multiplexing.MultiplexorRanges.Count);
    }

    [Test]
    public void MultiplexedSingleDigitTest()
    {
        var multiplexedSignal = new Signal
        {
            m_multiplexing = "m3",
            Name = "Test",
            m_extendedMultiplex = null
        };
        
        var multiplexorSignal = new Signal
        {
            m_multiplexing = "M",
            Name = "Multiplexor",
            m_extendedMultiplex = null
        };
        
        var message = new Message
        {
            m_signals = new Dictionary<string, Signal>
            {
                { "Test", multiplexedSignal },
                { "Multiplexor", multiplexorSignal },
            }
        };

        var multiplexing = new MultiplexingInfo(multiplexedSignal, message, false);
        Assert.AreEqual(MultiplexingRole.Multiplexed, multiplexing.Role);
        Assert.IsNotNull(multiplexing.Multiplexing);
        Assert.AreEqual("Multiplexor", multiplexing.Multiplexing.MultiplexorSignal);
        Assert.AreEqual(1, multiplexing.Multiplexing.MultiplexorRanges.Count);
        Assert.AreEqual(3, multiplexing.Multiplexing.MultiplexorRanges.First().Lower);
        Assert.AreEqual(3, multiplexing.Multiplexing.MultiplexorRanges.First().Upper);
    }

    [Test]
    public void ExtendedMultiplexingUsedInvalid()
    {
        var multiplexedSignal = new Signal
        {
            m_multiplexing = "m3M",
            Name = "Test",
            m_extendedMultiplex = null
        };
        
        var multiplexorSignal = new Signal
        {
            m_multiplexing = "M",
            Name = "Multiplexor",
            m_extendedMultiplex = null
        };
        
        var message = new Message
        {
            m_signals = new Dictionary<string, Signal>
            {
                { "Test", multiplexedSignal },
                { "Multiplexor", multiplexorSignal },
            }
        };

        var multiplexing = new MultiplexingInfo(multiplexedSignal, message, false);
        
        Assert.AreEqual(MultiplexingRole.Unknown, multiplexing.Role);
        Assert.IsNotNull(multiplexing.Multiplexing);
        Assert.AreEqual(string.Empty, multiplexing.Multiplexing.MultiplexorSignal);
        Assert.AreEqual(0, multiplexing.Multiplexing.MultiplexorRanges.Count);
    }

    [Test]
    public void MultiplexedDoubleDigitTest()
    {
        var multiplexedSignal = new Signal
        {
            m_multiplexing = "m12",
            Name = "Test",
            m_extendedMultiplex = null
        };
        
        var multiplexorSignal = new Signal
        {
            m_multiplexing = "M",
            Name = "Multiplexor",
            m_extendedMultiplex = null
        };
        
        var message = new Message
        {
            m_signals = new Dictionary<string, Signal>
            {
                { "Test", multiplexedSignal },
                { "Multiplexor", multiplexorSignal },
            }
        };

        var multiplexing = new MultiplexingInfo(multiplexedSignal, message, false);
        
        Assert.AreEqual(MultiplexingRole.Multiplexed, multiplexing.Role);
        Assert.IsNotNull(multiplexing.Multiplexing);
        Assert.AreEqual("Multiplexor", multiplexing.Multiplexing.MultiplexorSignal);
        Assert.AreEqual(1, multiplexing.Multiplexing.MultiplexorRanges.Count);
        Assert.AreEqual(12, multiplexing.Multiplexing.MultiplexorRanges.First().Lower);
        Assert.AreEqual(12, multiplexing.Multiplexing.MultiplexorRanges.First().Upper);
    }
    
    [Test]
    public void ExtendedMultiplexing()
    {
        var multiplexedSignal = new Signal
        {
            m_multiplexing = "m3M",
            Name = "Test",
            m_extendedMultiplex = new Multiplexing
            {
                MultiplexorSignal = "Multiplexor",
                MultiplexorRanges = new List<MultiplexorRange> { new MultiplexorRange{ Lower = 3, Upper = 4} }
            }
        };
        
        var multiplexorSignal = new Signal
        {
            m_multiplexing = "M",
            Name = "Multiplexor",
            m_extendedMultiplex = null
        };
        
        var message = new Message
        {
            m_signals = new Dictionary<string, Signal>
            {
                { "Test", multiplexedSignal },
                { "Multiplexor", multiplexorSignal },
            }
        };

        var multiplexing = new MultiplexingInfo(multiplexedSignal, message, true);
        
        Assert.AreEqual(MultiplexingRole.MultiplexedMultiplexor, multiplexing.Role);
        Assert.IsNotNull(multiplexing.Multiplexing);
        Assert.AreEqual("Multiplexor", multiplexing.Multiplexing.MultiplexorSignal);
        Assert.AreEqual(1, multiplexing.Multiplexing.MultiplexorRanges.Count);
        Assert.AreEqual(3, multiplexing.Multiplexing.MultiplexorRanges.First().Lower);
        Assert.AreEqual(4, multiplexing.Multiplexing.MultiplexorRanges.First().Upper);
    }
}