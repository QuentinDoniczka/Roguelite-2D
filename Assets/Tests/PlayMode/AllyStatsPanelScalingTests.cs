#if UNITY_EDITOR
using System.Collections;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace RogueliteAutoBattler.Tests.PlayMode
{
    [Category("Scaling")]
    public class AllyStatsPanelScalingTests : PlayModeTestBase
    {
        private const string MainLayoutPath = "Assets/UI/Layouts/MainLayout.uxml";
        private const string MainStylePath = "Assets/UI/Styles/MainStyle.uss";
        private const string PanelSettingsPath = "Assets/UI/MainPanelSettings.asset";

        private const float InfoFontSize = 42f;
        private const float InfoNameFontSize = 34f;
        private const float InfoNavBtnSize = 72f;
        private const float InfoNavBtnFontSize = 40f;
        private const float InfoIconSize = 64f;
        private const float StatRowHeaderHeight = 100f;
        private const float StatBreakdownPaddingLeft = 32f;
        private const float BreakdownFontSize = 24f;
        private const float InfoAreaHeightPercent = 0.42f;
        private const float FloatTolerance = 0.5f;
        private const float AreaTolerancePixels = 1f;

        private UIDocument _uiDocument;

        [UnityTest]
        public IEnumerator ResolvedStyles_MatchMobileScaledValues()
        {
            VisualTreeAsset layoutAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(MainLayoutPath);
            Assert.IsNotNull(layoutAsset, $"MainLayout UXML not found at {MainLayoutPath}");

            StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(MainStylePath);
            Assert.IsNotNull(styleSheet, $"MainStyle USS not found at {MainStylePath}");

            PanelSettings panelSettings = AssetDatabase.LoadAssetAtPath<PanelSettings>(PanelSettingsPath);
            Assert.IsNotNull(panelSettings, $"MainPanelSettings not found at {PanelSettingsPath}");

            var documentGo = Track(new GameObject("TestUIDocument"));
            documentGo.SetActive(false);
            _uiDocument = documentGo.AddComponent<UIDocument>();
            _uiDocument.panelSettings = panelSettings;
            _uiDocument.visualTreeAsset = layoutAsset;
            documentGo.SetActive(true);

            yield return null;
            yield return null;

            VisualElement root = _uiDocument.rootVisualElement;
            if (root == null)
            {
                Assert.Inconclusive("rootVisualElement is null — UIDocument failed to initialize in the test environment.");
                yield break;
            }

            var infoContent = root.Q<VisualElement>("info-content");
            Assert.That(infoContent, Is.Not.Null, "info-content element missing");
            infoContent.style.display = DisplayStyle.Flex;
            yield return null;
            yield return null;

            VisualElement infoPanelRoot = root.Q<VisualElement>("info-panel-root");
            Assert.IsNotNull(infoPanelRoot, "info-panel-root not found in the visual tree.");

            Label infoNameLabel = root.Q<Label>("info-name-label");
            Assert.IsNotNull(infoNameLabel, "info-name-label not found.");
            Assert.That(infoNameLabel.resolvedStyle.fontSize, Is.EqualTo(InfoNameFontSize).Within(FloatTolerance),
                $"info-name-label font-size must resolve to {InfoNameFontSize}px (var --info-name-font-size)");

            Label infoTeamPosLabel = root.Q<Label>("info-team-pos-label");
            Assert.IsNotNull(infoTeamPosLabel, "info-team-pos-label not found.");
            Assert.That(infoTeamPosLabel.resolvedStyle.fontSize, Is.EqualTo(InfoFontSize).Within(FloatTolerance),
                $"info-team-pos-label font-size must resolve to {InfoFontSize}px (var --info-font-size)");

            Button infoTabStats = root.Q<Button>("info-tab-stats");
            Assert.IsNotNull(infoTabStats, "info-tab-stats not found.");
            Assert.That(infoTabStats.resolvedStyle.fontSize, Is.EqualTo(InfoFontSize).Within(FloatTolerance),
                $"info-tab-stats font-size must resolve to {InfoFontSize}px (var --info-font-size)");

            Button navPrevBtn = root.Q<Button>("nav-prev-btn");
            Assert.IsNotNull(navPrevBtn, "nav-prev-btn not found.");
            Assert.That(navPrevBtn.resolvedStyle.width, Is.EqualTo(InfoNavBtnSize).Within(FloatTolerance),
                $"nav-prev-btn width must resolve to {InfoNavBtnSize}px (var --info-nav-btn-size)");
            Assert.That(navPrevBtn.resolvedStyle.fontSize, Is.EqualTo(InfoNavBtnFontSize).Within(FloatTolerance),
                $"nav-prev-btn font-size must resolve to {InfoNavBtnFontSize}px");

            VisualElement infoIcon = root.Q<VisualElement>("info-icon");
            Assert.IsNotNull(infoIcon, "info-icon not found.");
            Assert.That(infoIcon.resolvedStyle.width, Is.EqualTo(InfoIconSize).Within(FloatTolerance),
                $"info-icon width must resolve to {InfoIconSize}px");
            Assert.That(infoIcon.resolvedStyle.height, Is.EqualTo(InfoIconSize).Within(FloatTolerance),
                $"info-icon height must resolve to {InfoIconSize}px");

            VisualElement statsContent = root.Q<VisualElement>("info-tab-content-stats");
            Assert.IsNotNull(statsContent, "info-tab-content-stats not found.");

            var statRow = new VisualElement { name = "test-stat-row-header" };
            statRow.AddToClassList("stat-row-header");
            statsContent.Add(statRow);

            var breakdownContainer = new VisualElement { name = "test-stat-breakdown" };
            breakdownContainer.AddToClassList("stat-breakdown");
            statsContent.Add(breakdownContainer);

            var breakdownText = new Label("breakdown sample") { name = "test-breakdown-text" };
            breakdownText.AddToClassList("breakdown-text");
            breakdownContainer.Add(breakdownText);

            yield return null;
            yield return null;

            Assert.That(statRow.resolvedStyle.height, Is.EqualTo(StatRowHeaderHeight).Within(FloatTolerance),
                $".stat-row-header height must resolve to {StatRowHeaderHeight}px");

            Assert.That(breakdownContainer.resolvedStyle.paddingLeft, Is.EqualTo(StatBreakdownPaddingLeft).Within(FloatTolerance),
                $".stat-breakdown padding-left must resolve to {StatBreakdownPaddingLeft}px");

            Assert.That(breakdownText.resolvedStyle.fontSize, Is.EqualTo(BreakdownFontSize).Within(FloatTolerance),
                $".breakdown-text font-size must resolve to {BreakdownFontSize}px (var --info-breakdown-font-size)");

            VisualElement infoArea = root.Q<VisualElement>("info-area");
            Assert.IsNotNull(infoArea, "info-area not found.");
            float panelHeight = root.resolvedStyle.height;
            Assert.Greater(panelHeight, 0f, "Panel root height must be greater than zero for percentage-based assertions.");
            float expectedInfoAreaHeight = panelHeight * InfoAreaHeightPercent;
            Assert.That(infoArea.resolvedStyle.height, Is.EqualTo(expectedInfoAreaHeight).Within(AreaTolerancePixels),
                $"info-area height must resolve to ~{InfoAreaHeightPercent * 100f}% of panel height (panel={panelHeight}px, expected={expectedInfoAreaHeight}px)");
        }
    }
}
#endif
