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
    public class SkillTreeDesignerVisualSubTabTests
    {
        private SkillTreeData _data;
        private SkillTreeDesignerWindow _window;

        [SetUp]
        public void SetUp()
        {
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
        public void SubTabLabels_FirstIsVisual_ThenSkillTreeAndNode()
        {
            var labels = SkillTreeDesignerWindow.TabLabelsWithoutBranchForTests;
            Assert.IsNotNull(labels, "TabLabelsWithoutBranch must not be null.");
            Assert.GreaterOrEqual(labels.Count, 3, "TabLabelsWithoutBranch must contain at least three entries.");
            Assert.AreEqual("Visual", labels[0], "First sub-tab label must be 'Visual'.");
            Assert.AreEqual("Skill Tree", labels[1], "Second sub-tab label must be 'Skill Tree'.");
            Assert.AreEqual("Node", labels[2], "Third sub-tab label must be 'Node'.");
        }

        [Test]
        public void SubTabLabels_WithBranch_VisualStillFirst_BranchLast()
        {
            var labels = SkillTreeDesignerWindow.TabLabelsWithBranchForTests;
            Assert.IsNotNull(labels, "TabLabelsWithBranch must not be null.");
            Assert.AreEqual("Visual", labels[0], "First sub-tab label with branch must be 'Visual'.");
            Assert.AreEqual("Branch", labels[labels.Count - 1], "Last sub-tab label with branch must be 'Branch'.");
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

        [Test]
        public void BeginBranchPreview_FromVisualTab_ForcesActiveTabToBranch_AndCapturesVisual()
        {
            _window.ActiveTabForTests = SkillTreeDesignerWindow.DesignerTab.Visual;

            _window.InvokeBeginBranchPreviewForTests(parentNodeIndex: 0);

            Assert.AreEqual(
                SkillTreeDesignerWindow.DesignerTab.Branch,
                _window.ActiveTabForTests,
                "BeginBranchPreview must force the active tab to Branch.");
            Assert.IsTrue(
                _window.BranchPreviewActiveForTests,
                "Branch preview must be active after BeginBranchPreview.");

            _window.InvokeCancelBranchPreviewForTests();
        }

        [Test]
        public void CancelBranchPreview_RestoresVisual_WhenStartedFromVisual()
        {
            _window.ActiveTabForTests = SkillTreeDesignerWindow.DesignerTab.Visual;

            _window.InvokeBeginBranchPreviewForTests(parentNodeIndex: 0);
            _window.InvokeCancelBranchPreviewForTests();

            Assert.AreEqual(
                SkillTreeDesignerWindow.DesignerTab.Visual,
                _window.ActiveTabForTests,
                "CancelBranchPreview must restore the Visual tab when branch preview started from Visual.");
            Assert.IsFalse(
                _window.BranchPreviewActiveForTests,
                "Branch preview must no longer be active after CancelBranchPreview.");
        }

        [Test]
        public void CancelBranchPreview_RestoresSkillTree_WhenStartedFromSkillTree()
        {
            _window.ActiveTabForTests = SkillTreeDesignerWindow.DesignerTab.SkillTree;

            _window.InvokeBeginBranchPreviewForTests(parentNodeIndex: 0);
            _window.InvokeCancelBranchPreviewForTests();

            Assert.AreEqual(
                SkillTreeDesignerWindow.DesignerTab.SkillTree,
                _window.ActiveTabForTests,
                "CancelBranchPreview must restore the SkillTree tab when branch preview started from SkillTree.");
        }
    }
}
#endif
