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
        public void Resolve_5Arg_LegacyThresholdZero_SingleNeighborInRadius_ReturnsLineCardinal()
        {
            var nodes = new List<SkillTreeData.SkillNodeEntry>
            {
                MakeNode(0, new Vector2(100f, 100f)),
                MakeNode(1, new Vector2(3f, 0f))
            };
            Vector2 candidate = new Vector2(3.1f, 5f);

            var result = NodeSnapEngine.Resolve(candidate, 0, nodes, 0f, 6f);

            Assert.That(result.SnappedAxis, Is.EqualTo(NodeSnapEngine.SnapAxis.LineCardinal));
            Assert.That(result.TargetNodeIndex, Is.EqualTo(1));
            Assert.That(result.SecondaryTargetNodeIndex, Is.EqualTo(-1));
            Assert.That(result.ResolvedPosition.x, Is.EqualTo(3f).Within(Tolerance));
            Assert.That(result.ResolvedPosition.y, Is.EqualTo(5f).Within(Tolerance));
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

            var result = NodeSnapEngine.Resolve(candidate, 0, nodes, 0f, 6f);

            Assert.That(result.SnappedAxis, Is.EqualTo(NodeSnapEngine.SnapAxis.LineCollinear));
            Assert.That(result.TargetNodeIndex, Is.GreaterThanOrEqualTo(0));
            Assert.That(result.SecondaryTargetNodeIndex, Is.GreaterThanOrEqualTo(0));
            Assert.That(result.ResolvedPosition.x, Is.EqualTo(result.ResolvedPosition.y).Within(Tolerance));
        }

        [Test]
        public void Resolve_5Arg_MidpointBetweenTwoCollinear_SnapsToMidpoint()
        {
            var nodes = new List<SkillTreeData.SkillNodeEntry>
            {
                MakeNode(0, new Vector2(100f, 100f)),
                MakeNode(1, new Vector2(0f, 0f)),
                MakeNode(2, new Vector2(4f, 0f))
            };
            Vector2 candidate = new Vector2(2.05f, 0.05f);

            var result = NodeSnapEngine.Resolve(candidate, 0, nodes, 0f, 6f);

            Assert.That(result.SnappedAxis, Is.EqualTo(NodeSnapEngine.SnapAxis.LineCollinear));
            Assert.That(result.ResolvedPosition.x, Is.EqualTo(2f).Within(Tolerance));
            Assert.That(result.ResolvedPosition.y, Is.EqualTo(0f).Within(Tolerance));
        }

        [Test]
        public void Resolve_5Arg_Tier1PreferredOverTier2_AtEqualResidual()
        {
            var nodes = new List<SkillTreeData.SkillNodeEntry>
            {
                MakeNode(0, new Vector2(100f, 100f)),
                MakeNode(1, new Vector2(3f, 0f)),
                MakeNode(2, new Vector2(0f, -1.05f)),
                MakeNode(3, new Vector2(4f, 2.95f))
            };
            Vector2 candidate = new Vector2(3.05f, 2.0f);

            var result = NodeSnapEngine.Resolve(candidate, 0, nodes, 0f, 6f);

            Assert.That(result.SnappedAxis, Is.EqualTo(NodeSnapEngine.SnapAxis.LineCardinal));
            Assert.That(result.TargetNodeIndex, Is.EqualTo(1));
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

            var result = NodeSnapEngine.Resolve(candidate, 0, nodes, 0f, 2f);

            Assert.That(result.SnappedAxis, Is.EqualTo(NodeSnapEngine.SnapAxis.None));
        }

        [Test]
        public void Resolve_5Arg_QuantizesResolvedPosition()
        {
            var nodes = new List<SkillTreeData.SkillNodeEntry>
            {
                MakeNode(0, new Vector2(100f, 100f)),
                MakeNode(1, new Vector2(3.137f, 0f))
            };
            Vector2 candidate = new Vector2(3.13f, 5f);

            var result = NodeSnapEngine.Resolve(candidate, 0, nodes, 0f, 6f);

            Assert.That(result.SnappedAxis, Is.EqualTo(NodeSnapEngine.SnapAxis.LineCardinal));
            Vector2 quantizedAgain = SkillTreeGrid.Quantize(result.ResolvedPosition);
            Assert.That(result.ResolvedPosition.x, Is.EqualTo(quantizedAgain.x).Within(1e-4f));
            Assert.That(result.ResolvedPosition.y, Is.EqualTo(quantizedAgain.y).Within(1e-4f));
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

            var result = NodeSnapEngine.Resolve(candidate, 0, nodes, 0f, 3f);

            Assert.That(result.SnappedAxis, Is.EqualTo(NodeSnapEngine.SnapAxis.None));
        }
    }
}
