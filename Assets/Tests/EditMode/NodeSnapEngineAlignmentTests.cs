using System.Collections.Generic;
using NUnit.Framework;
using RogueliteAutoBattler.Combat.Core;
using RogueliteAutoBattler.Data;
using RogueliteAutoBattler.Editor.Tools;
using UnityEngine;

namespace RogueliteAutoBattler.Tests.EditMode
{
    public class NodeSnapEngineAlignmentTests
    {
        private const float Tolerance = 1e-3f;

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
                statModifierValuePerLevel = 5f
            };
        }

        [Test]
        public void Resolve_5Arg_TierZeroSupersedesTier1()
        {
            var nodes = new List<SkillTreeData.SkillNodeEntry>
            {
                MakeNode(0, new Vector2(0f, 0f)),
                MakeNode(1, new Vector2(3.05f, 3.15f))
            };
            Vector2 candidate = new Vector2(3.1f, 3f);

            var result = NodeSnapEngine.Resolve(candidate, 0, nodes, 0.25f, 6f);

            Assert.That(result.SnappedAxis, Is.EqualTo(NodeSnapEngine.SnapAxis.X));
            Assert.That(result.ResolvedPosition.x, Is.EqualTo(3.05f).Within(Tolerance));
        }

        [Test]
        public void Resolve_5Arg_TwoCollinearNeighbors_ReturnsLineCollinear()
        {
            var nodes = new List<SkillTreeData.SkillNodeEntry>
            {
                MakeNode(0, new Vector2(100f, 100f)),
                MakeNode(1, new Vector2(1f, 1f)),
                MakeNode(2, new Vector2(3f, 3f))
            };
            Vector2 candidate = new Vector2(2.05f, 1.95f);

            var result = NodeSnapEngine.Resolve(candidate, 0, nodes, 0.25f, 6f);

            Assert.That(result.SnappedAxis, Is.EqualTo(NodeSnapEngine.SnapAxis.LineCollinear));
            Assert.That(result.TargetNodeIndex, Is.GreaterThanOrEqualTo(0));
            Assert.That(result.SecondaryTargetNodeIndex, Is.GreaterThanOrEqualTo(0));
            Assert.That(result.ResolvedPosition.x, Is.EqualTo(2f).Within(Tolerance));
            Assert.That(result.ResolvedPosition.y, Is.EqualTo(2f).Within(Tolerance));
        }

        [Test]
        public void Resolve_5Arg_MidpointBetweenTwoCollinear_SnapsToMidpoint()
        {
            var nodes = new List<SkillTreeData.SkillNodeEntry>
            {
                MakeNode(0, new Vector2(100f, 100f)),
                MakeNode(1, new Vector2(0f, 0f)),
                MakeNode(2, new Vector2(4f, 4f))
            };
            Vector2 candidate = new Vector2(2.05f, 1.95f);

            var result = NodeSnapEngine.Resolve(candidate, 0, nodes, 0.25f, 6f);

            Assert.That(result.SnappedAxis, Is.EqualTo(NodeSnapEngine.SnapAxis.LineCollinear));
            Assert.That(result.ResolvedPosition.x, Is.EqualTo(2f).Within(Tolerance));
            Assert.That(result.ResolvedPosition.y, Is.EqualTo(2f).Within(Tolerance));
        }

        [Test]
        public void Resolve_5Arg_NoNeighborsInRadius_ReturnsNoSnap()
        {
            var nodes = new List<SkillTreeData.SkillNodeEntry>
            {
                MakeNode(0, new Vector2(0f, 0f)),
                MakeNode(1, new Vector2(100f, 100f))
            };
            Vector2 candidate = new Vector2(0.5f, 0.5f);

            var result = NodeSnapEngine.Resolve(candidate, 0, nodes, 0.25f, 2f);

            Assert.That(result.SnappedAxis, Is.EqualTo(NodeSnapEngine.SnapAxis.None));
        }

        [Test]
        public void Resolve_5Arg_ExcludesDraggedNode()
        {
            var nodes = new List<SkillTreeData.SkillNodeEntry>
            {
                MakeNode(0, new Vector2(5f, 5f)),
                MakeNode(1, new Vector2(100f, 100f))
            };
            Vector2 candidate = new Vector2(5f, 5f);

            var result = NodeSnapEngine.Resolve(candidate, 0, nodes, 0.25f, 3f);

            Assert.That(result.SnappedAxis, Is.EqualTo(NodeSnapEngine.SnapAxis.None));
        }
    }
}
