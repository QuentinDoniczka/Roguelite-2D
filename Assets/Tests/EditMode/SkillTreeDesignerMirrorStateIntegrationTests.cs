using System.Collections.Generic;
using NUnit.Framework;
using RogueliteAutoBattler.Data;
using RogueliteAutoBattler.Editor.Tools;
using RogueliteAutoBattler.Editor.Windows;
using RogueliteAutoBattler.Tests.EditMode.TestUtils;
using UnityEditor;
using UnityEngine;

namespace RogueliteAutoBattler.Tests.EditMode
{
    public class SkillTreeDesignerMirrorStateIntegrationTests
    {
        private const float Tolerance = 1e-4f;

        private SkillTreeData _data;
        private SkillTreeDesignerWindow _window;
        private bool _hadAngleKey;
        private float _savedAngleKey;
        private bool _hadMirrorAxisKey;
        private float _savedMirrorAxisKey;

        [SetUp]
        public void SetUp()
        {
            _hadAngleKey = EditorPrefs.HasKey(BranchPreviewSettingsPersistence.AngleKey);
            _savedAngleKey = EditorPrefs.GetFloat(BranchPreviewSettingsPersistence.AngleKey, 0f);
            _hadMirrorAxisKey = EditorPrefs.HasKey(MirrorAxisPersistence.EditorPrefKey);
            _savedMirrorAxisKey = EditorPrefs.GetFloat(MirrorAxisPersistence.EditorPrefKey, 0f);
            EditorPrefs.DeleteKey(BranchPreviewSettingsPersistence.AngleKey);
            EditorPrefs.DeleteKey(MirrorAxisPersistence.EditorPrefKey);

            _data = ScriptableObject.CreateInstance<SkillTreeData>();
            _data.InitializeForTest(new List<SkillTreeData.SkillNodeEntry>
            {
                SkillNodeEntryFactory.Default(0, Vector2.zero),
                SkillNodeEntryFactory.Default(1, new Vector2(2f, 0f)),
                SkillNodeEntryFactory.Default(2, new Vector2(0f, 3f))
            });
            _window = ScriptableObject.CreateInstance<SkillTreeDesignerWindow>();
            _window.SetDataForTests(_data);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_window);
            Object.DestroyImmediate(_data);

            if (_hadAngleKey)
                EditorPrefs.SetFloat(BranchPreviewSettingsPersistence.AngleKey, _savedAngleKey);
            else
                EditorPrefs.DeleteKey(BranchPreviewSettingsPersistence.AngleKey);
            if (_hadMirrorAxisKey)
                EditorPrefs.SetFloat(MirrorAxisPersistence.EditorPrefKey, _savedMirrorAxisKey);
            else
                EditorPrefs.DeleteKey(MirrorAxisPersistence.EditorPrefKey);
        }

        [Test]
        public void ComputeResolvedPreview_BeforeBegin_ReturnsZeros()
        {
            var (parentPos, mirrorBranchAngle) = _window.ComputeResolvedPreview();

            Assert.That(parentPos, Is.EqualTo(Vector2.zero));
            Assert.That(mirrorBranchAngle, Is.EqualTo(0f).Within(Tolerance));
        }

        [Test]
        public void ComputeResolvedPreview_AfterBegin_ReturnsParentPosFromSelectedNode()
        {
            _window.BeginBranchPreview(parentIndex: 1);

            var (parentPos, _) = _window.ComputeResolvedPreview();

            Assert.That(parentPos.x, Is.EqualTo(2f).Within(Tolerance));
            Assert.That(parentPos.y, Is.EqualTo(0f).Within(Tolerance));
        }

        [Test]
        public void ComputeResolvedPreview_AfterBegin_MirrorEnabled_MirrorBranchAngleDiffersFromMirrorDisabled()
        {
            _window.BeginBranchPreview(parentIndex: 1);

            _window.SetMirrorEnabled(false);
            var (_, mirrorBranchAngleDisabled) = _window.ComputeResolvedPreview();

            _window.SetMirrorEnabled(true);
            var (_, mirrorBranchAngleEnabled) = _window.ComputeResolvedPreview();

            Assert.That(mirrorBranchAngleEnabled, Is.Not.EqualTo(mirrorBranchAngleDisabled).Within(Tolerance));
        }

        [Test]
        public void EndBranchPreview_AfterBegin_ResetsToInactiveAndComputeResolvedReturnsZeros()
        {
            _window.BeginBranchPreview(parentIndex: 1);
            _window.EndBranchPreview();

            var (parentPos, mirrorBranchAngle) = _window.ComputeResolvedPreview();

            Assert.That(parentPos, Is.EqualTo(Vector2.zero));
            Assert.That(mirrorBranchAngle, Is.EqualTo(0f).Within(Tolerance));
        }
    }
}
