using NUnit.Framework;
using DbcParserLib.Parsers;
using Moq;
using System.Collections.Generic;

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
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            Assert.IsFalse(commentLineParser.TryParse(string.Empty, dbcBuilderMock.Object, nextLineProviderMock.Object));
        }

        [Test]
        public void RandomStartIsIgnored()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            var commentLineParser = CreateParser();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            Assert.IsFalse(commentLineParser.TryParse("xfsgt_", dbcBuilderMock.Object, nextLineProviderMock.Object));
        }

        [Test]
        public void OnlyPrefixIsIgnored()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            var commentLineParser = CreateParser();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            Assert.IsFalse(commentLineParser.TryParse("CM_ ", dbcBuilderMock.Object, nextLineProviderMock.Object));
        }

        [Test]
        public void OnlyPrefixAndSignalIsIsAcceptedWithoutInteraction()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            var commentLineParser = CreateParser();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            Assert.IsTrue(commentLineParser.TryParse("CM_ SG_ ;", dbcBuilderMock.Object, nextLineProviderMock.Object));
        }

        [Test]
        public void IfCanLineIsNotANumberLineIsAcceptedWithoutInteraction()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            var commentLineParser = CreateParser();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            Assert.IsTrue(commentLineParser.TryParse("CM_ SG_ xxx;", dbcBuilderMock.Object, nextLineProviderMock.Object));
        }

        [Test]
        public void FullLineIsParsed()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            dbcBuilderMock.Setup(mock => mock.AddSignalComment(75, "channelName", "This is a description"));
            var commentLineParser = CreateParser();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            Assert.IsTrue(commentLineParser.TryParse(@"CM_ SG_ 75 channelName ""This is a description"";", dbcBuilderMock.Object, nextLineProviderMock.Object));
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

            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            dbcBuilderMock.Setup(mock => mock.AddSignalComment(75, "channelName", expectedText));
            var commentLineParser = CreateParser();

            var reader = new ArrayBasedLineProvider(multiLineComment);
            Assert.IsTrue(commentLineParser.TryParse(multiLineComment[0], dbcBuilderMock.Object, reader));
        }

        [Test]
        public void FullLineIsParsedAndRobustToWhiteSpace()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            dbcBuilderMock.Setup(mock => mock.AddSignalComment(75, "channelName", "This is a description"));
            var commentLineParser = CreateParser();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            Assert.IsTrue(commentLineParser.TryParse(@"CM_ SG_ 75    channelName      ""This is a description""     ;", dbcBuilderMock.Object, nextLineProviderMock.Object));
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

            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            dbcBuilderMock.Setup(mock => mock.AddSignalComment(75, "channelName", expectedText));
            var commentLineParser = CreateParser();

            var reader = new ArrayBasedLineProvider(multiLineComment);
            Assert.IsTrue(commentLineParser.TryParse(multiLineComment[0], dbcBuilderMock.Object, reader));
        }

        [Test]
        public void FullLineIsParsedForMessageAndRobustToWhiteSpace()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            dbcBuilderMock.Setup(mock => mock.AddMessageComment(75, "This is a description"));
            var commentLineParser = CreateParser();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            Assert.IsTrue(commentLineParser.TryParse(@"CM_ BO_ 75 ""This is a description""  ;", dbcBuilderMock.Object, nextLineProviderMock.Object));
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

            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            dbcBuilderMock.Setup(mock => mock.AddMessageComment(75, expectedText));
            var commentLineParser = CreateParser();

            var reader = new ArrayBasedLineProvider(multiLineComment);
            Assert.IsTrue(commentLineParser.TryParse(multiLineComment[0], dbcBuilderMock.Object, reader));
        }

        [Test]
        public void IncompleteLineIsAcceptedWithoutInteraction()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            var commentLineParser = CreateParser();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            Assert.IsTrue(commentLineParser.TryParse(@"CM_ BO_ ;", dbcBuilderMock.Object, nextLineProviderMock.Object));
        }

        [Test]
        public void IncompleteLineWithCanIdAsStringIsAcceptedWithoutInteraction()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            var commentLineParser = CreateParser();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            Assert.IsTrue(commentLineParser.TryParse(@"CM_ BO_ xxx;", dbcBuilderMock.Object, nextLineProviderMock.Object));
        }

        [Test]
        public void FullLineIsParsedForNodeAndRobustToWhiteSpace()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            dbcBuilderMock.Setup(mock => mock.AddNodeComment("node_name", "This is a description"));
            var commentLineParser = CreateParser();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            Assert.IsTrue(commentLineParser.TryParse(@"CM_ BU_ node_name ""This is a description""  ;", dbcBuilderMock.Object, nextLineProviderMock.Object));
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

            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            dbcBuilderMock.Setup(mock => mock.AddNodeComment("node_name", expectedText));
            var commentLineParser = CreateParser();

            var reader = new ArrayBasedLineProvider(multiLineComment);
            Assert.IsTrue(commentLineParser.TryParse(multiLineComment[0], dbcBuilderMock.Object, reader));
        }

        [Test]
        public void IncompleteLineForNodeIsAcceptedWithoutInteraction()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            var commentLineParser = CreateParser();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            Assert.IsTrue(commentLineParser.TryParse(@"CM_ BU_ ;", dbcBuilderMock.Object, nextLineProviderMock.Object));
        }

        [Test]
        public void MalformedLineIsAcceptedWithoutInteraction()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            var commentLineParser = CreateParser();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            Assert.IsTrue(commentLineParser.TryParse(@"CM_ BU_ xxx;", dbcBuilderMock.Object, nextLineProviderMock.Object));
        }

        [Test]
        public void AnotherMalformedLineIsAcceptedWithoutInteraction()
        {
            // This behaviour is a bit loose. Quotes should be required, here a regex would be more accurate
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            dbcBuilderMock.Setup(mock => mock.AddNodeComment("xxx", "no quotes"));
            var commentLineParser = CreateParser();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            Assert.IsTrue(commentLineParser.TryParse(@"CM_ BU_ xxx no quotes;", dbcBuilderMock.Object, nextLineProviderMock.Object));
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