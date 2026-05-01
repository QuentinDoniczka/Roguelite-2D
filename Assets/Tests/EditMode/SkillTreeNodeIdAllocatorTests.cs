using System.Collections.Generic;
using NUnit.Framework;
using RogueliteAutoBattler.Combat.Core;
using RogueliteAutoBattler.Data;
using RogueliteAutoBattler.Editor.Tools;
using UnityEngine;

namespace RogueliteAutoBattler.Tests.EditMode
{
    public class SkillTreeNodeIdAllocatorTests
    {
        private static SkillTreeData.SkillNodeEntry MakeNode(int id)
        {
            return new SkillTreeData.SkillNodeEntry
            {
                id = id,
                position = Vector2.zero,
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
        public void ComputeNextNodeId_EmptyList_ReturnsZero()
        {
            var nodes = new List<SkillTreeData.SkillNodeEntry>();

            int next = SkillTreeNodeIdAllocator.ComputeNextNodeId(nodes);

            Assert.AreEqual(0, next);
        }

        [Test]
        public void ComputeNextNodeId_SequentialIds_ReturnsMaxPlusOne()
        {
            var nodes = new List<SkillTreeData.SkillNodeEntry>
            {
                MakeNode(0),
                MakeNode(1),
                MakeNode(2)
            };

            int next = SkillTreeNodeIdAllocator.ComputeNextNodeId(nodes);

            Assert.AreEqual(3, next);
        }

        [Test]
        public void ComputeNextNodeId_WithGaps_ReturnsMaxPlusOne()
        {
            var nodes = new List<SkillTreeData.SkillNodeEntry>
            {
                MakeNode(0),
                MakeNode(1),
                MakeNode(5)
            };

            int next = SkillTreeNodeIdAllocator.ComputeNextNodeId(nodes);

            Assert.AreEqual(6, next);
        }

        [Test]
        public void ComputeNextNodeId_SingleNode_ReturnsIdPlusOne()
        {
            var nodes = new List<SkillTreeData.SkillNodeEntry>
            {
                MakeNode(42)
            };

            int next = SkillTreeNodeIdAllocator.ComputeNextNodeId(nodes);

            Assert.AreEqual(43, next);
        }
    }
}
