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

            _window.BeginBranchPreview(parentIndex: 1);

            Assert.AreEqual(-1, _window.MirrorSourceNodeIndexForTests);
            Assert.IsFalse(_window.AngleRelativeToAxisForTests);
        }

        [Test]
        public void SetMirrorEnabled_FalseToTrue_ResetsAngleRelativeToAxis()
        {
            _window.BeginBranchPreview(parentIndex: 1);
            _window.AngleRelativeToAxisForTests = true;

            _window.SetMirrorEnabled(false);
            _window.SetMirrorEnabled(true);

            Assert.IsFalse(_window.AngleRelativeToAxisForTests);
        }

        [Test]
        public void ComputeResolvedPreview_WithOverrideAndRelative_ProducesExpectedValues()
        {
            _window.BeginBranchPreview(parentIndex: 1);
            _window.MirrorSourceNodeIndexForTests = 2;
            _window.BranchPreviewSettingsForTests.angleDegrees = 30f;
            _window.BranchPreviewSettingsForTests.mirrorAxisDegrees = 45f;
            _window.AngleRelativeToAxisForTests = true;

            var (workingPos, mirrorSourcePos, resolvedAngle, mirrorAngle) = _window.ComputeResolvedPreview();

            Assert.That(workingPos.x, Is.EqualTo(2f).Within(Tolerance));
            Assert.That(workingPos.y, Is.EqualTo(0f).Within(Tolerance));
            Assert.That(mirrorSourcePos.x, Is.EqualTo(0f).Within(Tolerance));
            Assert.That(mirrorSourcePos.y, Is.EqualTo(3f).Within(Tolerance));
            Assert.That(resolvedAngle, Is.EqualTo(75f).Within(Tolerance));
            Assert.That(mirrorAngle, Is.EqualTo(BranchPlacement.MirrorAngle(75f, 45f)).Within(Tolerance));
        }

        [Test]
        public void EndBranchPreview_ResetsMirrorSourceIndex()
        {
            _window.BeginBranchPreview(parentIndex: 1);
            _window.MirrorSourceNodeIndexForTests = 2;

            _window.EndBranchPreview();

            Assert.AreEqual(-1, _window.MirrorSourceNodeIndexForTests);
        }

        [Test]
        public void ClampMirrorSourceIndex_OutOfRangeAfterShrink_ResetsToMinusOne()
        {
            _window.BeginBranchPreview(parentIndex: 1);
            _window.MirrorSourceNodeIndexForTests = 2;
            _data.InitializeForTest(new List<SkillTreeData.SkillNodeEntry>
            {
                MakeNode(0, Vector2.zero),
                MakeNode(1, new Vector2(2f, 0f))
            });

            _window.ClampMirrorSourceIndex();

            Assert.AreEqual(-1, _window.MirrorSourceNodeIndexForTests);
        }

        [Test]
        public void ComputeResolvedPreview_OverrideOutOfRange_FallsBackToWorkingPos()
        {
            _window.BeginBranchPreview(parentIndex: 1);
            _window.MirrorSourceNodeIndexForTests = 99;

            var (workingPos, mirrorSourcePos, _, _) = _window.ComputeResolvedPreview();

            Assert.That(mirrorSourcePos.x, Is.EqualTo(workingPos.x).Within(Tolerance));
            Assert.That(mirrorSourcePos.y, Is.EqualTo(workingPos.y).Within(Tolerance));
        }
    }
}
