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
        private const float HeaderHeight = 54f;
        private const float CharIconSize = 46f;
        private const float TabButtonSize = 26f;
        private const float StatRowHeight = 52f;
        private const float MainFontSize = 26f;
        private const float HeaderFontSize = 28f;
        private const float BreakdownFontSize = 22f;
        private const float EmptyStateFontSize = 26f;
        private const float TabLabelFontSize = 20f;
        private const float ArrowButtonSize = 48f;
        private const float ArrowFontSize = 24f;
        private const float PlaceholderFontSize = 20f;
        private const float HeaderSpacing = 6f;
        private const float TabButtonSpacing = 4f;
        private const float RowMainSpacing = 6f;
        private const float ContentSpacing = 2f;
        private const float ContentPaddingH = 20f;
        private const float ContentPaddingTop = 12f;
        private const float ContentPaddingBottom = 24f;
        private const float HeaderPaddingH = 8f;
        private const float BreakdownPaddingH = 16f;
        private const float BreakdownPaddingV = 6f;
        private const float StatNamePreferredWidth = 120f;
        private const int HeaderPaddingV = 4;
        private const int RowMainPaddingH = 12;

        private const float StatsColumnMinWidth = 200f;
        private const float EquipColumnMinWidth = 140f;
        private const float SectionHeaderHeight = 24f;
        private const float VerticalSeparatorWidth = 1f;
        private const float EquipColumnPadding = 12f;
        private const float EquipColumnSpacing = 6f;
        private const float HorizontalSeparatorHeight = 1f;
        private const float NameNavPaddingLeft = 8f;
        private const float TeamPosPreferredWidth = 50f;

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
        private static readonly Color ArrowPressedColor = new Color32(80, 80, 110, 255);
        private static readonly Color CharIconPlaceholderColor = new Color32(80, 80, 100, 200);
        private static readonly Color SectionHeaderColor = new Color32(140, 140, 160, 255);

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
            TMP_FontAsset bangersFont = EditorUIFactory.LoadBangersFont(nameof(CombatInfoBuilder));

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

            GameObject headerGo = CreateHeader(allyStatsPanelGo.transform, bangersFont,
                out Image[] tabButtonImages,
                out TextMeshProUGUI nameLabel,
                out TextMeshProUGUI teamPosLabel,
                out Button prevAllyButton,
                out Button nextAllyButton,
                out Button teamPosButton);

            LayoutElement headerLE = headerGo.AddComponent<LayoutElement>();
            headerLE.preferredHeight = HeaderHeight;
            headerLE.flexibleHeight = 0;

            GameObject tabContentStats = CreateStatsTabContent(allyStatsPanelGo.transform, bangersFont,
                out ScrollRect scrollRect,
                out TextMeshProUGUI[] statNameLabels,
                out TextMeshProUGUI[] statValueLabels,
                out GameObject[] breakdownContainers,
                out TextMeshProUGUI[] breakdownTexts,
                out CanvasGroup[] statRowGroups,
                out AllyStatsPanel panelComponent);

            LayoutElement statsContentLE = tabContentStats.AddComponent<LayoutElement>();
            statsContentLE.flexibleHeight = 1;

            GameObject tabContentTraits = CreatePlaceholderTabContent(allyStatsPanelGo.transform, "TabContent_Traits", "Traits \u2014 Coming soon", false, bangersFont);
            GameObject tabContentLoot = CreatePlaceholderTabContent(allyStatsPanelGo.transform, "TabContent_Loot", "Loot \u2014 Coming soon", false, bangersFont);

            var emptyStateLabelGo = new GameObject("EmptyStateLabel");
            GameObjectUtility.SetParentAndAlign(emptyStateLabelGo, rootGo);
            EditorUIFactory.Stretch(emptyStateLabelGo.AddComponent<RectTransform>());
            LayoutElement emptyStateLabelLE = emptyStateLabelGo.AddComponent<LayoutElement>();
            emptyStateLabelLE.ignoreLayout = true;
            TextMeshProUGUI emptyStateLabel = emptyStateLabelGo.AddComponent<TextMeshProUGUI>();
            emptyStateLabel.text = "Select an ally";
            emptyStateLabel.fontSize = EmptyStateFontSize;
            emptyStateLabel.color = LabelColor;
            emptyStateLabel.alignment = TextAlignmentOptions.Center;
            EditorUIFactory.ApplyFont(emptyStateLabel, bangersFont);

            WireTabButtonListeners(tabButtonImages, panelComponent);
            WireStatRowButtonListeners(tabContentStats, panelComponent);

            var so = new SerializedObject(panelComponent);

            EditorUIFactory.SetObj(so, "_canvasGroup", panelCanvasGroup);
            EditorUIFactory.SetObj(so, "_emptyStateLabel", emptyStateLabel);
            EditorUIFactory.SetObj(so, "_scrollRect", scrollRect);

            EditorUIFactory.SetColor(so, "_tabActiveColor", TabActiveColor);
            EditorUIFactory.SetColor(so, "_tabInactiveColor", TabInactiveColor);

            EditorUIFactory.WireArray(so, "_tabContents", new Object[]
            {
                tabContentStats, tabContentTraits, tabContentLoot
            });

            EditorUIFactory.WireArray(so, "_tabButtonImages", tabButtonImages);
            EditorUIFactory.WireArray(so, "_statValueLabels", statValueLabels);
            EditorUIFactory.WireArray(so, "_statNameLabels", statNameLabels);
            EditorUIFactory.WireArray(so, "_breakdownContainers", breakdownContainers);
            EditorUIFactory.WireArray(so, "_breakdownTexts", breakdownTexts);
            EditorUIFactory.WireArray(so, "_statRowGroups", statRowGroups);

            EditorUIFactory.SetObj(so, "_nameLabel", nameLabel);
            EditorUIFactory.SetObj(so, "_teamPosLabel", teamPosLabel);

            so.ApplyModifiedProperties();

            UnityEventTools.AddPersistentListener(prevAllyButton.onClick, panelComponent.NavigateToPreviousAlly);
            UnityEventTools.AddPersistentListener(nextAllyButton.onClick, panelComponent.NavigateToNextAlly);
            UnityEventTools.AddPersistentListener(teamPosButton.onClick, panelComponent.NavigateToNextAlly);

            return screen;
        }

        private static GameObject CreateHeader(Transform parent, TMP_FontAsset font,
            out Image[] tabButtonImages,
            out TextMeshProUGUI nameLabel,
            out TextMeshProUGUI teamPosLabel,
            out Button prevAllyButton,
            out Button nextAllyButton,
            out Button teamPosButton)
        {
            var headerGo = new GameObject("Header");
            GameObjectUtility.SetParentAndAlign(headerGo, parent.gameObject);
            headerGo.AddComponent<RectTransform>();
            headerGo.AddComponent<Image>().color = HeaderBg;

            HorizontalLayoutGroup headerLayout = headerGo.AddComponent<HorizontalLayoutGroup>();
            headerLayout.padding = new RectOffset((int)HeaderPaddingH, (int)HeaderPaddingH, HeaderPaddingV, HeaderPaddingV);
            headerLayout.spacing = HeaderSpacing;
            headerLayout.childAlignment = TextAnchor.MiddleLeft;
            headerLayout.childControlWidth = true;
            headerLayout.childControlHeight = true;
            headerLayout.childForceExpandWidth = false;
            headerLayout.childForceExpandHeight = false;

            var charIconGo = new GameObject("CharacterIcon");
            GameObjectUtility.SetParentAndAlign(charIconGo, headerGo);
            charIconGo.AddComponent<RectTransform>();
            Image charIconImage = charIconGo.AddComponent<Image>();
            charIconImage.color = CharIconPlaceholderColor;
            LayoutElement charIconLE = charIconGo.AddComponent<LayoutElement>();
            charIconLE.minWidth = CharIconSize;
            charIconLE.minHeight = CharIconSize;
            charIconLE.preferredWidth = CharIconSize;
            charIconLE.preferredHeight = CharIconSize;
            charIconLE.flexibleWidth = 0;
            charIconLE.flexibleHeight = 0;

            var nameAndNavGo = new GameObject("NameAndNav");
            GameObjectUtility.SetParentAndAlign(nameAndNavGo, headerGo);
            nameAndNavGo.AddComponent<RectTransform>();
            HorizontalLayoutGroup nameAndNavLayout = nameAndNavGo.AddComponent<HorizontalLayoutGroup>();
            nameAndNavLayout.padding = new RectOffset((int)NameNavPaddingLeft, 0, 0, 0);
            nameAndNavLayout.spacing = 0;
            nameAndNavLayout.childAlignment = TextAnchor.MiddleLeft;
            nameAndNavLayout.childControlWidth = true;
            nameAndNavLayout.childControlHeight = true;
            nameAndNavLayout.childForceExpandWidth = false;
            nameAndNavLayout.childForceExpandHeight = false;
            LayoutElement nameAndNavLE = nameAndNavGo.AddComponent<LayoutElement>();
            nameAndNavLE.flexibleWidth = 1;

            var nameLabelGo = new GameObject("NameLabel");
            GameObjectUtility.SetParentAndAlign(nameLabelGo, nameAndNavGo);
            nameLabelGo.AddComponent<RectTransform>();
            LayoutElement nameLabelLE = nameLabelGo.AddComponent<LayoutElement>();
            nameLabelLE.flexibleWidth = 0;
            nameLabel = nameLabelGo.AddComponent<TextMeshProUGUI>();
            nameLabel.text = "";
            nameLabel.fontSize = HeaderFontSize;
            nameLabel.color = Color.white;
            nameLabel.alignment = TextAlignmentOptions.Left;
            nameLabel.fontStyle = FontStyles.Bold;
            nameLabel.raycastTarget = false;
            EditorUIFactory.ApplyFont(nameLabel, font);

            prevAllyButton = CreateArrowButton(nameAndNavGo, "PrevAllyButton", "\u25C4", font);

            var teamPosLabelGo = new GameObject("TeamPosLabel");
            GameObjectUtility.SetParentAndAlign(teamPosLabelGo, nameAndNavGo);
            teamPosLabelGo.AddComponent<RectTransform>();
            LayoutElement teamPosLE = teamPosLabelGo.AddComponent<LayoutElement>();
            teamPosLE.flexibleWidth = 0;
            teamPosLE.preferredWidth = TeamPosPreferredWidth;
            teamPosButton = teamPosLabelGo.AddComponent<Button>();
            teamPosLabel = teamPosLabelGo.AddComponent<TextMeshProUGUI>();
            teamPosLabel.raycastTarget = true;
            teamPosLabel.text = "1/1";
            teamPosLabel.fontSize = MainFontSize;
            teamPosLabel.color = LabelColor;
            teamPosLabel.alignment = TextAlignmentOptions.Center;
            EditorUIFactory.ApplyFont(teamPosLabel, font);

            nextAllyButton = CreateArrowButton(nameAndNavGo, "NextAllyButton", "\u25BA", font);

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

            var tabConfigs = new[] { "\u2665", "\u2726", "\uD83D\uDCE6" };
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
                tabLabel.raycastTarget = false;
                EditorUIFactory.ApplyFont(tabLabel, font);
            }

            return headerGo;
        }

        private static string GetTabName(int index)
        {
            switch (index)
            {
                case 0: return "Stats";
                case 1: return "Traits";
                case 2: return "Loot";
                default: return index.ToString();
            }
        }

        private static GameObject CreateStatsTabContent(Transform parent, TMP_FontAsset font,
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

            HorizontalLayoutGroup tabContentLayout = tabContentGo.AddComponent<HorizontalLayoutGroup>();
            tabContentLayout.spacing = 0;
            tabContentLayout.childControlWidth = true;
            tabContentLayout.childControlHeight = true;
            tabContentLayout.childForceExpandWidth = false;
            tabContentLayout.childForceExpandHeight = true;

            var statsColumnGo = new GameObject("StatsColumn");
            GameObjectUtility.SetParentAndAlign(statsColumnGo, tabContentGo);
            statsColumnGo.AddComponent<RectTransform>();
            VerticalLayoutGroup statsColumnLayout = statsColumnGo.AddComponent<VerticalLayoutGroup>();
            statsColumnLayout.childControlWidth = true;
            statsColumnLayout.childControlHeight = true;
            statsColumnLayout.childForceExpandWidth = true;
            statsColumnLayout.childForceExpandHeight = false;
            LayoutElement statsColumnLE = statsColumnGo.AddComponent<LayoutElement>();
            statsColumnLE.minWidth = StatsColumnMinWidth;
            statsColumnLE.flexibleWidth = 1;
            statsColumnLE.flexibleHeight = 1;

            CreateSectionHeader(statsColumnGo, "StatsHeader", "STATS", font);

            var scrollViewGo = new GameObject("StatsScrollView");
            GameObjectUtility.SetParentAndAlign(scrollViewGo, statsColumnGo);
            EditorUIFactory.Stretch(scrollViewGo.AddComponent<RectTransform>());
            scrollViewGo.AddComponent<RectMask2D>();
            scrollRect = scrollViewGo.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            LayoutElement scrollViewLE = scrollViewGo.AddComponent<LayoutElement>();
            scrollViewLE.flexibleHeight = 1;

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
                (int)ContentPaddingTop, (int)ContentPaddingBottom);
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

            for (int i = 0; i < rowCount; i++)
            {
                StatRowCfg cfg = StatRowConfigs[i];
                CreateStatRow(contentGo.transform, cfg, i, font,
                    out statNameLabels[i], out statValueLabels[i],
                    out breakdownContainers[i], out breakdownTexts[i],
                    out statRowGroups[i]);
            }

            panelComponent = parent.gameObject.GetComponent<AllyStatsPanel>();
            if (panelComponent == null)
                panelComponent = parent.gameObject.AddComponent<AllyStatsPanel>();

            CreateEquipColumn(tabContentGo.transform, font);

            return tabContentGo;
        }

        private static void CreateEquipColumn(Transform tabContentParent, TMP_FontAsset font)
        {
            var columnSeparatorGo = new GameObject("ColumnSeparator");
            GameObjectUtility.SetParentAndAlign(columnSeparatorGo, tabContentParent.gameObject);
            columnSeparatorGo.AddComponent<RectTransform>();
            columnSeparatorGo.AddComponent<Image>().color = SeparatorColor;
            LayoutElement columnSeparatorLE = columnSeparatorGo.AddComponent<LayoutElement>();
            columnSeparatorLE.preferredWidth = VerticalSeparatorWidth;
            columnSeparatorLE.flexibleWidth = 0;

            var equipColumnGo = new GameObject("EquipColumn");
            GameObjectUtility.SetParentAndAlign(equipColumnGo, tabContentParent.gameObject);
            equipColumnGo.AddComponent<RectTransform>();
            VerticalLayoutGroup equipColumnLayout = equipColumnGo.AddComponent<VerticalLayoutGroup>();
            equipColumnLayout.padding = new RectOffset(
                (int)EquipColumnPadding, (int)EquipColumnPadding,
                (int)EquipColumnPadding, (int)EquipColumnPadding);
            equipColumnLayout.spacing = EquipColumnSpacing;
            equipColumnLayout.childControlWidth = true;
            equipColumnLayout.childControlHeight = false;
            equipColumnLayout.childForceExpandWidth = true;
            equipColumnLayout.childForceExpandHeight = false;
            LayoutElement equipColumnLE = equipColumnGo.AddComponent<LayoutElement>();
            equipColumnLE.minWidth = EquipColumnMinWidth;
            equipColumnLE.flexibleWidth = 1;
            equipColumnLE.flexibleHeight = 1;

            CreateSectionHeader(equipColumnGo, "EquipHeader", "EQUIP", font);

            var placeholderLabelGo = new GameObject("EquipPlaceholderLabel");
            GameObjectUtility.SetParentAndAlign(placeholderLabelGo, equipColumnGo);
            EditorUIFactory.Stretch(placeholderLabelGo.AddComponent<RectTransform>());
            TextMeshProUGUI placeholderLabel = placeholderLabelGo.AddComponent<TextMeshProUGUI>();
            placeholderLabel.text = "Coming soon";
            placeholderLabel.fontSize = PlaceholderFontSize;
            placeholderLabel.color = LabelColor;
            placeholderLabel.alignment = TextAlignmentOptions.Center;
            EditorUIFactory.ApplyFont(placeholderLabel, font);
        }

        private static void CreateStatRow(Transform parent, StatRowCfg cfg, int rowIndex, TMP_FontAsset font,
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
            statNameLabel.fontSize = MainFontSize;
            statNameLabel.color = cfg.Accent;
            statNameLabel.alignment = TextAlignmentOptions.Left;
            EditorUIFactory.ApplyFont(statNameLabel, font);
            LayoutElement nameLE = statNameGo.AddComponent<LayoutElement>();
            nameLE.preferredWidth = StatNamePreferredWidth;
            nameLE.flexibleWidth = 0;

            var statValueGo = new GameObject("StatValue");
            GameObjectUtility.SetParentAndAlign(statValueGo, rowMainGo);
            statValueGo.AddComponent<RectTransform>();
            statValueLabel = statValueGo.AddComponent<TextMeshProUGUI>();
            statValueLabel.text = rowIndex == 0 ? "--- / ---" : "---";
            statValueLabel.fontSize = MainFontSize;
            statValueLabel.color = Color.white;
            statValueLabel.alignment = TextAlignmentOptions.Right;
            statValueLabel.fontStyle = FontStyles.Bold;
            EditorUIFactory.ApplyFont(statValueLabel, font);
            LayoutElement valueLE = statValueGo.AddComponent<LayoutElement>();
            valueLE.flexibleWidth = 1;

            var separatorGo = new GameObject("Separator");
            GameObjectUtility.SetParentAndAlign(separatorGo, rowGo);
            separatorGo.AddComponent<RectTransform>();
            separatorGo.AddComponent<Image>().color = SeparatorColor;
            LayoutElement separatorLE = separatorGo.AddComponent<LayoutElement>();
            separatorLE.preferredHeight = HorizontalSeparatorHeight;
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
            EditorUIFactory.ApplyFont(breakdownText, font);
        }

        private static GameObject CreatePlaceholderTabContent(Transform parent, string name, string labelText, bool activeByDefault, TMP_FontAsset font)
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
            EditorUIFactory.ApplyFont(label, font);

            return go;
        }

        private static void CreateSectionHeader(GameObject parent, string gameObjectName, string labelText, TMP_FontAsset font)
        {
            var headerGo = new GameObject(gameObjectName);
            GameObjectUtility.SetParentAndAlign(headerGo, parent);
            headerGo.AddComponent<RectTransform>();
            LayoutElement headerLE = headerGo.AddComponent<LayoutElement>();
            headerLE.preferredHeight = SectionHeaderHeight;
            headerLE.flexibleHeight = 0;
            TextMeshProUGUI headerLabel = headerGo.AddComponent<TextMeshProUGUI>();
            headerLabel.text = labelText;
            headerLabel.fontSize = MainFontSize;
            headerLabel.color = SectionHeaderColor;
            headerLabel.alignment = TextAlignmentOptions.Center;
            headerLabel.fontStyle = FontStyles.Bold;
            EditorUIFactory.ApplyFont(headerLabel, font);
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
            Transform contentTransform = tabContentStats.transform.Find("StatsColumn/StatsScrollView/Content");
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

        private static Button CreateArrowButton(GameObject parent, string name, string symbol, TMP_FontAsset font)
        {
            var buttonGo = new GameObject(name);
            GameObjectUtility.SetParentAndAlign(buttonGo, parent);
            buttonGo.AddComponent<RectTransform>();
            LayoutElement buttonLE = buttonGo.AddComponent<LayoutElement>();
            buttonLE.minWidth = ArrowButtonSize;
            buttonLE.minHeight = ArrowButtonSize;
            buttonLE.preferredWidth = ArrowButtonSize;
            buttonLE.preferredHeight = ArrowButtonSize;
            buttonLE.flexibleWidth = 0;
            buttonGo.AddComponent<Image>().color = TabInactiveColor;

            Button button = buttonGo.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = TabInactiveColor;
            colors.highlightedColor = TabActiveColor;
            colors.pressedColor = ArrowPressedColor;
            colors.selectedColor = TabActiveColor;
            button.colors = colors;

            var labelGo = new GameObject("Label");
            GameObjectUtility.SetParentAndAlign(labelGo, buttonGo);
            EditorUIFactory.Stretch(labelGo.AddComponent<RectTransform>());
            TextMeshProUGUI label = labelGo.AddComponent<TextMeshProUGUI>();
            label.text = symbol;
            label.fontSize = ArrowFontSize;
            label.color = Color.white;
            label.alignment = TextAlignmentOptions.Center;
            label.fontStyle = FontStyles.Bold;
            label.raycastTarget = false;
            EditorUIFactory.ApplyFont(label, font);

            return button;
        }
    }
}
