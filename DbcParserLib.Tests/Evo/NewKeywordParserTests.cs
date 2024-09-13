using System.Collections.Generic;
using DbcParserLib.Parsers.Evo;
using DbcParserLib.Parsers.Evo.Parsers;
using Moq;
using NUnit.Framework;

namespace DbcParserLib.Tests.Evo;

public class NewKeywordParserTests
{
    [Test]
    public void EmptyNodeListOneLinerTest()
    {
        var lines = new List<string>(){"BU_: BO_ 2364540158 EEC1: 8 Vector_XXX"};
        var browser = new Parsers.Evo.TextBrowser(new ArrayBasedLineProvider(lines));


        var read = browser.TryReadId(out var id);
        Assert.That(read, Is.True);
        Assert.That(id, Is.EqualTo("BU_"));

        var repository = new MockRepository(MockBehavior.Strict);
        var storeMock = repository.Create<IKeywordStore>();

        IKeywordParser parser = new MessageKeywordParser();
        storeMock.Setup(ks => ks.TryGetKeywordParser("BO_", out parser))
            .Returns(true);

        var nodeParser = new NodesKeywordParser();
        var nextParser = nodeParser.TryParse(browser, storeMock.Object);

        Assert.That(nextParser, Is.EqualTo(parser));
        Assert.That(nextParser, Is.InstanceOf<MessageKeywordParser>());
    }

    [Test]
    public void EmptyNodeListTest()
    {
        var lines = new List<string>(){"BU_:", string.Empty, "\t\t ", "BO_ 2364540158 EEC1: 8 Vector_XXX"};
        var browser = new Parsers.Evo.TextBrowser(new ArrayBasedLineProvider(lines));


        var read = browser.TryReadId(out var id);
        Assert.That(read, Is.True);
        Assert.That(id, Is.EqualTo("BU_"));

        var repository = new MockRepository(MockBehavior.Strict);
        var storeMock = repository.Create<IKeywordStore>();

        IKeywordParser parser = new MessageKeywordParser();
        storeMock.Setup(ks => ks.TryGetKeywordParser("BO_", out parser))
            .Returns(true);

        var nodeParser = new NodesKeywordParser();
        var nextParser = nodeParser.TryParse(browser, storeMock.Object);

        Assert.That(nextParser, Is.EqualTo(parser));
        Assert.That(nextParser, Is.InstanceOf<MessageKeywordParser>());
    }

    [Test]
    public void SomeNodeListAndASignalTest()
    {
        var lines = new List<string>(){"BU_: ABC D3F", string.Empty, "\t\t ", "BO_ 2364540158 EEC1: 8 Vector_XXX", "  SG_ EngineSpeed : 24|16@1+ (0.125,0) [0|8031.875] \"rpm\" Vector_XXX"};
        var browser = new Parsers.Evo.TextBrowser(new ArrayBasedLineProvider(lines));


        var read = browser.TryReadId(out var id);
        Assert.That(read, Is.True);
        Assert.That(id, Is.EqualTo("BU_"));

        var repository = new MockRepository(MockBehavior.Strict);
        var storeMock = repository.Create<IKeywordStore>();

        IKeywordParser parser = new MessageKeywordParser();
        storeMock.Setup(ks => ks.TryGetKeywordParser("BO_", out parser))
            .Returns(true);

        IKeywordParser nullparser = null;
        storeMock.Setup(ks => ks.TryGetKeywordParser("ABC", out nullparser))
            .Returns(false);
        storeMock.Setup(ks => ks.TryGetKeywordParser("D3F", out nullparser))
            .Returns(false);
        storeMock.Setup(ks => ks.TryGetKeywordParser("Vector_XXX", out nullparser))
            .Returns(false);

        IKeywordParser expectedSignalParser = new SignalKeywordParser();
        storeMock.Setup(ks => ks.TryGetKeywordParser("SG_", out expectedSignalParser))
            .Returns(true);

        var nodeParser = new NodesKeywordParser();
        var messageParser = nodeParser.TryParse(browser, storeMock.Object);
        var signalParser = messageParser.TryParse(browser, storeMock.Object);
        var genericParser = signalParser.TryParse(browser, storeMock.Object);

        Assert.That(messageParser, Is.EqualTo(parser));
        Assert.That(messageParser, Is.InstanceOf<MessageKeywordParser>());

        Assert.That(signalParser, Is.EqualTo(expectedSignalParser));
        Assert.That(signalParser, Is.InstanceOf<SignalKeywordParser>());

        Assert.That(genericParser, Is.InstanceOf<GenericNextKeywordFinder>());
    }

    [Test]
    public void MultipleReceiversSignalTest()
    {
        var lines = new List<string>(){"  SG_ EngineSpeed : 24|16@1+ (0.125,0) [0|8031.875] \"rpm\" Vector_XXX, Pluto"};
        var browser = new Parsers.Evo.TextBrowser(new ArrayBasedLineProvider(lines));

        var next = browser.TryNext();
        Assert.That(next, Is.True);
        var read = browser.TryReadId(out var id);
        Assert.That(read, Is.True);
        Assert.That(id, Is.EqualTo("SG_"));

        var repository = new MockRepository(MockBehavior.Strict);
        var storeMock = repository.Create<IKeywordStore>();

        IKeywordParser parser = new MessageKeywordParser();
        storeMock.Setup(ks => ks.TryGetKeywordParser("BO_", out parser))
            .Returns(true);

        var signalParser = new SignalKeywordParser();
        var genericParser = signalParser.TryParse(browser, storeMock.Object);

        Assert.That(genericParser, Is.InstanceOf<GenericNextKeywordFinder>());
    }

    [Test]
    public void MessageRootTest()
    {
        var lines = new List<string>(){"CM_ \"This is a comment we want to keep\" ;"};
        var browser = new Parsers.Evo.TextBrowser(new ArrayBasedLineProvider(lines));

        var next = browser.TryNext();
        Assert.That(next, Is.True);
        var read = browser.TryReadId(out var id);
        Assert.That(read, Is.True);
        Assert.That(id, Is.EqualTo("CM_"));

        var repository = new MockRepository(MockBehavior.Strict);
        var storeMock = repository.Create<IKeywordStore>();

        var commentKeywordParser = new CommentKeywordParser();
        var rootMessageParser = commentKeywordParser.TryParse(browser, storeMock.Object);
        var nothing = rootMessageParser.TryParse(browser, storeMock.Object);

        Assert.That(rootMessageParser, Is.InstanceOf<RootCommentKeywordParser>());
        Assert.That(nothing, Is.InstanceOf<GenericNextKeywordFinder>());
    }

    [Test]
    public void MessageCommentTest()
    {
        var lines = new List<string>(){"CM_ BO_ 458458 \"This is a comment we want to keep\" ;"};
        var browser = new Parsers.Evo.TextBrowser(new ArrayBasedLineProvider(lines));

        var next = browser.TryNext();
        Assert.That(next, Is.True);
        var read = browser.TryReadId(out var id);
        Assert.That(read, Is.True);
        Assert.That(id, Is.EqualTo("CM_"));

        var repository = new MockRepository(MockBehavior.Strict);
        var storeMock = repository.Create<IKeywordStore>();

        var commentKeywordParser = new CommentKeywordParser();
        var commentMessageParser = commentKeywordParser.TryParse(browser, storeMock.Object);
        var nothing = commentMessageParser.TryParse(browser, storeMock.Object);

        Assert.That(commentMessageParser, Is.InstanceOf<MessageCommentKeywordParser>());
        Assert.That(nothing, Is.Null);
    }

    [Test]
    public void MessageMultilineCommentTest()
    {
        var lines = new List<string>()
        {
            "CM_ BO_ 458458 \"This is a comment", 
            "we want to keep;", 
            "Even with a semicolon at the end of the line!\";"
        };
        var browser = new Parsers.Evo.TextBrowser(new ArrayBasedLineProvider(lines));

        var next = browser.TryNext();
        Assert.That(next, Is.True);
        var read = browser.TryReadId(out var id);
        Assert.That(read, Is.True);
        Assert.That(id, Is.EqualTo("CM_"));

        var repository = new MockRepository(MockBehavior.Strict);
        var storeMock = repository.Create<IKeywordStore>();

        var commentKeywordParser = new CommentKeywordParser();
        var commentMessageParser = commentKeywordParser.TryParse(browser, storeMock.Object);
        var nothing = commentMessageParser.TryParse(browser, storeMock.Object);

        Assert.That(commentMessageParser, Is.InstanceOf<MessageCommentKeywordParser>());
        Assert.That(nothing, Is.Null);
    }
}