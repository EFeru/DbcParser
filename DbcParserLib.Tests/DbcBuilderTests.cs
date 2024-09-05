using System.Collections.Generic;
using System.Linq;
using DbcParserLib.Model;
using DbcParserLib.Observers;
using Moq;
using NUnit.Framework;

namespace DbcParserLib.Tests
{
    [TestFixture]
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
            var builder = new DbcBuilder(new SilentFailureObserver());
            var dbc = builder.Build();

            Assert.Multiple(() =>
            {
                Assert.That(dbc.Nodes, Is.Empty);
                Assert.That(dbc.Messages, Is.Empty);
            });
        }

        [Test]
        public void SingleNodeIsAdded()
        {
            var builder = new DbcBuilder(new SilentFailureObserver());
            var node = new Node { Name = "nodeName" };
            builder.AddNode(node);

            var dbc = builder.Build();

            Assert.Multiple(() =>
            {
                Assert.That(dbc.Nodes.Count(), Is.EqualTo(1));
                Assert.That(dbc.Nodes.First().Name, Is.EqualTo("nodeName"));
            });
        }

        [Test]
        public void DuplicatedNodesAreSkipped()
        {
            var builder = new DbcBuilder(new SilentFailureObserver());
            var node = new Node { Name = "nodeName" };
            var node2 = new Node { Name = "nodeName2" };
            var node3 = new Node { Name = "nodeName" };
            builder.AddNode(node);
            builder.AddNode(node2);
            builder.AddNode(node3);

            var dbc = builder.Build();

            Assert.Multiple(() =>
            {
                Assert.That(dbc.Nodes.Count(), Is.EqualTo(2));
                Assert.That(dbc.Nodes.First().Name, Is.EqualTo("nodeName"));
                Assert.That(dbc.Nodes.Skip(1).First().Name, Is.EqualTo("nodeName2"));
            });
        }

        [Test]
        public void NodeCommentIsAddedToNode()
        {
            var builder = new DbcBuilder(new SilentFailureObserver());
            var node = new Node { Name = "nodeName" };
            builder.AddNode(node);
            builder.AddNodeComment("nodeName", "this is a comment");

            var dbc = builder.Build();

            Assert.Multiple(() =>
            {
                Assert.That(dbc.Nodes.Count(), Is.EqualTo(1));
                Assert.That(dbc.Nodes.First().Name, Is.EqualTo("nodeName"));
                Assert.That(dbc.Nodes.First().Comment, Is.EqualTo("this is a comment"));
            });
        }

        [Test]
        public void NodeCommentIsSkippedIfNodeIsNotFound()
        {
            var builder = new DbcBuilder(new SilentFailureObserver());
            var node = new Node { Name = "nodeName" };
            builder.AddNode(node);
            builder.AddNodeComment("anotherNodeName", "this is a comment");

            var dbc = builder.Build();

            Assert.Multiple(() =>
            {
                Assert.That(dbc.Nodes.Count(), Is.EqualTo(1));
                Assert.That(dbc.Nodes.First().Name, Is.EqualTo("nodeName"));
                Assert.That(dbc.Nodes.First().Comment, Is.Null);
            });
        }

        [Test]
        public void MessageIsAdded()
        {
            var builder = new DbcBuilder(new SilentFailureObserver());
            var message = new Message { ID = 1};
            builder.AddMessage(message);
            var dbc = builder.Build();

            Assert.Multiple(() =>
            {
                Assert.That(dbc.Nodes, Is.Empty);
                Assert.That(dbc.Messages.Count(), Is.EqualTo(1));
                Assert.That(dbc.Messages.First().ID, Is.EqualTo(1));
                Assert.That(dbc.Messages.First().IsExtID, Is.False);
            });
        }

        [Test]
        public void ExtendedMessageIsAdded()
        {
            var builder = new DbcBuilder(new SilentFailureObserver());
            var message = new Message { ID = 2147483649 };
            builder.AddMessage(message);
            var dbc = builder.Build();

            Assert.Multiple(() =>
            {
                Assert.That(dbc.Nodes, Is.Empty);
                Assert.That(dbc.Messages.Count(), Is.EqualTo(1));
                Assert.That(dbc.Messages.First().ID, Is.EqualTo(1));
                Assert.That(dbc.Messages.First().IsExtID, Is.True);
            });
        }

        [Test]
        public void CommentIsAddedToMessage()
        {
            var builder = new DbcBuilder(new SilentFailureObserver());
            var message = new Message { ID = 234 };
            builder.AddMessage(message);
            builder.AddMessageComment(234, "comment");
            var dbc = builder.Build();

            Assert.Multiple(() =>
            {
                Assert.That(dbc.Nodes, Is.Empty);
                Assert.That(dbc.Messages.Count(), Is.EqualTo(1));
                Assert.That(dbc.Messages.First().ID, Is.EqualTo(234));
                Assert.That(dbc.Messages.First().Comment, Is.EqualTo("comment"));
            });
        }

        [Test]
        public void CommentIsNotAddedToMissingMessage()
        {
            var builder = new DbcBuilder(new SilentFailureObserver());
            var message = new Message { ID = 234 };
            builder.AddMessage(message);
            builder.AddMessageComment(235, "comment");
            var dbc = builder.Build();

            Assert.Multiple(() =>
            {
                Assert.That(dbc.Nodes, Is.Empty);
                Assert.That(dbc.Messages.Count(), Is.EqualTo(1));
                Assert.That(dbc.Messages.First().ID, Is.EqualTo(234));
                Assert.That(dbc.Messages.First().Comment, Is.Null);
            });
        }

        [Test]
        public void SignalIsAddedToCurrentMessage()
        {
            var builder = new DbcBuilder(new SilentFailureObserver());
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

            Assert.Multiple(() =>
            {
                Assert.That(dbc.Nodes, Is.Empty);
                Assert.That(dbc.Messages.Count(), Is.EqualTo(2));
            });

            var messagesToArray = dbc.Messages.ToArray();
            Assert.Multiple(() =>
            {
                Assert.That(messagesToArray[0].ID, Is.EqualTo(234));
                Assert.That(messagesToArray[0].Signals.Count(), Is.EqualTo(1));
                Assert.That(messagesToArray[0].Signals.First().Name, Is.EqualTo("name1"));

                Assert.That(messagesToArray[1].ID, Is.EqualTo(235));
                Assert.That(messagesToArray[1].Signals.Count(), Is.EqualTo(2));
                Assert.That(messagesToArray[1].Signals.First().Name, Is.EqualTo("name2"));
                Assert.That(messagesToArray[1].Signals.Skip(1).First().Name, Is.EqualTo("name3"));
            });
        }

        [Test]
        public void SignalIsNotAddedIfNoMessageHasBeenProvidedFirst()
        {
            var builder = new DbcBuilder(new SilentFailureObserver());
            builder.AddSignal(new Signal { });
            var dbc = builder.Build();

            Assert.Multiple(() =>
            {
                Assert.That(dbc.Nodes, Is.Empty);
                Assert.That(dbc.Messages, Is.Empty);
            });
        }

        [Test]
        public void CommentIsAddedToSignal()
        {
            var builder = new DbcBuilder(new SilentFailureObserver());
            var message = new Message { ID = 234 };
            builder.AddMessage(message);
            var signal = new Signal { Name = "name1" };
            builder.AddSignal(signal);

            builder.AddSignalComment(234, "name1", "comment");
            var dbc = builder.Build();

            Assert.Multiple(() =>
            {
                Assert.That(dbc.Nodes, Is.Empty);
                Assert.That(dbc.Messages.Count(), Is.EqualTo(1));
                Assert.That(dbc.Messages.First().ID, Is.EqualTo(234));
                Assert.That(dbc.Messages.First().Signals.First().Name, Is.EqualTo("name1"));
                Assert.That(dbc.Messages.First().Signals.First().Comment, Is.EqualTo("comment"));
            });
        }

        [Test]
        public void CommentIsNotAddedToMissingSignalMessageId()
        {
            var builder = new DbcBuilder(new SilentFailureObserver());
            var message = new Message { ID = 234 };
            builder.AddMessage(message);
            var signal = new Signal { Name = "name1" };
            builder.AddSignal(signal);

            builder.AddSignalComment(235, "name1", "comment");
            var dbc = builder.Build();

            Assert.Multiple(() =>
            {
                Assert.That(dbc.Nodes, Is.Empty);
                Assert.That(dbc.Messages.Count(), Is.EqualTo(1));
                Assert.That(dbc.Messages.First().ID, Is.EqualTo(234));
                Assert.That(dbc.Messages.First().Signals.First().Name, Is.EqualTo("name1"));
                Assert.That(dbc.Messages.First().Signals.First().Comment, Is.Null);
            });
        }

        [Test]
        public void CommentIsNotAddedToMissingSignalName()
        {
            var builder = new DbcBuilder(new SilentFailureObserver());
            var message = new Message { ID = 234 };
            builder.AddMessage(message);
            var signal = new Signal { Name = "name1" };
            builder.AddSignal(signal);

            builder.AddSignalComment(234, "name2", "comment");
            var dbc = builder.Build();

            Assert.Multiple(() =>
            {
                Assert.That(dbc.Nodes, Is.Empty);
                Assert.That(dbc.Messages.Count(), Is.EqualTo(1));
                Assert.That(dbc.Messages.First().ID, Is.EqualTo(234));
                Assert.That(dbc.Messages.First().Signals.First().Name, Is.EqualTo("name1"));
                Assert.That(dbc.Messages.First().Signals.First().Comment, Is.Null);
            });
        }

        [Test]
        public void TableValuesAreAddedToSignal()
        {
            var builder = new DbcBuilder(new SilentFailureObserver());
            var message = new Message { ID = 234 };
            builder.AddMessage(message);
            var signal = new Signal { Name = "name1" };
            builder.AddSignal(signal);
            var testValuesDict = new Dictionary<int, string>() { { 1, "fake" } };

            builder.LinkTableValuesToSignal(234, "name1", testValuesDict);
            var dbc = builder.Build();

            Assert.Multiple(() =>
            {
                Assert.That(dbc.Nodes, Is.Empty);
                Assert.That(dbc.Messages.Count(), Is.EqualTo(1));
                Assert.That(dbc.Messages.First().ID, Is.EqualTo(234));
                Assert.That(dbc.Messages.First().Signals.First().Name, Is.EqualTo("name1"));
                Assert.That(dbc.Messages.First().Signals.First().ValueTableMap, Is.EqualTo(testValuesDict));
            });
            Assert.Multiple(() =>
            {
                Assert.That(dbc.Messages.First().Signals.First().ValueTableMap.Keys.First(), Is.EqualTo(1));
                Assert.That(dbc.Messages.First().Signals.First().ValueTableMap.Values.First(), Is.EqualTo("fake"));
            });
        }

        [Test]
        public void TableValuesWithExtendedMessageIdAreAddedToSignal()
        {
            var builder = new DbcBuilder(new SilentFailureObserver());
            var message = new Message { ID = 2566896411 };
            builder.AddMessage(message);
            var signal = new Signal { Name = "name1" };
            builder.AddSignal(signal);
            var testValuesDict = new Dictionary<int, string>() { { 1, "fake" } };

            builder.LinkTableValuesToSignal(2566896411, "name1", testValuesDict);
            var dbc = builder.Build();

            Assert.Multiple(() =>
            {
                Assert.That(dbc.Nodes, Is.Empty);
                Assert.That(dbc.Messages.Count(), Is.EqualTo(1));
                Assert.That(dbc.Messages.First().ID, Is.EqualTo(message.ID));
                Assert.That(dbc.Messages.First().Signals.First().Name, Is.EqualTo("name1"));
                Assert.That(dbc.Messages.First().Signals.First().ValueTableMap, Is.EqualTo(testValuesDict));
            });
            Assert.Multiple(() =>
            {
                Assert.That(dbc.Messages.First().Signals.First().ValueTableMap.Keys.First(), Is.EqualTo(1));
                Assert.That(dbc.Messages.First().Signals.First().ValueTableMap.Values.First(), Is.EqualTo("fake"));
            });
        }

        [Test]
        public void TableValueIsNotAddedToMissingSignalMessageId()
        {
            var builder = new DbcBuilder(new SilentFailureObserver());
            var message = new Message { ID = 234 };
            builder.AddMessage(message);
            var signal = new Signal { Name = "name1" };
            builder.AddSignal(signal);
            var testValuesDict = new Dictionary<int, string>() { { 1, "fake" } };

            builder.LinkTableValuesToSignal(235, "name1", testValuesDict);
            var dbc = builder.Build();

            Assert.Multiple(() =>
            {
                Assert.That(dbc.Nodes, Is.Empty);
                Assert.That(dbc.Messages.Count(), Is.EqualTo(1));
                Assert.That(dbc.Messages.First().ID, Is.EqualTo(234));
                Assert.That(dbc.Messages.First().Signals.First().Name, Is.EqualTo("name1"));
                Assert.That(dbc.Messages.First().Signals.First().ValueTableMap, Is.Empty);
            });
        }

        [Test]
        public void TableValueIsNotAddedToMissingSignalName()
        {
            var builder = new DbcBuilder(new SilentFailureObserver());
            var message = new Message { ID = 234 };
            builder.AddMessage(message);
            var signal = new Signal { Name = "name1" };
            builder.AddSignal(signal);
            var testValuesDict = new Dictionary<int, string>() { { 1, "fake" } };

            builder.LinkTableValuesToSignal(234, "name2", testValuesDict);
            var dbc = builder.Build();

            Assert.Multiple(() =>
            {
                Assert.That(dbc.Nodes, Is.Empty);
                Assert.That(dbc.Messages.Count(), Is.EqualTo(1));
                Assert.That(dbc.Messages.First().ID, Is.EqualTo(234));
                Assert.That(dbc.Messages.First().Signals.First().Name, Is.EqualTo("name1"));
                Assert.That(dbc.Messages.First().Signals.First().ValueTableMap, Is.Empty);
            });
        }

        [Test]
        public void NamedTableValuesAreAddedToSignal()
        {
            var builder = new DbcBuilder(new SilentFailureObserver());
            var message = new Message { ID = 234 };
            builder.AddMessage(message);
            var signal = new Signal { Name = "name1" };
            builder.AddSignal(signal);
            var testValuesDict = new Dictionary<int, string>() { { 1, "fake" } };

            builder.AddNamedValueTable("aTableName", testValuesDict);

            builder.LinkNamedTableToSignal(234, "name1", "aTableName");
            var dbc = builder.Build();

            Assert.Multiple(() =>
            {
                Assert.That(dbc.Nodes, Is.Empty);
                Assert.That(dbc.Messages.Count(), Is.EqualTo(1));
                Assert.That(dbc.Messages.First().ID, Is.EqualTo(234));
                Assert.That(dbc.Messages.First().Signals.First().Name, Is.EqualTo("name1"));
                Assert.That(dbc.Messages.First().Signals.First().ValueTableMap, Is.EqualTo(testValuesDict));
            });
            Assert.Multiple(() =>
            {
                Assert.That(dbc.Messages.First().Signals.First().ValueTableMap.Keys.First(), Is.EqualTo(1));
                Assert.That(dbc.Messages.First().Signals.First().ValueTableMap.Values.First(), Is.EqualTo("fake"));
            });
        }

        [Test]
        public void NamedTableValueIsNotAddedToMissingSignalMessageId()
        {
            var builder = new DbcBuilder(new SilentFailureObserver());
            var message = new Message { ID = 234 };
            builder.AddMessage(message);
            var signal = new Signal { Name = "name1" };
            builder.AddSignal(signal);
            var testValuesDict = new Dictionary<int, string>() { { 1, "fake" } };

            builder.AddNamedValueTable("aTableName", testValuesDict);

            builder.LinkNamedTableToSignal(235, "name1", "aTableName");
            var dbc = builder.Build();

            Assert.Multiple(() =>
            {
                Assert.That(dbc.Nodes, Is.Empty);
                Assert.That(dbc.Messages.Count(), Is.EqualTo(1));
                Assert.That(dbc.Messages.First().ID, Is.EqualTo(234));
                Assert.That(dbc.Messages.First().Signals.First().Name, Is.EqualTo("name1"));
                Assert.That(dbc.Messages.First().Signals.First().ValueTableMap, Is.Empty);
            });
        }

        [Test]
        public void NamedTableValueIsNotAddedToMissingSignalName()
        {
            var builder = new DbcBuilder(new SilentFailureObserver());
            var message = new Message { ID = 234 };
            builder.AddMessage(message);
            var signal = new Signal { Name = "name1" };
            builder.AddSignal(signal);
            var testValuesDict = new Dictionary<int, string>() { { 1, "fake" } };

            builder.AddNamedValueTable("aTableName", testValuesDict);

            builder.LinkNamedTableToSignal(234, "name2", "aTableName");
            var dbc = builder.Build();

            Assert.Multiple(() =>
            {
                Assert.That(dbc.Nodes, Is.Empty);
                Assert.That(dbc.Messages.Count(), Is.EqualTo(1));
                Assert.That(dbc.Messages.First().ID, Is.EqualTo(234));
                Assert.That(dbc.Messages.First().Signals.First().Name, Is.EqualTo("name1"));
                Assert.That(dbc.Messages.First().Signals.First().ValueTableMap, Is.Empty);
            });
        }

        [Test]
        public void NamedTableValueIsNotAddedIfTableNameDoesNotExist()
        {
            var builder = new DbcBuilder(new SilentFailureObserver());
            var message = new Message { ID = 234 };
            builder.AddMessage(message);
            var signal = new Signal { Name = "name1" };
            builder.AddSignal(signal);

            builder.LinkNamedTableToSignal(234, "name1", "aTableName");
            var dbc = builder.Build();

            Assert.Multiple(() =>
            {
                Assert.That(dbc.Nodes, Is.Empty);
                Assert.That(dbc.Messages.Count(), Is.EqualTo(1));
                Assert.That(dbc.Messages.First().ID, Is.EqualTo(234));
                Assert.That(dbc.Messages.First().Signals.First().Name, Is.EqualTo("name1"));
                Assert.That(dbc.Messages.First().Signals.First().ValueTableMap, Is.Empty);
            });
        }

        [Test]
        public void NamedTableValueThatAreNotUsedDoNotHarmOnBuild()
        {
            var testValuesDict = new Dictionary<int, string>() { { 1, "fake" } };
            var builder = new DbcBuilder(new SilentFailureObserver());

            builder.AddNamedValueTable("aTableName", testValuesDict);
            builder.AddNamedValueTable("aTableName2", testValuesDict);
            builder.AddNamedValueTable("aTableName3", testValuesDict);
            var dbc = builder.Build();

            Assert.Multiple(() =>
            {
                Assert.That(dbc.Nodes, Is.Empty);
                Assert.That(dbc.Messages, Is.Empty);
            });
        }

        [Test]
        public void NamedTablesWithSameNameAreManaged()
        {
            var testValuesDict = new Dictionary<int, string>() { { 1, "fake" } };
            var builder = new DbcBuilder(new SilentFailureObserver());

            builder.AddNamedValueTable("aTableName", testValuesDict);
            builder.AddNamedValueTable("aTableName", testValuesDict);
            builder.AddNamedValueTable("aTableName", testValuesDict);
            var dbc = builder.Build();

            Assert.Multiple(() =>
            {
                Assert.That(dbc.Nodes, Is.Empty);
                Assert.That(dbc.Messages, Is.Empty);
            });
        }
    }
}