using System;
using System.Collections.Generic;
using NUnit.Framework;
using RogueliteAutoBattler.Combat.Core;
using RogueliteAutoBattler.Data;
using UnityEngine;

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
    }
}
