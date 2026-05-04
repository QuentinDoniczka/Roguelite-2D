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
        public void ComputeResolvedPreview_BeforeBegin_ReturnsZeros()
        {
            var (parentPos, resolvedAngle, mirrorBranchAngle) = _window.ComputeResolvedPreview();

            Assert.That(parentPos, Is.EqualTo(Vector2.zero));
            Assert.That(resolvedAngle, Is.EqualTo(0f).Within(Tolerance));
            Assert.That(mirrorBranchAngle, Is.EqualTo(0f).Within(Tolerance));
        }

        [Test]
        public void ComputeResolvedPreview_AfterBegin_ReturnsParentPosFromSelectedNode()
        {
            _window.BeginBranchPreview(parentIndex: 1);

            var (parentPos, _, _) = _window.ComputeResolvedPreview();

            Assert.That(parentPos.x, Is.EqualTo(2f).Within(Tolerance));
            Assert.That(parentPos.y, Is.EqualTo(0f).Within(Tolerance));
        }

        [Test]
        public void ComputeResolvedPreview_AfterBegin_MirrorDisabled_ResolvedAngleEqualsMirrorBranchAngle()
        {
            _window.BeginBranchPreview(parentIndex: 1);
            _window.SetMirrorEnabled(false);

            var (_, resolvedAngle, mirrorBranchAngle) = _window.ComputeResolvedPreview();

            Assert.That(mirrorBranchAngle, Is.EqualTo(resolvedAngle).Within(Tolerance));
        }

        [Test]
        public void ComputeResolvedPreview_MirrorToggleDoesNotChangeResolvedAngle()
        {
            _window.BeginBranchPreview(parentIndex: 1);

            _window.SetMirrorEnabled(false);
            var (_, resolvedAngleDisabled, _) = _window.ComputeResolvedPreview();

            _window.SetMirrorEnabled(true);
            var (_, resolvedAngleEnabled, _) = _window.ComputeResolvedPreview();

            Assert.That(resolvedAngleEnabled, Is.EqualTo(resolvedAngleDisabled).Within(Tolerance));
        }

        [Test]
        public void EndBranchPreview_AfterBegin_ResetsToInactiveAndComputeResolvedReturnsZeros()
        {
            _window.BeginBranchPreview(parentIndex: 1);
            _window.EndBranchPreview();

            var (parentPos, resolvedAngle, mirrorBranchAngle) = _window.ComputeResolvedPreview();

            Assert.That(parentPos, Is.EqualTo(Vector2.zero));
            Assert.That(resolvedAngle, Is.EqualTo(0f).Within(Tolerance));
            Assert.That(mirrorBranchAngle, Is.EqualTo(0f).Within(Tolerance));
        }
    }
}
