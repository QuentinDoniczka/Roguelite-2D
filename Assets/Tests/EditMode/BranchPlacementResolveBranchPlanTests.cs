using System.Collections.Generic;
using NUnit.Framework;
using RogueliteAutoBattler.Data;
using RogueliteAutoBattler.Editor.Tools;
using RogueliteAutoBattler.Tests.EditMode.TestUtils;
using UnityEngine;

namespace RogueliteAutoBattler.Tests.EditMode
{
    public class BranchPlacementResolveBranchPlanTests
    {
        private const float Tolerance = 1e-4f;

        private static List<SkillTreeData.SkillNodeEntry> MakeStandardNodes()
        {
            return new List<SkillTreeData.SkillNodeEntry>
            {
                SkillNodeEntryFactory.Default(0, Vector2.zero),
                SkillNodeEntryFactory.Default(1, new Vector2(2f, 0f)),
                SkillNodeEntryFactory.Default(2, new Vector2(0f, 3f))
            };
        }

        [Test]
        public void ResolveBranchPlan_NullNodes_ReturnsZeroParentPos()
        {
            var (parentPos, mirrorBranchAngle) = BranchPlacement.ResolveBranchPlan(
                nodes: null,
                parentIndex: 0,
                angleDegrees: 30f,
                mirrorAxisDegrees: 45f,
                mirrorEnabled: true);

            Assert.That(parentPos, Is.EqualTo(Vector2.zero));
            Assert.That(mirrorBranchAngle, Is.EqualTo(0f).Within(Tolerance));
        }

        [Test]
        public void ResolveBranchPlan_ParentIndexOutOfRange_ReturnsZeroParentPos()
        {
            var nodes = MakeStandardNodes();

            var (parentPos, mirrorBranchAngle) = BranchPlacement.ResolveBranchPlan(
                nodes,
                parentIndex: 99,
                angleDegrees: 30f,
                mirrorAxisDegrees: 45f,
                mirrorEnabled: true);

            Assert.That(parentPos, Is.EqualTo(Vector2.zero));
            Assert.That(mirrorBranchAngle, Is.EqualTo(0f).Within(Tolerance));
        }

        [Test]
        public void ResolveBranchPlan_EmptyNodes_ReturnsZeroParentPos()
        {
            var emptyNodes = new List<SkillTreeData.SkillNodeEntry>();

            var (parentPos, mirrorBranchAngle) = BranchPlacement.ResolveBranchPlan(
                emptyNodes,
                parentIndex: 0,
                angleDegrees: 30f,
                mirrorAxisDegrees: 45f,
                mirrorEnabled: false);

            Assert.That(parentPos, Is.EqualTo(Vector2.zero));
            Assert.That(mirrorBranchAngle, Is.EqualTo(0f).Within(Tolerance));
        }

        [Test]
        public void ResolveBranchPlan_MirrorDisabled_ProducesParentPosAndAngles()
        {
            var nodes = MakeStandardNodes();

            var (parentPos, mirrorBranchAngle) = BranchPlacement.ResolveBranchPlan(
                nodes,
                parentIndex: 1,
                angleDegrees: 30f,
                mirrorAxisDegrees: 45f,
                mirrorEnabled: false);

            Assert.That(parentPos.x, Is.EqualTo(2f).Within(Tolerance));
            Assert.That(parentPos.y, Is.EqualTo(0f).Within(Tolerance));
            Assert.That(mirrorBranchAngle, Is.EqualTo(30f).Within(Tolerance));
        }

        [Test]
        public void ResolveBranchPlan_MirrorEnabled_ProducesMirroredAngle()
        {
            var nodes = MakeStandardNodes();

            var (parentPos, mirrorBranchAngle) = BranchPlacement.ResolveBranchPlan(
                nodes,
                parentIndex: 1,
                angleDegrees: 30f,
                mirrorAxisDegrees: 90f,
                mirrorEnabled: true);

            Assert.That(parentPos.x, Is.EqualTo(2f).Within(Tolerance));
            Assert.That(parentPos.y, Is.EqualTo(0f).Within(Tolerance));
            Assert.That(mirrorBranchAngle, Is.EqualTo(BranchPlacement.MirrorAngle(30f, 90f)).Within(Tolerance));
        }
    }
}
