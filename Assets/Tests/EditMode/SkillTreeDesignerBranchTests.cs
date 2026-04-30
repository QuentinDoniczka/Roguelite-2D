using System.Collections.Generic;
using NUnit.Framework;
using RogueliteAutoBattler.Combat.Core;
using RogueliteAutoBattler.Data;
using RogueliteAutoBattler.Editor.Tools;
using UnityEngine;

namespace RogueliteAutoBattler.Tests.EditMode
{
    public class SkillTreeDesignerBranchTests
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
                costType = SkillTreeData.CostType.Gold,
                maxLevel = 5,
                baseCost = 1,
                costMultiplierOdd = 1f,
                costMultiplierEven = 1f,
                costAdditivePerLevel = 0,
                statModifierType = StatType.Hp,
                statModifierMode = SkillTreeData.StatModifierMode.Flat,
                statModifierValuePerLevel = 5f
            };
        }

        private static int ComputeNextNodeId(IReadOnlyList<SkillTreeData.SkillNodeEntry> nodes)
        {
            if (nodes.Count == 0) return 0;
            int max = nodes[0].id;
            for (int i = 1; i < nodes.Count; i++)
            {
                if (nodes[i].id > max)
                    max = nodes[i].id;
            }
            return max + 1;
        }

        [Test]
        public void Generate_AddsNodeAtComputedPosition()
        {
            var parent = MakeNode(0, new Vector2(3f, 0f));
            _data.InitializeForTest(new List<SkillTreeData.SkillNodeEntry> { parent });

            float distance = 2f;
            Vector2 expectedPos = BranchPlacement.ComputeBranchPosition(parent.position, distance);
            int newId = ComputeNextNodeId(_data.Nodes);
            var newEntry = SkillTreeNodeFactory.CreateBranchNode(newId, expectedPos);
            _data.AddNode(newEntry);

            Assert.AreEqual(1, newId);
            Assert.That(_data.Nodes[1].position.x, Is.EqualTo(5f).Within(0.001f));
            Assert.That(_data.Nodes[1].position.y, Is.EqualTo(0f).Within(0.001f));
        }

        [Test]
        public void Generate_AddsEdgeFromParentToNewNode()
        {
            var parent = MakeNode(0, new Vector2(3f, 0f));
            _data.InitializeForTest(new List<SkillTreeData.SkillNodeEntry> { parent });

            Vector2 newPos = BranchPlacement.ComputeBranchPosition(parent.position, 2f);
            int newId = ComputeNextNodeId(_data.Nodes);
            var newEntry = SkillTreeNodeFactory.CreateBranchNode(newId, newPos);
            _data.AddNode(newEntry);
            _data.AddEdge(parent.id, newId);

            Assert.AreEqual(1, _data.Nodes[0].connectedNodeIds.Count);
            Assert.AreEqual(newId, _data.Nodes[0].connectedNodeIds[0]);
        }

        [Test]
        public void Generate_AssignsNextAvailableId()
        {
            var existing = new List<SkillTreeData.SkillNodeEntry>
            {
                MakeNode(0, new Vector2(1f, 0f)),
                MakeNode(1, new Vector2(2f, 0f)),
                MakeNode(2, new Vector2(3f, 0f)),
                MakeNode(5, new Vector2(4f, 0f))
            };
            _data.InitializeForTest(existing);

            int newId = ComputeNextNodeId(_data.Nodes);

            Assert.AreEqual(6, newId);
        }

        [Test]
        public void Generate_AppliesV1Defaults()
        {
            var parent = MakeNode(0, new Vector2(2f, 0f));
            _data.InitializeForTest(new List<SkillTreeData.SkillNodeEntry> { parent });

            Vector2 newPos = BranchPlacement.ComputeBranchPosition(parent.position, 2f);
            int newId = ComputeNextNodeId(_data.Nodes);
            var newEntry = SkillTreeNodeFactory.CreateBranchNode(newId, newPos);
            _data.AddNode(newEntry);

            var created = _data.Nodes[1];
            Assert.AreEqual(1, created.maxLevel);
            Assert.AreEqual(SkillTreeData.CostType.SkillPoint, created.costType);
            Assert.AreEqual(1, created.baseCost);
            Assert.AreEqual(StatType.Hp, created.statModifierType);
            Assert.AreEqual(SkillTreeData.StatModifierMode.Flat, created.statModifierMode);
            Assert.That(created.statModifierValuePerLevel, Is.EqualTo(5f).Within(0.001f));
        }
    }
}
