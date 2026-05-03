using System.Collections.Generic;
using NUnit.Framework;
using RogueliteAutoBattler.Combat.Core;
using RogueliteAutoBattler.Data;
using RogueliteAutoBattler.Editor.Tools;
using UnityEngine;

namespace RogueliteAutoBattler.Tests.EditMode
{
    public class NodeSnapEngineTests
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
                statModifierValuePerLevel = 5f
            };
        }

        [Test]
        public void Resolve_NoOtherNodesInRange_ReturnsNoSnap()
        {
            var nodes = new List<SkillTreeData.SkillNodeEntry>
            {
                MakeNode(0, new Vector2(0f, 0f)),
                MakeNode(1, new Vector2(10f, 10f))
            };
            Vector2 candidate = new Vector2(0.5f, 0.5f);

            var result = NodeSnapEngine.Resolve(candidate, 0, nodes, 0.25f);

            Assert.That(result.SnappedAxis, Is.EqualTo(NodeSnapEngine.SnapAxis.None));
            Assert.That(result.ResolvedPosition, Is.EqualTo(candidate));
            Assert.That(result.TargetNodeIndex, Is.EqualTo(-1));
        }

        [Test]
        public void Resolve_NodeAlignedOnXWithinThreshold_SnapsX()
        {
            var nodes = new List<SkillTreeData.SkillNodeEntry>
            {
                MakeNode(0, new Vector2(3f, 0f)),
                MakeNode(1, new Vector2(3.1f, 5f))
            };
            Vector2 candidate = new Vector2(3.1f, 3f);

            var result = NodeSnapEngine.Resolve(candidate, 0, nodes, 0.25f);

            Assert.That(result.SnappedAxis, Is.EqualTo(NodeSnapEngine.SnapAxis.X));
            Assert.That(result.ResolvedPosition.x, Is.EqualTo(3.1f).Within(Tolerance));
            Assert.That(result.ResolvedPosition.y, Is.EqualTo(3f).Within(Tolerance));
        }

        [Test]
        public void Resolve_NodeAlignedOnYWithinThreshold_SnapsY()
        {
            var nodes = new List<SkillTreeData.SkillNodeEntry>
            {
                MakeNode(0, new Vector2(0f, 5f)),
                MakeNode(1, new Vector2(4f, 5.1f))
            };
            Vector2 candidate = new Vector2(2f, 5.1f);

            var result = NodeSnapEngine.Resolve(candidate, 0, nodes, 0.25f);

            Assert.That(result.SnappedAxis, Is.EqualTo(NodeSnapEngine.SnapAxis.Y));
            Assert.That(result.ResolvedPosition.x, Is.EqualTo(2f).Within(Tolerance));
            Assert.That(result.ResolvedPosition.y, Is.EqualTo(5.1f).Within(Tolerance));
        }

        [Test]
        public void Resolve_BothAxesWithinThreshold_PicksSmallerDelta()
        {
            var nodes = new List<SkillTreeData.SkillNodeEntry>
            {
                MakeNode(0, new Vector2(0f, 0f)),
                MakeNode(1, new Vector2(3.05f, 3.15f))
            };
            // candidate near node1: dx=0.05 (X), dy=0.15 (Y) — X wins (smaller)
            Vector2 candidate = new Vector2(3.1f, 3f);

            var result = NodeSnapEngine.Resolve(candidate, 0, nodes, 0.25f);

            Assert.That(result.SnappedAxis, Is.EqualTo(NodeSnapEngine.SnapAxis.X));
            Assert.That(result.ResolvedPosition.x, Is.EqualTo(3.05f).Within(Tolerance));
        }

        [Test]
        public void Resolve_OnlyDraggedNodeInList_ReturnsNoSnap()
        {
            var nodes = new List<SkillTreeData.SkillNodeEntry>
            {
                MakeNode(0, new Vector2(1f, 1f))
            };
            Vector2 candidate = new Vector2(1f, 1f);

            var result = NodeSnapEngine.Resolve(candidate, 0, nodes, 1f);

            Assert.That(result.SnappedAxis, Is.EqualTo(NodeSnapEngine.SnapAxis.None));
        }

        [Test]
        public void Resolve_ThresholdZero_ReturnsNoSnap()
        {
            var nodes = new List<SkillTreeData.SkillNodeEntry>
            {
                MakeNode(0, new Vector2(0f, 0f)),
                MakeNode(1, new Vector2(0f, 5f))
            };
            Vector2 candidate = new Vector2(0f, 2f);

            var result = NodeSnapEngine.Resolve(candidate, 0, nodes, 0f);

            Assert.That(result.SnappedAxis, Is.EqualTo(NodeSnapEngine.SnapAxis.None));
        }

        [Test]
        public void Resolve_ExactlyAtThreshold_DoesNotSnap()
        {
            var nodes = new List<SkillTreeData.SkillNodeEntry>
            {
                MakeNode(0, new Vector2(0f, 0f)),
                MakeNode(1, new Vector2(1f, 5f))
            };
            // candidate.x = 1.25f, node1.x = 1f → dx = 0.25 = threshold → strict < fails → no snap
            Vector2 candidate = new Vector2(1.25f, 2f);

            var result = NodeSnapEngine.Resolve(candidate, 0, nodes, 0.25f);

            Assert.That(result.SnappedAxis, Is.EqualTo(NodeSnapEngine.SnapAxis.None));
        }

        [Test]
        public void Resolve_ExcludesDraggedNodeIndex()
        {
            var nodes = new List<SkillTreeData.SkillNodeEntry>
            {
                MakeNode(0, new Vector2(2f, 2f)),
                MakeNode(1, new Vector2(10f, 10f))
            };
            // candidate is exactly at node 0's position; drag index is 0 — must not snap to itself
            Vector2 candidate = new Vector2(2f, 2f);

            var result = NodeSnapEngine.Resolve(candidate, 0, nodes, 1f);

            Assert.That(result.SnappedAxis, Is.EqualTo(NodeSnapEngine.SnapAxis.None));
        }

        [Test]
        public void Resolve_TargetNodeIndex_ReturnsCorrectIndex()
        {
            var nodes = new List<SkillTreeData.SkillNodeEntry>
            {
                MakeNode(0, new Vector2(0f, 0f)),
                MakeNode(1, new Vector2(5f, 0f)),
                MakeNode(2, new Vector2(5.1f, 3f))
            };
            // candidate near node 2 on X axis: dx = 0.1
            Vector2 candidate = new Vector2(5.1f, 1.5f);

            var result = NodeSnapEngine.Resolve(candidate, 0, nodes, 0.25f);

            Assert.That(result.SnappedAxis, Is.EqualTo(NodeSnapEngine.SnapAxis.X));
            Assert.That(result.TargetNodeIndex, Is.EqualTo(2));
        }
    }
}
