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

        [Test]
        public void Resolve_6Arg_HoldsPreviousAxisX_WhenSlidingAlongAndAnotherNodeIsCloserOnX()
        {
            var nodes = new List<SkillTreeData.SkillNodeEntry>
            {
                MakeNode(0, new Vector2(50f, 50f)),
                MakeNode(1, new Vector2(3f, 0f)),
                MakeNode(2, new Vector2(3.05f, 5f))
            };
            Vector2 previousResolvedOnXAxisOfNodeA = new Vector2(3f, 2f);
            var previousSnap = new NodeSnapEngine.SnapResult(
                previousResolvedOnXAxisOfNodeA, NodeSnapEngine.SnapAxis.X, 1);
            Vector2 candidate = new Vector2(3.06f, 4f);

            var result = NodeSnapEngine.Resolve(candidate, 0, nodes, 0.25f, 6f, previousSnap);

            Assert.That(result.SnappedAxis, Is.EqualTo(NodeSnapEngine.SnapAxis.X));
            Assert.That(result.TargetNodeIndex, Is.EqualTo(1));
            Assert.That(result.ResolvedPosition.x, Is.EqualTo(3f).Within(Tolerance));
            Assert.That(result.ResolvedPosition.y, Is.EqualTo(4f).Within(Tolerance));
        }

        [Test]
        public void Resolve_6Arg_ReleasesPreviousAxisX_WhenCandidateLeavesThreshold()
        {
            var nodes = new List<SkillTreeData.SkillNodeEntry>
            {
                MakeNode(0, new Vector2(50f, 50f)),
                MakeNode(1, new Vector2(3f, 0f))
            };
            Vector2 previousResolvedOnXAxisOfNodeA = new Vector2(3f, 2f);
            var previousSnap = new NodeSnapEngine.SnapResult(
                previousResolvedOnXAxisOfNodeA, NodeSnapEngine.SnapAxis.X, 1);
            Vector2 candidate = new Vector2(3.5f, 2f);

            var result = NodeSnapEngine.Resolve(candidate, 0, nodes, 0.25f, 6f, previousSnap);

            Assert.That(result.SnappedAxis, Is.EqualTo(NodeSnapEngine.SnapAxis.None));
        }

        [Test]
        public void Resolve_6Arg_HoldsLineCollinearDiagonal_WhenApproachingVerticalSnap()
        {
            var nodes = new List<SkillTreeData.SkillNodeEntry>
            {
                MakeNode(0, new Vector2(50f, 50f)),
                MakeNode(1, new Vector2(0f, 0f)),
                MakeNode(2, new Vector2(4f, 4f)),
                MakeNode(3, new Vector2(2.05f, 5f))
            };
            Vector2 previousResolvedOnSegmentOfNodesAandB = new Vector2(2f, 2f);
            var previousSnap = new NodeSnapEngine.SnapResult(
                previousResolvedOnSegmentOfNodesAandB, NodeSnapEngine.SnapAxis.LineCollinear, 1, 2);
            Vector2 candidate = new Vector2(2.05f, 1.95f);

            var result = NodeSnapEngine.Resolve(candidate, 0, nodes, 0.25f, 6f, previousSnap);

            Assert.That(result.SnappedAxis, Is.EqualTo(NodeSnapEngine.SnapAxis.LineCollinear));
            Assert.That(result.TargetNodeIndex, Is.EqualTo(1));
            Assert.That(result.SecondaryTargetNodeIndex, Is.EqualTo(2));
        }

        [Test]
        public void Resolve_6Arg_FallsBackToFreshResolve_WhenPreviousIsNoSnap()
        {
            var nodes = new List<SkillTreeData.SkillNodeEntry>
            {
                MakeNode(0, new Vector2(0f, 0f)),
                MakeNode(1, new Vector2(3.05f, 3.15f))
            };
            Vector2 candidate = new Vector2(3.1f, 3f);
            var previousSnap = NodeSnapEngine.SnapResult.NoSnap(Vector2.zero);

            var result = NodeSnapEngine.Resolve(candidate, 0, nodes, 0.25f, 6f, previousSnap);

            Assert.That(result.SnappedAxis, Is.EqualTo(NodeSnapEngine.SnapAxis.X));
            Assert.That(result.ResolvedPosition.x, Is.EqualTo(3.05f).Within(Tolerance));
        }

        [Test]
        public void Resolve_6Arg_HoldsPreviousAxisY_WhenSlidingAlongAndAnotherNodeIsCloserOnY()
        {
            var nodes = new List<SkillTreeData.SkillNodeEntry>
            {
                MakeNode(0, new Vector2(50f, 50f)),
                MakeNode(1, new Vector2(0f, 3f)),
                MakeNode(2, new Vector2(5f, 3.05f))
            };
            Vector2 previousResolvedOnYAxisOfNodeA = new Vector2(2f, 3f);
            var previousSnap = new NodeSnapEngine.SnapResult(
                previousResolvedOnYAxisOfNodeA, NodeSnapEngine.SnapAxis.Y, 1);
            Vector2 candidate = new Vector2(4f, 3.06f);

            var result = NodeSnapEngine.Resolve(candidate, 0, nodes, 0.25f, 6f, previousSnap);

            Assert.That(result.SnappedAxis, Is.EqualTo(NodeSnapEngine.SnapAxis.Y));
            Assert.That(result.TargetNodeIndex, Is.EqualTo(1));
            Assert.That(result.ResolvedPosition.x, Is.EqualTo(4f).Within(Tolerance));
            Assert.That(result.ResolvedPosition.y, Is.EqualTo(3f).Within(Tolerance));
        }

        [Test]
        public void Resolve_6Arg_HoldsPreviousLineCardinal_WhenAnotherTier1IsCloser()
        {
            var nodes = new List<SkillTreeData.SkillNodeEntry>
            {
                MakeNode(0, new Vector2(50f, 50f)),
                MakeNode(1, new Vector2(3f, 0f)),
                MakeNode(2, new Vector2(3.02f, 5f))
            };
            Vector2 previousResolvedOnHorizontalLineOfNodeA = new Vector2(3f, 4f);
            var previousSnap = new NodeSnapEngine.SnapResult(
                previousResolvedOnHorizontalLineOfNodeA, NodeSnapEngine.SnapAxis.LineCardinal, 1);
            Vector2 candidate = new Vector2(3.05f, 4.5f);

            var result = NodeSnapEngine.Resolve(candidate, 0, nodes, 0.25f, 6f, previousSnap);

            Assert.That(result.SnappedAxis, Is.EqualTo(NodeSnapEngine.SnapAxis.LineCardinal));
            Assert.That(result.TargetNodeIndex, Is.EqualTo(1));
            Assert.That(result.ResolvedPosition.x, Is.EqualTo(3f).Within(Tolerance));
        }

        [Test]
        public void Resolve_6Arg_ReleasesLineCollinear_WhenPerpResidualLeavesThreshold()
        {
            var nodes = new List<SkillTreeData.SkillNodeEntry>
            {
                MakeNode(0, new Vector2(50f, 50f)),
                MakeNode(1, new Vector2(0f, 0f)),
                MakeNode(2, new Vector2(4f, 4f))
            };
            Vector2 previousResolvedOnSegmentOfNodesAandB = new Vector2(2f, 2f);
            var previousSnap = new NodeSnapEngine.SnapResult(
                previousResolvedOnSegmentOfNodesAandB, NodeSnapEngine.SnapAxis.LineCollinear, 1, 2);
            Vector2 candidate = new Vector2(2.5f, 1.5f);

            var result = NodeSnapEngine.Resolve(candidate, 0, nodes, 0.25f, 6f, previousSnap);

            Assert.That(result.SnappedAxis, Is.Not.EqualTo(NodeSnapEngine.SnapAxis.LineCollinear));
        }

        [Test]
        public void Resolve_6Arg_CrossSnap_HeldLineCollinearDiagonalAndCardinalX_LocksToIntersection()
        {
            var nodes = new List<SkillTreeData.SkillNodeEntry>
            {
                MakeNode(0, new Vector2(50f, 50f)),
                MakeNode(1, new Vector2(0f, 0f)),
                MakeNode(2, new Vector2(4f, 4f)),
                MakeNode(3, new Vector2(2f, 5f))
            };
            Vector2 previousResolvedOnDiagonalNearMidpoint = new Vector2(1.95f, 1.95f);
            var previousSnap = new NodeSnapEngine.SnapResult(
                previousResolvedOnDiagonalNearMidpoint, NodeSnapEngine.SnapAxis.LineCollinear, 1, 2);
            Vector2 candidate = new Vector2(2.04f, 1.96f);

            var result = NodeSnapEngine.Resolve(candidate, 0, nodes, 0.25f, 6f, previousSnap);

            Assert.That(result.SnappedAxis, Is.EqualTo(NodeSnapEngine.SnapAxis.LineCollinear));
            Assert.That(result.TargetNodeIndex, Is.EqualTo(1));
            Assert.That(result.SecondaryTargetNodeIndex, Is.EqualTo(2));
            Assert.That(result.CrossAxis, Is.EqualTo(NodeSnapEngine.SnapAxis.X));
            Assert.That(result.CrossTargetNodeIndex, Is.EqualTo(3));
            Assert.That(result.ResolvedPosition.x, Is.EqualTo(2f).Within(Tolerance));
            Assert.That(result.ResolvedPosition.y, Is.EqualTo(2f).Within(Tolerance));
        }

        [Test]
        public void Resolve_6Arg_CrossSnap_HeldAxisXAndCardinalY_LocksToIntersection()
        {
            var nodes = new List<SkillTreeData.SkillNodeEntry>
            {
                MakeNode(0, new Vector2(50f, 50f)),
                MakeNode(1, new Vector2(3f, 0f)),
                MakeNode(2, new Vector2(8f, 5f))
            };
            Vector2 previousResolvedOnXAxisOfNodeA = new Vector2(3f, 3f);
            var previousSnap = new NodeSnapEngine.SnapResult(
                previousResolvedOnXAxisOfNodeA, NodeSnapEngine.SnapAxis.X, 1);
            Vector2 candidate = new Vector2(3.05f, 4.96f);

            var result = NodeSnapEngine.Resolve(candidate, 0, nodes, 0.25f, 6f, previousSnap);

            Assert.That(result.SnappedAxis, Is.EqualTo(NodeSnapEngine.SnapAxis.X));
            Assert.That(result.TargetNodeIndex, Is.EqualTo(1));
            Assert.That(result.CrossAxis, Is.EqualTo(NodeSnapEngine.SnapAxis.Y));
            Assert.That(result.CrossTargetNodeIndex, Is.EqualTo(2));
            Assert.That(result.ResolvedPosition.x, Is.EqualTo(3f).Within(Tolerance));
            Assert.That(result.ResolvedPosition.y, Is.EqualTo(5f).Within(Tolerance));
        }

        [Test]
        public void Resolve_6Arg_NoCross_WhenAllSecondariesOutsideThreshold()
        {
            var nodes = new List<SkillTreeData.SkillNodeEntry>
            {
                MakeNode(0, new Vector2(50f, 50f)),
                MakeNode(1, new Vector2(3f, 0f)),
                MakeNode(2, new Vector2(8f, 5f))
            };
            Vector2 previousResolvedOnXAxisOfNodeA = new Vector2(3f, 2f);
            var previousSnap = new NodeSnapEngine.SnapResult(
                previousResolvedOnXAxisOfNodeA, NodeSnapEngine.SnapAxis.X, 1);
            Vector2 candidate = new Vector2(3.05f, 2f);

            var result = NodeSnapEngine.Resolve(candidate, 0, nodes, 0.25f, 6f, previousSnap);

            Assert.That(result.SnappedAxis, Is.EqualTo(NodeSnapEngine.SnapAxis.X));
            Assert.That(result.TargetNodeIndex, Is.EqualTo(1));
            Assert.That(result.CrossAxis, Is.EqualTo(NodeSnapEngine.SnapAxis.None));
            Assert.That(result.CrossTargetNodeIndex, Is.EqualTo(-1));
        }

        [Test]
        public void Resolve_6Arg_NoCross_OnFreshNoSnap()
        {
            var nodes = new List<SkillTreeData.SkillNodeEntry>
            {
                MakeNode(0, new Vector2(0f, 0f)),
                MakeNode(1, new Vector2(100f, 100f))
            };
            Vector2 candidate = new Vector2(0.5f, 0.5f);
            var previousSnap = NodeSnapEngine.SnapResult.NoSnap(Vector2.zero);

            var result = NodeSnapEngine.Resolve(candidate, 0, nodes, 0.25f, 2f, previousSnap);

            Assert.That(result.SnappedAxis, Is.EqualTo(NodeSnapEngine.SnapAxis.None));
            Assert.That(result.CrossAxis, Is.EqualTo(NodeSnapEngine.SnapAxis.None));
            Assert.That(result.CrossTargetNodeIndex, Is.EqualTo(-1));
        }

        [Test]
        public void Resolve_6Arg_CrossSnap_HeldAxisYAndCardinalX_LocksToIntersection()
        {
            var nodes = new List<SkillTreeData.SkillNodeEntry>
            {
                MakeNode(0, new Vector2(50f, 50f)),
                MakeNode(1, new Vector2(0f, 3f)),
                MakeNode(2, new Vector2(5f, 8f))
            };
            Vector2 previousResolvedOnYAxisOfNodeA = new Vector2(3f, 3f);
            var previousSnap = new NodeSnapEngine.SnapResult(
                previousResolvedOnYAxisOfNodeA, NodeSnapEngine.SnapAxis.Y, 1);
            Vector2 candidate = new Vector2(4.96f, 3.05f);

            var result = NodeSnapEngine.Resolve(candidate, 0, nodes, 0.25f, 6f, previousSnap);

            Assert.That(result.SnappedAxis, Is.EqualTo(NodeSnapEngine.SnapAxis.Y));
            Assert.That(result.TargetNodeIndex, Is.EqualTo(1));
            Assert.That(result.CrossAxis, Is.EqualTo(NodeSnapEngine.SnapAxis.X));
            Assert.That(result.CrossTargetNodeIndex, Is.EqualTo(2));
            Assert.That(result.ResolvedPosition.x, Is.EqualTo(5f).Within(Tolerance));
            Assert.That(result.ResolvedPosition.y, Is.EqualTo(3f).Within(Tolerance));
        }

        [Test]
        public void Resolve_6Arg_CrossSnap_HeldLineCardinalAndCrossY_LocksToIntersection()
        {
            var nodes = new List<SkillTreeData.SkillNodeEntry>
            {
                MakeNode(0, new Vector2(50f, 50f)),
                MakeNode(1, new Vector2(2f, 0f)),
                MakeNode(2, new Vector2(2f, 4f)),
                MakeNode(3, new Vector2(8f, 5f))
            };
            Vector2 previousResolvedOnVerticalLineOfNodeA = new Vector2(2f, 3f);
            var previousSnap = new NodeSnapEngine.SnapResult(
                previousResolvedOnVerticalLineOfNodeA, NodeSnapEngine.SnapAxis.LineCardinal, 1);
            Vector2 candidate = new Vector2(2.05f, 4.96f);

            var result = NodeSnapEngine.Resolve(candidate, 0, nodes, 0.25f, 6f, previousSnap);

            Assert.That(result.SnappedAxis, Is.EqualTo(NodeSnapEngine.SnapAxis.LineCardinal));
            Assert.That(result.TargetNodeIndex, Is.EqualTo(1));
            Assert.That(result.CrossAxis, Is.EqualTo(NodeSnapEngine.SnapAxis.Y));
            Assert.That(result.CrossTargetNodeIndex, Is.EqualTo(3));
            Assert.That(result.ResolvedPosition.x, Is.EqualTo(2f).Within(Tolerance));
            Assert.That(result.ResolvedPosition.y, Is.EqualTo(5f).Within(Tolerance));
        }

        [Test]
        public void Resolve_6Arg_CrossSnap_HeldLineCollinearDiagonalAndCardinalY_LocksToIntersection()
        {
            var nodes = new List<SkillTreeData.SkillNodeEntry>
            {
                MakeNode(0, new Vector2(50f, 50f)),
                MakeNode(1, new Vector2(0f, 0f)),
                MakeNode(2, new Vector2(4f, 4f)),
                MakeNode(3, new Vector2(7f, 2f))
            };
            Vector2 previousResolvedOnDiagonalNearMidpoint = new Vector2(1.95f, 1.95f);
            var previousSnap = new NodeSnapEngine.SnapResult(
                previousResolvedOnDiagonalNearMidpoint, NodeSnapEngine.SnapAxis.LineCollinear, 1, 2);
            Vector2 candidate = new Vector2(1.96f, 2.04f);

            var result = NodeSnapEngine.Resolve(candidate, 0, nodes, 0.25f, 6f, previousSnap);

            Assert.That(result.SnappedAxis, Is.EqualTo(NodeSnapEngine.SnapAxis.LineCollinear));
            Assert.That(result.TargetNodeIndex, Is.EqualTo(1));
            Assert.That(result.SecondaryTargetNodeIndex, Is.EqualTo(2));
            Assert.That(result.CrossAxis, Is.EqualTo(NodeSnapEngine.SnapAxis.Y));
            Assert.That(result.CrossTargetNodeIndex, Is.EqualTo(3));
            Assert.That(result.ResolvedPosition.x, Is.EqualTo(2f).Within(Tolerance));
            Assert.That(result.ResolvedPosition.y, Is.EqualTo(2f).Within(Tolerance));
        }
    }
}
