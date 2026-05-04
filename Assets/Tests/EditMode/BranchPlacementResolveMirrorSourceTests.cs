using System.Collections.Generic;
using NUnit.Framework;
using RogueliteAutoBattler.Combat.Core;
using RogueliteAutoBattler.Data;
using RogueliteAutoBattler.Editor.Tools;
using UnityEngine;

namespace RogueliteAutoBattler.Tests.EditMode
{
    public class BranchPlacementResolveMirrorSourceTests
    {
        private const float Tolerance = 1e-4f;

        private static SkillTreeData.SkillNodeEntry MakeNode(int id, Vector2 position)
        {
            return new SkillTreeData.SkillNodeEntry
            {
                id = id,
                position = position,
                connectedNodeIds = new List<int>(),
                costType = SkillTreeData.CostType.Gold,
                maxLevel = 1,
                baseCost = 1,
                costMultiplierOdd = 1f,
                costMultiplierEven = 1f,
                costAdditivePerLevel = 0,
                statModifierType = StatType.Hp,
                statModifierMode = SkillTreeData.StatModifierMode.Flat,
                statModifierValuePerLevel = 1f
            };
        }

        [Test]
        public void ResolveMirrorSourcePosition_ValidOverride_ReturnsOverridePosition()
        {
            var nodes = new List<SkillTreeData.SkillNodeEntry>
            {
                MakeNode(0, Vector2.zero),
                MakeNode(1, new Vector2(2f, 0f)),
                MakeNode(2, new Vector2(0f, 3f))
            };

            Vector2 result = BranchPlacement.ResolveMirrorSourcePosition(nodes, parentIndex: 1, sourceOverrideIndex: 2);

            Assert.That(result.x, Is.EqualTo(0f).Within(Tolerance));
            Assert.That(result.y, Is.EqualTo(3f).Within(Tolerance));
        }

        [Test]
        public void ResolveMirrorSourcePosition_OverrideMinusOne_FallsBackToParent()
        {
            var nodes = new List<SkillTreeData.SkillNodeEntry>
            {
                MakeNode(0, Vector2.zero),
                MakeNode(1, new Vector2(2f, 0f))
            };

            Vector2 result = BranchPlacement.ResolveMirrorSourcePosition(nodes, parentIndex: 1, sourceOverrideIndex: -1);

            Assert.That(result.x, Is.EqualTo(2f).Within(Tolerance));
            Assert.That(result.y, Is.EqualTo(0f).Within(Tolerance));
        }

        [Test]
        public void ResolveMirrorSourcePosition_OverrideOutOfRangeHigh_FallsBackToParent()
        {
            var nodes = new List<SkillTreeData.SkillNodeEntry>
            {
                MakeNode(0, Vector2.zero),
                MakeNode(1, new Vector2(2f, 0f))
            };

            Vector2 result = BranchPlacement.ResolveMirrorSourcePosition(nodes, parentIndex: 1, sourceOverrideIndex: 99);

            Assert.That(result.x, Is.EqualTo(2f).Within(Tolerance));
            Assert.That(result.y, Is.EqualTo(0f).Within(Tolerance));
        }

        [Test]
        public void ResolveMirrorSourcePosition_BothInvalid_ReturnsZero()
        {
            var nodes = new List<SkillTreeData.SkillNodeEntry>
            {
                MakeNode(0, Vector2.zero)
            };

            Vector2 result = BranchPlacement.ResolveMirrorSourcePosition(nodes, parentIndex: 99, sourceOverrideIndex: 99);

            Assert.That(result, Is.EqualTo(Vector2.zero));
        }

        [Test]
        public void ResolveMirrorSourcePosition_NullNodes_ReturnsZero()
        {
            Vector2 result = BranchPlacement.ResolveMirrorSourcePosition(null, parentIndex: 0, sourceOverrideIndex: 0);

            Assert.That(result, Is.EqualTo(Vector2.zero));
        }
    }
}
