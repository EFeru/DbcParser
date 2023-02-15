using NUnit.Framework;
using DbcParserLib.Parsers;
using DbcParserLib.Model;
using Moq;
using System.Collections.Generic;
using NUnit.Framework.Internal;
using System.IO;

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

        private static ILineParser CreateParser()
        {
            return new ValueTableLineParser();
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

            Assert.IsFalse(commentLineParser.TryParse("CF_", dbcBuilderMock.Object, nextLineProviderMock.Object));
        }

        [Test]
        public void OnlyPrefixForDefinitionIsAcceptedWithNoInteractions()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            var commentLineParser = CreateParser();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            Assert.IsFalse(commentLineParser.TryParse("VAL_TABLE_", dbcBuilderMock.Object, nextLineProviderMock.Object));
        }

        [Test]
        public void OnlyPrefixForDefinitionWithSpacesIsAcceptedWithNoInteractions()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            var commentLineParser = CreateParser();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            Assert.IsFalse(commentLineParser.TryParse("VAL_TABLE_        ", dbcBuilderMock.Object, nextLineProviderMock.Object));
        }

        [Test]
        public void MalformedLineWithOnlyNameIsAcceptedButIgnored()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            var commentLineParser = CreateParser();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            Assert.IsTrue(commentLineParser.TryParse("VAL_TABLE_ name       ", dbcBuilderMock.Object, nextLineProviderMock.Object));
        }

        [Test]
        public void OnlyPrefixIsAcceptedWithNoInteractions()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            var commentLineParser = CreateParser();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            Assert.IsFalse(commentLineParser.TryParse("VAL_", dbcBuilderMock.Object, nextLineProviderMock.Object));
        }

        [Test]
        public void OnlyPrefixWithSpacesIsAcceptedWithNoInteractions()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            var commentLineParser = CreateParser();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            Assert.IsFalse(commentLineParser.TryParse("VAL_        ", dbcBuilderMock.Object, nextLineProviderMock.Object));
        }

        [Test]
        public void MalformedLineWithOnlyMessageIdIsAcceptedButIgnored()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            var commentLineParser = CreateParser();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            Assert.IsTrue(commentLineParser.TryParse("VAL_ 470       ", dbcBuilderMock.Object, nextLineProviderMock.Object));
        }

        [Test]
        public void MalformedLineWithInvalidMessageIdIsAcceptedButIgnored()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            var commentLineParser = CreateParser();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            Assert.IsTrue(commentLineParser.TryParse("VAL_ xxx       ", dbcBuilderMock.Object, nextLineProviderMock.Object));
        }

        [Test]
        public void ValueTableDefinitionIsParsedAndCallsBuilder()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            dbcBuilderMock.Setup(builder => builder.AddNamedValueTable("DI_aebLockState",
                new Dictionary<int, string>() { { 3, @"""AEB_LOCK_STATE_SNA""" }, { 2, @"""AEB_LOCK_STATE_UNUSED""" }, { 1, @"""AEB_LOCK_STATE_UNLOCKED""" }, { 0, @"""AEB_LOCK_STATE_LOCKED""" } },
                Helpers.ConvertToMultiLine(@"3 ""AEB_LOCK_STATE_SNA"" 2 ""AEB_LOCK_STATE_UNUSED"" 1 ""AEB_LOCK_STATE_UNLOCKED"" 0 ""AEB_LOCK_STATE_LOCKED""".SplitBySpace(), 0)));
            var commentLineParser = CreateParser();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            Assert.IsTrue(commentLineParser.TryParse(@"VAL_TABLE_ DI_aebLockState 3 ""AEB_LOCK_STATE_SNA"" 2 ""AEB_LOCK_STATE_UNUSED"" 1 ""AEB_LOCK_STATE_UNLOCKED"" 0 ""AEB_LOCK_STATE_LOCKED"" ;", dbcBuilderMock.Object, nextLineProviderMock.Object));
        }

        [Test]
        public void ValueTableWithTableNameIsParsedAndLinkedToChannel()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            dbcBuilderMock.Setup(builder => builder.LinkNamedTableToSignal(470, "channelName", "tableName"));
            var commentLineParser = CreateParser();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            Assert.IsTrue(commentLineParser.TryParse(@"VAL_ 470 channelName tableName;", dbcBuilderMock.Object, nextLineProviderMock.Object));

        }

        [Test]
        public void ValueTableWithoutSemicolonIsParsedAndLinkedToChannel()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            dbcBuilderMock.Setup(builder => builder.LinkNamedTableToSignal(470, "channelName", "tableName"));
            var commentLineParser = CreateParser();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            Assert.IsTrue(commentLineParser.TryParse(@"VAL_ 470 channelName tableName", dbcBuilderMock.Object, nextLineProviderMock.Object));

        }

        [Test]
        public void ValueTableWithMapDefinitionIsParsedAndLinkedToChannel()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            dbcBuilderMock.Setup(builder => builder.LinkTableValuesToSignal(470, "channelName",
                new Dictionary<int, string>() { { 3, @"""AEB_LOCK_STATE_SNA""" }, { 2, @"""AEB_LOCK_STATE_UNUSED""" }, { 1, @"""AEB_LOCK_STATE_UNLOCKED""" }, { 0, @"""AEB_LOCK_STATE_LOCKED""" } },
                Helpers.ConvertToMultiLine(@"3 ""AEB_LOCK_STATE_SNA"" 2 ""AEB_LOCK_STATE_UNUSED"" 1 ""AEB_LOCK_STATE_UNLOCKED"" 0 ""AEB_LOCK_STATE_LOCKED""".SplitBySpace(), 0)));
            var commentLineParser = CreateParser();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            Assert.IsTrue(commentLineParser.TryParse(@"VAL_ 470 channelName 3 ""AEB_LOCK_STATE_SNA"" 2 ""AEB_LOCK_STATE_UNUSED"" 1 ""AEB_LOCK_STATE_UNLOCKED"" 0 ""AEB_LOCK_STATE_LOCKED"" ;", dbcBuilderMock.Object, nextLineProviderMock.Object));

        }
    }
}