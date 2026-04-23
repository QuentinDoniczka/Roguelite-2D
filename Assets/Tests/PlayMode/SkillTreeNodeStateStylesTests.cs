#if UNITY_EDITOR
using System.Collections;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace RogueliteAutoBattler.Tests.PlayMode
{
    public class SkillTreeNodeStateStylesTests : PlayModeTestBase
    {
        private const string MainStylePath = "Assets/UI/Styles/MainStyle.uss";
        private const string PanelSettingsPath = "Assets/UI/MainPanelSettings.asset";

        private const string NodeBaseClass = "skill-tree-node";
        private const string NodeLockedClass = "skill-tree-node--locked";
        private const string NodeAvailableClass = "skill-tree-node--available";
        private const string NodePurchasedClass = "skill-tree-node--purchased";
        private const string NodeMaxClass = "skill-tree-node--max";
        private const string NodeSelectedClass = "skill-tree-node--selected";
        private const string DetailPanelClass = "skill-tree-detail-panel";
        private const string HiddenClass = "hidden";

        private const float OpacityTolerance = 0.01f;
        private const float BorderWidthTolerance = 0.01f;

        private IEnumerator BuildPanelWithRoot(System.Action<VisualElement> populateRoot, System.Action<VisualElement> afterLayout)
        {
            StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(MainStylePath);
            Assert.IsNotNull(styleSheet, $"MainStyle USS not found at {MainStylePath}");

            PanelSettings panelSettings = AssetDatabase.LoadAssetAtPath<PanelSettings>(PanelSettingsPath);
            Assert.IsNotNull(panelSettings, $"MainPanelSettings not found at {PanelSettingsPath}");

            var documentGo = Track(new GameObject("TestUIDocument"));
            documentGo.SetActive(false);
            UIDocument uiDocument = documentGo.AddComponent<UIDocument>();
            uiDocument.panelSettings = panelSettings;
            documentGo.SetActive(true);

            yield return null;
            yield return null;

            VisualElement root = uiDocument.rootVisualElement;
            if (root == null)
            {
                Assert.Inconclusive("rootVisualElement is null - UIDocument failed to initialize in the test environment.");
                yield break;
            }

            root.styleSheets.Add(styleSheet);
            root.style.width = 1080;
            root.style.height = 1920;

            populateRoot?.Invoke(root);

            yield return null;
            yield return null;

            afterLayout?.Invoke(root);
        }

        private static VisualElement CreateNodeWithClasses(VisualElement parent, params string[] classNames)
        {
            var node = new VisualElement();
            foreach (var className in classNames)
            {
                node.AddToClassList(className);
            }
            parent.Add(node);
            return node;
        }

        private static VisualElement CreatePanelWithClasses(VisualElement parent, params string[] classNames)
        {
            var panel = new VisualElement();
            foreach (var className in classNames)
            {
                panel.AddToClassList(className);
            }
            parent.Add(panel);
            return panel;
        }

        [UnityTest]
        public IEnumerator NodeBase_HasAbsolutePosition()
        {
            VisualElement baseNode = null;
            yield return BuildPanelWithRoot(
                root => baseNode = CreateNodeWithClasses(root, NodeBaseClass),
                _ => Assert.AreEqual(Position.Absolute, baseNode.resolvedStyle.position,
                    $".{NodeBaseClass} must resolve to position: absolute"));
        }

        [UnityTest]
        public IEnumerator Locked_And_Available_BackgroundColorsDiffer()
        {
            VisualElement lockedNode = null;
            VisualElement availableNode = null;
            yield return BuildPanelWithRoot(
                root =>
                {
                    lockedNode = CreateNodeWithClasses(root, NodeBaseClass, NodeLockedClass);
                    availableNode = CreateNodeWithClasses(root, NodeBaseClass, NodeAvailableClass);
                },
                _ => Assert.AreNotEqual(lockedNode.resolvedStyle.backgroundColor, availableNode.resolvedStyle.backgroundColor,
                    $".{NodeLockedClass} and .{NodeAvailableClass} must resolve to different background-colors"));
        }

        [UnityTest]
        public IEnumerator Purchased_And_Max_BackgroundColorsDiffer()
        {
            VisualElement purchasedNode = null;
            VisualElement maxNode = null;
            yield return BuildPanelWithRoot(
                root =>
                {
                    purchasedNode = CreateNodeWithClasses(root, NodeBaseClass, NodePurchasedClass);
                    maxNode = CreateNodeWithClasses(root, NodeBaseClass, NodeMaxClass);
                },
                _ => Assert.AreNotEqual(purchasedNode.resolvedStyle.backgroundColor, maxNode.resolvedStyle.backgroundColor,
                    $".{NodePurchasedClass} and .{NodeMaxClass} must resolve to different background-colors"));
        }

        [UnityTest]
        public IEnumerator Locked_HasReducedOpacity()
        {
            VisualElement lockedNode = null;
            yield return BuildPanelWithRoot(
                root => lockedNode = CreateNodeWithClasses(root, NodeBaseClass, NodeLockedClass),
                _ => Assert.Less(lockedNode.resolvedStyle.opacity, 1f - OpacityTolerance,
                    $".{NodeLockedClass} must resolve to opacity < 1"));
        }

        [UnityTest]
        public IEnumerator Selected_BorderWidth_ExceedsBase()
        {
            VisualElement baseNode = null;
            VisualElement selectedNode = null;
            yield return BuildPanelWithRoot(
                root =>
                {
                    baseNode = CreateNodeWithClasses(root, NodeBaseClass, NodeAvailableClass);
                    selectedNode = CreateNodeWithClasses(root, NodeBaseClass, NodeAvailableClass, NodeSelectedClass);
                },
                _ => Assert.Greater(selectedNode.resolvedStyle.borderLeftWidth, baseNode.resolvedStyle.borderLeftWidth + BorderWidthTolerance,
                    $".{NodeSelectedClass} must resolve to a strictly greater border-left-width than a non-selected node"));
        }

        [UnityTest]
        public IEnumerator DetailPanel_HiddenClass_SetsDisplayNone()
        {
            VisualElement hiddenPanel = null;
            yield return BuildPanelWithRoot(
                root => hiddenPanel = CreatePanelWithClasses(root, DetailPanelClass, HiddenClass),
                _ => Assert.AreEqual(DisplayStyle.None, hiddenPanel.resolvedStyle.display,
                    $".{DetailPanelClass}.{HiddenClass} must resolve to display: none"));
        }

        [UnityTest]
        public IEnumerator DetailPanel_WithoutHiddenClass_SetsDisplayFlex()
        {
            VisualElement visiblePanel = null;
            yield return BuildPanelWithRoot(
                root => visiblePanel = CreatePanelWithClasses(root, DetailPanelClass),
                _ => Assert.AreEqual(DisplayStyle.Flex, visiblePanel.resolvedStyle.display,
                    $".{DetailPanelClass} without .{HiddenClass} must resolve to display: flex"));
        }
    }
}
#endif
