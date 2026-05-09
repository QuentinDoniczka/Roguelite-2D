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
        private const string MainStylePath = "Assets/UI/Styles/MainStyle.uss";

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

        private static void AttachMainStyle(UIDocument doc)
        {
            StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(MainStylePath);
            Assert.IsNotNull(styleSheet, $"MainStyle USS not found at {MainStylePath}");
            doc.rootVisualElement.styleSheets.Add(styleSheet);
        }

        [Test]
        public void Constructor_CreatesHaloOuterChild()
        {
            var node = new SkillTreeNodeElement(0);

            var haloOuter = node.Q(className: "skill-tree-node__halo-outer");

            Assert.IsNotNull(haloOuter, "Constructor must create a halo-outer child VisualElement.");
            Assert.AreEqual(PickingMode.Ignore, haloOuter.pickingMode,
                "HaloOuter must not intercept pointer events.");
            Assert.IsNotNull(haloOuter.style.backgroundImage.value.texture,
                "HaloOuter must receive the orb texture as background-image.");
        }

        [Test]
        public void Constructor_CreatesHaloInnerChild()
        {
            var node = new SkillTreeNodeElement(0);

            var haloInner = node.Q(className: "skill-tree-node__halo-inner");

            Assert.IsNotNull(haloInner, "Constructor must create a halo-inner child VisualElement.");
            Assert.AreEqual(PickingMode.Ignore, haloInner.pickingMode,
                "HaloInner must not intercept pointer events.");
            Assert.IsNotNull(haloInner.style.backgroundImage.value.texture,
                "HaloInner must receive the orb texture as background-image.");
        }

        [Test]
        public void Constructor_CreatesRaysChild()
        {
            var node = new SkillTreeNodeElement(0);

            var rays = node.Q(className: "skill-tree-node__rays");

            Assert.IsNotNull(rays, "Constructor must create a rays child VisualElement.");
            Assert.AreEqual(PickingMode.Ignore, rays.pickingMode,
                "Rays must not intercept pointer events.");
            Assert.IsNotNull(rays.style.backgroundImage.value.texture,
                "Rays must receive the orb texture as background-image.");
        }

        [Test]
        public void SetColorTag_AppliesTintColor_ToHaloOuter()
        {
            var node = new SkillTreeNodeElement(0);
            var color = new Color(0.8f, 0.2f, 0.2f, 1f);

            node.SetColorTag(color);

            var haloOuter = node.Q(className: "skill-tree-node__halo-outer");
            Assert.AreEqual(color, haloOuter.style.unityBackgroundImageTintColor.value,
                "SetColorTag must apply the tint color as inline style to halo-outer.");
        }

        [Test]
        public void SetColorTag_AppliesTintColor_ToHaloInner()
        {
            var node = new SkillTreeNodeElement(0);
            var color = new Color(0.8f, 0.2f, 0.2f, 1f);

            node.SetColorTag(color);

            var haloInner = node.Q(className: "skill-tree-node__halo-inner");
            Assert.AreEqual(color, haloInner.style.unityBackgroundImageTintColor.value,
                "SetColorTag must apply the tint color as inline style to halo-inner.");
        }

        [Test]
        public void SetColorTag_DoesNotSetInlineTint_OnRays()
        {
            var node = new SkillTreeNodeElement(0);

            node.SetColorTag(new Color(0.8f, 0.2f, 0.2f, 1f));

            var rays = node.Q(className: "skill-tree-node__rays");
            Assert.AreEqual(StyleKeyword.Null, rays.style.unityBackgroundImageTintColor.keyword,
                "SetColorTag must not set an inline tint on the rays layer.");
        }

        [UnityTest]
        public IEnumerator MaxState_ShowsRays()
        {
            var doc = CreateDocument();
            yield return null;
            yield return null;

            if (doc.rootVisualElement == null)
            {
                Assert.Inconclusive("rootVisualElement is null - UIDocument failed to initialize in the test environment.");
                yield break;
            }
            AttachMainStyle(doc);

            var node = new SkillTreeNodeElement(0);
            doc.rootVisualElement.Add(node);
            node.SetState(SkillTreeNodeVisualState.Max);

            yield return null;
            yield return null;

            var rays = node.Q(className: "skill-tree-node__rays");
            Assert.AreEqual(DisplayStyle.Flex, rays.resolvedStyle.display,
                "Rays must be displayed when node is in Max state.");
        }

        [UnityTest]
        public IEnumerator LockedState_HidesHaloOuterAndHaloInner()
        {
            var doc = CreateDocument();
            yield return null;
            yield return null;

            if (doc.rootVisualElement == null)
            {
                Assert.Inconclusive("rootVisualElement is null - UIDocument failed to initialize in the test environment.");
                yield break;
            }
            AttachMainStyle(doc);

            var node = new SkillTreeNodeElement(0);
            doc.rootVisualElement.Add(node);
            node.SetState(SkillTreeNodeVisualState.Locked);

            yield return null;
            yield return null;

            var haloOuter = node.Q(className: "skill-tree-node__halo-outer");
            var haloInner = node.Q(className: "skill-tree-node__halo-inner");
            Assert.AreEqual(DisplayStyle.None, haloOuter.resolvedStyle.display,
                "HaloOuter must be hidden when node is in Locked state.");
            Assert.AreEqual(DisplayStyle.None, haloInner.resolvedStyle.display,
                "HaloInner must be hidden when node is in Locked state.");
        }

        [UnityTest]
        public IEnumerator AvailableState_DoesNotShowRays()
        {
            var doc = CreateDocument();
            yield return null;
            yield return null;

            if (doc.rootVisualElement == null)
            {
                Assert.Inconclusive("rootVisualElement is null - UIDocument failed to initialize in the test environment.");
                yield break;
            }
            AttachMainStyle(doc);

            var node = new SkillTreeNodeElement(0);
            doc.rootVisualElement.Add(node);
            node.SetState(SkillTreeNodeVisualState.Available);

            yield return null;
            yield return null;

            var rays = node.Q(className: "skill-tree-node__rays");
            Assert.AreEqual(DisplayStyle.None, rays.resolvedStyle.display,
                "Rays must not be displayed when node is in Available state.");
        }
    }
}
#endif
