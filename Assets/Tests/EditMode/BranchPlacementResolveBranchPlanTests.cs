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
        public void ResolveBranchPlan_MirrorDisabled_RelativeFlagIgnored()
        {
            var nodes = MakeStandardNodes();

            var (parentPos, mirrorSourcePos, resolvedAngle, mirrorBranchAngle) = BranchPlacement.ResolveBranchPlan(
                nodes,
                parentIndex: 1,
                mirrorSourceOverrideIndex: 2,
                angleDegrees: 30f,
                mirrorAxisDegrees: 45f,
                angleIsRelativeToMirrorAxis: true,
                mirrorEnabled: false);

            Assert.That(parentPos.x, Is.EqualTo(2f).Within(Tolerance));
            Assert.That(parentPos.y, Is.EqualTo(0f).Within(Tolerance));
            Assert.That(resolvedAngle, Is.EqualTo(30f).Within(Tolerance));
            Assert.That(mirrorBranchAngle, Is.EqualTo(30f).Within(Tolerance));
            Assert.That(mirrorSourcePos.x, Is.EqualTo(0f).Within(Tolerance));
            Assert.That(mirrorSourcePos.y, Is.EqualTo(3f).Within(Tolerance));
        }

        [Test]
        public void ResolveBranchPlan_MirrorEnabled_RelativeFalse_UsesAngleDirectly()
        {
            var nodes = MakeStandardNodes();

            var (_, _, resolvedAngle, mirrorBranchAngle) = BranchPlacement.ResolveBranchPlan(
                nodes,
                parentIndex: 1,
                mirrorSourceOverrideIndex: BranchPlacement.NoMirrorSourceOverride,
                angleDegrees: 60f,
                mirrorAxisDegrees: 90f,
                angleIsRelativeToMirrorAxis: false,
                mirrorEnabled: true);

            Assert.That(resolvedAngle, Is.EqualTo(60f).Within(Tolerance));
            Assert.That(mirrorBranchAngle, Is.EqualTo(BranchPlacement.MirrorAngle(60f, 90f)).Within(Tolerance));
        }

        [Test]
        public void ResolveBranchPlan_MirrorEnabled_RelativeTrue_AddsAxisToAngle()
        {
            var nodes = MakeStandardNodes();

            var (_, _, resolvedAngle, mirrorBranchAngle) = BranchPlacement.ResolveBranchPlan(
                nodes,
                parentIndex: 1,
                mirrorSourceOverrideIndex: BranchPlacement.NoMirrorSourceOverride,
                angleDegrees: 30f,
                mirrorAxisDegrees: 45f,
                angleIsRelativeToMirrorAxis: true,
                mirrorEnabled: true);

            Assert.That(resolvedAngle, Is.EqualTo(75f).Within(Tolerance));
            Assert.That(mirrorBranchAngle, Is.EqualTo(BranchPlacement.MirrorAngle(75f, 45f)).Within(Tolerance));
        }

        [Test]
        public void ResolveBranchPlan_OverrideValid_UsesOverrideForMirrorSource()
        {
            var nodes = MakeStandardNodes();

            var (parentPos, mirrorSourcePos, _, _) = BranchPlacement.ResolveBranchPlan(
                nodes,
                parentIndex: 1,
                mirrorSourceOverrideIndex: 2,
                angleDegrees: 0f,
                mirrorAxisDegrees: 0f,
                angleIsRelativeToMirrorAxis: false,
                mirrorEnabled: true);

            Assert.That(parentPos.x, Is.EqualTo(2f).Within(Tolerance));
            Assert.That(parentPos.y, Is.EqualTo(0f).Within(Tolerance));
            Assert.That(mirrorSourcePos.x, Is.EqualTo(0f).Within(Tolerance));
            Assert.That(mirrorSourcePos.y, Is.EqualTo(3f).Within(Tolerance));
        }

        [Test]
        public void ResolveBranchPlan_OverrideOutOfRange_FallsBackToParentPos()
        {
            var nodes = MakeStandardNodes();

            var (parentPos, mirrorSourcePos, _, _) = BranchPlacement.ResolveBranchPlan(
                nodes,
                parentIndex: 1,
                mirrorSourceOverrideIndex: 99,
                angleDegrees: 0f,
                mirrorAxisDegrees: 0f,
                angleIsRelativeToMirrorAxis: false,
                mirrorEnabled: true);

            Assert.That(mirrorSourcePos.x, Is.EqualTo(parentPos.x).Within(Tolerance));
            Assert.That(mirrorSourcePos.y, Is.EqualTo(parentPos.y).Within(Tolerance));
        }

        [Test]
        public void ResolveBranchPlan_OverrideNoOverrideSentinel_FallsBackToParentPos()
        {
            var nodes = MakeStandardNodes();

            var (parentPos, mirrorSourcePos, _, _) = BranchPlacement.ResolveBranchPlan(
                nodes,
                parentIndex: 1,
                mirrorSourceOverrideIndex: BranchPlacement.NoMirrorSourceOverride,
                angleDegrees: 0f,
                mirrorAxisDegrees: 0f,
                angleIsRelativeToMirrorAxis: false,
                mirrorEnabled: true);

            Assert.That(mirrorSourcePos.x, Is.EqualTo(parentPos.x).Within(Tolerance));
            Assert.That(mirrorSourcePos.y, Is.EqualTo(parentPos.y).Within(Tolerance));
        }

        [Test]
        public void ResolveBranchPlan_RelativePlusAxis_WrapsAround360()
        {
            var nodes = MakeStandardNodes();

            var (_, _, resolvedAngle, _) = BranchPlacement.ResolveBranchPlan(
                nodes,
                parentIndex: 1,
                mirrorSourceOverrideIndex: BranchPlacement.NoMirrorSourceOverride,
                angleDegrees: 90f,
                mirrorAxisDegrees: 270f,
                angleIsRelativeToMirrorAxis: true,
                mirrorEnabled: true);

            Assert.That(resolvedAngle, Is.EqualTo(0f).Within(Tolerance));
        }

        [Test]
        public void ResolveBranchPlan_NullNodes_ReturnsZeroPositions()
        {
            var (parentPos, mirrorSourcePos, resolvedAngle, mirrorBranchAngle) = BranchPlacement.ResolveBranchPlan(
                nodes: null,
                parentIndex: 0,
                mirrorSourceOverrideIndex: BranchPlacement.NoMirrorSourceOverride,
                angleDegrees: 30f,
                mirrorAxisDegrees: 45f,
                angleIsRelativeToMirrorAxis: false,
                mirrorEnabled: true);

            Assert.That(parentPos, Is.EqualTo(Vector2.zero));
            Assert.That(mirrorSourcePos, Is.EqualTo(Vector2.zero));
            Assert.That(resolvedAngle, Is.EqualTo(30f).Within(Tolerance));
            Assert.That(mirrorBranchAngle, Is.EqualTo(BranchPlacement.MirrorAngle(30f, 45f)).Within(Tolerance));
        }
    }
}
