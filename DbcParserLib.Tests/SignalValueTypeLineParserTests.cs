using DbcParserLib.Model;
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
            return new SignalValueTypeLineParser();
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
        public void PrefixAndMessageIsIgnored()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            var signalLineParser = CreateParser();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            Assert.IsFalse(signalLineParser.TryParse("SIG_VALTYPE_ 32 ", dbcBuilderMock.Object, nextLineProviderMock.Object));
        }

        [Test]
        public void PrefixMessageAndSignalIsIgnored()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            var signalLineParser = CreateParser();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            Assert.IsFalse(signalLineParser.TryParse("SIG_VALTYPE_ 32 signal", dbcBuilderMock.Object, nextLineProviderMock.Object));
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
        public void MessageMustBeANumber()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            var signalLineParser = CreateParser();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            Assert.IsFalse(signalLineParser.TryParse("SIG_VALTYPE_ xxx signal 0", dbcBuilderMock.Object, nextLineProviderMock.Object));
        }

        [Test]
        public void ValueMustBeANumber()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            var signalLineParser = CreateParser();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            Assert.IsFalse(signalLineParser.TryParse("SIG_VALTYPE_ 45 signal xx", dbcBuilderMock.Object, nextLineProviderMock.Object));
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
    }
}