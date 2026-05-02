using System.Collections.Generic;
using NUnit.Framework;
using RogueliteAutoBattler.Combat.Core;
using RogueliteAutoBattler.Data;
using UnityEngine;

namespace RogueliteAutoBattler.Tests.EditMode
{
    public class SkillTreeRefundCalculatorTests
    {
        private SkillTreeData _tree;

        [TearDown]
        public void TearDown()
        {
            if (_tree != null) Object.DestroyImmediate(_tree);
        }

        private static SkillTreeData.SkillNodeEntry MakeNode(
            int id,
            SkillTreeData.CostType costType,
            int maxLevel,
            int baseCost,
            float multOdd = 1f,
            float multEven = 1f,
            int additive = 0)
        {
            return new SkillTreeData.SkillNodeEntry
            {
                id = id,
                position = Vector2.zero,
                connectedNodeIds = new List<int>(),
                costType = costType,
                maxLevel = maxLevel,
                baseCost = baseCost,
                costMultiplierOdd = multOdd,
                costMultiplierEven = multEven,
                costAdditivePerLevel = additive,
                statModifierType = StatType.Hp,
                statModifierMode = SkillTreeData.StatModifierMode.Flat,
                statModifierValuePerLevel = 5f
            };
        }

        private static int SumLevelCost(SkillTreeData.SkillNodeEntry node, int level)
        {
            int total = 0;
            for (int lvl = 0; lvl < level; lvl++)
                total += SkillTreeData.ComputeNodeCost(node, lvl);
            return total;
        }

        [Test]
        public void Compute_NoSpentLevels_ReturnsZero()
        {
            _tree = ScriptableObject.CreateInstance<SkillTreeData>();
            _tree.InitializeForTest(new List<SkillTreeData.SkillNodeEntry>
            {
                MakeNode(0, SkillTreeData.CostType.Gold, 1, 100),
                MakeNode(1, SkillTreeData.CostType.Gold, 3, 10),
                MakeNode(2, SkillTreeData.CostType.SkillPoint, 3, 5)
            });

            var refund = SkillTreeRefundCalculator.Compute(_tree, new[] { 0, 0, 0 });

            Assert.AreEqual(0, refund.Gold);
            Assert.AreEqual(0, refund.SkillPoint);
        }

        [Test]
        public void Compute_GoldNodeOnly_AccumulatesGoldRefund()
        {
            var goldNode = MakeNode(1, SkillTreeData.CostType.Gold, 3, 10);
            _tree = ScriptableObject.CreateInstance<SkillTreeData>();
            _tree.InitializeForTest(new List<SkillTreeData.SkillNodeEntry>
            {
                MakeNode(0, SkillTreeData.CostType.Gold, 1, 100),
                goldNode
            });

            var refund = SkillTreeRefundCalculator.Compute(_tree, new[] { 0, 3 });

            int expectedGold = SumLevelCost(goldNode, 3);
            Assert.AreEqual(expectedGold, refund.Gold);
            Assert.AreEqual(0, refund.SkillPoint);
        }

        [Test]
        public void Compute_SkillPointNodeOnly_AccumulatesSkillPointRefund()
        {
            var spNode = MakeNode(1, SkillTreeData.CostType.SkillPoint, 3, 4, multOdd: 1.5f, multEven: 1.2f);
            _tree = ScriptableObject.CreateInstance<SkillTreeData>();
            _tree.InitializeForTest(new List<SkillTreeData.SkillNodeEntry>
            {
                MakeNode(0, SkillTreeData.CostType.Gold, 1, 100),
                spNode
            });

            var refund = SkillTreeRefundCalculator.Compute(_tree, new[] { 0, 2 });

            int expectedSp = SumLevelCost(spNode, 2);
            Assert.AreEqual(0, refund.Gold);
            Assert.AreEqual(expectedSp, refund.SkillPoint);
        }

        [Test]
        public void Compute_MixedNodes_SplitsByCostType()
        {
            var goldNode = MakeNode(1, SkillTreeData.CostType.Gold, 3, 10);
            var spNode = MakeNode(2, SkillTreeData.CostType.SkillPoint, 3, 5);
            _tree = ScriptableObject.CreateInstance<SkillTreeData>();
            _tree.InitializeForTest(new List<SkillTreeData.SkillNodeEntry>
            {
                MakeNode(0, SkillTreeData.CostType.Gold, 1, 100),
                goldNode,
                spNode
            });

            var refund = SkillTreeRefundCalculator.Compute(_tree, new[] { 0, 2, 3 });

            int expectedGold = SumLevelCost(goldNode, 2);
            int expectedSp = SumLevelCost(spNode, 3);
            Assert.AreEqual(expectedGold, refund.Gold);
            Assert.AreEqual(expectedSp, refund.SkillPoint);
        }

        [Test]
        public void Compute_SavedLevelsLongerThanTree_IgnoresExtras()
        {
            var goldNode = MakeNode(1, SkillTreeData.CostType.Gold, 3, 10);
            _tree = ScriptableObject.CreateInstance<SkillTreeData>();
            _tree.InitializeForTest(new List<SkillTreeData.SkillNodeEntry>
            {
                MakeNode(0, SkillTreeData.CostType.Gold, 1, 100),
                goldNode
            });

            var refund = SkillTreeRefundCalculator.Compute(_tree, new[] { 0, 2, 99, 99, 99 });

            int expectedGold = SumLevelCost(goldNode, 2);
            Assert.AreEqual(expectedGold, refund.Gold);
            Assert.AreEqual(0, refund.SkillPoint);
        }

        [Test]
        public void Compute_SavedLevelExceedsMaxLevel_RefundsCappedAtMax()
        {
            var cappedNode = MakeNode(1, SkillTreeData.CostType.Gold, 2, 10);
            _tree = ScriptableObject.CreateInstance<SkillTreeData>();
            _tree.InitializeForTest(new List<SkillTreeData.SkillNodeEntry>
            {
                MakeNode(0, SkillTreeData.CostType.Gold, 1, 100),
                cappedNode
            });

            var refund = SkillTreeRefundCalculator.Compute(_tree, new[] { 0, 10 });

            int expectedGold = SumLevelCost(cappedNode, 2);
            Assert.AreEqual(expectedGold, refund.Gold);
            Assert.AreEqual(0, refund.SkillPoint);
        }
    }
}
