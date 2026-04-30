using NUnit.Framework;
using RogueliteAutoBattler.Combat.Core;
using RogueliteAutoBattler.Data;
using RogueliteAutoBattler.Editor.Tools;
using UnityEngine;

namespace RogueliteAutoBattler.Tests.EditMode
{
    public class SkillTreeNodeFactoryTests
    {
        [Test]
        public void CreateBranchNode_AssignsId()
        {
            var entry = SkillTreeNodeFactory.CreateBranchNode(42, new Vector2(1f, 2f));

            Assert.AreEqual(42, entry.id);
        }

        [Test]
        public void CreateBranchNode_AssignsPosition()
        {
            Vector2 position = new Vector2(7.5f, -3.25f);

            var entry = SkillTreeNodeFactory.CreateBranchNode(0, position);

            Assert.AreEqual(position, entry.position);
        }

        [Test]
        public void CreateBranchNode_DefaultMaxLevelIsOne()
        {
            var entry = SkillTreeNodeFactory.CreateBranchNode(0, Vector2.zero);

            Assert.AreEqual(1, entry.maxLevel);
        }

        [Test]
        public void CreateBranchNode_DefaultCostIsOneSkillPoint()
        {
            var entry = SkillTreeNodeFactory.CreateBranchNode(0, Vector2.zero);

            Assert.AreEqual(SkillTreeData.CostType.SkillPoint, entry.costType);
            Assert.AreEqual(1, entry.baseCost);
            Assert.AreEqual(1f, entry.costMultiplierOdd);
            Assert.AreEqual(1f, entry.costMultiplierEven);
            Assert.AreEqual(0, entry.costAdditivePerLevel);
        }

        [Test]
        public void CreateBranchNode_DefaultStatIsHpFlatFive()
        {
            var entry = SkillTreeNodeFactory.CreateBranchNode(0, Vector2.zero);

            Assert.AreEqual(StatType.Hp, entry.statModifierType);
            Assert.AreEqual(SkillTreeData.StatModifierMode.Flat, entry.statModifierMode);
            Assert.AreEqual(5f, entry.statModifierValuePerLevel);
        }

        [Test]
        public void CreateBranchNode_StartsWithEmptyConnections()
        {
            var entry = SkillTreeNodeFactory.CreateBranchNode(0, Vector2.zero);

            Assert.IsNotNull(entry.connectedNodeIds);
            Assert.AreEqual(0, entry.connectedNodeIds.Count);
        }
    }
}
