using NUnit.Framework;
using DbcParserLib.Parsers;
using Moq;
using System.Collections.Generic;

namespace DbcParserLib.Tests
{
    public class ValueTableLineParserTests
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

        private static List<ILineParser> CreateParser()
        {
            return new List<ILineParser>() {
                new ValueTableDefinitionLineParser(),
                new ValueTableLineParser()
            };
        }

        private static bool ParseLine(string line, List<ILineParser> lineParser, IDbcBuilder builder, INextLineProvider nextLineProvider)
        {
            foreach (var parser in lineParser)
            {
                if (parser.TryParse(line, builder, nextLineProvider))
                    return true;
            }
            return false;
        }

        [Test]
        public void EmptyCommentLineIsIgnored()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            var valueTableLineParsers = CreateParser();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            Assert.IsFalse(ParseLine(string.Empty, valueTableLineParsers, dbcBuilderMock.Object, nextLineProviderMock.Object));
        }

        [Test]
        public void RandomStartIsIgnored()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            var valueTableLineParsers = CreateParser();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            Assert.IsFalse(ParseLine("CF_", valueTableLineParsers, dbcBuilderMock.Object, nextLineProviderMock.Object));
        }

        [Test]
        public void OnlyPrefixForDefinitionIsAcceptedWithNoInteractions()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            var valueTableLineParsers = CreateParser();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            Assert.IsFalse(ParseLine("VAL_TABLE_", valueTableLineParsers, dbcBuilderMock.Object, nextLineProviderMock.Object));
        }

        [Test]
        public void OnlyPrefixForDefinitionWithSpacesIsAcceptedWithNoInteractions()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            var valueTableLineParsers = CreateParser();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            Assert.IsFalse(ParseLine("VAL_TABLE_        ", valueTableLineParsers, dbcBuilderMock.Object, nextLineProviderMock.Object));
        }

        [Test]
        public void MalformedLineWithOnlyNameIsAcceptedButIgnored()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            var valueTableLineParsers = CreateParser();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            Assert.IsTrue(ParseLine("VAL_TABLE_ name       ", valueTableLineParsers, dbcBuilderMock.Object, nextLineProviderMock.Object));
        }

        [Test]
        public void OnlyPrefixIsAcceptedWithNoInteractions()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            var valueTableLineParsers = CreateParser();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            Assert.IsFalse(ParseLine("VAL_", valueTableLineParsers, dbcBuilderMock.Object, nextLineProviderMock.Object));
        }

        [Test]
        public void OnlyPrefixWithSpacesIsAcceptedWithNoInteractions()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            var valueTableLineParsers = CreateParser();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            Assert.IsFalse(ParseLine("VAL_        ", valueTableLineParsers, dbcBuilderMock.Object, nextLineProviderMock.Object));
        }

        [Test]
        public void MalformedLineWithOnlyMessageIdIsAcceptedButIgnored()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            var valueTableLineParsers = CreateParser();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            Assert.IsTrue(ParseLine("VAL_ 470       ", valueTableLineParsers, dbcBuilderMock.Object, nextLineProviderMock.Object));
        }

        [Test]
        public void MalformedLineWithInvalidMessageIdIsAcceptedButIgnored()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            var valueTableLineParsers = CreateParser();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            Assert.IsTrue(ParseLine("VAL_ xxx       ", valueTableLineParsers, dbcBuilderMock.Object, nextLineProviderMock.Object));
        }

        [Test]
        public void ValueTableDefinitionIsParsedAndCallsBuilder()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            dbcBuilderMock.Setup(builder => builder.AddNamedValueTable("DI_aebLockState",
                new Dictionary<int, string>() { { 3, @"""AEB_LOCK_STATE_SNA""" }, { 2, @"""AEB_LOCK_STATE_UNUSED""" }, { 1, @"""AEB_LOCK_STATE_UNLOCKED""" }, { 0, @"""AEB_LOCK_STATE_LOCKED""" } },
                Helpers.ConvertToMultiLine(@"3 ""AEB_LOCK_STATE_SNA"" 2 ""AEB_LOCK_STATE_UNUSED"" 1 ""AEB_LOCK_STATE_UNLOCKED"" 0 ""AEB_LOCK_STATE_LOCKED""".SplitBySpace(), 0)));
            var valueTableLineParsers = CreateParser();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            Assert.IsTrue(ParseLine(@"VAL_TABLE_ DI_aebLockState 3 ""AEB_LOCK_STATE_SNA"" 2 ""AEB_LOCK_STATE_UNUSED"" 1 ""AEB_LOCK_STATE_UNLOCKED"" 0 ""AEB_LOCK_STATE_LOCKED"" ;", valueTableLineParsers, dbcBuilderMock.Object, nextLineProviderMock.Object));
        }

        [Test]
        public void ValueTableWithTableNameIsParsedAndLinkedToChannel()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            dbcBuilderMock.Setup(builder => builder.LinkNamedTableToSignal(470, "channelName", "tableName"));
            var valueTableLineParsers = CreateParser();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            Assert.IsTrue(ParseLine(@"VAL_ 470 channelName tableName;", valueTableLineParsers, dbcBuilderMock.Object, nextLineProviderMock.Object));

        }

        [Test]
        public void ValueTableWithMapDefinitionIsParsedAndLinkedToChannel()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            dbcBuilderMock.Setup(builder => builder.LinkTableValuesToSignal(470, "channelName",
                new Dictionary<int, string>() { { 3, @"""AEB_LOCK_STATE_SNA""" }, { 2, @"""AEB_LOCK_STATE_UNUSED""" }, { 1, @"""AEB_LOCK_STATE_UNLOCKED""" }, { 0, @"""AEB_LOCK_STATE_LOCKED""" } },
                Helpers.ConvertToMultiLine(@"3 ""AEB_LOCK_STATE_SNA"" 2 ""AEB_LOCK_STATE_UNUSED"" 1 ""AEB_LOCK_STATE_UNLOCKED"" 0 ""AEB_LOCK_STATE_LOCKED""".SplitBySpace(), 0)));
            var valueTableLineParsers = CreateParser();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            Assert.IsTrue(ParseLine(@"VAL_ 470 channelName 3 ""AEB_LOCK_STATE_SNA"" 2 ""AEB_LOCK_STATE_UNUSED"" 1 ""AEB_LOCK_STATE_UNLOCKED"" 0 ""AEB_LOCK_STATE_LOCKED"" ;", valueTableLineParsers, dbcBuilderMock.Object, nextLineProviderMock.Object));

        }
    }
}