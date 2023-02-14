using System.Collections.Generic;
using System.Linq;
using DbcParserLib.Model;
using DbcParserLib.Parsers;
using Moq;
using NUnit.Framework;

namespace DbcParserLib.Tests
{
    public class DbcBuilderTests
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

        [Test]
        public void NoInteractionProduceAnEmptyDbc()
        {
            var builder = new DbcBuilder();
            var dbc = builder.Build();

            Assert.IsEmpty(dbc.Nodes);
            Assert.IsEmpty(dbc.Messages);
        }

        [Test]
        public void SingleNodeIsAdded()
        {
            var builder = new DbcBuilder();
            var node = new Node { Name = "nodeName" };
            builder.AddNode(node);

            var dbc = builder.Build();

            Assert.AreEqual(1, dbc.Nodes.Count());
            Assert.AreEqual(node, dbc.Nodes.First());
        }

        [Test]
        public void DuplicatedNodesAreSkipped()
        {
            var builder = new DbcBuilder();
            var node = new Node { Name = "nodeName" };
            var node2 = new Node { Name = "nodeName2" };
            var node3 = new Node { Name = "nodeName" };
            builder.AddNode(node);
            builder.AddNode(node2);
            builder.AddNode(node3);

            var dbc = builder.Build();

            Assert.AreEqual(2, dbc.Nodes.Count());
            Assert.AreEqual(node, dbc.Nodes.First());
            Assert.AreEqual(node2, dbc.Nodes.Skip(1).First());
        }

        [Test]
        public void NodeCommentIsAddedToNode()
        {
            var builder = new DbcBuilder();
            var node = new Node { Name = "nodeName" };
            builder.AddNode(node);
            builder.AddNodeComment("nodeName", "this is a comment");

            var dbc = builder.Build();

            Assert.AreEqual(1, dbc.Nodes.Count());
            Assert.AreEqual(node, dbc.Nodes.First());
            Assert.AreEqual("this is a comment", dbc.Nodes.First().Comment);
        }

        [Test]
        public void NodeCommentIsSkippedIfNodeIsNotFound()
        {
            var builder = new DbcBuilder();
            var node = new Node { Name = "nodeName" };
            builder.AddNode(node);
            builder.AddNodeComment("anotherNodeName", "this is a comment");

            var dbc = builder.Build();

            Assert.AreEqual(1, dbc.Nodes.Count());
            Assert.AreEqual(node, dbc.Nodes.First());
            Assert.IsNull(dbc.Nodes.First().Comment);
        }

        [Test]
        public void MessageIsAdded()
        {
            var builder = new DbcBuilder();
            var message = new Message { };
            builder.AddMessage(message);
            var dbc = builder.Build();

            Assert.IsEmpty(dbc.Nodes);
            Assert.AreEqual(1, dbc.Messages.Count());
            Assert.AreEqual(message, dbc.Messages.First());
        }

        [Test]
        public void CommentIsAddedToMessage()
        {
            var builder = new DbcBuilder();
            var message = new Message { ID = 234 };
            builder.AddMessage(message);
            builder.AddMessageComment(234, "comment");
            var dbc = builder.Build();

            Assert.IsEmpty(dbc.Nodes);
            Assert.AreEqual(1, dbc.Messages.Count());
            Assert.AreEqual(message, dbc.Messages.First());
            Assert.AreEqual("comment", dbc.Messages.First().Comment);
        }

        [Test]
        public void CommentIsNotAddedToMissingMessage()
        {
            var builder = new DbcBuilder();
            var message = new Message { ID = 234 };
            builder.AddMessage(message);
            builder.AddMessageComment(235, "comment");
            var dbc = builder.Build();

            Assert.IsEmpty(dbc.Nodes);
            Assert.AreEqual(1, dbc.Messages.Count());
            Assert.AreEqual(message, dbc.Messages.First());
            Assert.IsNull(dbc.Messages.First().Comment);
        }

        [Test]
        public void SignalIsAddedToCurrentMessage()
        {
            var builder = new DbcBuilder();
            var message1 = new Message { ID = 234 };
            builder.AddMessage(message1);

            var signal1 = new Signal { Name = "name1" };
            builder.AddSignal(signal1);

            var message2 = new Message { ID = 235 };
            builder.AddMessage(message2);

            var signal2 = new Signal { Name = "name2" };
            builder.AddSignal(signal2);

            var signal3 = new Signal { Name = "name3" };
            builder.AddSignal(signal3);

            var dbc = builder.Build();

            Assert.IsEmpty(dbc.Nodes);
            Assert.AreEqual(2, dbc.Messages.Count());

            var messagesToArray = dbc.Messages.ToArray();
            Assert.AreEqual(message1, messagesToArray[0]);
            Assert.AreEqual(1, messagesToArray[0].Signals.Count());
            Assert.AreEqual(signal1, messagesToArray[0].Signals.First());

            Assert.AreEqual(message2, messagesToArray[1]);
            Assert.AreEqual(2, messagesToArray[1].Signals.Count());
            Assert.AreEqual(signal2, messagesToArray[1].Signals.First());
            Assert.AreEqual(signal3, messagesToArray[1].Signals.Skip(1).First());
        }

        [Test]
        public void SignalIsNotAddedIfNoMessageHasBeenProvidedFirst()
        {
            var builder = new DbcBuilder();
            builder.AddSignal(new Signal { });
            var dbc = builder.Build();

            Assert.IsEmpty(dbc.Nodes);
            Assert.IsEmpty(dbc.Messages);
        }

        [Test]
        public void CommentIsAddedToSignal()
        {
            var builder = new DbcBuilder();
            var message = new Message { ID = 234 };
            builder.AddMessage(message);
            var signal = new Signal { Name = "name1" };
            builder.AddSignal(signal);

            builder.AddSignalComment(234, "name1", "comment");
            var dbc = builder.Build();

            Assert.IsEmpty(dbc.Nodes);
            Assert.AreEqual(1, dbc.Messages.Count());
            Assert.AreEqual(message, dbc.Messages.First());
            Assert.AreEqual(signal, dbc.Messages.First().Signals.First());
            Assert.AreEqual("comment", dbc.Messages.First().Signals.First().Comment);
        }

        [Test]
        public void CommentIsNotAddedToMissingSignalMessageId()
        {
            var builder = new DbcBuilder();
            var message = new Message { ID = 234 };
            builder.AddMessage(message);
            var signal = new Signal { Name = "name1" };
            builder.AddSignal(signal);

            builder.AddSignalComment(235, "name1", "comment");
            var dbc = builder.Build();

            Assert.IsEmpty(dbc.Nodes);
            Assert.AreEqual(1, dbc.Messages.Count());
            Assert.AreEqual(message, dbc.Messages.First());
            Assert.AreEqual(signal, dbc.Messages.First().Signals.First());
            Assert.IsNull(dbc.Messages.First().Signals.First().Comment);
        }

        [Test]
        public void CommentIsNotAddedToMissingSignalName()
        {
            var builder = new DbcBuilder();
            var message = new Message { ID = 234 };
            builder.AddMessage(message);
            var signal = new Signal { Name = "name1" };
            builder.AddSignal(signal);

            builder.AddSignalComment(234, "name2", "comment");
            var dbc = builder.Build();

            Assert.IsEmpty(dbc.Nodes);
            Assert.AreEqual(1, dbc.Messages.Count());
            Assert.AreEqual(message, dbc.Messages.First());
            Assert.AreEqual(signal, dbc.Messages.First().Signals.First());
            Assert.IsNull(dbc.Messages.First().Signals.First().Comment);
        }

        [Test]
        public void TableValuesAreAddedToSignal()
        {
            var builder = new DbcBuilder();
            var message = new Message { ID = 234 };
            builder.AddMessage(message);
            var signal = new Signal { Name = "name1" };
            builder.AddSignal(signal);
            var testValuesDict = new Dictionary<int, string>() { { 1, "fake" } };

            builder.LinkTableValuesToSignal(234, "name1", testValuesDict, "1 fake");
            var dbc = builder.Build();

            Assert.IsEmpty(dbc.Nodes);
            Assert.AreEqual(1, dbc.Messages.Count());
            Assert.AreEqual(message, dbc.Messages.First());
            Assert.AreEqual(signal, dbc.Messages.First().Signals.First());
            Assert.AreEqual(testValuesDict, dbc.Messages.First().Signals.First().ValueTableDict);
            Assert.AreEqual("1 fake", dbc.Messages.First().Signals.First().ValueTable);
        }

        [Test]
        public void TableValuesWithExtendedMessageIdAreAddedToSignal()
        {
            var builder = new DbcBuilder();
            var message = new Message { ID = 2566896411 };
            message.IsExtID = DbcBuilder.IsExtID(ref message.ID);
            builder.AddMessage(message);
            var signal = new Signal { Name = "name1" };
            builder.AddSignal(signal);
            var testValuesDict = new Dictionary<int, string>() { { 1, "fake" } };

            builder.LinkTableValuesToSignal(2566896411, "name1", testValuesDict, "1 fake");
            var dbc = builder.Build();

            Assert.IsEmpty(dbc.Nodes);
            Assert.AreEqual(1, dbc.Messages.Count());
            Assert.AreEqual(message, dbc.Messages.First());
            Assert.AreEqual(signal, dbc.Messages.First().Signals.First());
            Assert.AreEqual(testValuesDict, dbc.Messages.First().Signals.First().ValueTableDict);
            Assert.AreEqual("1 fake", dbc.Messages.First().Signals.First().ValueTable);
        }

        [Test]
        public void TableValueIsNotAddedToMissingSignalMessageId()
        {
            var builder = new DbcBuilder();
            var message = new Message { ID = 234 };
            builder.AddMessage(message);
            var signal = new Signal { Name = "name1" };
            builder.AddSignal(signal);
            var testValuesDict = new Dictionary<int, string>() { { 1, "fake" } };

            builder.LinkTableValuesToSignal(235, "name1", testValuesDict, "1 fake");
            var dbc = builder.Build();

            Assert.IsEmpty(dbc.Nodes);
            Assert.AreEqual(1, dbc.Messages.Count());
            Assert.AreEqual(message, dbc.Messages.First());
            Assert.AreEqual(signal, dbc.Messages.First().Signals.First());
            Assert.IsNull(dbc.Messages.First().Signals.First().ValueTableDict);
            Assert.IsNull(dbc.Messages.First().Signals.First().ValueTable);
        }

        [Test]
        public void TableValueIsNotAddedToMissingSignalName()
        {
            var builder = new DbcBuilder();
            var message = new Message { ID = 234 };
            builder.AddMessage(message);
            var signal = new Signal { Name = "name1" };
            builder.AddSignal(signal);
            var testValuesDict = new Dictionary<int, string>() { { 1, "fake" } };

            builder.LinkTableValuesToSignal(234, "name2", testValuesDict, "1 fake");
            var dbc = builder.Build();

            Assert.IsEmpty(dbc.Nodes);
            Assert.AreEqual(1, dbc.Messages.Count());
            Assert.AreEqual(message, dbc.Messages.First());
            Assert.AreEqual(signal, dbc.Messages.First().Signals.First());
            Assert.IsNull(dbc.Messages.First().Signals.First().ValueTableDict);
            Assert.IsNull(dbc.Messages.First().Signals.First().ValueTable);
        }

        [Test]
        public void NamedTableValuesAreAddedToSignal()
        {
            var builder = new DbcBuilder();
            var message = new Message { ID = 234 };
            builder.AddMessage(message);
            var signal = new Signal { Name = "name1" };
            builder.AddSignal(signal);
            var testValuesDict = new Dictionary<int, string>() { { 1, "fake" } };

            builder.AddNamedValueTable("aTableName", testValuesDict, "1 fake");

            builder.LinkNamedTableToSignal(234, "name1", "aTableName");
            var dbc = builder.Build();

            Assert.IsEmpty(dbc.Nodes);
            Assert.AreEqual(1, dbc.Messages.Count());
            Assert.AreEqual(message, dbc.Messages.First());
            Assert.AreEqual(signal, dbc.Messages.First().Signals.First());
            Assert.AreEqual(testValuesDict, dbc.Messages.First().Signals.First().ValueTableDict);
            Assert.AreEqual("1 fake", dbc.Messages.First().Signals.First().ValueTable);
        }

        [Test]
        public void NamedTableValueIsNotAddedToMissingSignalMessageId()
        {
            var builder = new DbcBuilder();
            var message = new Message { ID = 234 };
            builder.AddMessage(message);
            var signal = new Signal { Name = "name1" };
            builder.AddSignal(signal);
            var testValuesDict = new Dictionary<int, string>() { { 1, "fake" } };

            builder.AddNamedValueTable("aTableName", testValuesDict, "1 fake");

            builder.LinkNamedTableToSignal(235, "name1", "aTableName");
            var dbc = builder.Build();

            Assert.IsEmpty(dbc.Nodes);
            Assert.AreEqual(1, dbc.Messages.Count());
            Assert.AreEqual(message, dbc.Messages.First());
            Assert.AreEqual(signal, dbc.Messages.First().Signals.First());
            Assert.IsNull(dbc.Messages.First().Signals.First().ValueTableDict);
            Assert.IsNull(dbc.Messages.First().Signals.First().ValueTable);
        }

        [Test]
        public void NamedTableValueIsNotAddedToMissingSignalName()
        {
            var builder = new DbcBuilder();
            var message = new Message { ID = 234 };
            builder.AddMessage(message);
            var signal = new Signal { Name = "name1" };
            builder.AddSignal(signal);
            var testValuesDict = new Dictionary<int, string>() { { 1, "fake" } };

            builder.AddNamedValueTable("aTableName", testValuesDict, "1 fake");

            builder.LinkNamedTableToSignal(234, "name2", "aTableName");
            var dbc = builder.Build();

            Assert.IsEmpty(dbc.Nodes);
            Assert.AreEqual(1, dbc.Messages.Count());
            Assert.AreEqual(message, dbc.Messages.First());
            Assert.AreEqual(signal, dbc.Messages.First().Signals.First());
            Assert.IsNull(dbc.Messages.First().Signals.First().ValueTableDict);
            Assert.IsNull(dbc.Messages.First().Signals.First().ValueTable);
        }

        [Test]
        public void NamedTableValueIsNotAddedIfTableNameDoesNotExist()
        {
            var builder = new DbcBuilder();
            var message = new Message { ID = 234 };
            builder.AddMessage(message);
            var signal = new Signal { Name = "name1" };
            builder.AddSignal(signal);

            builder.LinkNamedTableToSignal(234, "name1", "aTableName");
            var dbc = builder.Build();

            Assert.IsEmpty(dbc.Nodes);
            Assert.AreEqual(1, dbc.Messages.Count());
            Assert.AreEqual(message, dbc.Messages.First());
            Assert.AreEqual(signal, dbc.Messages.First().Signals.First());
            Assert.IsNull(dbc.Messages.First().Signals.First().ValueTable);
        }

        [Test]
        public void NamedTableValueThatAreNotUsedDoNotHarmOnBuild()
        {
            var testValuesDict = new Dictionary<int, string>() { { 1, "fake" } };
            var builder = new DbcBuilder();

            builder.AddNamedValueTable("aTableName", testValuesDict, "1 fake");
            builder.AddNamedValueTable("aTableName2", testValuesDict, "1 fake");
            builder.AddNamedValueTable("aTableName3", testValuesDict, "1 fake");
            var dbc = builder.Build();

            Assert.IsEmpty(dbc.Nodes);
            Assert.IsEmpty(dbc.Messages);
        }

        [Test]
        public void NamedTablesWithSameNameAreManaged()
        {
            var testValuesDict = new Dictionary<int, string>() { { 1, "fake" } };
            var builder = new DbcBuilder();

            builder.AddNamedValueTable("aTableName", testValuesDict, "1 fake");
            builder.AddNamedValueTable("aTableName", testValuesDict, "1 fake");
            builder.AddNamedValueTable("aTableName", testValuesDict, "1 fake");
            var dbc = builder.Build();

            Assert.IsEmpty(dbc.Nodes);
            Assert.IsEmpty(dbc.Messages);
        }

        [Test]
        public void NamedTablesWithSameNameOverridesPrevious()
        {
            var builder = new DbcBuilder();
            var message = new Message { ID = 234 };
            builder.AddMessage(message);
            var signal = new Signal { Name = "name1" };
            builder.AddSignal(signal);
            var testValuesDict = new Dictionary<int, string>() { { 1, "fake1" } };
            var testValuesDict2 = new Dictionary<int, string>() { { 2, "fake2" } };

            builder.AddNamedValueTable("aTableName", testValuesDict, "1 fake1");
            builder.AddNamedValueTable("aTableName", testValuesDict2, "2 fake2");

            builder.LinkNamedTableToSignal(234, "name1", "aTableName");
            var dbc = builder.Build();

            Assert.IsEmpty(dbc.Nodes);
            Assert.AreEqual(1, dbc.Messages.Count());
            Assert.AreEqual(message, dbc.Messages.First());
            Assert.AreEqual(signal, dbc.Messages.First().Signals.First());
            Assert.AreEqual(testValuesDict2, dbc.Messages.First().Signals.First().ValueTableDict);
            Assert.AreEqual("2 fake2", dbc.Messages.First().Signals.First().ValueTable);
        }
    }
}