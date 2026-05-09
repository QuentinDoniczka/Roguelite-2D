#if UNITY_EDITOR
using System.Collections.Generic;
using NUnit.Framework;
using RogueliteAutoBattler.Combat.Core;
using RogueliteAutoBattler.Data;
using RogueliteAutoBattler.Editor.Windows;
using RogueliteAutoBattler.Tests.PlayMode;
using RogueliteAutoBattler.UI.Toolkit.SkillTree;
using UnityEditor;
using UnityEngine;

namespace RogueliteAutoBattler.Tests.EditMode
{
    [TestFixture]
    public class SkillTreeDesignerTopLevelTabTests
    {
        private SkillTreeData _data;
        private SkillTreeDesignerWindow _window;
        private bool _hadPriorPref;
        private int _priorPrefValue;

        [SetUp]
        public void SetUp()
        {
            string key = SkillTreeDesignerWindow.TopLevelTabEditorPrefKeyForTests;
            _hadPriorPref = EditorPrefs.HasKey(key);
            _priorPrefValue = EditorPrefs.GetInt(key, 0);
            EditorPrefs.DeleteKey(key);

            _data = ScriptableObject.CreateInstance<SkillTreeData>();
            _data.InitializeForTest(new List<SkillTreeData.SkillNodeEntry>
            {
                MakeNode(0, Vector2.zero),
                MakeNode(1, new Vector2(2f, 0f))
            });
            _window = ScriptableObject.CreateInstance<SkillTreeDesignerWindow>();
            _window.SetDataForTests(_data);
        }

        [TearDown]
        public void TearDown()
        {
            if (_window != null) Object.DestroyImmediate(_window);
            if (_data != null) Object.DestroyImmediate(_data);

            string key = SkillTreeDesignerWindow.TopLevelTabEditorPrefKeyForTests;
            if (_hadPriorPref)
                EditorPrefs.SetInt(key, _priorPrefValue);
            else
                EditorPrefs.DeleteKey(key);
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
        public void TopLevelTabLabels_WhenAccessed_ContainsDesignerThenVisualInOrder()
        {
            var labels = SkillTreeDesignerWindow.TopLevelTabLabelsForTests;
            Assert.IsNotNull(labels, "TopLevelTabLabels must not be null.");
            Assert.AreEqual(2, labels.Count, "TopLevelTabLabels must contain exactly two entries.");
            Assert.AreEqual("Designer", labels[0], "First top-level tab label must be 'Designer'.");
            Assert.AreEqual("Visual", labels[1], "Second top-level tab label must be 'Visual'.");
        }

        [Test]
        public void BeginBranchPreview_ForcesTopLevelToDesigner_AndCapturesPrevious()
        {
            _window.TopLevelTabForTests = SkillTreeDesignerWindow.TopLevelTab.Visual;

            _window.InvokeBeginBranchPreviewForTests(parentNodeIndex: 0);

            Assert.AreEqual(
                SkillTreeDesignerWindow.TopLevelTab.Designer,
                _window.TopLevelTabForTests,
                "BeginBranchPreview must force the top-level tab to Designer.");
            Assert.IsTrue(
                _window.BranchPreviewActiveForTests,
                "Branch preview must be active after BeginBranchPreview.");

            _window.InvokeCancelBranchPreviewForTests();
        }

        [Test]
        public void CancelBranchPreview_RestoresPreviousTopLevel()
        {
            _window.TopLevelTabForTests = SkillTreeDesignerWindow.TopLevelTab.Visual;

            _window.InvokeBeginBranchPreviewForTests(parentNodeIndex: 0);

            Assert.AreEqual(
                SkillTreeDesignerWindow.TopLevelTab.Designer,
                _window.TopLevelTabForTests,
                "Sanity: top-level tab must be Designer during branch preview.");

            _window.InvokeCancelBranchPreviewForTests();

            Assert.AreEqual(
                SkillTreeDesignerWindow.TopLevelTab.Visual,
                _window.TopLevelTabForTests,
                "CancelBranchPreview must restore the previous top-level tab (Visual).");
            Assert.IsFalse(
                _window.BranchPreviewActiveForTests,
                "Branch preview must no longer be active after CancelBranchPreview.");
        }

        [Test]
        public void CancelBranchPreview_FromDesigner_KeepsTopLevelOnDesigner()
        {
            _window.TopLevelTabForTests = SkillTreeDesignerWindow.TopLevelTab.Designer;

            _window.InvokeBeginBranchPreviewForTests(parentNodeIndex: 0);
            _window.InvokeCancelBranchPreviewForTests();

            Assert.AreEqual(
                SkillTreeDesignerWindow.TopLevelTab.Designer,
                _window.TopLevelTabForTests,
                "Cancel must restore the captured Designer top-level tab.");
        }

        [Test]
        public void EditorPrefs_LoadTopLevelTab_ReadsVisualWhenStored()
        {
            string key = SkillTreeDesignerWindow.TopLevelTabEditorPrefKeyForTests;
            EditorPrefs.SetInt(key, (int)SkillTreeDesignerWindow.TopLevelTab.Visual);

            _window.TopLevelTabForTests = SkillTreeDesignerWindow.TopLevelTab.Designer;
            _window.InvokeLoadTopLevelTabFromPrefsForTests();

            Assert.AreEqual(
                SkillTreeDesignerWindow.TopLevelTab.Visual,
                _window.TopLevelTabForTests,
                "LoadTopLevelTabFromPrefs must read the Visual value from EditorPrefs.");
        }

        [Test]
        public void EditorPrefs_LoadTopLevelTab_DefaultsToDesignerWhenAbsent()
        {
            string key = SkillTreeDesignerWindow.TopLevelTabEditorPrefKeyForTests;
            Assert.IsFalse(EditorPrefs.HasKey(key), "Test precondition: pref key must be absent (cleared in SetUp).");

            _window.TopLevelTabForTests = SkillTreeDesignerWindow.TopLevelTab.Visual;
            _window.InvokeLoadTopLevelTabFromPrefsForTests();

            Assert.AreEqual(
                SkillTreeDesignerWindow.TopLevelTab.Designer,
                _window.TopLevelTabForTests,
                "LoadTopLevelTabFromPrefs must default to Designer when EditorPrefs key is absent.");
        }

        [Test]
        public void EditorPrefs_SaveTopLevelTab_WritesVisualValue()
        {
            string key = SkillTreeDesignerWindow.TopLevelTabEditorPrefKeyForTests;

            _window.TopLevelTabForTests = SkillTreeDesignerWindow.TopLevelTab.Visual;
            _window.InvokeSaveTopLevelTabToPrefsForTests();

            Assert.IsTrue(EditorPrefs.HasKey(key), "Save must create the EditorPrefs key.");
            Assert.AreEqual(
                (int)SkillTreeDesignerWindow.TopLevelTab.Visual,
                EditorPrefs.GetInt(key, -1),
                "Save must persist the Visual enum value.");
        }

        [Test]
        public void EditorPrefs_RoundTripTopLevelTab()
        {
            string key = SkillTreeDesignerWindow.TopLevelTabEditorPrefKeyForTests;
            EditorPrefs.SetInt(key, (int)SkillTreeDesignerWindow.TopLevelTab.Visual);

            _window.InvokeLoadTopLevelTabFromPrefsForTests();
            Assert.AreEqual(
                SkillTreeDesignerWindow.TopLevelTab.Visual,
                _window.TopLevelTabForTests,
                "Round-trip step 1: loaded tab must be Visual.");

            _window.TopLevelTabForTests = SkillTreeDesignerWindow.TopLevelTab.Designer;
            _window.InvokeSaveTopLevelTabToPrefsForTests();

            Assert.AreEqual(
                (int)SkillTreeDesignerWindow.TopLevelTab.Designer,
                EditorPrefs.GetInt(key, -1),
                "Round-trip step 2: saved value in EditorPrefs must be Designer.");
        }

        [Test]
        public void VisualTab_Initialize_PopulatesSerializedObject()
        {
            using var scope = new SkillTreeVisualSettingsProviderScope();

            var presenter = _window.VisualTabPresenterForTests;
            presenter.InvokeInitializeForTests();

            Assert.IsNotNull(
                presenter.SerializedObjectForTests,
                "SerializedObject must be populated after Initialize.");
            Assert.AreEqual(
                presenter.SettingsForTests,
                presenter.SerializedObjectForTests.targetObject,
                "SerializedObject target must be the resolved SkillTreeVisualSettings asset.");
        }

        [Test]
        public void VisualTab_Initialize_PreviewPanelInstance_NotNull()
        {
            using var scope = new SkillTreeVisualSettingsProviderScope();

            var presenter = _window.VisualTabPresenterForTests;
            presenter.InvokeInitializeForTests();

            Assert.IsNotNull(
                presenter.PreviewPanelForTests,
                "Visual tab preview panel must be non-null after Initialize.");
        }

        [Test]
        public void VisualTab_OnSettingsChanged_MarksAssetDirty_AndResetsCache()
        {
            using var scope = new SkillTreeVisualSettingsProviderScope();
            var presenter = _window.VisualTabPresenterForTests;
            presenter.InvokeInitializeForTests();

            var so = presenter.SerializedObjectForTests;
            var prop = so.FindProperty(SkillTreeVisualSettings.FieldNames.HaloSize);
            float orig = prop.floatValue;

            int providerCallCount = 0;
            var stub = scope.Stub;
            SkillTreeVisualSettingsResolver.Provider = () =>
            {
                providerCallCount++;
                return stub;
            };
            SkillTreeVisualSettingsResolver.ResetCache();

            try
            {
                prop.floatValue = orig + 1f;
                so.ApplyModifiedProperties();

                int callCountBeforeChange = providerCallCount;
                presenter.InvokeOnSettingsChangedForTests();

                Assert.IsTrue(
                    EditorUtility.IsDirty(stub),
                    "SkillTreeVisualSettings must be marked dirty after OnSettingsChanged.");

                SkillTreeVisualSettingsResolver.Get();
                Assert.Greater(
                    providerCallCount,
                    callCountBeforeChange,
                    "Resolver cache must be reset by OnSettingsChanged so the provider is re-invoked.");
            }
            finally
            {
                prop.floatValue = orig;
                so.ApplyModifiedProperties();
                EditorUtility.ClearDirty(stub);
            }
        }
    }
}
#endif
