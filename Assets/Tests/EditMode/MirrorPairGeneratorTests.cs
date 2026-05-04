using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;
using RogueliteAutoBattler.Combat.Core;
using RogueliteAutoBattler.Data;
using RogueliteAutoBattler.Editor.Tools;
using UnityEngine;

namespace RogueliteAutoBattler.Tests.EditMode
{
    public class MirrorPairGeneratorTests
    {
        private const float Tolerance = 1e-4f;

        private SkillTreeData _data;

        [SetUp]
        public void SetUp()
        {
            _data = ScriptableObject.CreateInstance<SkillTreeData>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_data);
        }

        private static SkillTreeData.SkillNodeEntry MakeNode(int id, Vector2 position)
        {
            return new SkillTreeData.SkillNodeEntry
            {
                id = id,
                position = position,
                connectedNodeIds = new List<int>(),
                costType = SkillTreeData.CostType.Gold,
                maxLevel = 5,
                baseCost = 1,
                costMultiplierOdd = 1f,
                costMultiplierEven = 1f,
                costAdditivePerLevel = 0,
                statModifierType = StatType.Hp,
                statModifierMode = SkillTreeData.StatModifierMode.Flat,
                statModifierValuePerLevel = 5f
            };
        }

        private static SkillTreeData.SkillNodeEntry MakeCentral()
        {
            return MakeNode(SkillTreeData.CentralNodeId, Vector2.zero);
        }

        [Test]
        public void TryGenerate_MirrorDisabled_CreatesOnlyOriginal_ReturnsOriginalOnly()
        {
            var central = MakeCentral();
            var parent = MakeNode(1, new Vector2(0f, 3f));
            _data.InitializeForTest(new List<SkillTreeData.SkillNodeEntry> { central, parent });
            int initialCount = _data.Nodes.Count;

            int parentIndex = 1;
            var result = MirrorPairGenerator.TryGenerate(
                _data,
                parentIndex,
                distance: 2f,
                resolvedAngleDegrees: 90f,
                mirrorEnabled: false,
                mirrorSourcePosition: parent.position,
                mirrorBranchAngleDegrees: 90f);

            Assert.IsTrue(result.OriginalCreated);
            Assert.IsFalse(result.MirrorCreated);
            Assert.IsTrue(string.IsNullOrEmpty(result.WarningMessage));
            Assert.AreEqual(initialCount + 1, _data.Nodes.Count);
        }

        [Test]
        public void TryGenerate_MirrorEnabled_NoCollision_CreatesPair_BothChildrenOfParent()
        {
            var central = MakeCentral();
            var parent = MakeNode(1, new Vector2(0f, 3f));
            _data.InitializeForTest(new List<SkillTreeData.SkillNodeEntry> { central, parent });
            int initialCount = _data.Nodes.Count;

            int parentIndex = 1;
            int parentId = parent.id;
            float mirrorAngle = BranchPlacement.MirrorAngle(60f, 0f);
            var result = MirrorPairGenerator.TryGenerate(
                _data,
                parentIndex,
                distance: 2f,
                resolvedAngleDegrees: 60f,
                mirrorEnabled: true,
                mirrorSourcePosition: parent.position,
                mirrorBranchAngleDegrees: mirrorAngle);

            Assert.IsTrue(result.OriginalCreated);
            Assert.IsTrue(result.MirrorCreated);
            Assert.AreEqual(initialCount + 2, _data.Nodes.Count);

            Vector2 expectedMirrorPos = SkillTreeGrid.Quantize(BranchPlacement.ComputeBranchPosition(new Vector2(0f, 3f), 2f, mirrorAngle));
            var lastNode = _data.Nodes[_data.Nodes.Count - 1];
            Assert.That(lastNode.position.x, Is.EqualTo(expectedMirrorPos.x).Within(Tolerance));
            Assert.That(lastNode.position.y, Is.EqualTo(expectedMirrorPos.y).Within(Tolerance));

            var edges = _data.GetEdges();
            int edgesFromParentToNewIds = 0;
            foreach (var (fromId, toId) in edges)
            {
                if (fromId == parentId && (toId == result.OriginalNewId || toId == result.MirrorNewId))
                    edgesFromParentToNewIds++;
            }
            Assert.AreEqual(2, edgesFromParentToNewIds);
        }

        [Test]
        public void TryGenerate_MirrorEnabled_CollisionAtMirrorPosition_OriginalOnly_WithWarning()
        {
            var central = MakeCentral();
            var parent = MakeNode(1, new Vector2(0f, 3f));
            _data.InitializeForTest(new List<SkillTreeData.SkillNodeEntry> { central, parent });

            float mirrorAngle = BranchPlacement.MirrorAngle(60f, 0f);
            Vector2 expectedMirrorPos = BranchPlacement.ComputeBranchPosition(new Vector2(0f, 3f), 2f, mirrorAngle);
            var blocker = MakeNode(99, expectedMirrorPos);
            _data.AddBranchNode(blocker, SkillTreeData.CentralNodeId);

            int initialCount = _data.Nodes.Count;

            int parentIndex = 1;
            var result = MirrorPairGenerator.TryGenerate(
                _data,
                parentIndex,
                distance: 2f,
                resolvedAngleDegrees: 60f,
                mirrorEnabled: true,
                mirrorSourcePosition: parent.position,
                mirrorBranchAngleDegrees: mirrorAngle);

            Assert.IsTrue(result.OriginalCreated);
            Assert.IsFalse(result.MirrorCreated);
            Assert.AreEqual(initialCount + 1, _data.Nodes.Count);
            string expectedWarning = string.Format(CultureInfo.InvariantCulture, MirrorPairGenerator.MirrorSkippedWarningFormat, BranchPlacement.PositionTolerance);
            Assert.AreEqual(expectedWarning, result.WarningMessage);
        }

        [Test]
        public void TryGenerate_CollisionWithinToleranceButNotExact_StillSkipsMirror()
        {
            var central = MakeCentral();
            var parent = MakeNode(1, new Vector2(0f, 3f));
            _data.InitializeForTest(new List<SkillTreeData.SkillNodeEntry> { central, parent });

            float mirrorAngle = BranchPlacement.MirrorAngle(60f, 0f);
            Vector2 expectedMirrorPos = BranchPlacement.ComputeBranchPosition(new Vector2(0f, 3f), 2f, mirrorAngle);
            var blocker = MakeNode(99, expectedMirrorPos + new Vector2(0.5f, 0f));
            _data.AddBranchNode(blocker, SkillTreeData.CentralNodeId);

            int initialCount = _data.Nodes.Count;

            int parentIndex = 1;
            var result = MirrorPairGenerator.TryGenerate(
                _data,
                parentIndex,
                distance: 2f,
                resolvedAngleDegrees: 60f,
                mirrorEnabled: true,
                mirrorSourcePosition: parent.position,
                mirrorBranchAngleDegrees: mirrorAngle);

            Assert.IsTrue(result.OriginalCreated);
            Assert.IsFalse(result.MirrorCreated);
            Assert.AreEqual(initialCount + 1, _data.Nodes.Count);
            string expectedWarning = string.Format(CultureInfo.InvariantCulture, MirrorPairGenerator.MirrorSkippedWarningFormat, BranchPlacement.PositionTolerance);
            Assert.AreEqual(expectedWarning, result.WarningMessage);
        }

        [Test]
        public void TryGenerate_BlockerOutsideTolerance_PairCreated()
        {
            var central = MakeCentral();
            var parent = MakeNode(1, new Vector2(0f, 3f));
            _data.InitializeForTest(new List<SkillTreeData.SkillNodeEntry> { central, parent });

            float mirrorAngle = BranchPlacement.MirrorAngle(60f, 0f);
            Vector2 expectedMirrorPos = BranchPlacement.ComputeBranchPosition(new Vector2(0f, 3f), 2f, mirrorAngle);
            var blocker = MakeNode(99, expectedMirrorPos + new Vector2(1.5f, 0f));
            _data.AddBranchNode(blocker, SkillTreeData.CentralNodeId);

            int parentIndex = 1;
            var result = MirrorPairGenerator.TryGenerate(
                _data,
                parentIndex,
                distance: 2f,
                resolvedAngleDegrees: 60f,
                mirrorEnabled: true,
                mirrorSourcePosition: parent.position,
                mirrorBranchAngleDegrees: mirrorAngle);

            Assert.IsTrue(result.OriginalCreated);
            Assert.IsTrue(result.MirrorCreated);
            Assert.IsTrue(string.IsNullOrEmpty(result.WarningMessage));
        }

        [Test]
        public void TryGenerate_OnAxisAngle_PairWouldOverlap_SkipsMirror()
        {
            var central = MakeCentral();
            var parent = MakeNode(1, new Vector2(0f, 3f));
            _data.InitializeForTest(new List<SkillTreeData.SkillNodeEntry> { central, parent });
            int initialCount = _data.Nodes.Count;

            int parentIndex = 1;
            float mirrorAngle = BranchPlacement.MirrorAngle(0f, 0f);
            var result = MirrorPairGenerator.TryGenerate(
                _data,
                parentIndex,
                distance: 2f,
                resolvedAngleDegrees: 0f,
                mirrorEnabled: true,
                mirrorSourcePosition: parent.position,
                mirrorBranchAngleDegrees: mirrorAngle);

            Assert.IsTrue(result.OriginalCreated);
            Assert.IsFalse(result.MirrorCreated);
            Assert.AreEqual(initialCount + 1, _data.Nodes.Count);
            string expectedWarning = string.Format(CultureInfo.InvariantCulture, MirrorPairGenerator.MirrorSkippedWarningFormat, BranchPlacement.PositionTolerance);
            Assert.AreEqual(expectedWarning, result.WarningMessage);
        }

        [Test]
        public void TryGenerate_AssignsSequentialIds()
        {
            var central = MakeCentral();
            var parent = MakeNode(5, new Vector2(0f, 3f));
            _data.InitializeForTest(new List<SkillTreeData.SkillNodeEntry> { central, parent });

            int parentIndex = 1;
            float mirrorAngle = BranchPlacement.MirrorAngle(60f, 0f);
            var result = MirrorPairGenerator.TryGenerate(
                _data,
                parentIndex,
                distance: 2f,
                resolvedAngleDegrees: 60f,
                mirrorEnabled: true,
                mirrorSourcePosition: parent.position,
                mirrorBranchAngleDegrees: mirrorAngle);

            Assert.AreEqual(6, result.OriginalNewId);
            Assert.AreEqual(7, result.MirrorNewId);
        }

        [Test]
        public void TryGenerate_DataNull_ReturnsInvalid()
        {
            var result = MirrorPairGenerator.TryGenerate(
                null,
                parentIndex: 0,
                distance: 1f,
                resolvedAngleDegrees: 0f,
                mirrorEnabled: false,
                mirrorSourcePosition: Vector2.zero,
                mirrorBranchAngleDegrees: 0f);

            Assert.IsFalse(result.OriginalCreated);
            Assert.IsFalse(result.MirrorCreated);
        }

        [Test]
        public void TryGenerate_InvalidParentIndex_ReturnsInvalid()
        {
            var central = MakeCentral();
            _data.InitializeForTest(new List<SkillTreeData.SkillNodeEntry> { central });
            int initialCount = _data.Nodes.Count;

            var result = MirrorPairGenerator.TryGenerate(
                _data,
                parentIndex: 999,
                distance: 1f,
                resolvedAngleDegrees: 0f,
                mirrorEnabled: false,
                mirrorSourcePosition: Vector2.zero,
                mirrorBranchAngleDegrees: 0f);

            Assert.IsFalse(result.OriginalCreated);
            Assert.IsFalse(result.MirrorCreated);
            Assert.AreEqual(initialCount, _data.Nodes.Count);
        }

        [Test]
        public void HasCollisionAt_NoNodes_ReturnsFalse()
        {
            var nodes = new List<SkillTreeData.SkillNodeEntry>();

            bool collision = MirrorPairGenerator.HasCollisionAt(nodes, new Vector2(5f, 5f), 1f);

            Assert.IsFalse(collision);
        }

        [Test]
        public void HasCollisionAt_NodeExactlyAtTolerance_ReturnsFalse()
        {
            var nodes = new List<SkillTreeData.SkillNodeEntry>
            {
                MakeNode(0, new Vector2(1f, 0f))
            };

            bool collision = MirrorPairGenerator.HasCollisionAt(nodes, Vector2.zero, 1f);

            Assert.IsFalse(collision);
        }

        [Test]
        public void HasCollisionAt_NodeJustInsideTolerance_ReturnsTrue()
        {
            var nodes = new List<SkillTreeData.SkillNodeEntry>
            {
                MakeNode(0, new Vector2(0.999f, 0f))
            };

            bool collision = MirrorPairGenerator.HasCollisionAt(nodes, Vector2.zero, 1f);

            Assert.IsTrue(collision);
        }

        [Test]
        public void UndoLabel_ForMirroredPair_MatchesSpec()
        {
            Assert.AreEqual("Create Branch Node Pair (Mirrored)", MirrorPairGenerator.UndoLabelMirroredPair);
        }

        [Test]
        public void UndoLabel_ForSingleNode_MatchesExisting()
        {
            Assert.AreEqual("Create Branch Node", MirrorPairGenerator.UndoLabelSingleNode);
        }
    }
}
