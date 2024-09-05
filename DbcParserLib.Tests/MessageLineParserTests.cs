using NUnit.Framework;
using DbcParserLib.Parsers;
using DbcParserLib.Model;
using DbcParserLib.Observers;
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
            return new MessageLineParser(new SilentFailureObserver());
        }

        [Test]
        public void EmptyCommentLineIsIgnored()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            var messageLineParser = CreateParser();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            Assert.That(messageLineParser.TryParse(string.Empty, dbcBuilderMock.Object, nextLineProviderMock.Object), Is.False);
        }

        [Test]
        public void RandomStartIsIgnored()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            var messageLineParser = CreateParser();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            Assert.That(messageLineParser.TryParse("CF_", dbcBuilderMock.Object, nextLineProviderMock.Object), Is.False);
        }

        [Test]
        public void OnlyPrefixIsIgnored()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            var messageLineParser = CreateParser();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            Assert.That(messageLineParser.TryParse("BO_ ", dbcBuilderMock.Object, nextLineProviderMock.Object), Is.False);
        }

        [Test]
        public void MalformedLineIsAcceptedWithoutInteraction()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            var messageLineParser = CreateParser();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            Assert.That(messageLineParser.TryParse("BO_ 234 xxx", dbcBuilderMock.Object, nextLineProviderMock.Object), Is.True);
        }

        [Test]
        public void FullLineIsParsed()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            dbcBuilderMock.Setup(mock => mock.AddMessage(It.IsAny<Message>()))
                .Callback<Message>(message => 
                {
                    Assert.Multiple(() =>
                    {
                        Assert.That(message.ID, Is.EqualTo(1041));
                        Assert.That(message.Name, Is.EqualTo("DOORS_SEATBELTS"));
                        Assert.That(message.DLC, Is.EqualTo(8));
                        Assert.That(message.Transmitter, Is.EqualTo("TRX"));
                        Assert.That(message.IsExtID, Is.False);
                    });
                });

            var messageLineParser = CreateParser();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            Assert.That(messageLineParser.TryParse(@"BO_ 1041 DOORS_SEATBELTS: 8 TRX", dbcBuilderMock.Object, nextLineProviderMock.Object), Is.True);
        }

        [Test]
        public void FullLineWithSomeRamdomSpacesIsParsed()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            dbcBuilderMock.Setup(mock => mock.AddMessage(It.IsAny<Message>()))
                .Callback<Message>(message =>
                {
                    Assert.Multiple(() =>
                    {
                        Assert.That(message.ID, Is.EqualTo(1041));
                        Assert.That(message.Name, Is.EqualTo("DOORS_SEATBELTS"));
                        Assert.That(message.DLC, Is.EqualTo(8));
                        Assert.That(message.Transmitter, Is.EqualTo("TRX"));
                        Assert.That(message.IsExtID, Is.False);
                    });
                });

            var messageLineParser = CreateParser();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            Assert.That(messageLineParser.TryParse(@"BO_ 1041    DOORS_SEATBELTS  :    8  TRX   ", dbcBuilderMock.Object, nextLineProviderMock.Object), Is.True);
        }

        [TestCase("BO_ 123 msgName : -32 transmitter")]
        [TestCase("BO_ -123 msgName : 32 transmitter")]
        [TestCase("BO_ 123 0msgName : 32 transmitter")]
        public void MessageSyntaxErrorIsObserved(string line)
        {
            var observerMock = m_repository.Create<IParseFailureObserver>();
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            observerMock.Setup(o => o.MessageSyntaxError());

            var lineParser = new MessageLineParser(observerMock.Object);
            lineParser.TryParse(line, dbcBuilderMock.Object, nextLineProviderMock.Object);
        }

        [Test]
        public void DuplicateMessageErrorIsObserved()
        {
            uint messageId = 123;
            var line = $"BO_ {messageId} msgName : 32 transmitter";

            var observerMock = m_repository.Create<IParseFailureObserver>();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();
            var dbcBuilder = new DbcBuilder(observerMock.Object);

            observerMock.Setup(o => o.DuplicatedMessage(messageId));

            var lineParser = new MessageLineParser(observerMock.Object);
            lineParser.TryParse(line, dbcBuilder, nextLineProviderMock.Object);
            lineParser.TryParse(line, dbcBuilder, nextLineProviderMock.Object);
        }
    }
}