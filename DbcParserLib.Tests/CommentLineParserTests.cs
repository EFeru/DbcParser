using NUnit.Framework;
using DbcParserLib.Parsers;
using Moq;
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
            return new CommentLineParser();
        }

        [Test]
        public void EmptyCommentLineIsIgnored()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            var commentLineParser = CreateParser();
            var nextLineProvider = new NextLineProvider(new StringReader(string.Empty));

            Assert.IsFalse(commentLineParser.TryParse(string.Empty, dbcBuilderMock.Object, nextLineProvider));
        }

        [Test]
        public void RandomStartIsIgnored()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            var commentLineParser = CreateParser();
            var nextLineProvider = new NextLineProvider(new StringReader(string.Empty));

            Assert.IsFalse(commentLineParser.TryParse("xfsgt_", dbcBuilderMock.Object, nextLineProvider));
        }

        [Test]
        public void OnlyPrefixIsIgnored()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            var commentLineParser = CreateParser();
            var nextLineProvider = new NextLineProvider(new StringReader(string.Empty));

            Assert.IsFalse(commentLineParser.TryParse("CM_ ", dbcBuilderMock.Object, nextLineProvider));
        }

        [Test]
        public void OnlyPrefixAndSignalIsIsAcceptedWithoutInteraction()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            var commentLineParser = CreateParser();
            var nextLineProvider = new NextLineProvider(new StringReader(string.Empty));

            Assert.IsTrue(commentLineParser.TryParse("CM_ SG_ ", dbcBuilderMock.Object, nextLineProvider));
        }

        [Test]
        public void IfCanLineIsNotANumberLineIsAcceptedWithoutInteraction()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            var commentLineParser = CreateParser();
            var nextLineProvider = new NextLineProvider(new StringReader(string.Empty));

            Assert.IsTrue(commentLineParser.TryParse("CM_ SG_ xxx", dbcBuilderMock.Object, nextLineProvider));
        }

        [Test]
        public void FullLineIsParsed()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            dbcBuilderMock.Setup(mock => mock.AddSignalComment(75, "channelName", "This is a description"));
            var commentLineParser = CreateParser();
            var nextLineProvider = new NextLineProvider(new StringReader(string.Empty));

            Assert.IsTrue(commentLineParser.TryParse(@"CM_ SG_ 75 channelName ""This is a description"";", dbcBuilderMock.Object, nextLineProvider));
        }

        [Test]
        public void MultipleLineIsParsed()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            dbcBuilderMock.Setup(mock => mock.AddSignalComment(75, "channelName", "This is the first line this is the second line this is the third line"));
            var commentLineParser = CreateParser();

            string multiLineComment = @"this is the second line
this is the third line"";";

            var nextLineProvider = new NextLineProvider(new StringReader(multiLineComment));
            Assert.IsTrue(commentLineParser.TryParse(@"CM_ SG_ 75 channelName ""This is the first line", dbcBuilderMock.Object, nextLineProvider));
        }

        [Test]
        public void FullLineIsParsedAndRobustToWhiteSpace()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            dbcBuilderMock.Setup(mock => mock.AddSignalComment(75, "channelName", "This is a description"));
            var commentLineParser = CreateParser();
            var nextLineProvider = new NextLineProvider(new StringReader(string.Empty));

            Assert.IsTrue(commentLineParser.TryParse(@"CM_ SG_ 75    channelName      ""This is a description""     ;", dbcBuilderMock.Object, nextLineProvider));
        }

        [Test]
        public void FullLineIsParsedForMessageAndRobustToWhiteSpace()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            dbcBuilderMock.Setup(mock => mock.AddMessageComment(75, "This is a description"));
            var commentLineParser = CreateParser();
            var nextLineProvider = new NextLineProvider(new StringReader(string.Empty));

            Assert.IsTrue(commentLineParser.TryParse(@"CM_ BO_ 75 ""This is a description""  ;", dbcBuilderMock.Object, nextLineProvider));
        }

        [Test]
        public void IncompleteLineIsAcceptedWithoutInteraction()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            var commentLineParser = CreateParser();
            var nextLineProvider = new NextLineProvider(new StringReader(string.Empty));

            Assert.IsTrue(commentLineParser.TryParse(@"CM_ BO_ ", dbcBuilderMock.Object, nextLineProvider));
        }

        [Test]
        public void IncompleteLineWithCanIdAsStringIsAcceptedWithoutInteraction()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            var commentLineParser = CreateParser();
            var nextLineProvider = new NextLineProvider(new StringReader(string.Empty));

            Assert.IsTrue(commentLineParser.TryParse(@"CM_ BO_ xxx", dbcBuilderMock.Object, nextLineProvider));
        }

        [Test]
        public void FullLineIsParsedForNodeAndRobustToWhiteSpace()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            dbcBuilderMock.Setup(mock => mock.AddNodeComment("node_name", "This is a description"));
            var commentLineParser = CreateParser();
            var nextLineProvider = new NextLineProvider(new StringReader(string.Empty));

            Assert.IsTrue(commentLineParser.TryParse(@"CM_ BU_ node_name ""This is a description""  ;", dbcBuilderMock.Object, nextLineProvider));
        }

        [Test]
        public void IncompleteLineForNodeIsAcceptedWithoutInteraction()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            var commentLineParser = CreateParser();
            var nextLineProvider = new NextLineProvider(new StringReader(string.Empty));

            Assert.IsTrue(commentLineParser.TryParse(@"CM_ BU_ ", dbcBuilderMock.Object, nextLineProvider));
        }

        [Test]
        public void MalformedLineIsAcceptedWithoutInteraction()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            var commentLineParser = CreateParser();
            var nextLineProvider = new NextLineProvider(new StringReader(string.Empty));

            Assert.IsTrue(commentLineParser.TryParse(@"CM_ BU_ xxx", dbcBuilderMock.Object, nextLineProvider));
        }

        [Test]
        public void AnotherMalformedLineIsAcceptedWithoutInteraction()
        {
            // This behaviour is a bit loose. Quotes should be required, here a regex would be more accurate
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            dbcBuilderMock.Setup(mock => mock.AddNodeComment("xxx", "no quotes"));
            var commentLineParser = CreateParser();
            var nextLineProvider = new NextLineProvider(new StringReader(string.Empty));

            Assert.IsTrue(commentLineParser.TryParse(@"CM_ BU_ xxx no quotes", dbcBuilderMock.Object, nextLineProvider));
        }
    }
}