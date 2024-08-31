using NUnit.Framework;
using DbcParserLib.Parsers;
using Moq;
using System.Collections.Generic;
using DbcParserLib.Observers;
using System.IO;

namespace DbcParserLib.Tests
{
    [TestFixture]
    public class CommentLineParserTests
    {
        private MockRepository m_repository;

        [SetUp]
        public void Setup()
        {
            m_repository = new MockRepository(MockBehavior.Strict);
        }

        [TearDown]
        public void Teardown()
        {
            m_repository.VerifyAll();
        }

        private static ILineParser CreateParser()
        {
            return new CommentLineParser(new SilentFailureObserver());
        }

        [Test]
        public void EmptyCommentLineIsIgnored()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            var commentLineParser = CreateParser();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            Assert.That(commentLineParser.TryParse(string.Empty, dbcBuilderMock.Object, nextLineProviderMock.Object), Is.False);
        }

        [Test]
        public void RandomStartIsIgnored()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            var commentLineParser = CreateParser();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            Assert.That(commentLineParser.TryParse("xfsgt_", dbcBuilderMock.Object, nextLineProviderMock.Object), Is.False);
        }

        [Test]
        // Should parse as it is a comment but should be observed as error
        // This however would be catched previously by the IgnoreLineParser
        public void OnlyPrefixIsIgnored()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();

            var counter = 0;
            var failureObserverMock = new Mock<IParseFailureObserver>();
            failureObserverMock
                .Setup(observer => observer.CommentSyntaxError())
                .Callback(() => counter++);

            var commentLineParser = new CommentLineParser(failureObserverMock.Object);

            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            Assert.That(commentLineParser.TryParse("CM_ ", dbcBuilderMock.Object, nextLineProviderMock.Object), Is.True);
            Assert.That(counter, Is.EqualTo(1));
        }

        [Test]
        public void OnlyPrefixAndSignalIsIsAcceptedWithoutInteraction()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            var commentLineParser = CreateParser();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            Assert.That(commentLineParser.TryParse("CM_ SG_ ;", dbcBuilderMock.Object, nextLineProviderMock.Object), Is.True);
        }

        [Test]
        public void IfCanLineIsNotANumberLineIsAcceptedWithoutInteraction()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            var commentLineParser = CreateParser();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            Assert.That(commentLineParser.TryParse("CM_ SG_ xxx;", dbcBuilderMock.Object, nextLineProviderMock.Object), Is.True);
        }

        [Test]
        public void FullLineIsParsed()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            dbcBuilderMock.Setup(mock => mock.AddSignalComment(75, "channelName", "This is a description"));
            var commentLineParser = CreateParser();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            Assert.That(commentLineParser.TryParse(@"CM_ SG_ 75 channelName ""This is a description"";", dbcBuilderMock.Object, nextLineProviderMock.Object), Is.True);
        }

        [Test]
        public void FullMultilineIsParsed()
        {
            var dbcString = @"CM_ SG_ 75 channelName ""This is the first line
this is the second line
this is the third line"";";

            var expectedText = "This is the first line\r\nthis is the second line\r\nthis is the third line";

            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            dbcBuilderMock.Setup(mock => mock.AddSignalComment(75, "channelName", expectedText));
            var commentLineParser = CreateParser();

            using (var reader = new StringReader(dbcString))
            {
                var nextLineProvider = new NextLineProvider(reader, new SilentFailureObserver());
                nextLineProvider.TryGetLine(out var line);
                Assert.That(commentLineParser.TryParse(line, dbcBuilderMock.Object, nextLineProvider), Is.True);
            }
        }

        [Test]
        public void FullLineIsParsedAndRobustToWhiteSpace()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            dbcBuilderMock.Setup(mock => mock.AddSignalComment(75, "channelName", "This is a description"));
            var commentLineParser = CreateParser();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            Assert.That(commentLineParser.TryParse(@"CM_ SG_ 75    channelName      ""This is a description""     ;", dbcBuilderMock.Object, nextLineProviderMock.Object), Is.True);
        }

        [Test]
        public void FullMultilineIsParsedAndRobustToWhiteSpace()
        {
            var dbcString = @"CM_ SG_ 75 channelName ""This is the first line
   this is the second line
   this is the third line"";";

            // Spaces at linestart are always removed
            var expectedText = "This is the first line\r\nthis is the second line\r\nthis is the third line";

            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            dbcBuilderMock.Setup(mock => mock.AddSignalComment(75, "channelName", expectedText));
            var commentLineParser = CreateParser();

            using (var reader = new StringReader(dbcString))
            {
                var nextLineProvider = new NextLineProvider(reader, new SilentFailureObserver());
                nextLineProvider.TryGetLine(out var line);
                Assert.That(commentLineParser.TryParse(line, dbcBuilderMock.Object, nextLineProvider), Is.True);
            }
        }

        [Test]
        public void FullLineIsParsedForMessageAndRobustToWhiteSpace()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            dbcBuilderMock.Setup(mock => mock.AddMessageComment(75, "This is a description"));
            var commentLineParser = CreateParser();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            Assert.That(commentLineParser.TryParse(@"CM_ BO_ 75 ""This is a description""  ;", dbcBuilderMock.Object, nextLineProviderMock.Object), Is.True);
        }

        [Test]
        public void FullMultilineIsParsedForMessageAndRobustToWhiteSpace()
        {
            var dbcString = @"CM_ BO_ 75 ""This is the first line
   this is the second line
   this is the third line"";";

            // Spaces at linestart are always removed
            var expectedText = "This is the first line\r\nthis is the second line\r\nthis is the third line";

            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            dbcBuilderMock.Setup(mock => mock.AddMessageComment(75, expectedText));
            var commentLineParser = CreateParser();

            using (var reader = new StringReader(dbcString))
            {
                var nextLineProvider = new NextLineProvider(reader, new SilentFailureObserver());
                nextLineProvider.TryGetLine(out var line);
                Assert.That(commentLineParser.TryParse(line, dbcBuilderMock.Object, nextLineProvider), Is.True);
            }
        }

        [Test]
        public void IncompleteLineIsAcceptedWithoutInteraction()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            var commentLineParser = CreateParser();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            Assert.That(commentLineParser.TryParse(@"CM_ BO_ ;", dbcBuilderMock.Object, nextLineProviderMock.Object), Is.True);
        }

        [Test]
        public void IncompleteLineWithCanIdAsStringIsAcceptedWithoutInteraction()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            var commentLineParser = CreateParser();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            Assert.That(commentLineParser.TryParse(@"CM_ BO_ xxx;", dbcBuilderMock.Object, nextLineProviderMock.Object), Is.True);
        }

        [Test]
        public void FullLineIsParsedForNodeAndRobustToWhiteSpace()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            dbcBuilderMock.Setup(mock => mock.AddNodeComment("node_name", "This is a description"));
            var commentLineParser = CreateParser();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            Assert.That(commentLineParser.TryParse(@"CM_ BU_ node_name ""This is a description""  ;", dbcBuilderMock.Object, nextLineProviderMock.Object), Is.True);
        }

        [Test]
        public void FullMultilineIsParsedForNodeAndRobustToWhiteSpace()
        {
            var dbcString = @"CM_ BU_ node_name ""This is the first line
   this is the second line
   this is the third line"";";

            // Spaces at linestart are always removed
            var expectedText = "This is the first line\r\nthis is the second line\r\nthis is the third line";

            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            dbcBuilderMock.Setup(mock => mock.AddNodeComment("node_name", expectedText));
            var commentLineParser = CreateParser();

            using (var reader = new StringReader(dbcString))
            {
                var nextLineProvider = new NextLineProvider(reader, new SilentFailureObserver());
                nextLineProvider.TryGetLine(out var line);
                Assert.That(commentLineParser.TryParse(line, dbcBuilderMock.Object, nextLineProvider), Is.True);
            }
        }

        [Test]
        public void IncompleteLineForNodeIsAcceptedWithoutInteraction()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            var commentLineParser = CreateParser();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            Assert.That(commentLineParser.TryParse(@"CM_ BU_ ;", dbcBuilderMock.Object, nextLineProviderMock.Object), Is.True);
        }

        [Test]
        public void MalformedLineIsAcceptedWithoutInteraction()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            var commentLineParser = CreateParser();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            Assert.That(commentLineParser.TryParse(@"CM_ BU_ xxx;", dbcBuilderMock.Object, nextLineProviderMock.Object), Is.True);
        }

        [Test]
        public void AnotherMalformedLineIsAcceptedWithoutInteraction()
        {
            // This behaviour is a bit loose. Quotes should be required, here a regex would be more accurate
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            dbcBuilderMock.Setup(mock => mock.AddNodeComment("xxx", "no quotes"));
            var commentLineParser = CreateParser();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            Assert.That(commentLineParser.TryParse(@"CM_ BU_ xxx no quotes;", dbcBuilderMock.Object, nextLineProviderMock.Object), Is.True);
        }

        [TestCase("CM_ SG_ 865 \"Test with incorrect \"syntax\"\";")]
        [TestCase("CM_ BU_ NodeName \"Test with incorrect \"syntax\"\";")]
        [TestCase("CM_ BO_ 865 \"Test with incorrect \"syntax\"\";")]
        [TestCase("CM_ EV_ VarName \"Test with incorrect \"syntax\"\";")]
        [TestCase("CM_ \"Test with incorrect \"syntax\"\";")]
        public void CommentSyntaxErrorIsObserved(string commentLine)
        {
            var observerMock = m_repository.Create<IParseFailureObserver>();
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            observerMock.Setup(o => o.CommentSyntaxError());

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