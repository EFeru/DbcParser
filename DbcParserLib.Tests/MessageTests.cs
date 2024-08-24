using System.Collections.Generic;
using DbcParserLib.Model;
using NUnit.Framework;

namespace DbcParserLib.Tests;

internal class MessageTests
{
    [Test]
    public void MultiplexedMessageTest()
    {
        var sig = new Signal
        {
            m_multiplexing = "M",
            Name = "Test"
        };

        var message = new Message
        {
            m_signals = new Dictionary<string, Signal>
            {
                { "Test", sig }
            }
        };
        message.FinishUp();
        Assert.IsTrue(message.IsMultiplexed);
    }

    [Test]
    public void MessageWithNoMutiplexorIsNotMultiplexedTest()
    {
        var sig = new Signal
        {
            m_multiplexing = null,
            Name = "Test"
        };

        var message = new Message
        {
            m_signals = new Dictionary<string, Signal>
            {
                { "Test", sig }
            }
        };
        message.FinishUp();

        Assert.IsFalse(message.IsMultiplexed);
    }

    [Test]
    public void EmptyMessageIsNotMultiplexedTest()
    {
        var message = new Message();
        Assert.IsFalse(message.IsMultiplexed);
    }
}