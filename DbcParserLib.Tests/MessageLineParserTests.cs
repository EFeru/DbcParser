using NUnit.Framework;
using DbcParserLib.Parsers;
using DbcParserLib.Model;
using Moq;
using System.IO;

namespace DbcParserLib.Tests
{
    public class MessageLineParserTests
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
            return new MessageLineParser();
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

            Assert.IsFalse(commentLineParser.TryParse("CF_", dbcBuilderMock.Object, nextLineProvider));
        }

        [Test]
        public void OnlyPrefixIsIgnored()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            var commentLineParser = CreateParser();
            var nextLineProvider = new NextLineProvider(new StringReader(string.Empty));

            Assert.IsFalse(commentLineParser.TryParse("BO_ ", dbcBuilderMock.Object, nextLineProvider));
        }

        [Test]
        public void MalformedLineIsAcceptedWithoutInteraction()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            var commentLineParser = CreateParser();
            var nextLineProvider = new NextLineProvider(new StringReader(string.Empty));

            Assert.IsTrue(commentLineParser.TryParse("BO_ 234 xxx", dbcBuilderMock.Object, nextLineProvider));
        }

        [Test]
        public void FullLineIsParsed()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            dbcBuilderMock.Setup(mock => mock.AddMessage(It.IsAny<Message>()))
                .Callback<Message>(message => 
                {
                    Assert.AreEqual(1041, message.ID);
                    Assert.AreEqual("DOORS_SEATBELTS", message.Name);
                    Assert.AreEqual(8, message.DLC);
                    Assert.AreEqual("TRX", message.Transmitter);
                    Assert.IsFalse(message.IsExtID);
                });

            var commentLineParser = CreateParser();
            var nextLineProvider = new NextLineProvider(new StringReader(string.Empty));

            Assert.IsTrue(commentLineParser.TryParse(@"BO_ 1041 DOORS_SEATBELTS: 8 TRX", dbcBuilderMock.Object, nextLineProvider));
        }

        [Test]
        public void FullLineWithSomeRamdomSpacesIsParsed()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            dbcBuilderMock.Setup(mock => mock.AddMessage(It.IsAny<Message>()))
                .Callback<Message>(message =>
                {
                    Assert.AreEqual(1041, message.ID);
                    Assert.AreEqual("DOORS_SEATBELTS", message.Name);
                    Assert.AreEqual(8, message.DLC);
                    Assert.AreEqual("TRX", message.Transmitter);
                    Assert.IsFalse(message.IsExtID);
                });

            var commentLineParser = CreateParser();
            var nextLineProvider = new NextLineProvider(new StringReader(string.Empty));

            Assert.IsTrue(commentLineParser.TryParse(@"BO_ 1041    DOORS_SEATBELTS  :    8  TRX   ", dbcBuilderMock.Object, nextLineProvider));
        }
    }
}