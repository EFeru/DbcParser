using NUnit.Framework;
using DbcParserLib.Parsers;
using DbcParserLib.Model;
using Moq;
using System.Collections.Generic;
using DbcParserLib.Observers;

namespace DbcParserLib.Tests
{
    public class NodeLineParserTests
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
            return new NodeLineParser(new SilentFailureObserver());
        }

        [Test]
        public void EmptyCommentLineIsIgnored()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            var nodeLineParser = CreateParser();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            Assert.IsFalse(nodeLineParser.TryParse(string.Empty, dbcBuilderMock.Object, nextLineProviderMock.Object));
        }

        [Test]
        public void RandomStartIsIgnored()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            var nodeLineParser = CreateParser();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            Assert.IsFalse(nodeLineParser.TryParse("CF_", dbcBuilderMock.Object, nextLineProviderMock.Object));
        }

        [Test]
        public void OnlyPrefixIsAcceptedWithNoInteractions()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            var nodeLineParser = CreateParser();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            Assert.IsTrue(nodeLineParser.TryParse("BU_:", dbcBuilderMock.Object, nextLineProviderMock.Object));
        }

        [Test]
        public void OnlyPrefixWithSpacesIsAcceptedWithNoInteractions()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            var nodeLineParser = CreateParser();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            Assert.IsTrue(nodeLineParser.TryParse("BU_:        ", dbcBuilderMock.Object, nextLineProviderMock.Object));
        }

        [Test]
        public void FullLineIsParsed()
        {
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            var expectations = new List<string>()
            {
                "NODE_1","NODE_2","NODE_4"
            };

            var results = new List<string>();
            dbcBuilderMock.Setup(mock => mock.AddNode(It.IsAny<Node>()))
                .Callback<Node>(node => 
                {
                    Assert.IsFalse(string.IsNullOrWhiteSpace(node.Name));
                    Assert.IsTrue(string.IsNullOrWhiteSpace(node.Comment));
                    results.Add(node.Name);
                });

            var nodeLineParser = CreateParser();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            Assert.IsTrue(nodeLineParser.TryParse(@"BU_: NODE_1 NODE_2    NODE_4   ", dbcBuilderMock.Object, nextLineProviderMock.Object));
            CollectionAssert.AreEquivalent(expectations, results);
        }

        [TestCase("BU_: 0nodeName")]
        [TestCase("BU_:nodeName")]
        public void NodeSyntaxErrorIsObserved(string line)
        {
            var observerMock = m_repository.Create<IParseFailureObserver>();
            var dbcBuilderMock = m_repository.Create<IDbcBuilder>();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();

            observerMock.Setup(o => o.NodeSyntaxError());

            var lineParser = new NodeLineParser(observerMock.Object);
            lineParser.TryParse(line, dbcBuilderMock.Object, nextLineProviderMock.Object);
        }

        [Test]
        public void DuplicateNodeErrorIsObserved()
        {
            var nodeName = "testNode";
            var line = $"BU_: {nodeName} {nodeName}";
        
            var observerMock = m_repository.Create<IParseFailureObserver>();
            var nextLineProviderMock = m_repository.Create<INextLineProvider>();
            var dbcBuilder = new DbcBuilder(observerMock.Object);
        
            observerMock.Setup(o => o.DuplicateNode(nodeName));
        
            var lineParser = new NodeLineParser(observerMock.Object);
            lineParser.TryParse(line, dbcBuilder, nextLineProviderMock.Object);
        }
    }
}