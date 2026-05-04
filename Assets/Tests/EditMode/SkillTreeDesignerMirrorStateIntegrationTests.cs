using System.Collections.Generic;
using NUnit.Framework;
using RogueliteAutoBattler.Combat.Core;
using RogueliteAutoBattler.Data;
using RogueliteAutoBattler.Editor.Tools;
using RogueliteAutoBattler.Editor.Windows;
using UnityEngine;

namespace RogueliteAutoBattler.Tests.EditMode
{
    public class SkillTreeDesignerMirrorStateIntegrationTests
    {
        private const float Tolerance = 1e-4f;

        private SkillTreeData _data;
        private SkillTreeDesignerWindow _window;

        [SetUp]
        public void SetUp()
        {
            _data = ScriptableObject.CreateInstance<SkillTreeData>();
            _data.InitializeForTest(new List<SkillTreeData.SkillNodeEntry>
            {
                MakeNode(0, Vector2.zero),
                MakeNode(1, new Vector2(2f, 0f)),
                MakeNode(2, new Vector2(0f, 3f))
            });
            _window = ScriptableObject.CreateInstance<SkillTreeDesignerWindow>();
            _window.SetDataForTests(_data);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_window);
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
        public void BeginBranchPreview_ProducesDefaultMirrorSourceAndRelativeFlag()
        {
            _window.MirrorSourceNodeIndexForTests = 2;
            _window.AngleIsRelativeToMirrorAxisForTests = true;

            _window.BeginBranchPreview(parentIndex: 1);

            Assert.AreEqual(BranchPlacement.NoMirrorSourceOverride, _window.MirrorSourceNodeIndexForTests);
            Assert.IsFalse(_window.AngleIsRelativeToMirrorAxisForTests);
        }

        [Test]
        public void SetMirrorEnabled_FalseToTrue_ResetsAngleRelativeToAxis()
        {
            _window.BeginBranchPreview(parentIndex: 1);
            _window.AngleIsRelativeToMirrorAxisForTests = true;

            _window.SetMirrorEnabled(false);
            _window.SetMirrorEnabled(true);

            Assert.IsFalse(_window.AngleIsRelativeToMirrorAxisForTests);
        }

        [Test]
        public void ResolveBranchPlan_WithOverrideAndRelative_ProducesExpectedValues()
        {
            var nodes = new List<SkillTreeData.SkillNodeEntry>
            {
                MakeNode(0, Vector2.zero),
                MakeNode(1, new Vector2(2f, 0f)),
                MakeNode(2, new Vector2(0f, 3f))
            };

            var (parentPos, mirrorSourcePos, resolvedAngle, mirrorBranchAngle) = BranchPlacement.ResolveBranchPlan(
                nodes,
                parentIndex: 1,
                mirrorSourceOverrideIndex: 2,
                angleDegrees: 30f,
                mirrorAxisDegrees: 45f,
                angleIsRelativeToMirrorAxis: true,
                mirrorEnabled: true);

            Assert.That(parentPos.x, Is.EqualTo(2f).Within(Tolerance));
            Assert.That(parentPos.y, Is.EqualTo(0f).Within(Tolerance));
            Assert.That(mirrorSourcePos.x, Is.EqualTo(0f).Within(Tolerance));
            Assert.That(mirrorSourcePos.y, Is.EqualTo(3f).Within(Tolerance));
            Assert.That(resolvedAngle, Is.EqualTo(75f).Within(Tolerance));
            Assert.That(mirrorBranchAngle, Is.EqualTo(BranchPlacement.MirrorAngle(75f, 45f)).Within(Tolerance));
        }

        [Test]
        public void EndBranchPreview_ResetsMirrorSourceIndex()
        {
            _window.BeginBranchPreview(parentIndex: 1);
            _window.MirrorSourceNodeIndexForTests = 2;

            _window.EndBranchPreview();

            Assert.AreEqual(BranchPlacement.NoMirrorSourceOverride, _window.MirrorSourceNodeIndexForTests);
        }

        [Test]
        public void EndBranchPreview_ResetsAngleIsRelativeToMirrorAxis()
        {
            _window.BeginBranchPreview(parentIndex: 1);
            _window.AngleIsRelativeToMirrorAxisForTests = true;

            _window.EndBranchPreview();

            Assert.IsFalse(_window.AngleIsRelativeToMirrorAxisForTests);
        }

        [Test]
        public void InvalidateMirrorSourceIfOutOfRange_OutOfRangeAfterShrink_ResetsToNoOverride()
        {
            _window.BeginBranchPreview(parentIndex: 1);
            _window.MirrorSourceNodeIndexForTests = 2;
            _data.InitializeForTest(new List<SkillTreeData.SkillNodeEntry>
            {
                MakeNode(0, Vector2.zero),
                MakeNode(1, new Vector2(2f, 0f))
            });

            _window.InvalidateMirrorSourceIfOutOfRange();

            Assert.AreEqual(BranchPlacement.NoMirrorSourceOverride, _window.MirrorSourceNodeIndexForTests);
        }

        [Test]
        public void ComputeResolvedPreview_OverrideOutOfRange_FallsBackToParentPos()
        {
            _window.BeginBranchPreview(parentIndex: 1);
            _window.MirrorSourceNodeIndexForTests = 99;

            var (parentPos, mirrorSourcePos, _, _) = _window.ComputeResolvedPreview();

            Assert.That(mirrorSourcePos.x, Is.EqualTo(parentPos.x).Within(Tolerance));
            Assert.That(mirrorSourcePos.y, Is.EqualTo(parentPos.y).Within(Tolerance));
        }
    }
}
