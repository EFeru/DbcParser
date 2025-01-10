using NUnit.Framework;
using DbcParserLib.Parsers;
using Moq;
using System.Collections.Generic;
using DbcParserLib.Observers;

namespace DbcParserLib.Tests
{
    [TestFixture]
    public class CommentLineParserTests
    {
        private MockRepository m_repository;
        
        private ILineParser m_commentLineParser;
        private Mock<IDbcBuilder> m_dbcBuilderMock;
        private Mock<INextLineProvider> m_nextLineProviderMock;

        [SetUp]
        public void Setup()
        {
            m_commentLineParser = new CommentLineParser(new SilentFailureObserver());
            m_repository = new MockRepository(MockBehavior.Strict);
            m_dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            m_nextLineProviderMock = m_repository.Create<INextLineProvider>();
        }

        [TearDown]
        public void Teardown()
        {
            m_repository.VerifyAll();
        }

        [Test]
        public void EmptyCommentLineIsIgnored()
        {
            Assert.That(m_commentLineParser.TryParse(string.Empty, m_dbcBuilderMock.Object, m_nextLineProviderMock.Object), Is.False);
        }

        [Test]
        public void RandomStartIsIgnored()
        {
            Assert.That(m_commentLineParser.TryParse("xfsgt_", m_dbcBuilderMock.Object, m_nextLineProviderMock.Object), Is.False);
        }

        [Test]
        public void OnlyPrefixIsIgnored()
        {
            Assert.That(m_commentLineParser.TryParse("CM_ ", m_dbcBuilderMock.Object, m_nextLineProviderMock.Object), Is.False);
        }

        
        [Test]
        public void FullLineIsParsed()
        {
            m_dbcBuilderMock.Setup(mock => mock.AddSignalComment(75, "channelName", "This is a description"));

            Assert.That(m_commentLineParser.TryParse(@"CM_ SG_ 75 channelName ""This is a description"";", m_dbcBuilderMock.Object, m_nextLineProviderMock.Object), Is.True);
        }

        [Test]
        public void FullLineWithSemicolonIsParsed()
        {
            m_dbcBuilderMock.Setup(mock => mock.AddSignalComment(75, "channelName", "This is a description; Right?"));

            Assert.That(m_commentLineParser.TryParse(@"CM_ SG_ 75 channelName ""This is a description; Right?"";", m_dbcBuilderMock.Object, m_nextLineProviderMock.Object), Is.True);
        }

        [Test]
        public void FullMultilineIsParsed()
        {
            var multiLineComment = new[]
            {
                "CM_ SG_ 75 channelName \"This is the first line",
                "this is the second line",
                "this is the third line\";"
            };
            var expectedText = Helpers.ConcatenateTextComment(multiLineComment, 23);
            var reader = new ArrayBasedLineProvider(multiLineComment);

            m_dbcBuilderMock.Setup(mock => mock.AddSignalComment(75, "channelName", expectedText));

            Assert.That(m_commentLineParser.TryParse(multiLineComment[0], m_dbcBuilderMock.Object, reader), Is.True);
        }

        [Test]
        public void FullMultilineWithSemicolonIsParsed()
        {
            var multiLineComment = new[]
            {
                "CM_ SG_ 75 channelName \"This is the first line",
                "this is the second line;",
                "this is the third line\";"
            };
            var expectedText = Helpers.ConcatenateTextComment(multiLineComment, 23);
            var reader = new ArrayBasedLineProvider(multiLineComment);

            m_dbcBuilderMock.Setup(mock => mock.AddSignalComment(75, "channelName", expectedText));
            Assert.That(m_commentLineParser.TryParse(multiLineComment[0], m_dbcBuilderMock.Object, reader), Is.True);
        }

        [Test]
        public void FullLineIsParsedAndRobustToWhiteSpace()
        {
            m_dbcBuilderMock.Setup(mock => mock.AddSignalComment(75, "channelName", "This is a description"));

            Assert.That(m_commentLineParser.TryParse(@"CM_ SG_ 75    channelName      ""This is a description""     ;", m_dbcBuilderMock.Object, m_nextLineProviderMock.Object), Is.True);
        }

        [Test]
        public void FullMultilineIsParsedAndRobustToWhiteSpace()
        {
            var multiLineComment = new[]
            {
                "CM_ SG_ 75 channelName \"This is the first line",
                "   this is the second line",
                "   this is the third line\";"
            };
            var expectedText = Helpers.ConcatenateTextComment(multiLineComment, 23);
            var reader = new ArrayBasedLineProvider(multiLineComment);

            m_dbcBuilderMock.Setup(mock => mock.AddSignalComment(75, "channelName", expectedText));

            Assert.That(m_commentLineParser.TryParse(multiLineComment[0],m_dbcBuilderMock.Object, reader), Is.True);
        }

        [Test]
        public void FullLineIsParsedForMessageAndRobustToWhiteSpace()
        {
            m_dbcBuilderMock.Setup(mock => mock.AddMessageComment(75, "This is a description"));

            Assert.That(m_commentLineParser.TryParse(@"CM_ BO_ 75 ""This is a description""  ;", m_dbcBuilderMock.Object, m_nextLineProviderMock.Object), Is.True);
        }

        [Test]
        public void FullMultilineIsParsedForMessageAndRobustToWhiteSpace()
        {
            var multiLineComment = new[]
            {
                "CM_ BO_ 75 \"This is the first line",
                "   this is the second line",
                "   this is the third line\";"
            };
            var expectedText = Helpers.ConcatenateTextComment(multiLineComment, 11);
            var reader = new ArrayBasedLineProvider(multiLineComment);

            m_dbcBuilderMock.Setup(mock => mock.AddMessageComment(75, expectedText));

            Assert.That(m_commentLineParser.TryParse(multiLineComment[0], m_dbcBuilderMock.Object, reader), Is.True);
        }

       [Test]
        public void FullLineIsParsedForNodeAndRobustToWhiteSpace()
        {
            m_dbcBuilderMock.Setup(mock => mock.AddNodeComment("node_name", "This is a description"));

            Assert.That(m_commentLineParser.TryParse(@"CM_ BU_ node_name ""This is a description""  ;", m_dbcBuilderMock.Object, m_nextLineProviderMock.Object), Is.True);
        }

        [Test]
        public void FullMultilineIsParsedForNodeAndRobustToWhiteSpace()
        {
            var multiLineComment = new[]
            {
                "CM_ BU_ node_name \"This is the first line",
                "   this is the second line",
                "   this is the third line\";"
            };
            var expectedText = Helpers.ConcatenateTextComment(multiLineComment, 18);
            var reader = new ArrayBasedLineProvider(multiLineComment);

            m_dbcBuilderMock.Setup(mock => mock.AddNodeComment("node_name", expectedText));

            Assert.That(m_commentLineParser.TryParse(multiLineComment[0], m_dbcBuilderMock.Object, reader), Is.True);
        }

        [TestCase("CM_ SG_ 865 \"Test with incorrect \"syntax\"\";")]
        [TestCase("CM_ BU_ NodeName \"Test with incorrect \"syntax\"\";")]
        [TestCase("CM_ BO_ 865 \"Test with incorrect \"syntax\"\";")]
        [TestCase("CM_ EV_ VarName \"Test with incorrect \"syntax\"\";")]
        [TestCase("CM_ \"Test with incorrect \"syntax\"\";")]
        [TestCase("CM_ Test with incorrect\";")]
        public void CommentSyntaxErrorIsObserved(string commentLine)
        {
            var observerMock = m_repository.Create<IParseFailureObserver>();

            observerMock.Setup(o => o.CommentSyntaxError());

            var commentParser = new CommentLineParser(observerMock.Object);
            commentParser.TryParse(commentLine, m_dbcBuilderMock.Object, m_nextLineProviderMock.Object);
        }

        [TestCase("CM_ \"Test with incorrect;")]
        [TestCase("CM_ Test with no quotes;")]
        public void CommentWithNextLineSyntaxErrorIsObserved(string commentLine)
        {
            var observerMock = m_repository.Create<IParseFailureObserver>();
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            var line = string.Empty;
            observerMock.Setup(o => o.CommentSyntaxError());
            nextLineProviderMock.Setup(n => n.TryGetLine(out line)).Returns(false);
        
            var commentParser = new CommentLineParser(observerMock.Object);
            commentParser.TryParse(commentLine, dbcBuilderMock.Object, nextLineProviderMock.Object);
        }
    }

    internal class ArrayBasedLineProvider : INextLineProvider
    {
        private readonly IList<string> m_lines;
        private int m_index;

        public ArrayBasedLineProvider(IList<string> lines)
        {
            m_lines = lines;
        }

        public bool TryGetLine(out string line)
        {
            line = null;
            if(++m_index < m_lines.Count)
            {
                line = m_lines[m_index];
                return true;
            }
            return false;
        }
    }
}