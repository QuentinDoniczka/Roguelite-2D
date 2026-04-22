#if UNITY_EDITOR
using System.Collections;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using RogueliteAutoBattler.UI.Toolkit.SkillTree;

namespace RogueliteAutoBattler.Tests.PlayMode
{
    public class SkillTreeNodeElementTests : PlayModeTestBase
    {
        private const string PanelSettingsPath = "Assets/UI/MainPanelSettings.asset";

        private const string NodeBaseClassName = "skill-tree-node";
        private const string NodeLockedClassName = "skill-tree-node--locked";
        private const string NodeAvailableClassName = "skill-tree-node--available";
        private const string NodePurchasedClassName = "skill-tree-node--purchased";
        private const string NodeMaxClassName = "skill-tree-node--max";
        private const string NodeSelectedClassName = "skill-tree-node--selected";

        private const float NodeHalfSize = 32f;
        private const float PositionTolerance = 0.01f;

        [Test]
        public void Constructor_AppliesBaseClass()
        {
            var node = new SkillTreeNodeElement(0);

            Assert.IsTrue(node.ClassListContains(NodeBaseClassName),
                $"Newly constructed node must contain the '{NodeBaseClassName}' class.");
        }

        [Test]
        public void Constructor_NodeIndexMatches()
        {
            var node = new SkillTreeNodeElement(7);

            Assert.AreEqual(7, node.NodeIndex,
                "NodeIndex must match the value passed to the constructor.");
        }

        [Test]
        public void Constructor_InitialState_IsLocked()
        {
            var node = new SkillTreeNodeElement(0);

            Assert.AreEqual(SkillTreeNodeVisualState.Locked, node.CurrentState,
                "CurrentState must default to Locked on construction.");
            Assert.IsTrue(node.ClassListContains(NodeLockedClassName),
                $"Newly constructed node must contain the '{NodeLockedClassName}' class.");
        }

        [Test]
        public void SetState_Available_RemovesLockedClass()
        {
            var node = new SkillTreeNodeElement(0);

            node.SetState(SkillTreeNodeVisualState.Available);

            Assert.IsTrue(node.ClassListContains(NodeAvailableClassName),
                $"Node must contain '{NodeAvailableClassName}' after SetState(Available).");
            Assert.IsFalse(node.ClassListContains(NodeLockedClassName),
                $"Node must no longer contain '{NodeLockedClassName}' after SetState(Available).");
            Assert.AreEqual(SkillTreeNodeVisualState.Available, node.CurrentState,
                "CurrentState must reflect the latest SetState call.");
        }

        [Test]
        public void SetState_Purchased_FromAvailable_SwitchesClasses()
        {
            var node = new SkillTreeNodeElement(0);
            node.SetState(SkillTreeNodeVisualState.Available);

            node.SetState(SkillTreeNodeVisualState.Purchased);

            Assert.IsTrue(node.ClassListContains(NodePurchasedClassName),
                $"Node must contain '{NodePurchasedClassName}' after SetState(Purchased).");
            Assert.IsFalse(node.ClassListContains(NodeLockedClassName),
                $"Node must not contain '{NodeLockedClassName}' after SetState(Purchased).");
            Assert.IsFalse(node.ClassListContains(NodeAvailableClassName),
                $"Node must not contain '{NodeAvailableClassName}' after SetState(Purchased).");
            Assert.IsFalse(node.ClassListContains(NodeMaxClassName),
                $"Node must not contain '{NodeMaxClassName}' after SetState(Purchased).");
            Assert.AreEqual(SkillTreeNodeVisualState.Purchased, node.CurrentState,
                "CurrentState must be Purchased after the chain of SetState calls.");
        }

        [Test]
        public void SetState_Max_RemovesAllOtherStateClasses()
        {
            var node = new SkillTreeNodeElement(0);
            node.SetState(SkillTreeNodeVisualState.Available);
            node.SetState(SkillTreeNodeVisualState.Purchased);

            node.SetState(SkillTreeNodeVisualState.Max);

            Assert.IsTrue(node.ClassListContains(NodeMaxClassName),
                $"Node must contain '{NodeMaxClassName}' after SetState(Max).");
            Assert.IsFalse(node.ClassListContains(NodeLockedClassName),
                $"Node must not contain '{NodeLockedClassName}' after SetState(Max).");
            Assert.IsFalse(node.ClassListContains(NodeAvailableClassName),
                $"Node must not contain '{NodeAvailableClassName}' after SetState(Max).");
            Assert.IsFalse(node.ClassListContains(NodePurchasedClassName),
                $"Node must not contain '{NodePurchasedClassName}' after SetState(Max).");
            Assert.AreEqual(SkillTreeNodeVisualState.Max, node.CurrentState,
                "CurrentState must be Max after SetState(Max).");
        }

        [Test]
        public void SetSelected_True_AddsSelectedClass_WithoutRemovingStateClass()
        {
            var node = new SkillTreeNodeElement(0);
            node.SetState(SkillTreeNodeVisualState.Available);

            node.SetSelected(true);

            Assert.IsTrue(node.ClassListContains(NodeAvailableClassName),
                $"Node must still contain '{NodeAvailableClassName}' after SetSelected(true).");
            Assert.IsTrue(node.ClassListContains(NodeSelectedClassName),
                $"Node must contain '{NodeSelectedClassName}' after SetSelected(true).");
            Assert.IsTrue(node.IsSelected,
                "IsSelected must be true after SetSelected(true).");
        }

        [Test]
        public void SetSelected_False_RemovesSelectedClass_KeepsStateClass()
        {
            var node = new SkillTreeNodeElement(0);
            node.SetState(SkillTreeNodeVisualState.Available);
            node.SetSelected(true);

            node.SetSelected(false);

            Assert.IsTrue(node.ClassListContains(NodeAvailableClassName),
                $"Node must still contain '{NodeAvailableClassName}' after SetSelected(false).");
            Assert.IsFalse(node.ClassListContains(NodeSelectedClassName),
                $"Node must not contain '{NodeSelectedClassName}' after SetSelected(false).");
            Assert.IsFalse(node.IsSelected,
                "IsSelected must be false after SetSelected(false).");
        }

        [UnityTest]
        public IEnumerator SetDataPosition_CentersNodeOnPosition()
        {
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

            var node = new SkillTreeNodeElement(3);
            root.Add(node);

            node.SetDataPosition(new Vector2(100f, 200f), 1f);

            yield return null;

            float expectedLeft = 100f - NodeHalfSize;
            float expectedTop = 200f - NodeHalfSize;

            Assert.AreEqual(expectedLeft, node.style.left.value.value, PositionTolerance,
                $"style.left must equal dataPosition.x - NodeHalfSize ({expectedLeft}).");
            Assert.AreEqual(expectedTop, node.style.top.value.value, PositionTolerance,
                $"style.top must equal dataPosition.y - NodeHalfSize ({expectedTop}).");
        }

        [UnityTest]
        public IEnumerator Click_RaisesClickedEventWithNodeIndex()
        {
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

            var node = new SkillTreeNodeElement(5);
            root.Add(node);

            int capturedNodeIndex = -1;
            int invocationCount = 0;
            node.Clicked += index =>
            {
                capturedNodeIndex = index;
                invocationCount++;
            };

            using (var clickEvent = ClickEvent.GetPooled())
            {
                clickEvent.target = node;
                node.SendEvent(clickEvent);
            }

            yield return null;

            Assert.AreEqual(1, invocationCount,
                "Clicked handler must be invoked exactly once after a ClickEvent.");
            Assert.AreEqual(node.NodeIndex, capturedNodeIndex,
                "Clicked handler must receive the node's NodeIndex.");
        }
    }
}
#endif
