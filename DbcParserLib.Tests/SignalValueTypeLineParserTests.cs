using DbcParserLib.Model;
using DbcParserLib.Observers;
using DbcParserLib.Parsers;
using Moq;
using NUnit.Framework;

namespace DbcParserLib.Tests
{
    [TestFixture]
    public class SignalValueTypeLineParserTests
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
            return new SignalValueTypeLineParser(new SilentFailureObserver());
        }

        [Test]
        public void EmptyCommentLineIsIgnored()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            var signalLineParser = CreateParser();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            Assert.IsFalse(signalLineParser.TryParse(string.Empty, dbcBuilderMock.Object, nextLineProviderMock.Object));
        }

        [Test]
        public void RandomStartIsIgnored()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            var signalLineParser = CreateParser();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            Assert.IsFalse(signalLineParser.TryParse("xfsgt_", dbcBuilderMock.Object, nextLineProviderMock.Object));
        }

        [Test]
        public void OnlyPrefixIsIgnored()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            var signalLineParser = CreateParser();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            Assert.IsFalse(signalLineParser.TryParse("SIG_VALTYPE_ ", dbcBuilderMock.Object, nextLineProviderMock.Object));
        }

        [Test]
        public void FullLineIsParsed()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            var signalLineParser = CreateParser();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            Assert.IsTrue(signalLineParser.TryParse("SIG_VALTYPE_ 32 signal 0;", dbcBuilderMock.Object, nextLineProviderMock.Object));
        }

        [Test]
        public void ValueOneCallsFloat()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            var signalLineParser = CreateParser();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            dbcBuilderMock.Setup(x => x.AddSignalValueType(45, "signal", DbcValueType.IEEEFloat));

            Assert.IsTrue(signalLineParser.TryParse("SIG_VALTYPE_ 45 signal 1;", dbcBuilderMock.Object, nextLineProviderMock.Object));
        }

        [Test]
        public void ValueTwoCallsDouble()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            var signalLineParser = CreateParser();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            dbcBuilderMock.Setup(x => x.AddSignalValueType(45, "signal", DbcValueType.IEEEDouble));

            Assert.IsTrue(signalLineParser.TryParse("SIG_VALTYPE_ 45 signal 2;", dbcBuilderMock.Object, nextLineProviderMock.Object));
        }

        [TestCase("SIG_VALTYPE_ 869 qGearboxOil 1")]
        [TestCase("SIG_VALTYPE_ -869 qGearboxOil 1;")]
        [TestCase("SIG_VALTYPE_ 869 \"qGearboxOil\" 1;")]
        [TestCase("SIG_VALTYPE_ 869 qGearboxOil 4;")]
        [TestCase("SIG_VALTYPE_ 45 signal xx;")]
        [TestCase("SIG_VALTYPE_ xx signal 1;")]
        public void SignalValueTypeSyntaxErrorIsObserved(string line)
        {
            var observerMock = m_repository.Create<IParseFailureObserver>();
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            observerMock.Setup(o => o.SignalValueTypeSyntaxError());

            var lineParser = new SignalValueTypeLineParser(observerMock.Object);
            lineParser.TryParse(line, dbcBuilderMock.Object, nextLineProviderMock.Object);
        }

        [Test]
        public void SignalValueTypeNotFoundErrorIsObserved()
        {
            uint messageId = 123;
            var signalName = "signalName";
            var line = $"SIG_VALTYPE_ {messageId} {signalName} : 1;";

            var observerMock = m_repository.Create<IParseFailureObserver>();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();
            var dbcBuilder = new DbcBuilder(observerMock.Object);

            observerMock.Setup(o => o.SignalNameNotFound(messageId, signalName));

            var lineParser = new SignalValueTypeLineParser(observerMock.Object);
            lineParser.TryParse(line, dbcBuilder, nextLineProviderMock.Object);
        }
    }
}