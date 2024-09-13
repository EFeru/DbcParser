using System.Collections.Generic;
using System.Linq;
using DbcParserLib.Parsers.Evo;
using NUnit.Framework;

namespace DbcParserLib.Tests.Evo;

public class TextBrowser
{
    [Test]
    public void WithNoLineInitializationTest()
    {
        var lines = new List<string>();
        var browser = new Parsers.Evo.TextBrowser(new ArrayBasedLineProvider(lines));

        var index = 0;
        browser.Walk(item =>
        {
            Assert.Fail();
            return PeekResult.Stop;
        });
    }

    [Test]
    public void WithOneEmptyLineInitializationTest()
    {
        var lines = new List<string>() { string.Empty };
        var browser = new Parsers.Evo.TextBrowser(new ArrayBasedLineProvider(lines));

        var index = 0;
        browser.Walk(item =>
        {
            Assert.Fail();
            return PeekResult.Stop;
        });
    }

    [Test]
    public void WithMultipleEmptyLinesInitializationTest()
    {
        var lines = new List<string>() { string.Empty, string.Empty };
        var browser = new Parsers.Evo.TextBrowser(new ArrayBasedLineProvider(lines));

        var index = 0;
        browser.Walk(item =>
        {
            index++;
            Assert.That(item, Is.EqualTo('\n'));
            return PeekResult.Continue;
        });

        Assert.That(index, Is.EqualTo(1));

    }

    [Test]
    public void WithOneLineInitializationTest()
    {
        var lines = new List<string>() { "First line" };
        var browser = new Parsers.Evo.TextBrowser(new ArrayBasedLineProvider(lines));

        var index = 0;
        browser.Walk(item =>
        {
            Assert.That(browser.Row, Is.EqualTo(1));
            Assert.That(item, Is.EqualTo(lines[0][index++]));
            return PeekResult.Continue;
        });

        Assert.That(index, Is.EqualTo(lines[0].Length));

    }

    [Test]
    public void WithTwoLinesInitializationTest()
    {
        var lines = new List<string>() { "First line", "Second line" };
        var browser = new Parsers.Evo.TextBrowser(new ArrayBasedLineProvider(lines));

        var index = 0;
        var expectations = lines[0].ToList();
        expectations.Add('\n');
        expectations.AddRange(lines[1]);

        browser.Walk(item =>
        {
            Assert.That(browser.Row, Is.EqualTo(index <= lines[0].Length ? 1 : 2));
            Assert.That(item, Is.EqualTo(expectations[index++]));
            return PeekResult.Continue;
        });

        Assert.That(index, Is.EqualTo(expectations.Count));
    }

    [Test]
    public void ReturningStopKeepsPointerWhereItWasTest()
    {
        var lines = new List<string>() { "First line", "Second line" };
        var browser = new Parsers.Evo.TextBrowser(new ArrayBasedLineProvider(lines));

        browser.Walk(item =>
        {
            Assert.That(item, Is.EqualTo('F'));
            return PeekResult.Stop;
        });

        browser.Walk(item =>
        {
            Assert.That(item, Is.EqualTo('F'));
            return PeekResult.Stop;
        });
    }

    [Test]
    public void ReturningConsumeMovesThePointerForwardTest()
    {
        var lines = new List<string>() { "First line", "Second line" };
        var browser = new Parsers.Evo.TextBrowser(new ArrayBasedLineProvider(lines));

        browser.Walk(item =>
        {
            Assert.That(item, Is.EqualTo('F'));
            return PeekResult.Consume;
        });

        browser.Walk(item =>
        {
            Assert.That(item, Is.EqualTo('i'));
            return PeekResult.Stop;
        });
    }

}