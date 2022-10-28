using NUnit.Framework;
using DbcParserLib.Parsers;
using DbcParserLib.Model;
using Moq;
using System.Collections.Generic;
using NUnit.Framework.Internal;

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
        public void OnlyPrefixForDefinitionIsAcceptedWithNoInteractions()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            var commentLineParser = CreateParser();

            Assert.IsFalse(commentLineParser.TryParse("VAL_TABLE_", dbcBuilderMock.Object));
        }

        [Test]
        public void OnlyPrefixForDefinitionWithSpacesIsAcceptedWithNoInteractions()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            var commentLineParser = CreateParser();

            Assert.IsFalse(commentLineParser.TryParse("VAL_TABLE_        ", dbcBuilderMock.Object));
        }

        [Test]
        public void MalformedLineWithOnlyNameIsAccetpedButIgnored()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            var commentLineParser = CreateParser();

            Assert.IsTrue(commentLineParser.TryParse("VAL_TABLE_ name       ", dbcBuilderMock.Object));
        }

        [Test]
        public void OnlyPrefixIsAcceptedWithNoInteractions()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            var commentLineParser = CreateParser();

            Assert.IsFalse(commentLineParser.TryParse("VAL_", dbcBuilderMock.Object));
        }

        [Test]
        public void OnlyPrefixWithSpacesIsAcceptedWithNoInteractions()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            var commentLineParser = CreateParser();

            Assert.IsFalse(commentLineParser.TryParse("VAL_        ", dbcBuilderMock.Object));
        }

        [Test]
        public void MalformedLineWithOnlyMessageIdIsAccetpedButIgnored()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            var commentLineParser = CreateParser();

            Assert.IsTrue(commentLineParser.TryParse("VAL_ 470       ", dbcBuilderMock.Object));
        }

        [Test]
        public void MalformedLineWithInvalidMessageIdIsAccetpedButIgnored()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            var commentLineParser = CreateParser();

            Assert.IsTrue(commentLineParser.TryParse("VAL_ xxx       ", dbcBuilderMock.Object));
        }

        [Test]
        public void ValueTableDefinitionIsParsedAndCallsBuilder()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            dbcBuilderMock.Setup(builder => builder.AddNamedValueTable("DI_aebLockState", Helpers.ConvertToMultiLine(@"3 ""AEB_LOCK_STATE_SNA"" 2 ""AEB_LOCK_STATE_UNUSED"" 1 ""AEB_LOCK_STATE_UNLOCKED"" 0 ""AEB_LOCK_STATE_LOCKED""".SplitBySpace(), 0)));
            var commentLineParser = CreateParser();

            Assert.IsTrue(commentLineParser.TryParse(@"VAL_TABLE_ DI_aebLockState 3 ""AEB_LOCK_STATE_SNA"" 2 ""AEB_LOCK_STATE_UNUSED"" 1 ""AEB_LOCK_STATE_UNLOCKED"" 0 ""AEB_LOCK_STATE_LOCKED"" ;", dbcBuilderMock.Object));
            
        }

        [Test]
        public void ValueTableWithTableNameIsParsedAndLinkedToChannel()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            dbcBuilderMock.Setup(builder => builder.LinkNamedTableToSignal(470, "channelName", "tableName"));
            var commentLineParser = CreateParser();

            Assert.IsTrue(commentLineParser.TryParse(@"VAL_ 470 channelName tableName;", dbcBuilderMock.Object));

        }

        [Test]
        public void ValueTableWithoutSemicolonIsParsedAndLinkedToChannel()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            dbcBuilderMock.Setup(builder => builder.LinkNamedTableToSignal(470, "channelName", "tableName"));
            var commentLineParser = CreateParser();

            Assert.IsTrue(commentLineParser.TryParse(@"VAL_ 470 channelName tableName", dbcBuilderMock.Object));

        }

        [Test]
        public void ValueTableWithMapDefinitionIsParsedAndLinkedToChannel()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            dbcBuilderMock.Setup(builder => builder.LinkTableValuesToSignal(470, "channelName", Helpers.ConvertToMultiLine(@"3 ""AEB_LOCK_STATE_SNA"" 2 ""AEB_LOCK_STATE_UNUSED"" 1 ""AEB_LOCK_STATE_UNLOCKED"" 0 ""AEB_LOCK_STATE_LOCKED""".SplitBySpace(), 0)));
            var commentLineParser = CreateParser();

            Assert.IsTrue(commentLineParser.TryParse(@"VAL_ 470 channelName 3 ""AEB_LOCK_STATE_SNA"" 2 ""AEB_LOCK_STATE_UNUSED"" 1 ""AEB_LOCK_STATE_UNLOCKED"" 0 ""AEB_LOCK_STATE_LOCKED"" ;", dbcBuilderMock.Object));

        }
    }
}