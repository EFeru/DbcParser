using NUnit.Framework;
using DbcParserLib.Parsers;
using DbcParserLib.Model;
using Moq;
using System.Collections.Generic;

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
            return new List<ILineParser>() {
                new EnvironmentDataVariableLineParser(),
                new EnvironmentVariableLineParser()
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
                    Assert.AreEqual("EnvKlemme45", envVariable.Name);
                    Assert.AreEqual("", envVariable.Unit);
                    Assert.AreEqual(EnvAccessibility.Unrestricted, envVariable.Access);
                    Assert.AreEqual(EnvDataType.Integer, envVariable.Type);
                    Assert.AreEqual(0, envVariable.IntegerEnvironmentVariable.Minimum);
                    Assert.AreEqual(1, envVariable.IntegerEnvironmentVariable.Maximum);
                    Assert.AreEqual(0, envVariable.IntegerEnvironmentVariable.Default);
                }
            );
            dbcBuilderMock.Setup(mock => mock.AddNodeEnvironmentVariable("ENTtest", "EnvKlemme45"));
            var environmentVariableLineParser = CreateParser();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            Assert.IsTrue(ParseLine(parsingLine, environmentVariableLineParser, dbcBuilderMock.Object, nextLineProviderMock.Object));
        }

        [Test]
        public void EnvironmentVariableAlwaysStringDefinitionLineIsParsedTest()
        {
            var parsingLine = @"EV_ EnvKlemme45: 0 [0|1] """" 0 2 DUMMY_NODE_VECTOR8000 ENTtest;";

            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            dbcBuilderMock.Setup(mock => mock.AddEnvironmentVariable("EnvKlemme45", It.IsAny<EnvironmentVariable>()))
                .Callback<string, EnvironmentVariable>((_, envVariable) =>
                {
                    Assert.AreEqual("EnvKlemme45", envVariable.Name);
                    Assert.AreEqual("", envVariable.Unit);
                    Assert.AreEqual(EnvAccessibility.Unrestricted, envVariable.Access);
                    Assert.AreEqual(EnvDataType.String, envVariable.Type);
                }
                );
            dbcBuilderMock.Setup(mock => mock.AddNodeEnvironmentVariable("ENTtest", "EnvKlemme45"));
            var environmentVariableLineParser = CreateParser();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            Assert.IsTrue(ParseLine(parsingLine, environmentVariableLineParser, dbcBuilderMock.Object, nextLineProviderMock.Object));
        }

        [Test]
        public void EnvironmentVariableFloatDefinitionLineIsParsedTest()
        {
            var parsingLine = @"EV_ EnvKlemme45: 1 [0|10] """" 5 0 DUMMY_NODE_VECTOR0 ENTtest;";

            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            dbcBuilderMock.Setup(mock => mock.AddEnvironmentVariable("EnvKlemme45", It.IsAny<EnvironmentVariable>()))
                .Callback<string, EnvironmentVariable>((_, envVariable) =>
                {
                    Assert.AreEqual("EnvKlemme45", envVariable.Name);
                    Assert.AreEqual("", envVariable.Unit);
                    Assert.AreEqual(EnvAccessibility.Unrestricted, envVariable.Access);
                    Assert.AreEqual(EnvDataType.Float, envVariable.Type);
                    Assert.AreEqual(0, envVariable.FloatEnvironmentVariable.Minimum);
                    Assert.AreEqual(10, envVariable.FloatEnvironmentVariable.Maximum);
                    Assert.AreEqual(5, envVariable.FloatEnvironmentVariable.Default);
                }
                );
            dbcBuilderMock.Setup(mock => mock.AddNodeEnvironmentVariable("ENTtest", "EnvKlemme45"));
            var environmentVariableLineParser = CreateParser();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            Assert.IsTrue(ParseLine(parsingLine, environmentVariableLineParser, dbcBuilderMock.Object, nextLineProviderMock.Object));
        }

        [Test]
        public void EnvironmentVariableFloatScientificNotationDefinitionLineIsParsedTest()
        {
            var parsingLine = @"EV_ EnvKlemme45: 1 [0|1e1] """" 5 0 DUMMY_NODE_VECTOR0 ENTtest;";

            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            dbcBuilderMock.Setup(mock => mock.AddEnvironmentVariable("EnvKlemme45", It.IsAny<EnvironmentVariable>()))
                .Callback<string, EnvironmentVariable>((_, envVariable) =>
                {
                    Assert.AreEqual("EnvKlemme45", envVariable.Name);
                    Assert.AreEqual("", envVariable.Unit);
                    Assert.AreEqual(EnvAccessibility.Unrestricted, envVariable.Access);
                    Assert.AreEqual(EnvDataType.Float, envVariable.Type);
                    Assert.AreEqual(0, envVariable.FloatEnvironmentVariable.Minimum);
                    Assert.AreEqual(10, envVariable.FloatEnvironmentVariable.Maximum);
                    Assert.AreEqual(5, envVariable.FloatEnvironmentVariable.Default);
                }
                );
            dbcBuilderMock.Setup(mock => mock.AddNodeEnvironmentVariable("ENTtest", "EnvKlemme45"));
            var environmentVariableLineParser = CreateParser();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            Assert.IsTrue(ParseLine(parsingLine, environmentVariableLineParser, dbcBuilderMock.Object, nextLineProviderMock.Object));
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

            Assert.IsTrue(ParseLine(parsingLine, environmentVariableLineParser, dbcBuilderMock.Object, nextLineProviderMock.Object));
        }

        [Test]
        public void EnvironmentDataLineIsParsedTest()
        {
            var parsingLine = @"ENVVAR_DATA_ EnvKlemme45: 5;";

            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            dbcBuilderMock.Setup(mock => mock.AddEnvironmentDataVariable("EnvKlemme45", 5));
            var environmentVariableLineParser = CreateParser();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            Assert.IsTrue(ParseLine(parsingLine, environmentVariableLineParser, dbcBuilderMock.Object, nextLineProviderMock.Object));
        }
    }
}