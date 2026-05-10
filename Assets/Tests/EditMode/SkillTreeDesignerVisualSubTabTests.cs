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
using UnityEngine.UIElements;

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
        public void SubTabLabels_VisualIsLast_BeforeBranch()
        {
            var labels = SkillTreeDesignerWindow.TabLabelsWithoutBranchForTests;
            Assert.IsNotNull(labels, "TabLabelsWithoutBranch must not be null.");
            Assert.GreaterOrEqual(labels.Count, 3, "TabLabelsWithoutBranch must contain at least three entries.");
            Assert.AreEqual("Skill Tree", labels[0], "First sub-tab label must be 'Skill Tree'.");
            Assert.AreEqual("Node", labels[1], "Second sub-tab label must be 'Node'.");
            Assert.AreEqual("Visual", labels[2], "Third sub-tab label must be 'Visual'.");
        }

        [Test]
        public void SubTabLabels_WithBranch_VisualThird_BranchLast()
        {
            var labels = SkillTreeDesignerWindow.TabLabelsWithBranchForTests;
            Assert.IsNotNull(labels, "TabLabelsWithBranch must not be null.");
            Assert.GreaterOrEqual(labels.Count, 4, "TabLabelsWithBranch must contain at least four entries.");
            Assert.AreEqual("Visual", labels[2], "Third sub-tab label with branch must be 'Visual'.");
            Assert.AreEqual("Branch", labels[labels.Count - 1], "Last sub-tab label with branch must be 'Branch'.");
        }

        [Test]
        public void VisualTab_Initialize_PopulatesSerializedObject()
        {
            using var scope = new SkillTreeVisualSettingsProviderScope();

            _window.InvokeInitializeVisualSettingsInspectorForTests();

            Assert.IsNotNull(
                _window.VisualSettingsSerializedObjectForTests,
                "SerializedObject must be populated after InitializeVisualSettingsInspector.");
            Assert.AreEqual(
                _window.VisualSettingsForTests,
                _window.VisualSettingsSerializedObjectForTests.targetObject,
                "SerializedObject target must be the resolved SkillTreeVisualSettings asset.");
        }

        [Test]
        public void VisualTab_Initialize_CanvasPanelInstance_NotNull()
        {
            using var scope = new SkillTreeVisualSettingsProviderScope();

            var testParent = new VisualElement();
            _window.InvokeAttachVisualCanvasRootForTests(testParent);

            Assert.IsNotNull(
                _window.VisualCanvasPanelForTests,
                "Visual canvas panel must be non-null when data is bound.");
        }

        [Test]
        public void VisualTab_OnSettingsChanged_MarksAssetDirty_AndResetsCache()
        {
            using var scope = new SkillTreeVisualSettingsProviderScope();
            _window.InvokeInitializeVisualSettingsInspectorForTests();

            var so = _window.VisualSettingsSerializedObjectForTests;
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
                _window.InvokeOnVisualSettingsChangedForTests();

                Assert.IsTrue(
                    EditorUtility.IsDirty(stub),
                    "SkillTreeVisualSettings must be marked dirty after OnVisualSettingsChanged.");

                SkillTreeVisualSettingsResolver.Get();
                Assert.Greater(
                    providerCallCount,
                    callCountBeforeChange,
                    "Resolver cache must be reset by OnVisualSettingsChanged so the provider is re-invoked.");
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

        [Test]
        public void VisualTab_BuildContent_CreatesEdgeLayer()
        {
            using var scope = new SkillTreeVisualSettingsProviderScope();

            var testParent = new VisualElement();
            _window.InvokeAttachVisualCanvasRootForTests(testParent);

            Assert.IsNotNull(_window.VisualCanvasPanelForTests,
                "Visual canvas panel must be created when data is bound.");

            var edgeLayers = testParent.Query<SkillTreeEdgeLayer>().ToList();
            Assert.AreEqual(1, edgeLayers.Count,
                "Visual sub-tab must build exactly one SkillTreeEdgeLayer (Bug #1 regression guard).");
        }

        [Test]
        public void VisualTab_BuildContent_NodesPositionedAtUnitToPixelScale()
        {
            using var scope = new SkillTreeVisualSettingsProviderScope();

            var testParent = new VisualElement();
            _window.InvokeAttachVisualCanvasRootForTests(testParent);

            Assert.IsNotNull(_window.VisualCanvasPanelForTests,
                "Visual canvas panel must be created when data is bound.");

            var nodeElements = testParent.Query<SkillTreeNodeElement>().ToList();
            Assert.AreEqual(_data.Nodes.Count, nodeElements.Count,
                "Visual sub-tab must spawn one SkillTreeNodeElement per data node.");

            const float PositionTolerance = 0.001f;

            for (int i = 0; i < _data.Nodes.Count; i++)
            {
                var dataPos = _data.Nodes[i].position;
                var element = FindNodeElementByIndex(nodeElements, i);
                Assert.IsNotNull(element, $"A SkillTreeNodeElement with NodeIndex={i} must exist in the visual tree.");

                float expectedLeft = dataPos.x * SkillTreeRenderer.UnitToPixelScale - SkillTreeRenderer.NodeHalfSize;
                float expectedTop = dataPos.y * SkillTreeRenderer.UnitToPixelScale - SkillTreeRenderer.NodeHalfSize;
                Assert.AreEqual(expectedLeft, element.style.left.value.value, PositionTolerance,
                    $"Node {i} must be positioned at dataPos.x * SkillTreeRenderer.UnitToPixelScale - 32 (catches the 30f→40f scale regression).");
                Assert.AreEqual(expectedTop, element.style.top.value.value, PositionTolerance,
                    $"Node {i} must be positioned at dataPos.y * SkillTreeRenderer.UnitToPixelScale - 32 (catches the 30f→40f scale regression).");
            }
        }

        private static SkillTreeNodeElement FindNodeElementByIndex(List<SkillTreeNodeElement> elements, int nodeIndex)
        {
            foreach (var element in elements)
            {
                if (element.NodeIndex == nodeIndex) return element;
            }
            return null;
        }
    }
}
#endif
