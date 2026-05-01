using System.Collections.Generic;
using NUnit.Framework;
using RogueliteAutoBattler.Combat.Core;
using RogueliteAutoBattler.Data;
using UnityEngine;

namespace RogueliteAutoBattler.Tests.EditMode
{
    public class SkillTreeDataCentralNodeTests
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

        private static SkillTreeData.SkillNodeEntry MakeCentralLikeNode(
            int baseCost,
            List<int> connectedNodeIds,
            Vector2 position)
        {
            return new SkillTreeData.SkillNodeEntry
            {
                id = SkillTreeData.CentralNodeId,
                position = position,
                connectedNodeIds = connectedNodeIds,
                costType = SkillTreeData.CostType.SkillPoint,
                maxLevel = 7,
                baseCost = baseCost,
                costMultiplierOdd = 2f,
                costMultiplierEven = 3f,
                costAdditivePerLevel = 4,
                statModifierType = StatType.Atk,
                statModifierMode = SkillTreeData.StatModifierMode.Percent,
                statModifierValuePerLevel = 9f
            };
        }

        private static int FindIndexOfId(SkillTreeData data, int id)
        {
            for (int i = 0; i < data.Nodes.Count; i++)
            {
                if (data.Nodes[i].id == id) return i;
            }
            return -1;
        }

        [Test]
        public void EnsureCentralNode_OnEmpty_CreatesNodeWithId0()
        {
            _data.InitializeForTest(new List<SkillTreeData.SkillNodeEntry>());

            _data.EnsureCentralNode();

            Assert.AreEqual(1, _data.Nodes.Count);
            Assert.AreEqual(0, _data.Nodes[0].id);
        }

        [Test]
        public void EnsureCentralNode_OverwritesBaseCost_FromCentralUnlockCost()
        {
            _data.CentralUnlockCost = 250;
            _data.InitializeForTest(new List<SkillTreeData.SkillNodeEntry>
            {
                MakeCentralLikeNode(999, new List<int>(), Vector2.zero)
            });

            _data.EnsureCentralNode();

            int index = FindIndexOfId(_data, 0);
            Assert.GreaterOrEqual(index, 0);
            Assert.AreEqual(250, _data.Nodes[index].baseCost);
        }

        [Test]
        public void EnsureCentralNode_PreservesExistingChildren()
        {
            var children = new List<int> { 5, 7 };
            _data.InitializeForTest(new List<SkillTreeData.SkillNodeEntry>
            {
                MakeCentralLikeNode(0, children, Vector2.zero)
            });

            _data.EnsureCentralNode();

            int index = FindIndexOfId(_data, 0);
            Assert.GreaterOrEqual(index, 0);
            var preserved = _data.Nodes[index].connectedNodeIds;
            Assert.IsNotNull(preserved);
            Assert.AreEqual(2, preserved.Count);
            Assert.AreEqual(5, preserved[0]);
            Assert.AreEqual(7, preserved[1]);
        }

        [Test]
        public void EnsureCentralNode_Idempotent()
        {
            _data.InitializeForTest(new List<SkillTreeData.SkillNodeEntry>());

            _data.EnsureCentralNode();
            int countAfterFirst = _data.Nodes.Count;
            int indexAfterFirst = FindIndexOfId(_data, 0);
            var firstSnapshot = _data.Nodes[indexAfterFirst];

            _data.EnsureCentralNode();
            int countAfterSecond = _data.Nodes.Count;
            int indexAfterSecond = FindIndexOfId(_data, 0);
            var secondSnapshot = _data.Nodes[indexAfterSecond];

            Assert.AreEqual(countAfterFirst, countAfterSecond);
            Assert.AreEqual(firstSnapshot.baseCost, secondSnapshot.baseCost);
            Assert.AreEqual(firstSnapshot.position, secondSnapshot.position);
            Assert.AreEqual(firstSnapshot.costType, secondSnapshot.costType);
            Assert.AreEqual(firstSnapshot.maxLevel, secondSnapshot.maxLevel);
        }

        [Test]
        public void EnsureCentralNode_SetsZeroBonus_AndMaxLevel1()
        {
            _data.InitializeForTest(new List<SkillTreeData.SkillNodeEntry>());

            _data.EnsureCentralNode();

            int index = FindIndexOfId(_data, 0);
            Assert.GreaterOrEqual(index, 0);
            Assert.AreEqual(0f, _data.Nodes[index].statModifierValuePerLevel);
            Assert.AreEqual(1, _data.Nodes[index].maxLevel);
            Assert.AreEqual(SkillTreeData.StatModifierMode.Flat, _data.Nodes[index].statModifierMode);
        }

        [Test]
        public void EnsureCentralNode_CostType_IsGold()
        {
            _data.InitializeForTest(new List<SkillTreeData.SkillNodeEntry>());

            _data.EnsureCentralNode();

            int index = FindIndexOfId(_data, 0);
            Assert.GreaterOrEqual(index, 0);
            Assert.AreEqual(SkillTreeData.CostType.Gold, _data.Nodes[index].costType);
        }

        [Test]
        public void CentralUnlockCost_DefaultsTo100()
        {
            Assert.AreEqual(100, _data.CentralUnlockCost);
        }
    }
}
