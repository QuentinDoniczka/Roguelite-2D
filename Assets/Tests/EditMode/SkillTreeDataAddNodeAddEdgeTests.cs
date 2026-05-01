using System;
using System.Collections.Generic;
using NUnit.Framework;
using RogueliteAutoBattler.Combat.Core;
using RogueliteAutoBattler.Data;
using UnityEngine;
using UnityEngine.TestTools;
using System.Text.RegularExpressions;

namespace RogueliteAutoBattler.Tests.EditMode
{
    public class SkillTreeDataAddNodeAddEdgeTests
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
            UnityEngine.Object.DestroyImmediate(_data);
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
        public void AddNode_AppendsToList()
        {
            _data.InitializeForTest(new List<SkillTreeData.SkillNodeEntry>());
            var entry = MakeNode(0, Vector2.zero);

            _data.AddNode(entry);

            Assert.AreEqual(1, _data.Nodes.Count);
            Assert.AreEqual(0, _data.Nodes[0].id);
        }

        [Test]
        public void AddNode_ThrowsOnDuplicateId()
        {
            _data.InitializeForTest(new List<SkillTreeData.SkillNodeEntry> { MakeNode(0, Vector2.zero) });

            Assert.Throws<ArgumentException>(() => _data.AddNode(MakeNode(0, Vector2.one)));
        }

        [Test]
        public void AddEdge_AppendsToParentConnections()
        {
            var parent = MakeNode(0, new Vector2(1f, 0f));
            var child = MakeNode(1, new Vector2(3f, 0f));
            _data.InitializeForTest(new List<SkillTreeData.SkillNodeEntry> { parent, child });

            _data.AddEdge(0, 1);

            Assert.AreEqual(1, _data.Nodes[0].connectedNodeIds.Count);
            Assert.AreEqual(1, _data.Nodes[0].connectedNodeIds[0]);
        }

        [Test]
        public void AddEdge_ThrowsOnUnknownParent()
        {
            _data.InitializeForTest(new List<SkillTreeData.SkillNodeEntry> { MakeNode(0, Vector2.zero) });

            Assert.Throws<ArgumentException>(() => _data.AddEdge(99, 0));
        }

        [Test]
        public void AddEdge_ThrowsOnSelfLoop()
        {
            _data.InitializeForTest(new List<SkillTreeData.SkillNodeEntry> { MakeNode(0, Vector2.zero) });

            Assert.Throws<ArgumentException>(() => _data.AddEdge(0, 0));
        }

        [Test]
        public void CentralNodeId_IsZero()
        {
            Assert.AreEqual(0, SkillTreeData.CentralNodeId);
        }

        [Test]
        public void RemoveNode_NonCentral_RemovesAndReturnsTrue()
        {
            _data.InitializeForTest(new List<SkillTreeData.SkillNodeEntry>
            {
                MakeNode(0, Vector2.zero),
                MakeNode(5, new Vector2(1f, 0f))
            });
            int originalCount = _data.Nodes.Count;

            bool result = _data.RemoveNode(5);

            Assert.IsTrue(result);
            Assert.AreEqual(originalCount - 1, _data.Nodes.Count);
            for (int i = 0; i < _data.Nodes.Count; i++)
            {
                Assert.AreNotEqual(5, _data.Nodes[i].id);
            }
        }

        [Test]
        public void RemoveNode_Central_LogsWarning_AndReturnsFalse_AndKeepsNode()
        {
            _data.InitializeForTest(new List<SkillTreeData.SkillNodeEntry>
            {
                MakeNode(0, Vector2.zero)
            });

            LogAssert.Expect(LogType.Warning, new Regex("[Cc]entral node"));

            bool result = _data.RemoveNode(0);

            Assert.IsFalse(result);
            Assert.AreEqual(1, _data.Nodes.Count);
            Assert.AreEqual(0, _data.Nodes[0].id);
        }

        [Test]
        public void RemoveNode_UnknownId_ReturnsFalse()
        {
            _data.InitializeForTest(new List<SkillTreeData.SkillNodeEntry>
            {
                MakeNode(0, Vector2.zero)
            });

            bool result = _data.RemoveNode(99);

            Assert.IsFalse(result);
            Assert.AreEqual(1, _data.Nodes.Count);
        }

        [Test]
        public void RemoveNode_AlsoStripsIncomingEdges()
        {
            _data.InitializeForTest(new List<SkillTreeData.SkillNodeEntry>
            {
                MakeNode(0, Vector2.zero),
                MakeNode(5, new Vector2(1f, 0f)),
                MakeNode(7, new Vector2(2f, 0f))
            });
            _data.AddEdge(0, 5);
            _data.AddEdge(5, 7);

            bool result = _data.RemoveNode(5);

            Assert.IsTrue(result);
            for (int i = 0; i < _data.Nodes.Count; i++)
            {
                if (_data.Nodes[i].id == 0)
                {
                    var connections = _data.Nodes[i].connectedNodeIds;
                    if (connections != null)
                        CollectionAssert.DoesNotContain(connections, 5);
                }
            }
        }

        [Test]
        public void OnEnable_AutoInjectsCentralNode_BeforeAnyAddNodeCall()
        {
            var data = ScriptableObject.CreateInstance<SkillTreeData>();
            try
            {
                Assert.AreEqual(1, data.Nodes.Count, "OnEnable must auto-inject central node id=0.");
                Assert.AreEqual(SkillTreeData.CentralNodeId, data.Nodes[0].id);
                Assert.Throws<ArgumentException>(
                    () => data.AddNode(new SkillTreeData.SkillNodeEntry { id = 0 }),
                    "AddNode(id=0) must throw because central is already there. Use InitializeForTest to bypass.");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(data);
            }
        }

        [Test]
        public void AddBranchNode_AtomicallyValidatesBeforeMutation()
        {
            var parent = MakeNode(0, Vector2.zero);
            _data.InitializeForTest(new List<SkillTreeData.SkillNodeEntry> { parent });
            int originalCount = _data.Nodes.Count;
            int originalParentConnectionCount = _data.Nodes[0].connectedNodeIds.Count;

            var newEntry = MakeNode(5, new Vector2(1f, 0f));

            Assert.Throws<ArgumentException>(() => _data.AddBranchNode(newEntry, 999));

            Assert.AreEqual(originalCount, _data.Nodes.Count);
            Assert.AreEqual(originalParentConnectionCount, _data.Nodes[0].connectedNodeIds.Count);
            for (int i = 0; i < _data.Nodes.Count; i++)
            {
                Assert.AreNotEqual(5, _data.Nodes[i].id);
            }
        }
    }
}
