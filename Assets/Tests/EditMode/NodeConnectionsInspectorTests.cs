using System.Collections.Generic;
using NUnit.Framework;
using RogueliteAutoBattler.Combat.Core;
using RogueliteAutoBattler.Data;
using RogueliteAutoBattler.Editor.Tools;
using UnityEngine;

namespace RogueliteAutoBattler.Tests.EditMode
{
    public class NodeConnectionsInspectorTests
    {
        private SkillTreeData _data;

        [SetUp]
        public void SetUp()
        {
            _data = ScriptableObject.CreateInstance<SkillTreeData>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_data);
        }

        private static SkillTreeData.SkillNodeEntry MakeNode(int id, Vector2 position)
        {
            return new SkillTreeData.SkillNodeEntry
            {
                id = id,
                position = position,
                connectedNodeIds = new List<int>(),
                costType = SkillTreeData.CostType.SkillPoint,
                maxLevel = 1,
                baseCost = 1,
                costMultiplierOdd = 1f,
                costMultiplierEven = 1f,
                costAdditivePerLevel = 0,
                statModifierType = StatType.Hp,
                statModifierMode = SkillTreeData.StatModifierMode.Flat,
                statModifierValuePerLevel = 5f
            };
        }

        [Test]
        public void CollectConnections_NullData_ReturnsEmpty()
        {
            var result = NodeConnectionsInspector.CollectConnections(null, 0);

            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void CollectConnections_OutOfRangeIndex_ReturnsEmpty()
        {
            _data.InitializeForTest(new List<SkillTreeData.SkillNodeEntry> { MakeNode(0, Vector2.zero) });

            var result = NodeConnectionsInspector.CollectConnections(_data, 5);

            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void CollectConnections_NoEdges_ReturnsEmpty()
        {
            _data.InitializeForTest(new List<SkillTreeData.SkillNodeEntry>
            {
                MakeNode(0, Vector2.zero),
                MakeNode(1, new Vector2(1f, 0f))
            });

            var result = NodeConnectionsInspector.CollectConnections(_data, 0);

            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void CollectConnections_OutgoingEdge_ReturnsOneRowWithIsOutgoingTrue()
        {
            _data.InitializeForTest(new List<SkillTreeData.SkillNodeEntry>
            {
                MakeNode(0, Vector2.zero),
                MakeNode(1, new Vector2(1f, 0f))
            });
            _data.AddEdge(0, 1);

            var result = NodeConnectionsInspector.CollectConnections(_data, 0);

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(1, result[0].OtherNodeId);
            Assert.IsTrue(result[0].IsOutgoing);
        }

        [Test]
        public void CollectConnections_IncomingEdge_ReturnsOneRowWithIsOutgoingFalse()
        {
            _data.InitializeForTest(new List<SkillTreeData.SkillNodeEntry>
            {
                MakeNode(0, Vector2.zero),
                MakeNode(1, new Vector2(1f, 0f))
            });
            _data.AddEdge(0, 1);

            var result = NodeConnectionsInspector.CollectConnections(_data, 1);

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(0, result[0].OtherNodeId);
            Assert.IsFalse(result[0].IsOutgoing);
        }

        [Test]
        public void CollectConnections_ComputesEuclideanDistance()
        {
            _data.InitializeForTest(new List<SkillTreeData.SkillNodeEntry>
            {
                MakeNode(0, new Vector2(0f, 0f)),
                MakeNode(1, new Vector2(3f, 4f))
            });
            _data.AddEdge(0, 1);

            var result = NodeConnectionsInspector.CollectConnections(_data, 0);

            Assert.AreEqual(1, result.Count);
            Assert.That(result[0].DistanceUnits, Is.EqualTo(5f).Within(0.001f));
        }

        [Test]
        public void CollectConnections_MultipleEdges_SortedByOtherNodeIdAscending()
        {
            _data.InitializeForTest(new List<SkillTreeData.SkillNodeEntry>
            {
                MakeNode(0, Vector2.zero),
                MakeNode(3, new Vector2(1f, 0f)),
                MakeNode(1, new Vector2(0f, 1f)),
                MakeNode(2, new Vector2(1f, 1f))
            });
            _data.AddEdge(0, 3);
            _data.AddEdge(0, 1);
            _data.AddEdge(0, 2);

            var result = NodeConnectionsInspector.CollectConnections(_data, 0);

            Assert.AreEqual(3, result.Count);
            Assert.AreEqual(1, result[0].OtherNodeId);
            Assert.AreEqual(2, result[1].OtherNodeId);
            Assert.AreEqual(3, result[2].OtherNodeId);
        }

        [Test]
        public void CollectConnections_AfterPositionMutation_ReflectsNewDistance()
        {
            _data.InitializeForTest(new List<SkillTreeData.SkillNodeEntry>
            {
                MakeNode(0, new Vector2(0f, 0f)),
                MakeNode(1, new Vector2(3f, 4f))
            });
            _data.AddEdge(0, 1);

            var before = NodeConnectionsInspector.CollectConnections(_data, 0);
            Assert.That(before[0].DistanceUnits, Is.EqualTo(5f).Within(0.001f));

            var mutated = _data.Nodes[1];
            mutated.position = new Vector2(0f, 10f);
            _data.SetNode(1, mutated);

            var after = NodeConnectionsInspector.CollectConnections(_data, 0);
            Assert.That(after[0].DistanceUnits, Is.EqualTo(10f).Within(0.001f));
        }

        [Test]
        public void CollectConnections_NodeWithBothIncomingAndOutgoing_ReturnsBothRows()
        {
            _data.InitializeForTest(new List<SkillTreeData.SkillNodeEntry>
            {
                MakeNode(0, Vector2.zero),
                MakeNode(1, new Vector2(1f, 0f)),
                MakeNode(2, new Vector2(2f, 0f))
            });
            _data.AddEdge(0, 1);
            _data.AddEdge(1, 2);

            var result = NodeConnectionsInspector.CollectConnections(_data, 1);

            Assert.AreEqual(2, result.Count);
            bool hasIncoming = false;
            bool hasOutgoing = false;
            foreach (var row in result)
            {
                if (row.OtherNodeId == 0 && !row.IsOutgoing) hasIncoming = true;
                if (row.OtherNodeId == 2 && row.IsOutgoing) hasOutgoing = true;
            }
            Assert.IsTrue(hasIncoming, "Expected incoming row from node 0");
            Assert.IsTrue(hasOutgoing, "Expected outgoing row to node 2");
        }
    }
}
