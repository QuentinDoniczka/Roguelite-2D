using System.Collections.Generic;
using NUnit.Framework;
using RogueliteAutoBattler.Combat.Core;
using RogueliteAutoBattler.Data;
using RogueliteAutoBattler.Editor.Tools;
using RogueliteAutoBattler.Editor.Windows;
using UnityEditor;
using UnityEngine;

namespace RogueliteAutoBattler.Tests.EditMode
{
    public class BranchPreviewPersistenceDesignerWiringTests
    {
        private const float Tolerance = 1e-4f;
        private const float StoredAngleDegrees = 137.25f;
        private const float ParentXForEastDefault = 1f;
        private const float ExpectedDefaultAngleEast = 90f;

        private SkillTreeData _data;
        private SkillTreeDesignerWindow _window;

        [SetUp]
        public void SetUp()
        {
            DeleteAllKeys();
            _data = ScriptableObject.CreateInstance<SkillTreeData>();
            _data.InitializeForTest(new List<SkillTreeData.SkillNodeEntry>
            {
                MakeNode(0, Vector2.zero),
                MakeNode(1, new Vector2(ParentXForEastDefault, 0f))
            });
            _window = ScriptableObject.CreateInstance<SkillTreeDesignerWindow>();
            _window.SetDataForTests(_data);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_window);
            Object.DestroyImmediate(_data);
            DeleteAllKeys();
        }

        private static void DeleteAllKeys()
        {
            EditorPrefs.DeleteKey(BranchPreviewPersistence.DistanceKey);
            EditorPrefs.DeleteKey(BranchPreviewPersistence.AngleKey);
            EditorPrefs.DeleteKey(BranchPreviewPersistence.MirrorEnabledKey);
            EditorPrefs.DeleteKey(MirrorAxisPersistence.EditorPrefKey);
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
        public void BeginBranchPreview_StoredAngleKey_PreservesStoredAngle()
        {
            Object.DestroyImmediate(_window);
            BranchPreviewPersistence.SaveAngle(StoredAngleDegrees);
            _window = ScriptableObject.CreateInstance<SkillTreeDesignerWindow>();
            _window.SetDataForTests(_data);

            _window.BeginBranchPreview(parentIndex: 1);

            Assert.That(_window.BranchPreviewSettingsForTests.angleDegrees, Is.EqualTo(StoredAngleDegrees).Within(Tolerance));
        }

        [Test]
        public void BeginBranchPreview_NoStoredAngle_RunsComputeDefaultAngle()
        {
            _window.BeginBranchPreview(parentIndex: 1);

            Assert.That(_window.BranchPreviewSettingsForTests.angleDegrees, Is.EqualTo(ExpectedDefaultAngleEast).Within(Tolerance));
        }
    }
}
