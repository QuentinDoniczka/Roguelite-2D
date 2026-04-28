using System.Collections;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace RogueliteAutoBattler.Tests.PlayMode
{
    [Category("HudIcons")]
    public class HudIconResolvedStyleTests : PlayModeTestBase
    {
        private const string MainLayoutPath = "Assets/UI/Layouts/MainLayout.uxml";
        private const string MainStylePath = "Assets/UI/Styles/MainStyle.uss";
        private const string PanelSettingsPath = "Assets/UI/MainPanelSettings.asset";

        private const float FloatTolerance = 1.5f;
        private const float GoldIconSize = 56f;
        private const float SkillPointIconSize = 50f;
        private const float InfoIconSize = 64f;
        private const float NavBtnIconSize = 56f;
        private const float TransparentAlphaTolerance = 0.01f;
        private const float ScaleTolerance = 0.01f;
        private const float RadiusTolerance = 0.01f;
        private const float RotationToleranceDegrees = 0.1f;
        private const float ReferenceWidthPx = 1080f;
        private const float ReferenceHeightPx = 1920f;

        [UnityTest]
        public IEnumerator AllHudIcons_ResolveToExpectedSpritesAndSizes()
        {
            VisualTreeAsset layoutAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(MainLayoutPath);
            Assert.IsNotNull(layoutAsset, $"MainLayout UXML not found at {MainLayoutPath}");

            StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(MainStylePath);
            Assert.IsNotNull(styleSheet, $"MainStyle USS not found at {MainStylePath}");

            PanelSettings panelSettings = AssetDatabase.LoadAssetAtPath<PanelSettings>(PanelSettingsPath);
            Assert.IsNotNull(panelSettings, $"MainPanelSettings not found at {PanelSettingsPath}");

            var documentGo = Track(new GameObject("TestUIDocument"));
            documentGo.SetActive(false);
            UIDocument uiDocument = documentGo.AddComponent<UIDocument>();
            uiDocument.panelSettings = panelSettings;
            uiDocument.visualTreeAsset = layoutAsset;
            documentGo.SetActive(true);

            yield return null;
            yield return null;

            VisualElement root = uiDocument.rootVisualElement;
            if (root == null)
            {
                Assert.Inconclusive("rootVisualElement is null - UIDocument failed to initialize in the test environment.");
                yield break;
            }

            root.style.width = ReferenceWidthPx;
            root.style.height = ReferenceHeightPx;
            yield return null;
            yield return null;

            var infoContent = root.Q<VisualElement>("info-content");
            Assert.That(infoContent, Is.Not.Null, "info-content element missing");
            infoContent.style.display = DisplayStyle.Flex;
            yield return null;
            yield return null;

            VisualElement goldIcon = root.Q<VisualElement>("gold-icon");
            Assert.IsNotNull(goldIcon, "gold-icon not found");
            Assert.That(goldIcon.resolvedStyle.width, Is.EqualTo(GoldIconSize).Within(FloatTolerance),
                $"gold-icon width must resolve to {GoldIconSize}px (var --hud-gold-icon-size)");
            Assert.That(goldIcon.resolvedStyle.height, Is.EqualTo(GoldIconSize).Within(FloatTolerance),
                $"gold-icon height must resolve to {GoldIconSize}px (var --hud-gold-icon-size)");
            AssertBackgroundImageBound(goldIcon, "gold-icon");
            Assert.That(goldIcon.resolvedStyle.backgroundColor.a, Is.LessThan(TransparentAlphaTolerance),
                "gold-icon background-color must be transparent so the sprite shows through");
            AssertBackgroundImageNameContains(goldIcon, "gold", "gold-icon");

            VisualElement skillPointIcon = root.Q<VisualElement>("skill-point-icon");
            Assert.IsNotNull(skillPointIcon, "skill-point-icon not found");
            Assert.That(skillPointIcon.resolvedStyle.width, Is.EqualTo(SkillPointIconSize).Within(FloatTolerance),
                $"skill-point-icon width must resolve to {SkillPointIconSize}px");
            Assert.That(skillPointIcon.resolvedStyle.height, Is.EqualTo(SkillPointIconSize).Within(FloatTolerance),
                $"skill-point-icon height must resolve to {SkillPointIconSize}px");
            AssertBackgroundImageBound(skillPointIcon, "skill-point-icon");
            Assert.That(skillPointIcon.resolvedStyle.backgroundColor.a, Is.LessThan(TransparentAlphaTolerance),
                "skill-point-icon background-color must be transparent so the sprite shows through");
            float skillPointRotationDegrees = skillPointIcon.resolvedStyle.rotate.angle.ToDegrees();
            Assert.That(skillPointRotationDegrees, Is.EqualTo(0f).Within(RotationToleranceDegrees),
                $"skill-point-icon must have no rotation (got {skillPointRotationDegrees} degrees)");
            AssertBackgroundImageNameContains(skillPointIcon, "diamant", "skill-point-icon");

            VisualElement infoIcon = root.Q<VisualElement>("info-icon");
            Assert.IsNotNull(infoIcon, "info-icon not found");
            Assert.That(infoIcon.resolvedStyle.width, Is.EqualTo(InfoIconSize).Within(FloatTolerance),
                $"info-icon width must resolve to {InfoIconSize}px");
            Assert.That(infoIcon.resolvedStyle.height, Is.EqualTo(InfoIconSize).Within(FloatTolerance),
                $"info-icon height must resolve to {InfoIconSize}px");
            AssertBackgroundImageBound(infoIcon, "info-icon");
            Assert.That(infoIcon.resolvedStyle.backgroundColor.a, Is.LessThan(TransparentAlphaTolerance),
                "info-icon background-color must be transparent so the sprite shows through");
            Assert.That(infoIcon.resolvedStyle.borderTopLeftRadius, Is.LessThan(RadiusTolerance),
                "info-icon border-top-left-radius must drop to 0 (no longer a circle)");
            Assert.That(infoIcon.resolvedStyle.borderTopRightRadius, Is.LessThan(RadiusTolerance),
                "info-icon border-top-right-radius must drop to 0 (no longer a circle)");
            Assert.That(infoIcon.resolvedStyle.borderBottomLeftRadius, Is.LessThan(RadiusTolerance),
                "info-icon border-bottom-left-radius must drop to 0 (no longer a circle)");
            Assert.That(infoIcon.resolvedStyle.borderBottomRightRadius, Is.LessThan(RadiusTolerance),
                "info-icon border-bottom-right-radius must drop to 0 (no longer a circle)");
            AssertBackgroundImageNameContains(infoIcon, "warrior", "info-icon");

            Button navPrevBtn = root.Q<Button>("nav-prev-btn");
            Assert.IsNotNull(navPrevBtn, "nav-prev-btn not found");
            VisualElement prevIcon = navPrevBtn.Q<VisualElement>(className: "info-nav-btn-icon");
            Assert.IsNotNull(prevIcon, "nav-prev-btn must contain a child with class 'info-nav-btn-icon'");
            Assert.That(prevIcon.resolvedStyle.width, Is.EqualTo(NavBtnIconSize).Within(FloatTolerance),
                $"prev nav-btn icon width must resolve to {NavBtnIconSize}px");
            Assert.That(prevIcon.resolvedStyle.height, Is.EqualTo(NavBtnIconSize).Within(FloatTolerance),
                $"prev nav-btn icon height must resolve to {NavBtnIconSize}px");
            AssertBackgroundImageBound(prevIcon, "nav-prev-btn icon child");
            Assert.That(prevIcon.resolvedStyle.scale.value.x, Is.EqualTo(1f).Within(ScaleTolerance),
                "prev nav-btn icon scale.x must be 1 (not flipped)");
            Assert.IsFalse(prevIcon.ClassListContains("info-nav-btn-icon--flip"),
                "prev nav-btn icon must NOT have the 'info-nav-btn-icon--flip' modifier class");
            AssertBackgroundImageNameContains(prevIcon, "arrow", "nav-prev-btn icon child");

            Button navNextBtn = root.Q<Button>("nav-next-btn");
            Assert.IsNotNull(navNextBtn, "nav-next-btn not found");
            VisualElement nextIcon = navNextBtn.Q<VisualElement>(className: "info-nav-btn-icon");
            Assert.IsNotNull(nextIcon, "nav-next-btn must contain a child with class 'info-nav-btn-icon'");
            Assert.That(nextIcon.resolvedStyle.width, Is.EqualTo(NavBtnIconSize).Within(FloatTolerance),
                $"next nav-btn icon width must resolve to {NavBtnIconSize}px");
            Assert.That(nextIcon.resolvedStyle.height, Is.EqualTo(NavBtnIconSize).Within(FloatTolerance),
                $"next nav-btn icon height must resolve to {NavBtnIconSize}px");
            AssertBackgroundImageBound(nextIcon, "nav-next-btn icon child");
            Assert.That(nextIcon.resolvedStyle.scale.value.x, Is.EqualTo(-1f).Within(ScaleTolerance),
                "next nav-btn icon scale.x must be -1 (USS 'scale: -1 1' applied via --flip modifier)");
            Assert.IsTrue(nextIcon.ClassListContains("info-nav-btn-icon--flip"),
                "next nav-btn icon must have the 'info-nav-btn-icon--flip' modifier class");
            AssertBackgroundImageNameContains(nextIcon, "arrow", "nav-next-btn icon child");
        }

        private static void AssertBackgroundImageBound(VisualElement element, string elementLabel)
        {
            Background background = element.resolvedStyle.backgroundImage;
            bool hasTexture = background.texture != null;
            bool hasSprite = background.sprite != null;
            Assert.IsTrue(hasTexture || hasSprite,
                $"{elementLabel} background-image must resolve to a texture or sprite (URL not bound)");
        }

        private static void AssertBackgroundImageNameContains(VisualElement element, string expectedSubstring, string elementLabel)
        {
            Background background = element.resolvedStyle.backgroundImage;
            string assetName = background.texture != null ? background.texture.name
                : background.sprite != null ? background.sprite.name
                : null;
            Assert.IsNotNull(assetName,
                $"{elementLabel} background-image has no texture or sprite name to check against '{expectedSubstring}'");
            Assert.That(assetName.ToLowerInvariant(), Does.Contain(expectedSubstring.ToLowerInvariant()),
                $"{elementLabel} background-image asset name '{assetName}' must contain '{expectedSubstring}' (sprite identity guard)");
        }
    }
}
