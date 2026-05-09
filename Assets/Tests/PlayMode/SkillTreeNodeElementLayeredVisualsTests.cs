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
    public class SkillTreeNodeElementLayeredVisualsTests : PlayModeTestBase
    {
        private const string PanelSettingsPath = "Assets/UI/MainPanelSettings.asset";

        [SetUp]
        public void SetUp()
        {
            SkillTreeNodeOrbResolver.ResetCache();
        }

        private UIDocument CreateDocument()
        {
            PanelSettings panelSettings = AssetDatabase.LoadAssetAtPath<PanelSettings>(PanelSettingsPath);
            Assert.IsNotNull(panelSettings, $"MainPanelSettings not found at {PanelSettingsPath}");

            var documentGo = Track(new GameObject("TestLayeredVisualsUIDocument"));
            documentGo.SetActive(false);
            var doc = documentGo.AddComponent<UIDocument>();
            doc.panelSettings = panelSettings;
            documentGo.SetActive(true);
            return doc;
        }

        [Test]
        public void Constructor_CreatesFrameChild()
        {
            var node = new SkillTreeNodeElement(0);

            var frame = node.Q(className: "skill-tree-node__frame");

            Assert.IsNotNull(frame, "Constructor must create a frame child VisualElement.");
            Assert.AreEqual(PickingMode.Ignore, frame.pickingMode,
                "Frame must not intercept pointer events.");
            Assert.IsNotNull(frame.style.backgroundImage.value.texture,
                "Frame must receive the orb texture as background-image.");
        }

        [Test]
        public void Constructor_CreatesRimChild()
        {
            var node = new SkillTreeNodeElement(0);

            var rim = node.Q(className: "skill-tree-node__rim");

            Assert.IsNotNull(rim, "Constructor must create a rim child VisualElement.");
            Assert.AreEqual(PickingMode.Ignore, rim.pickingMode,
                "Rim must not intercept pointer events.");
            Assert.IsNotNull(rim.style.backgroundImage.value.texture,
                "Rim must receive the orb texture as background-image.");
        }

        [Test]
        public void Constructor_CreatesInnerGlowChild()
        {
            var node = new SkillTreeNodeElement(0);

            var glow = node.Q(className: "skill-tree-node__inner-glow");

            Assert.IsNotNull(glow, "Constructor must create an inner-glow child VisualElement.");
            Assert.AreEqual(PickingMode.Ignore, glow.pickingMode,
                "InnerGlow must not intercept pointer events.");
            Assert.IsNotNull(glow.style.backgroundImage.value.texture,
                "InnerGlow must receive the orb texture as background-image.");
        }

        [Test]
        public void Constructor_CreatesSparkleChild()
        {
            var node = new SkillTreeNodeElement(0);

            var sparkle = node.Q(className: "skill-tree-node__sparkle");

            Assert.IsNotNull(sparkle, "Constructor must create a sparkle child VisualElement.");
            Assert.AreEqual(PickingMode.Ignore, sparkle.pickingMode,
                "Sparkle must not intercept pointer events.");
            Assert.IsNotNull(sparkle.style.backgroundImage.value.texture,
                "Sparkle must receive the orb texture as background-image.");
        }

        [Test]
        public void SetColorTag_AppliesTintColor_ToInnerGlow()
        {
            var node = new SkillTreeNodeElement(0);
            var color = new Color(0.8f, 0.2f, 0.2f, 1f);

            node.SetColorTag(color);

            var glow = node.Q(className: "skill-tree-node__inner-glow");
            Assert.AreEqual(color, glow.style.unityBackgroundImageTintColor.value,
                "SetColorTag must apply the tint color as inline style to inner-glow.");
        }

        [Test]
        public void SetColorTag_DoesNotSetInlineTint_OnFrame()
        {
            var node = new SkillTreeNodeElement(0);

            node.SetColorTag(new Color(0.8f, 0.2f, 0.2f, 1f));

            var frame = node.Q(className: "skill-tree-node__frame");
            Assert.AreEqual(StyleKeyword.Null, frame.style.unityBackgroundImageTintColor.keyword,
                "SetColorTag must not set an inline tint on the frame layer.");
        }

        [Test]
        public void SetColorTag_DoesNotSetInlineTint_OnRim()
        {
            var node = new SkillTreeNodeElement(0);

            node.SetColorTag(new Color(0.8f, 0.2f, 0.2f, 1f));

            var rim = node.Q(className: "skill-tree-node__rim");
            Assert.AreEqual(StyleKeyword.Null, rim.style.unityBackgroundImageTintColor.keyword,
                "SetColorTag must not set an inline tint on the rim layer.");
        }

        [Test]
        public void SetColorTag_DoesNotSetInlineTint_OnSparkle()
        {
            var node = new SkillTreeNodeElement(0);

            node.SetColorTag(new Color(0.8f, 0.2f, 0.2f, 1f));

            var sparkle = node.Q(className: "skill-tree-node__sparkle");
            Assert.AreEqual(StyleKeyword.Null, sparkle.style.unityBackgroundImageTintColor.keyword,
                "SetColorTag must not set an inline tint on the sparkle layer.");
        }

        [UnityTest]
        public IEnumerator MaxState_ShowsSparkle()
        {
            var doc = CreateDocument();
            yield return null;
            yield return null;

            var node = new SkillTreeNodeElement(0);
            doc.rootVisualElement.Add(node);
            node.SetState(SkillTreeNodeVisualState.Max);

            yield return null;
            yield return null;

            var sparkle = node.Q(className: "skill-tree-node__sparkle");
            Assert.AreEqual(DisplayStyle.Flex, sparkle.resolvedStyle.display,
                "Sparkle must be displayed when node is in Max state.");
        }

        [UnityTest]
        public IEnumerator LockedState_HidesRimAndInnerGlow()
        {
            var doc = CreateDocument();
            yield return null;
            yield return null;

            var node = new SkillTreeNodeElement(0);
            doc.rootVisualElement.Add(node);
            node.SetState(SkillTreeNodeVisualState.Locked);

            yield return null;
            yield return null;

            var rim = node.Q(className: "skill-tree-node__rim");
            var glow = node.Q(className: "skill-tree-node__inner-glow");
            Assert.AreEqual(DisplayStyle.None, rim.resolvedStyle.display,
                "Rim must be hidden when node is in Locked state.");
            Assert.AreEqual(DisplayStyle.None, glow.resolvedStyle.display,
                "InnerGlow must be hidden when node is in Locked state.");
        }

        [UnityTest]
        public IEnumerator AvailableState_DoesNotShowSparkle()
        {
            var doc = CreateDocument();
            yield return null;
            yield return null;

            var node = new SkillTreeNodeElement(0);
            doc.rootVisualElement.Add(node);
            node.SetState(SkillTreeNodeVisualState.Available);

            yield return null;
            yield return null;

            var sparkle = node.Q(className: "skill-tree-node__sparkle");
            Assert.AreEqual(DisplayStyle.None, sparkle.resolvedStyle.display,
                "Sparkle must not be displayed when node is in Available state.");
        }
    }
}
#endif
