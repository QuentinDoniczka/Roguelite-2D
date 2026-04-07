using RogueliteAutoBattler.UI.Core;
using RogueliteAutoBattler.UI.Widgets;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;

namespace RogueliteAutoBattler.Editor
{
    internal static class CombatInfoBuilder
    {
        private const float HeaderHeight = 50f;
        private const float CharIconSize = 34f;
        private const float TabButtonSize = 26f;
        private const float StatRowHeight = 40f;
        private const float StatNameFontSize = 14f;
        private const float StatValueFontSize = 14f;
        private const float BreakdownFontSize = 12f;
        private const float EmptyStateFontSize = 24f;
        private const float HeaderSpacing = 6f;
        private const float TabButtonSpacing = 4f;
        private const float RowMainSpacing = 6f;
        private const float ContentSpacing = 2f;
        private const float ContentPaddingH = 8f;
        private const float ContentPaddingV = 6f;
        private const float HeaderPaddingH = 8f;
        private const float BreakdownPaddingH = 10f;
        private const float BreakdownPaddingV = 6f;
        private const float CharNameFontSize = 13f;
        private const float TeamPosFontSize = 11f;
        private const float TabLabelFontSize = 11f;
        private const float StatNamePreferredWidth = 90f;
        private const float PlaceholderFontSize = 16f;
        private const int HeaderPaddingV = 4;
        private const int RowMainPaddingH = 6;

        private static readonly Color PanelBg = new Color32(0, 0, 0, 180);
        private static readonly Color HeaderBg = new Color32(40, 40, 55, 220);
        private static readonly Color BreakdownBg = new Color32(40, 40, 50, 200);
        private static readonly Color TabActiveColor = new Color32(60, 60, 80, 255);
        private static readonly Color TabInactiveColor = new Color32(35, 35, 50, 220);
        private static readonly Color LabelColor = new Color32(160, 160, 180, 255);
        private static readonly Color SeparatorColor = new Color32(80, 80, 100, 120);
        private static readonly Color BreakdownTextColor = new Color32(200, 200, 220, 255);

        private static readonly Color HpAccent = new Color32(220, 50, 50, 255);
        private static readonly Color AtkAccent = new Color32(255, 165, 0, 255);
        private static readonly Color DefAccent = new Color32(100, 130, 180, 255);
        private static readonly Color SpdAccent = new Color32(70, 150, 255, 255);
        private static readonly Color RegenAccent = new Color32(50, 200, 100, 255);
        private static readonly Color CritAccent = new Color32(255, 220, 0, 255);
        private static readonly Color CharIconPlaceholderColor = new Color32(80, 80, 100, 200);

        private readonly struct StatRowCfg
        {
            public readonly string GoName;
            public readonly string DisplayName;
            public readonly Color Accent;

            public StatRowCfg(string goName, string displayName, Color accent)
            {
                GoName = goName;
                DisplayName = displayName;
                Accent = accent;
            }
        }

        private static readonly StatRowCfg[] StatRowConfigs = new[]
        {
            new StatRowCfg("StatRow_HP",    "\u2665 HP",       HpAccent),
            new StatRowCfg("StatRow_ATK",   "\u2694 ATK",      AtkAccent),
            new StatRowCfg("StatRow_DEF",   "\uD83D\uDEE1 DEF",DefAccent),
            new StatRowCfg("StatRow_SPD",   "\u26A1 SPD",      SpdAccent),
            new StatRowCfg("StatRow_REGEN", "\u2665 REGEN",    RegenAccent),
            new StatRowCfg("StatRow_CRIT",  "\u2605 CRIT",     CritAccent),
        };

        internal static UIScreen CreateCombatInfo(Transform infoAreaParent)
        {
            var rootGo = new GameObject("CombatInfo");
            GameObjectUtility.SetParentAndAlign(rootGo, infoAreaParent.gameObject);
            EditorUIFactory.Stretch(rootGo.AddComponent<RectTransform>());
            rootGo.AddComponent<Image>().color = PanelBg;
            EditorUIFactory.SetupCanvasGroup(rootGo, true);
            UIScreen screen = rootGo.AddComponent<UIScreen>();

            VerticalLayoutGroup rootLayout = rootGo.AddComponent<VerticalLayoutGroup>();
            rootLayout.childControlWidth = true;
            rootLayout.childControlHeight = true;
            rootLayout.childForceExpandWidth = true;
            rootLayout.childForceExpandHeight = false;

            var allyStatsPanelGo = new GameObject("AllyStatsPanel");
            GameObjectUtility.SetParentAndAlign(allyStatsPanelGo, rootGo);
            EditorUIFactory.Stretch(allyStatsPanelGo.AddComponent<RectTransform>());
            LayoutElement allyStatsPanelLE = allyStatsPanelGo.AddComponent<LayoutElement>();
            allyStatsPanelLE.flexibleHeight = 1;
            CanvasGroup panelCanvasGroup = EditorUIFactory.SetupCanvasGroup(allyStatsPanelGo, false);

            VerticalLayoutGroup panelLayout = allyStatsPanelGo.AddComponent<VerticalLayoutGroup>();
            panelLayout.childControlWidth = true;
            panelLayout.childControlHeight = true;
            panelLayout.childForceExpandWidth = true;
            panelLayout.childForceExpandHeight = false;

            GameObject headerGo = CreateHeader(allyStatsPanelGo.transform,
                out GameObject tabButtonsContainer,
                out Image[] tabButtonImages);

            LayoutElement headerLE = headerGo.AddComponent<LayoutElement>();
            headerLE.preferredHeight = HeaderHeight;
            headerLE.flexibleHeight = 0;

            GameObject tabContentStats = CreateStatsTabContent(allyStatsPanelGo.transform,
                out ScrollRect scrollRect,
                out TextMeshProUGUI[] statNameLabels,
                out TextMeshProUGUI[] statValueLabels,
                out GameObject[] breakdownContainers,
                out TextMeshProUGUI[] breakdownTexts,
                out CanvasGroup[] statRowGroups,
                out AllyStatsPanel panelComponent);

            LayoutElement statsContentLE = tabContentStats.AddComponent<LayoutElement>();
            statsContentLE.flexibleHeight = 1;

            GameObject tabContentEquip = CreatePlaceholderTabContent(allyStatsPanelGo.transform, "TabContent_Equip", "Equipment \u2014 Coming soon", false);
            GameObject tabContentTraits = CreatePlaceholderTabContent(allyStatsPanelGo.transform, "TabContent_Traits", "Traits \u2014 Coming soon", false);
            GameObject tabContentLoot = CreatePlaceholderTabContent(allyStatsPanelGo.transform, "TabContent_Loot", "Loot \u2014 Coming soon", false);

            var emptyStateLabelGo = new GameObject("EmptyStateLabel");
            GameObjectUtility.SetParentAndAlign(emptyStateLabelGo, rootGo);
            RectTransform emptyStateLabelRT = emptyStateLabelGo.AddComponent<RectTransform>();
            emptyStateLabelRT.anchorMin = Vector2.zero;
            emptyStateLabelRT.anchorMax = Vector2.one;
            emptyStateLabelRT.offsetMin = Vector2.zero;
            emptyStateLabelRT.offsetMax = Vector2.zero;
            LayoutElement emptyStateLabelLE = emptyStateLabelGo.AddComponent<LayoutElement>();
            emptyStateLabelLE.ignoreLayout = true;
            TextMeshProUGUI emptyStateLabel = emptyStateLabelGo.AddComponent<TextMeshProUGUI>();
            emptyStateLabel.text = "Select an ally";
            emptyStateLabel.fontSize = EmptyStateFontSize;
            emptyStateLabel.color = LabelColor;
            emptyStateLabel.alignment = TextAlignmentOptions.Center;

            WireTabButtonListeners(tabButtonImages, panelComponent);
            WireStatRowButtonListeners(tabContentStats, panelComponent);

            var so = new SerializedObject(panelComponent);

            EditorUIFactory.SetObj(so, "_canvasGroup", panelCanvasGroup);
            EditorUIFactory.SetObj(so, "_emptyStateLabel", emptyStateLabel);
            EditorUIFactory.SetObj(so, "_tabHeaderContainer", tabButtonsContainer);
            EditorUIFactory.SetObj(so, "_scrollRect", scrollRect);

            EditorUIFactory.SetColor(so, "_tabActiveColor", TabActiveColor);
            EditorUIFactory.SetColor(so, "_tabInactiveColor", TabInactiveColor);

            WireObjectArray(so, "_tabContents", new GameObject[]
            {
                tabContentStats, tabContentEquip, tabContentTraits, tabContentLoot
            });

            WireComponentArray(so, "_tabButtonImages", tabButtonImages);
            WireComponentArray(so, "_statValueLabels", statValueLabels);
            WireComponentArray(so, "_statNameLabels", statNameLabels);
            WireObjectArray(so, "_breakdownContainers", breakdownContainers);
            WireComponentArray(so, "_breakdownTexts", breakdownTexts);
            WireComponentArray(so, "_statRowGroups", statRowGroups);

            EditorUIFactory.SetObj(so, "_hpLabel", statValueLabels[0]);
            EditorUIFactory.SetObj(so, "_atkLabel", statValueLabels[1]);
            EditorUIFactory.SetObj(so, "_attackSpeedLabel", statValueLabels[3]);

            SerializedProperty statCardGroupsProp = EditorUIFactory.FindProp(so, "_statCardGroups");
            if (statCardGroupsProp != null)
            {
                statCardGroupsProp.arraySize = 3;
                statCardGroupsProp.GetArrayElementAtIndex(0).objectReferenceValue = statRowGroups[0];
                statCardGroupsProp.GetArrayElementAtIndex(1).objectReferenceValue = statRowGroups[1];
                statCardGroupsProp.GetArrayElementAtIndex(2).objectReferenceValue = statRowGroups[2];
            }

            so.ApplyModifiedProperties();

            return screen;
        }

        private static GameObject CreateHeader(Transform parent,
            out GameObject tabButtonsContainer,
            out Image[] tabButtonImages)
        {
            var headerGo = new GameObject("Header");
            GameObjectUtility.SetParentAndAlign(headerGo, parent.gameObject);
            headerGo.AddComponent<RectTransform>();
            headerGo.AddComponent<Image>().color = HeaderBg;

            HorizontalLayoutGroup headerLayout = headerGo.AddComponent<HorizontalLayoutGroup>();
            headerLayout.padding = new RectOffset((int)HeaderPaddingH, (int)HeaderPaddingH, HeaderPaddingV, HeaderPaddingV);
            headerLayout.spacing = HeaderSpacing;
            headerLayout.childAlignment = TextAnchor.MiddleLeft;
            headerLayout.childControlWidth = false;
            headerLayout.childControlHeight = true;
            headerLayout.childForceExpandWidth = false;
            headerLayout.childForceExpandHeight = true;

            var charIconGo = new GameObject("CharacterIcon");
            GameObjectUtility.SetParentAndAlign(charIconGo, headerGo);
            charIconGo.AddComponent<RectTransform>();
            Image charIconImage = charIconGo.AddComponent<Image>();
            charIconImage.color = CharIconPlaceholderColor;
            LayoutElement charIconLE = charIconGo.AddComponent<LayoutElement>();
            charIconLE.minWidth = CharIconSize;
            charIconLE.preferredWidth = CharIconSize;
            charIconLE.flexibleWidth = 0;

            var charInfoGo = new GameObject("CharacterInfo");
            GameObjectUtility.SetParentAndAlign(charInfoGo, headerGo);
            charInfoGo.AddComponent<RectTransform>();
            VerticalLayoutGroup charInfoLayout = charInfoGo.AddComponent<VerticalLayoutGroup>();
            charInfoLayout.childControlWidth = true;
            charInfoLayout.childControlHeight = true;
            charInfoLayout.childForceExpandWidth = true;
            charInfoLayout.childForceExpandHeight = false;
            charInfoLayout.childAlignment = TextAnchor.MiddleLeft;
            LayoutElement charInfoLE = charInfoGo.AddComponent<LayoutElement>();
            charInfoLE.flexibleWidth = 1;

            var nameLabelGo = new GameObject("NameLabel");
            GameObjectUtility.SetParentAndAlign(nameLabelGo, charInfoGo);
            nameLabelGo.AddComponent<RectTransform>();
            TextMeshProUGUI nameLabel = nameLabelGo.AddComponent<TextMeshProUGUI>();
            nameLabel.text = "Guerrier C \u2014 Niv.8";
            nameLabel.fontSize = CharNameFontSize;
            nameLabel.color = Color.white;
            nameLabel.alignment = TextAlignmentOptions.Left;
            nameLabel.fontStyle = FontStyles.Bold;

            var teamPosLabelGo = new GameObject("TeamPosLabel");
            GameObjectUtility.SetParentAndAlign(teamPosLabelGo, charInfoGo);
            teamPosLabelGo.AddComponent<RectTransform>();
            TextMeshProUGUI teamPosLabel = teamPosLabelGo.AddComponent<TextMeshProUGUI>();
            teamPosLabel.text = "\u25C4 2/4 \u25BA";
            teamPosLabel.fontSize = TeamPosFontSize;
            teamPosLabel.color = LabelColor;
            teamPosLabel.alignment = TextAlignmentOptions.Left;

            var tabButtonsGo = new GameObject("TabButtons");
            GameObjectUtility.SetParentAndAlign(tabButtonsGo, headerGo);
            tabButtonsGo.AddComponent<RectTransform>();
            HorizontalLayoutGroup tabLayout = tabButtonsGo.AddComponent<HorizontalLayoutGroup>();
            tabLayout.spacing = TabButtonSpacing;
            tabLayout.childAlignment = TextAnchor.MiddleRight;
            tabLayout.childControlWidth = false;
            tabLayout.childControlHeight = false;
            tabLayout.childForceExpandWidth = false;
            tabLayout.childForceExpandHeight = false;
            LayoutElement tabButtonsLE = tabButtonsGo.AddComponent<LayoutElement>();
            tabButtonsLE.flexibleWidth = 0;

            tabButtonsContainer = tabButtonsGo;

            var tabConfigs = new[] { "S", "E", "T", "L" };
            tabButtonImages = new Image[tabConfigs.Length];

            for (int i = 0; i < tabConfigs.Length; i++)
            {
                var tabGo = new GameObject("Tab_" + GetTabName(i));
                GameObjectUtility.SetParentAndAlign(tabGo, tabButtonsGo);
                tabGo.AddComponent<RectTransform>();
                LayoutElement tabLE = tabGo.AddComponent<LayoutElement>();
                tabLE.minWidth = TabButtonSize;
                tabLE.minHeight = TabButtonSize;
                tabLE.preferredWidth = TabButtonSize;
                tabLE.preferredHeight = TabButtonSize;
                tabLE.flexibleWidth = 0;
                tabLE.flexibleHeight = 0;

                Image tabImage = tabGo.AddComponent<Image>();
                tabImage.color = i == 0 ? TabActiveColor : TabInactiveColor;
                tabButtonImages[i] = tabImage;

                tabGo.AddComponent<Button>();

                var tabLabelGo = new GameObject("Label");
                GameObjectUtility.SetParentAndAlign(tabLabelGo, tabGo);
                EditorUIFactory.Stretch(tabLabelGo.AddComponent<RectTransform>());
                TextMeshProUGUI tabLabel = tabLabelGo.AddComponent<TextMeshProUGUI>();
                tabLabel.text = tabConfigs[i];
                tabLabel.fontSize = TabLabelFontSize;
                tabLabel.color = Color.white;
                tabLabel.alignment = TextAlignmentOptions.Center;
                tabLabel.fontStyle = FontStyles.Bold;
            }

            return headerGo;
        }

        private static string GetTabName(int index)
        {
            switch (index)
            {
                case 0: return "Stats";
                case 1: return "Equip";
                case 2: return "Traits";
                case 3: return "Loot";
                default: return index.ToString();
            }
        }

        private static GameObject CreateStatsTabContent(Transform parent,
            out ScrollRect scrollRect,
            out TextMeshProUGUI[] statNameLabels,
            out TextMeshProUGUI[] statValueLabels,
            out GameObject[] breakdownContainers,
            out TextMeshProUGUI[] breakdownTexts,
            out CanvasGroup[] statRowGroups,
            out AllyStatsPanel panelComponent)
        {
            var tabContentGo = new GameObject("TabContent_Stats");
            GameObjectUtility.SetParentAndAlign(tabContentGo, parent.gameObject);
            tabContentGo.AddComponent<RectTransform>();

            var scrollViewGo = new GameObject("StatsScrollView");
            GameObjectUtility.SetParentAndAlign(scrollViewGo, tabContentGo);
            EditorUIFactory.Stretch(scrollViewGo.AddComponent<RectTransform>());
            scrollViewGo.AddComponent<RectMask2D>();
            scrollRect = scrollViewGo.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;

            var contentGo = new GameObject("Content");
            GameObjectUtility.SetParentAndAlign(contentGo, scrollViewGo);
            RectTransform contentRT = contentGo.AddComponent<RectTransform>();
            contentRT.anchorMin = new Vector2(0, 1);
            contentRT.anchorMax = new Vector2(1, 1);
            contentRT.pivot = new Vector2(0.5f, 1);
            contentRT.offsetMin = Vector2.zero;
            contentRT.offsetMax = Vector2.zero;

            VerticalLayoutGroup contentLayout = contentGo.AddComponent<VerticalLayoutGroup>();
            contentLayout.padding = new RectOffset(
                (int)ContentPaddingH, (int)ContentPaddingH,
                (int)ContentPaddingV, (int)ContentPaddingV);
            contentLayout.spacing = ContentSpacing;
            contentLayout.childControlWidth = true;
            contentLayout.childControlHeight = false;
            contentLayout.childForceExpandWidth = true;
            contentLayout.childForceExpandHeight = false;

            ContentSizeFitter contentFitter = contentGo.AddComponent<ContentSizeFitter>();
            contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            contentFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

            scrollRect.content = contentRT;
            scrollRect.viewport = (RectTransform)scrollViewGo.transform;

            int rowCount = StatRowConfigs.Length;
            statNameLabels = new TextMeshProUGUI[rowCount];
            statValueLabels = new TextMeshProUGUI[rowCount];
            breakdownContainers = new GameObject[rowCount];
            breakdownTexts = new TextMeshProUGUI[rowCount];
            statRowGroups = new CanvasGroup[rowCount];

            panelComponent = null;

            for (int i = 0; i < rowCount; i++)
            {
                StatRowCfg cfg = StatRowConfigs[i];
                CreateStatRow(contentGo.transform, cfg, i,
                    out statNameLabels[i], out statValueLabels[i],
                    out breakdownContainers[i], out breakdownTexts[i],
                    out statRowGroups[i]);
            }

            panelComponent = parent.gameObject.GetComponent<AllyStatsPanel>();
            if (panelComponent == null)
                panelComponent = parent.gameObject.AddComponent<AllyStatsPanel>();

            return tabContentGo;
        }

        private static void CreateStatRow(Transform parent, StatRowCfg cfg, int rowIndex,
            out TextMeshProUGUI statNameLabel,
            out TextMeshProUGUI statValueLabel,
            out GameObject breakdownContainer,
            out TextMeshProUGUI breakdownText,
            out CanvasGroup rowCanvasGroup)
        {
            var rowGo = new GameObject(cfg.GoName);
            GameObjectUtility.SetParentAndAlign(rowGo, parent.gameObject);
            rowGo.AddComponent<RectTransform>();
            rowCanvasGroup = rowGo.AddComponent<CanvasGroup>();
            rowCanvasGroup.alpha = 0f;

            VerticalLayoutGroup rowLayout = rowGo.AddComponent<VerticalLayoutGroup>();
            rowLayout.childControlWidth = true;
            rowLayout.childControlHeight = false;
            rowLayout.childForceExpandWidth = true;
            rowLayout.childForceExpandHeight = false;

            LayoutElement rowLE = rowGo.AddComponent<LayoutElement>();
            rowLE.preferredHeight = StatRowHeight;

            var rowButtonGo = new GameObject("RowButton");
            GameObjectUtility.SetParentAndAlign(rowButtonGo, rowGo);
            EditorUIFactory.Stretch(rowButtonGo.AddComponent<RectTransform>());
            Image rowBtnImage = rowButtonGo.AddComponent<Image>();
            rowBtnImage.color = Color.clear;
            rowButtonGo.AddComponent<Button>();
            rowButtonGo.AddComponent<LayoutElement>().ignoreLayout = true;

            var rowMainGo = new GameObject("RowMain");
            GameObjectUtility.SetParentAndAlign(rowMainGo, rowGo);
            rowMainGo.AddComponent<RectTransform>();
            HorizontalLayoutGroup rowMainLayout = rowMainGo.AddComponent<HorizontalLayoutGroup>();
            rowMainLayout.padding = new RectOffset(RowMainPaddingH, RowMainPaddingH, 0, 0);
            rowMainLayout.spacing = RowMainSpacing;
            rowMainLayout.childAlignment = TextAnchor.MiddleLeft;
            rowMainLayout.childControlWidth = false;
            rowMainLayout.childControlHeight = true;
            rowMainLayout.childForceExpandWidth = false;
            rowMainLayout.childForceExpandHeight = true;
            LayoutElement rowMainLE = rowMainGo.AddComponent<LayoutElement>();
            rowMainLE.preferredHeight = StatRowHeight;
            rowMainLE.flexibleWidth = 1;

            var statNameGo = new GameObject("StatName");
            GameObjectUtility.SetParentAndAlign(statNameGo, rowMainGo);
            statNameGo.AddComponent<RectTransform>();
            statNameLabel = statNameGo.AddComponent<TextMeshProUGUI>();
            statNameLabel.text = cfg.DisplayName;
            statNameLabel.fontSize = StatNameFontSize;
            statNameLabel.color = cfg.Accent;
            statNameLabel.alignment = TextAlignmentOptions.Left;
            LayoutElement nameLE = statNameGo.AddComponent<LayoutElement>();
            nameLE.preferredWidth = StatNamePreferredWidth;
            nameLE.flexibleWidth = 0;

            var statValueGo = new GameObject("StatValue");
            GameObjectUtility.SetParentAndAlign(statValueGo, rowMainGo);
            statValueGo.AddComponent<RectTransform>();
            statValueLabel = statValueGo.AddComponent<TextMeshProUGUI>();
            statValueLabel.text = rowIndex == 0 ? "--- / ---" : "---";
            statValueLabel.fontSize = StatValueFontSize;
            statValueLabel.color = Color.white;
            statValueLabel.alignment = TextAlignmentOptions.Right;
            statValueLabel.fontStyle = FontStyles.Bold;
            LayoutElement valueLE = statValueGo.AddComponent<LayoutElement>();
            valueLE.flexibleWidth = 1;

            var separatorGo = new GameObject("Separator");
            GameObjectUtility.SetParentAndAlign(separatorGo, rowGo);
            separatorGo.AddComponent<RectTransform>();
            separatorGo.AddComponent<Image>().color = SeparatorColor;
            LayoutElement separatorLE = separatorGo.AddComponent<LayoutElement>();
            separatorLE.preferredHeight = 1f;
            separatorLE.flexibleWidth = 1;

            var breakdownGo = new GameObject("Breakdown");
            GameObjectUtility.SetParentAndAlign(breakdownGo, rowGo);
            breakdownGo.AddComponent<RectTransform>();
            breakdownGo.AddComponent<Image>().color = BreakdownBg;
            VerticalLayoutGroup breakdownLayout = breakdownGo.AddComponent<VerticalLayoutGroup>();
            breakdownLayout.padding = new RectOffset(
                (int)BreakdownPaddingH, (int)BreakdownPaddingH,
                (int)BreakdownPaddingV, (int)BreakdownPaddingV);
            breakdownLayout.childControlWidth = true;
            breakdownLayout.childControlHeight = true;
            breakdownLayout.childForceExpandWidth = true;
            breakdownLayout.childForceExpandHeight = false;
            ContentSizeFitter breakdownFitter = breakdownGo.AddComponent<ContentSizeFitter>();
            breakdownFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            breakdownFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            breakdownContainer = breakdownGo;
            breakdownGo.SetActive(false);

            var breakdownTextGo = new GameObject("BreakdownText");
            GameObjectUtility.SetParentAndAlign(breakdownTextGo, breakdownGo);
            EditorUIFactory.Stretch(breakdownTextGo.AddComponent<RectTransform>());
            breakdownText = breakdownTextGo.AddComponent<TextMeshProUGUI>();
            breakdownText.text = "";
            breakdownText.fontSize = BreakdownFontSize;
            breakdownText.color = BreakdownTextColor;
            breakdownText.alignment = TextAlignmentOptions.Left;
            breakdownText.enableWordWrapping = true;
        }

        private static GameObject CreatePlaceholderTabContent(Transform parent, string name, string labelText, bool activeByDefault)
        {
            var go = new GameObject(name);
            GameObjectUtility.SetParentAndAlign(go, parent.gameObject);
            EditorUIFactory.Stretch(go.AddComponent<RectTransform>());
            LayoutElement placeholderLE = go.AddComponent<LayoutElement>();
            placeholderLE.flexibleHeight = 1;
            go.SetActive(activeByDefault);

            var labelGo = new GameObject("PlaceholderLabel");
            GameObjectUtility.SetParentAndAlign(labelGo, go);
            EditorUIFactory.Stretch(labelGo.AddComponent<RectTransform>());
            TextMeshProUGUI label = labelGo.AddComponent<TextMeshProUGUI>();
            label.text = labelText;
            label.fontSize = PlaceholderFontSize;
            label.color = LabelColor;
            label.alignment = TextAlignmentOptions.Center;

            return go;
        }

        private static void WireTabButtonListeners(Image[] tabButtonImages, AllyStatsPanel panel)
        {
            for (int i = 0; i < tabButtonImages.Length; i++)
            {
                Button btn = tabButtonImages[i].GetComponent<Button>();
                if (btn == null) continue;

                int tabIndex = i;
                UnityEventTools.AddIntPersistentListener(btn.onClick, panel.SwitchTab, tabIndex);
            }
        }

        private static void WireStatRowButtonListeners(GameObject tabContentStats, AllyStatsPanel panel)
        {
            Transform contentTransform = tabContentStats.transform.Find("StatsScrollView/Content");
            if (contentTransform == null) return;

            int rowCount = StatRowConfigs.Length;
            for (int i = 0; i < rowCount; i++)
            {
                StatRowCfg cfg = StatRowConfigs[i];
                Transform rowTransform = contentTransform.Find(cfg.GoName);
                if (rowTransform == null) continue;

                Transform rowButtonTransform = rowTransform.Find("RowButton");
                if (rowButtonTransform == null) continue;

                Button btn = rowButtonTransform.GetComponent<Button>();
                if (btn == null) continue;

                int rowIndex = i;
                UnityEventTools.AddIntPersistentListener(btn.onClick, panel.ToggleBreakdown, rowIndex);
            }
        }

        private static void WireObjectArray(SerializedObject so, string propertyName, GameObject[] items)
        {
            SerializedProperty prop = EditorUIFactory.FindProp(so, propertyName);
            if (prop == null) return;
            prop.arraySize = items.Length;
            for (int i = 0; i < items.Length; i++)
                prop.GetArrayElementAtIndex(i).objectReferenceValue = items[i];
        }

        private static void WireComponentArray<T>(SerializedObject so, string propertyName, T[] items) where T : Object
        {
            SerializedProperty prop = EditorUIFactory.FindProp(so, propertyName);
            if (prop == null) return;
            prop.arraySize = items.Length;
            for (int i = 0; i < items.Length; i++)
                prop.GetArrayElementAtIndex(i).objectReferenceValue = items[i];
        }
    }
}
