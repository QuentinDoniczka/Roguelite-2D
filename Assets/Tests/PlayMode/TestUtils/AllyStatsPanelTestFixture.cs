using System;
using RogueliteAutoBattler.Combat.Core;
using RogueliteAutoBattler.Common;
using RogueliteAutoBattler.Core;
using RogueliteAutoBattler.UI.Widgets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RogueliteAutoBattler.Tests.PlayMode
{
    public struct AllyStatsPanelTestFixtureResult
    {
        public Camera Camera;
        public UnitSelectionManager SelectionManager;
        public AllyStatsPanel Panel;
        public CanvasGroup PanelCanvasGroup;
        public TMP_Text EmptyStateLabel;
        public TMP_Text[] StatValueLabels;
        public TMP_Text[] StatNameLabels;
        public CanvasGroup[] StatRowGroups;
        public GameObject[] BreakdownContainers;
        public TMP_Text[] BreakdownTexts;
        public GameObject[] TabContents;
        public Image[] TabButtonImages;
        public Color ActiveTabColor;
        public Color InactiveTabColor;
    }

    public static class AllyStatsPanelTestFixture
    {
        public const int StatRowCount = 6;

        public static AllyStatsPanelTestFixtureResult Create(
            Func<GameObject, GameObject> track,
            int tabCount = 1)
        {
            if (PhysicsLayers.SelectionLayer < 0)
                NUnit.Framework.Assert.Ignore("Selection layer not configured in this environment.");

            if (UnitSelectionManager.Instance != null)
                UnityEngine.Object.DestroyImmediate(UnitSelectionManager.Instance.gameObject);

            var camGo = new GameObject("TestCamera");
            var camera = camGo.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 10f;
            camera.tag = "MainCamera";
            track(camGo);

            GameBootstrap.ResetForTest();
            var mainCameraProp = typeof(GameBootstrap).GetProperty("MainCamera",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            mainCameraProp.SetValue(null, camera);

            var managerGo = new GameObject("UnitSelectionManager");
            var selectionManager = managerGo.AddComponent<UnitSelectionManager>();
            track(managerGo);

            var canvasGo = new GameObject("TestCanvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<CanvasScaler>();
            track(canvasGo);

            var panelGo = new GameObject("AllyStatsPanel");
            panelGo.transform.SetParent(canvasGo.transform, false);
            panelGo.AddComponent<RectTransform>();
            var panelCanvasGroup = panelGo.AddComponent<CanvasGroup>();

            var emptyStateLabelGo = new GameObject("EmptyStateLabel");
            emptyStateLabelGo.transform.SetParent(panelGo.transform, false);
            var emptyStateLabel = emptyStateLabelGo.AddComponent<TextMeshProUGUI>();

            var statValueLabels = new TMP_Text[StatRowCount];
            var statNameLabels = new TMP_Text[StatRowCount];
            var statRowGroups = new CanvasGroup[StatRowCount];
            var breakdownContainers = new GameObject[StatRowCount];
            var breakdownTexts = new TMP_Text[StatRowCount];

            for (int i = 0; i < StatRowCount; i++)
            {
                var rowGo = new GameObject($"StatRow_{i}");
                rowGo.transform.SetParent(panelGo.transform, false);
                rowGo.AddComponent<RectTransform>();
                statRowGroups[i] = rowGo.AddComponent<CanvasGroup>();

                var nameGo = new GameObject($"StatName_{i}");
                nameGo.transform.SetParent(rowGo.transform, false);
                statNameLabels[i] = nameGo.AddComponent<TextMeshProUGUI>();

                var valueGo = new GameObject($"StatValue_{i}");
                valueGo.transform.SetParent(rowGo.transform, false);
                statValueLabels[i] = valueGo.AddComponent<TextMeshProUGUI>();

                var breakdownGo = new GameObject($"Breakdown_{i}");
                breakdownGo.transform.SetParent(rowGo.transform, false);
                breakdownGo.SetActive(false);
                breakdownContainers[i] = breakdownGo;

                var bdTextGo = new GameObject($"BreakdownText_{i}");
                bdTextGo.transform.SetParent(breakdownGo.transform, false);
                breakdownTexts[i] = bdTextGo.AddComponent<TextMeshProUGUI>();
            }

            var tabHeaderGo = new GameObject("TabHeader");
            tabHeaderGo.transform.SetParent(panelGo.transform, false);

            var tabContents = new GameObject[tabCount];
            var tabButtonImages = new Image[tabCount];

            for (int i = 0; i < tabCount; i++)
            {
                var contentGo = new GameObject($"TabContent_{i}");
                contentGo.transform.SetParent(panelGo.transform, false);
                tabContents[i] = contentGo;

                var btnGo = new GameObject($"TabBtn_{i}");
                btnGo.transform.SetParent(tabHeaderGo.transform, false);
                tabButtonImages[i] = btnGo.AddComponent<Image>();
            }

            var activeColor = Color.white;
            var inactiveColor = Color.gray;

            var panel = panelGo.AddComponent<AllyStatsPanel>();
            panel.InitializeForTest(
                selectionManager,
                panelCanvasGroup,
                PhysicsLayers.AllyLayer,
                emptyStateLabel,
                statValueLabels,
                statNameLabels,
                statRowGroups,
                breakdownContainers,
                breakdownTexts,
                tabContents,
                tabButtonImages,
                activeColor,
                inactiveColor);

            return new AllyStatsPanelTestFixtureResult
            {
                Camera = camera,
                SelectionManager = selectionManager,
                Panel = panel,
                PanelCanvasGroup = panelCanvasGroup,
                EmptyStateLabel = emptyStateLabel,
                StatValueLabels = statValueLabels,
                StatNameLabels = statNameLabels,
                StatRowGroups = statRowGroups,
                BreakdownContainers = breakdownContainers,
                BreakdownTexts = breakdownTexts,
                TabContents = tabContents,
                TabButtonImages = tabButtonImages,
                ActiveTabColor = activeColor,
                InactiveTabColor = inactiveColor,
            };
        }
    }
}
