using System.Collections.Generic;
using NUnit.Framework;
using RogueliteAutoBattler.Combat.Core;
using RogueliteAutoBattler.Data;
using RogueliteAutoBattler.Editor.Tools;
using UnityEngine;

namespace RogueliteAutoBattler.Tests.EditMode
{
    public class BranchPlacementResolveBranchPlanTests
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

        private static List<SkillTreeData.SkillNodeEntry> MakeStandardNodes()
        {
            return new List<SkillTreeData.SkillNodeEntry>
            {
                MakeNode(0, Vector2.zero),
                MakeNode(1, new Vector2(2f, 0f)),
                MakeNode(2, new Vector2(0f, 3f))
            };
        }

        [Test]
        public void ResolveBranchPlan_NullNodes_ReturnsZeroParentPos()
        {
            var (parentPos, resolvedAngle, mirrorBranchAngle) = BranchPlacement.ResolveBranchPlan(
                nodes: null,
                parentIndex: 0,
                angleDegrees: 30f,
                mirrorAxisDegrees: 45f,
                mirrorEnabled: true);

            Assert.That(parentPos, Is.EqualTo(Vector2.zero));
            Assert.That(resolvedAngle, Is.EqualTo(0f).Within(Tolerance));
            Assert.That(mirrorBranchAngle, Is.EqualTo(0f).Within(Tolerance));
        }

        [Test]
        public void ResolveBranchPlan_ParentIndexOutOfRange_ReturnsZeroParentPos()
        {
            var nodes = MakeStandardNodes();

            var (parentPos, resolvedAngle, mirrorBranchAngle) = BranchPlacement.ResolveBranchPlan(
                nodes,
                parentIndex: 99,
                angleDegrees: 30f,
                mirrorAxisDegrees: 45f,
                mirrorEnabled: true);

            Assert.That(parentPos, Is.EqualTo(Vector2.zero));
            Assert.That(resolvedAngle, Is.EqualTo(0f).Within(Tolerance));
            Assert.That(mirrorBranchAngle, Is.EqualTo(0f).Within(Tolerance));
        }

        [Test]
        public void ResolveBranchPlan_EmptyNodes_ReturnsZeroParentPos()
        {
            var emptyNodes = new List<SkillTreeData.SkillNodeEntry>();

            var (parentPos, resolvedAngle, mirrorBranchAngle) = BranchPlacement.ResolveBranchPlan(
                emptyNodes,
                parentIndex: 0,
                angleDegrees: 30f,
                mirrorAxisDegrees: 45f,
                mirrorEnabled: false);

            Assert.That(parentPos, Is.EqualTo(Vector2.zero));
            Assert.That(resolvedAngle, Is.EqualTo(0f).Within(Tolerance));
            Assert.That(mirrorBranchAngle, Is.EqualTo(0f).Within(Tolerance));
        }

        [Test]
        public void ResolveBranchPlan_MirrorDisabled_ProducesParentPosAndAngles()
        {
            var nodes = MakeStandardNodes();

            var (parentPos, resolvedAngle, mirrorBranchAngle) = BranchPlacement.ResolveBranchPlan(
                nodes,
                parentIndex: 1,
                angleDegrees: 30f,
                mirrorAxisDegrees: 45f,
                mirrorEnabled: false);

            Assert.That(parentPos.x, Is.EqualTo(2f).Within(Tolerance));
            Assert.That(parentPos.y, Is.EqualTo(0f).Within(Tolerance));
            Assert.That(resolvedAngle, Is.EqualTo(30f).Within(Tolerance));
            Assert.That(mirrorBranchAngle, Is.EqualTo(30f).Within(Tolerance));
        }

        [Test]
        public void ResolveBranchPlan_MirrorEnabled_ProducesMirroredAngle()
        {
            var nodes = MakeStandardNodes();

            var (parentPos, resolvedAngle, mirrorBranchAngle) = BranchPlacement.ResolveBranchPlan(
                nodes,
                parentIndex: 1,
                angleDegrees: 30f,
                mirrorAxisDegrees: 90f,
                mirrorEnabled: true);

            Assert.That(parentPos.x, Is.EqualTo(2f).Within(Tolerance));
            Assert.That(parentPos.y, Is.EqualTo(0f).Within(Tolerance));
            Assert.That(resolvedAngle, Is.EqualTo(30f).Within(Tolerance));
            Assert.That(mirrorBranchAngle, Is.EqualTo(BranchPlacement.MirrorAngle(30f, 90f)).Within(Tolerance));
        }
    }
}
