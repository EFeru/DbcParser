using NUnit.Framework;
using DbcParserLib.Parsers;
using DbcParserLib.Model;
using Moq;
using System.Collections.Generic;
using DbcParserLib.Observers;

namespace DbcParserLib.Tests
{
    [TestFixture]
    public class EnvironmentVariableLineParserTests
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
                new EnvironmentDataVariableLineParser(observer),
                new EnvironmentVariableLineParser(observer)
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
        public void EnvironmentVariableDefinitionLineIsParsedTest()
        {
            var parsingLine = @"EV_ EnvKlemme45: 0 [0|1] """" 0 2 DUMMY_NODE_VECTOR0 ENTtest;";

            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            dbcBuilderMock.Setup(mock => mock.AddEnvironmentVariable("EnvKlemme45", It.IsAny<EnvironmentVariable>()))
                .Callback<string, EnvironmentVariable>((_, envVariable) =>
                {
                    Assert.That(envVariable.Name, Is.EqualTo("EnvKlemme45"));
                    Assert.That(envVariable.Unit, Is.EqualTo(""));
                    Assert.That(envVariable.Access, Is.EqualTo(EnvAccessibility.Unrestricted));
                    Assert.That(envVariable.Type, Is.EqualTo(EnvDataType.Integer));
                    Assert.That(envVariable.IntegerEnvironmentVariable.Minimum, Is.EqualTo(0));
                    Assert.That(envVariable.IntegerEnvironmentVariable.Maximum, Is.EqualTo(1));
                    Assert.That(envVariable.IntegerEnvironmentVariable.Default, Is.EqualTo(0));
                }
            );
            dbcBuilderMock.Setup(mock => mock.AddNodeEnvironmentVariable("ENTtest", "EnvKlemme45"));
            var environmentVariableLineParser = CreateParser();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            Assert.That(ParseLine(parsingLine, environmentVariableLineParser, dbcBuilderMock.Object, nextLineProviderMock.Object), Is.True);
        }

        [Test]
        public void EnvironmentVariableAlwaysStringDefinitionLineIsParsedTest()
        {
            var parsingLine = @"EV_ EnvKlemme45: 0 [0|1] """" 0 2 DUMMY_NODE_VECTOR8000 ENTtest;";

            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            dbcBuilderMock.Setup(mock => mock.AddEnvironmentVariable("EnvKlemme45", It.IsAny<EnvironmentVariable>()))
                .Callback<string, EnvironmentVariable>((_, envVariable) =>
                {
                    Assert.That(envVariable.Name, Is.EqualTo("EnvKlemme45"));
                    Assert.That(envVariable.Unit, Is.EqualTo(""));
                    Assert.That(envVariable.Access, Is.EqualTo(EnvAccessibility.Unrestricted));
                    Assert.That(envVariable.Type, Is.EqualTo(EnvDataType.String));
                }
                );
            dbcBuilderMock.Setup(mock => mock.AddNodeEnvironmentVariable("ENTtest", "EnvKlemme45"));
            var environmentVariableLineParser = CreateParser();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            Assert.That(ParseLine(parsingLine, environmentVariableLineParser, dbcBuilderMock.Object, nextLineProviderMock.Object), Is.True);
        }

        [Test]
        public void EnvironmentVariableFloatDefinitionLineIsParsedTest()
        {
            var parsingLine = @"EV_ EnvKlemme45: 1 [0|10] """" 5 0 DUMMY_NODE_VECTOR0 ENTtest;";

            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            dbcBuilderMock.Setup(mock => mock.AddEnvironmentVariable("EnvKlemme45", It.IsAny<EnvironmentVariable>()))
                .Callback<string, EnvironmentVariable>((_, envVariable) =>
                {
                    Assert.That(envVariable.Name, Is.EqualTo("EnvKlemme45"));
                    Assert.That(envVariable.Unit, Is.EqualTo(""));
                    Assert.That(envVariable.Access, Is.EqualTo(EnvAccessibility.Unrestricted));
                    Assert.That(envVariable.Type, Is.EqualTo(EnvDataType.Float));
                    Assert.That(envVariable.FloatEnvironmentVariable.Minimum, Is.EqualTo(0));
                    Assert.That(envVariable.FloatEnvironmentVariable.Maximum, Is.EqualTo(10));
                    Assert.That(envVariable.FloatEnvironmentVariable.Default, Is.EqualTo(5));
                }
                );
            dbcBuilderMock.Setup(mock => mock.AddNodeEnvironmentVariable("ENTtest", "EnvKlemme45"));
            var environmentVariableLineParser = CreateParser();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            Assert.That(ParseLine(parsingLine, environmentVariableLineParser, dbcBuilderMock.Object, nextLineProviderMock.Object), Is.True);
        }

        [Test]
        public void EnvironmentVariableFloatScientificNotationDefinitionLineIsParsedTest()
        {
            var parsingLine = @"EV_ EnvKlemme45: 1 [0|1e1] """" 5 0 DUMMY_NODE_VECTOR0 ENTtest;";

            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            dbcBuilderMock.Setup(mock => mock.AddEnvironmentVariable("EnvKlemme45", It.IsAny<EnvironmentVariable>()))
                .Callback<string, EnvironmentVariable>((_, envVariable) =>
                {
                    Assert.That(envVariable.Name, Is.EqualTo("EnvKlemme45"));
                    Assert.That(envVariable.Unit, Is.EqualTo(""));
                    Assert.That(envVariable.Access, Is.EqualTo(EnvAccessibility.Unrestricted));
                    Assert.That(envVariable.Type, Is.EqualTo(EnvDataType.Float));
                    Assert.That(envVariable.FloatEnvironmentVariable.Minimum, Is.EqualTo(0));
                    Assert.That(envVariable.FloatEnvironmentVariable.Maximum, Is.EqualTo(10));
                    Assert.That(envVariable.FloatEnvironmentVariable.Default, Is.EqualTo(5));
                }
                );
            dbcBuilderMock.Setup(mock => mock.AddNodeEnvironmentVariable("ENTtest", "EnvKlemme45"));
            var environmentVariableLineParser = CreateParser();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            Assert.That(ParseLine(parsingLine, environmentVariableLineParser, dbcBuilderMock.Object, nextLineProviderMock.Object), Is.True);
        }

        [Test]
        public void EnvironmentVariableMultipleNodeDefinitionLineIsParsedTest()
        {
            var parsingLine = @"EV_ EnvKlemme45: 1 [0|1e1] """" 5 0 DUMMY_NODE_VECTOR0 Node1,Node2;";

            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            dbcBuilderMock.Setup(mock => mock.AddEnvironmentVariable("EnvKlemme45", It.IsAny<EnvironmentVariable>()));
            dbcBuilderMock.Setup(mock => mock.AddNodeEnvironmentVariable("Node1", "EnvKlemme45"));
            dbcBuilderMock.Setup(mock => mock.AddNodeEnvironmentVariable("Node2", "EnvKlemme45"));
            var environmentVariableLineParser = CreateParser();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            Assert.That(ParseLine(parsingLine, environmentVariableLineParser, dbcBuilderMock.Object, nextLineProviderMock.Object), Is.True);
        }

        [Test]
        public void EnvironmentDataLineIsParsedTest()
        {
            var parsingLine = @"ENVVAR_DATA_ EnvKlemme45: 5;";

            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            dbcBuilderMock.Setup(mock => mock.AddEnvironmentDataVariable("EnvKlemme45", 5));
            var environmentVariableLineParser = CreateParser();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            Assert.That(ParseLine(parsingLine, environmentVariableLineParser, dbcBuilderMock.Object, nextLineProviderMock.Object), Is.True);
        }

        [Test]
        public void EnvironmentDataSyntaxErrorIsObserved()
        {
            var line = "ENVVAR_DATA_ varName : -1024;";

            var observerMock = m_repository.Create<IParseFailureObserver>();
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            observerMock.Setup(o => o.EnvironmentDataVariableSyntaxError());

            var lineParser = new EnvironmentDataVariableLineParser(observerMock.Object);
            lineParser.TryParse(line, dbcBuilderMock.Object, nextLineProviderMock.Object);
        }

        [Test]
        public void EnvironmentDataNotFoundErrorIsObserved()
        {
            var varName = "testVar";
            uint varSize = 1024;
            var line = $"ENVVAR_DATA_ {varName} : {varSize};";
            
            var observerMock = m_repository.Create<IParseFailureObserver>();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();
            var dbcBuilder = new DbcBuilder(observerMock.Object);

            observerMock.Setup(o => o.EnvironmentVariableNameNotFound(varName));

            var lineParser = new EnvironmentDataVariableLineParser(observerMock.Object);
            lineParser.TryParse(line, dbcBuilder, nextLineProviderMock.Object);
        }
    }
}