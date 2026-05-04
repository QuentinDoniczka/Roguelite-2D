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
        public void ResolveBranchPlan_MirrorEnabled_UsesAngleDirectly()
        {
            var nodes = MakeStandardNodes();

            var (_, mirrorBranchAngle) = BranchPlacement.ResolveBranchPlan(
                nodes,
                parentIndex: 1,
                angleDegrees: 60f,
                mirrorAxisDegrees: 90f,
                mirrorEnabled: true);

            Assert.That(mirrorBranchAngle, Is.EqualTo(BranchPlacement.MirrorAngle(60f, 90f)).Within(Tolerance));
        }

        [Test]
        public void ResolveBranchPlan_NullNodes_ReturnsZeroParentAndDefaultAngles()
        {
            var (parentPos, mirrorBranchAngle) = BranchPlacement.ResolveBranchPlan(
                nodes: null,
                parentIndex: 0,
                angleDegrees: 30f,
                mirrorAxisDegrees: 45f,
                mirrorEnabled: true);

            Assert.That(parentPos, Is.EqualTo(Vector2.zero));
            Assert.That(mirrorBranchAngle, Is.EqualTo(BranchPlacement.MirrorAngle(30f, 45f)).Within(Tolerance));
        }
    }
}
