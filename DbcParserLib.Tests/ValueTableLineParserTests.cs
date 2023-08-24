using NUnit.Framework;
using DbcParserLib.Parsers;
using Moq;
using System.Collections.Generic;
using DbcParserLib.Observers;

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
            var observer = new SilentFailureObserver();
            return new List<ILineParser>() {
                new ValueTableDefinitionLineParser(observer),
                new ValueTableLineParser(observer)
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

        [TestCase("VAL_ 869 qGearboxOil 0 \"Running\" 1 \"Idle\" ")]
        [TestCase("VAL_ 869 qGearboxOil 0 \"Running\" 1 \"Idle\";")]
        [TestCase("VAL_ -869 qGearboxOil 0 \"Running\" 1 \"Idle\" ;")]
        [TestCase("VAL_ 869 \"qGearboxOil\" 0 \"Running\" 1 \"Idle\" ;")]
        [TestCase("VAL_ 869 qGearboxOil 0 \"Running\" 1 Idle ;")]
        [TestCase("VAL_ envVarName 0 \"Running\" 1 Idle ;")]
        [TestCase("VAL_ envVarName 0 \"Running\" 1 \"Idle\" ")]
        [TestCase("VAL_ envVarName 0 \"Running\" 1.5 \"Idle\" ;")]
        public void ValueTableSyntaxErrorIsObserved(string line)
        {
            var observerMock = m_repository.Create<IParseFailureObserver>();
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            observerMock.Setup(o => o.ValueTableSyntaxError());

            var lineParser = new ValueTableLineParser(observerMock.Object);
            lineParser.TryParse(line, dbcBuilderMock.Object, nextLineProviderMock.Object);
        }

        [Test]
        public void ValueTableNameNotFoundErrorIsObserved()
        {
            var tableName = "tableName";
            var line = $"VAL_ 123 signalName {tableName};";

            var observerMock = m_repository.Create<IParseFailureObserver>();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();
            var dbcBuilder = new DbcBuilder(observerMock.Object);

            observerMock.Setup(o => o.TableMapNameNotFound(tableName));

            var lineParser = new ValueTableLineParser(observerMock.Object);
            lineParser.TryParse(line, dbcBuilder, nextLineProviderMock.Object);
        }

        [Test]
        public void ValueTableSignalNameNotFoundErrorIsObserved()
        {
            uint messageId = 123;
            var signalName = "signalName";
            var line = $"VAL_ {messageId} {signalName} 0 \"Running\" 1 \"Idle\" ;";

            var observerMock = m_repository.Create<IParseFailureObserver>();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();
            var dbcBuilder = new DbcBuilder(observerMock.Object);

            observerMock.Setup(o => o.SignalNameNotFound(messageId, signalName));

            var lineParser = new ValueTableLineParser(observerMock.Object);
            lineParser.TryParse(line, dbcBuilder, nextLineProviderMock.Object);
        }

        [Test]
        public void ValueTableEnvironmentNameNotFoundErrorIsObserved()
        {
            var envName = "envName";
            var line = $"VAL_ {envName} 0 \"Running\" 1 \"Idle\" ;";

            var observerMock = m_repository.Create<IParseFailureObserver>();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();
            var dbcBuilder = new DbcBuilder(observerMock.Object);

            observerMock.Setup(o => o.EnvironmentVariableNameNotFound(envName));

            var lineParser = new ValueTableLineParser(observerMock.Object);
            lineParser.TryParse(line, dbcBuilder, nextLineProviderMock.Object);
        }

        [TestCase("VAL_TABLE_ tableName 0 \"Running\" 1 \"Idle\";")]
        [TestCase("VAL_TABLE_ tableName 0 \"Running\" 1 \"Idle\"")]
        [TestCase("VAL_TABLE_ tableName 0 \"Running\" 1 Idle;")]
        [TestCase("VAL_TABLE_ \"tableName\" 0 \"Running\" 1 \"Idle\" ;")]
        [TestCase("VAL_TABLE_ tableName 0 \"Running\" 1.5 \"Idle\" ;")]
        public void ValueTableDefinitionSyntaxErrorIsObserved(string line)
        {
            var observerMock = m_repository.Create<IParseFailureObserver>();
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            observerMock.Setup(o => o.ValueTableDefinitionSyntaxError());

            var lineParser = new ValueTableDefinitionLineParser(observerMock.Object);
            lineParser.TryParse(line, dbcBuilderMock.Object, nextLineProviderMock.Object);
        }

        [Test]
        public void ValueTableDefinitionDuplicateErrorIsObserved()
        {
            var tableName = "tableName";
            var line = $"VAL_TABLE_ {tableName} 0 \"Running\" 1 \"Idle\" ;";

            var observerMock = m_repository.Create<IParseFailureObserver>();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();
            var dbcBuilder = new DbcBuilder(observerMock.Object);

            observerMock.Setup(o => o.DuplicateValueTableName(tableName));

            var lineParser = new ValueTableDefinitionLineParser(observerMock.Object);
            lineParser.TryParse(line, dbcBuilder, nextLineProviderMock.Object);
            lineParser.TryParse(line, dbcBuilder, nextLineProviderMock.Object);
        }
    }
}