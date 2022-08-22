using NUnit.Framework;
using DbcParserLib.Parsers;
using Moq;

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

            Assert.IsFalse(commentLineParser.TryParse(string.Empty, dbcBuilderMock.Object));
        }

        [Test]
        public void RandomStartIsIgnored()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            var commentLineParser = CreateParser();

            Assert.IsFalse(commentLineParser.TryParse("CF_", dbcBuilderMock.Object));
        }

        [Test]
        public void OnlyPrefixIsIgnored()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            var commentLineParser = CreateParser();

            Assert.IsFalse(commentLineParser.TryParse("BO_ ", dbcBuilderMock.Object));
        }

        [Test]
        public void MalformedLineIsAcceptedWithoutInteraction()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            var commentLineParser = CreateParser();

            Assert.IsTrue(commentLineParser.TryParse("BO_ 234 xxx", dbcBuilderMock.Object));
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

            Assert.IsTrue(commentLineParser.TryParse(@"BO_ 1041 DOORS_SEATBELTS: 8 TRX", dbcBuilderMock.Object));
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

            Assert.IsTrue(commentLineParser.TryParse(@"BO_ 1041    DOORS_SEATBELTS  :    8  TRX   ", dbcBuilderMock.Object));
        }
    }
}