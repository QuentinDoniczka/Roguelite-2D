using RogueliteAutoBattler.Data;
using RogueliteAutoBattler.UI.Core;
using RogueliteAutoBattler.UI.Screens.Combat;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace RogueliteAutoBattler.Editor
{
    internal static class CombatHudBuilder
    {
        private const int HudFontSize = 28;
        private const int BattleFontSize = 40;
        private const int SettingsTitleFontSize = 28;
        private const int SettingsLabelFontSize = 22;
        private const int SettingsValueFontSize = 20;
        private const int SliderRowSpacing = 8;
        private const int ColorRowSpacing = 6;
        private const int ScrollContentSpacing = 10;
        private const int ScrollContentPadding = 16;
        private const float LabelPreferredWidth = 200f;
        private const float ValueLabelPreferredWidth = 80f;
        private const float SliderMinWidth = 200f;
        private const float ColorButtonPreferredHeight = 50f;
        private const int ColorButtonCount = 8;

        private static readonly Color HudBarBg = (Color)new Color32(0, 0, 0, 160);
        private static readonly Color SettingsOverlayBg = (Color)new Color32(0, 0, 0, 180);
        private static readonly Color SettingsPanelBg = (Color)new Color32(20, 20, 30, 230);
        private static readonly Color ResetButtonBg = (Color)new Color32(160, 40, 40, 220);

        internal static UIScreen CreateCombatPanel(Transform parent)
        {
            var go = new GameObject("CombatPanel");
            GameObjectUtility.SetParentAndAlign(go, parent.gameObject);
            EditorUIFactory.Stretch(go.AddComponent<RectTransform>());

            EditorUIFactory.SetupCanvasGroup(go, true);

            UIScreen screen = go.AddComponent<CombatScreen>();

            CreateHudBadge(go.transform, "ResetTimer", "Reset: 3j 14h",
                new Vector2(0, 0.92f), new Vector2(0.35f, 1f),
                new Vector2(12, -8), new Vector2(-8, -8),
                TextAlignmentOptions.Center);

            CreateCurrencyBadge(go.transform, "Gold", "1.93m",
                new Vector2(0.55f, 0.92f), new Vector2(0.77f, 1f),
                (Color)new Color32(255, 215, 0, 255));

            CreateCurrencyBadge(go.transform, "Diamond", "211",
                new Vector2(0.77f, 0.92f), new Vector2(1f, 1f),
                (Color)new Color32(185, 242, 255, 255));

            var battleGo = new GameObject("BattleIndicator");
            GameObjectUtility.SetParentAndAlign(battleGo, go);
            RectTransform battleRect = battleGo.AddComponent<RectTransform>();
            battleRect.anchorMin = new Vector2(0.2f, 0.78f);
            battleRect.anchorMax = new Vector2(0.8f, 0.90f);
            battleRect.offsetMin = Vector2.zero;
            battleRect.offsetMax = Vector2.zero;
            TextMeshProUGUI battleTmp = battleGo.AddComponent<TextMeshProUGUI>();
            battleTmp.text = "Battle 10-19";
            battleTmp.fontSize = BattleFontSize;
            battleTmp.color = Color.white;
            battleTmp.alignment = TextAlignmentOptions.Center;
            battleTmp.fontStyle = FontStyles.Bold;

            Button settingsButton = CreateSettingsButton(go.transform);
            DamageNumberSettingsPanel settingsPanel = CreateSettingsPanel(go.transform);

            WireCombatScreen(screen, settingsPanel, settingsButton);

            return screen;
        }

        private static Button CreateSettingsButton(Transform parent)
        {
            var go = new GameObject("SettingsButton");
            GameObjectUtility.SetParentAndAlign(go, parent.gameObject);
            RectTransform r = go.AddComponent<RectTransform>();
            r.anchorMin = new Vector2(0.35f, 0.92f);
            r.anchorMax = new Vector2(0.55f, 1f);
            r.offsetMin = new Vector2(4, -8);
            r.offsetMax = new Vector2(-4, -8);

            Image bg = go.AddComponent<Image>();
            bg.color = HudBarBg;

            Button button = go.AddComponent<Button>();

            var labelGo = new GameObject("Label");
            GameObjectUtility.SetParentAndAlign(labelGo, go);
            EditorUIFactory.Stretch(labelGo.AddComponent<RectTransform>());
            TextMeshProUGUI tmp = labelGo.AddComponent<TextMeshProUGUI>();
            tmp.text = "⚙";
            tmp.fontSize = HudFontSize;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontStyle = FontStyles.Bold;

            return button;
        }

        private static DamageNumberSettingsPanel CreateSettingsPanel(Transform parent)
        {
            var overlayGo = new GameObject("SettingsOverlay");
            GameObjectUtility.SetParentAndAlign(overlayGo, parent.gameObject);
            EditorUIFactory.Stretch(overlayGo.AddComponent<RectTransform>());
            Image overlayBg = overlayGo.AddComponent<Image>();
            overlayBg.color = SettingsOverlayBg;
            CanvasGroup overlayCanvasGroup = EditorUIFactory.SetupCanvasGroup(overlayGo, false);
            DamageNumberSettingsPanel settingsPanel = overlayGo.AddComponent<DamageNumberSettingsPanel>();

            var panelGo = new GameObject("SettingsPanel");
            GameObjectUtility.SetParentAndAlign(panelGo, overlayGo);
            RectTransform panelRect = panelGo.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.075f, 0.125f);
            panelRect.anchorMax = new Vector2(0.925f, 0.875f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            Image panelBg = panelGo.AddComponent<Image>();
            panelBg.color = SettingsPanelBg;

            var titleGo = new GameObject("Title");
            GameObjectUtility.SetParentAndAlign(titleGo, panelGo);
            RectTransform titleRect = titleGo.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0f, 0.88f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;
            TextMeshProUGUI titleTmp = titleGo.AddComponent<TextMeshProUGUI>();
            titleTmp.text = "Damage Numbers";
            titleTmp.fontSize = SettingsTitleFontSize;
            titleTmp.color = Color.white;
            titleTmp.alignment = TextAlignmentOptions.Center;
            titleTmp.fontStyle = FontStyles.Bold;

            var scrollContentGo = new GameObject("ScrollContent");
            GameObjectUtility.SetParentAndAlign(scrollContentGo, panelGo);
            RectTransform scrollContentRect = scrollContentGo.AddComponent<RectTransform>();
            scrollContentRect.anchorMin = new Vector2(0f, 0f);
            scrollContentRect.anchorMax = new Vector2(1f, 0.88f);
            scrollContentRect.offsetMin = Vector2.zero;
            scrollContentRect.offsetMax = Vector2.zero;
            VerticalLayoutGroup vlg = scrollContentGo.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(ScrollContentPadding, ScrollContentPadding, ScrollContentPadding, ScrollContentPadding);
            vlg.spacing = ScrollContentSpacing;
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            SliderResult fontSizeResult = CreateSliderRow(scrollContentGo.transform, "FontSizeRow", "Font Size");
            SliderResult lifetimeResult = CreateSliderRow(scrollContentGo.transform, "LifetimeRow", "Duration");
            SliderResult slideDistResult = CreateSliderRow(scrollContentGo.transform, "SlideDistanceRow", "Slide Dist");
            SliderResult spawnOffsetResult = CreateSliderRow(scrollContentGo.transform, "SpawnOffsetRow", "Offset Y");

            CreateSectionLabel(scrollContentGo.transform, "AllyColorLabel", "Ally Color");
            Button[] allyButtons = CreateColorButtonRow(scrollContentGo.transform, "AllyColorRow", DamageNumberSettingsPanel.AllyColorPresets);

            CreateSectionLabel(scrollContentGo.transform, "EnemyColorLabel", "Enemy Color");
            Button[] enemyButtons = CreateColorButtonRow(scrollContentGo.transform, "EnemyColorRow", DamageNumberSettingsPanel.EnemyColorPresets);

            Button resetButton = CreateResetButton(scrollContentGo.transform);

            DamageNumberConfig config = AssetDatabase.LoadAssetAtPath<DamageNumberConfig>("Assets/Data/DamageNumberConfig.asset");

            SerializedObject panelSO = new SerializedObject(settingsPanel);
            if (config != null)
                EditorUIFactory.SetObj(panelSO, "_config", config);
            EditorUIFactory.SetObj(panelSO, "_panelCanvasGroup", overlayCanvasGroup);
            EditorUIFactory.SetObj(panelSO, "_fontSizeSlider", fontSizeResult.Slider);
            EditorUIFactory.SetObj(panelSO, "_lifetimeSlider", lifetimeResult.Slider);
            EditorUIFactory.SetObj(panelSO, "_slideDistanceSlider", slideDistResult.Slider);
            EditorUIFactory.SetObj(panelSO, "_spawnOffsetYSlider", spawnOffsetResult.Slider);
            EditorUIFactory.SetObj(panelSO, "_fontSizeLabel", fontSizeResult.ValueLabel);
            EditorUIFactory.SetObj(panelSO, "_lifetimeLabel", lifetimeResult.ValueLabel);
            EditorUIFactory.SetObj(panelSO, "_slideDistanceLabel", slideDistResult.ValueLabel);
            EditorUIFactory.SetObj(panelSO, "_spawnOffsetYLabel", spawnOffsetResult.ValueLabel);
            EditorUIFactory.WireArray(panelSO, "_allyColorButtons", allyButtons, ColorButtonCount);
            EditorUIFactory.WireArray(panelSO, "_enemyColorButtons", enemyButtons, ColorButtonCount);
            EditorUIFactory.SetObj(panelSO, "_resetButton", resetButton);
            panelSO.ApplyModifiedPropertiesWithoutUndo();

            return settingsPanel;
        }

        private static void WireCombatScreen(UIScreen screen, DamageNumberSettingsPanel settingsPanel, Button settingsButton)
        {
            SerializedObject screenSO = new SerializedObject(screen);
            EditorUIFactory.SetObj(screenSO, "_settingsPanel", settingsPanel);
            EditorUIFactory.SetObj(screenSO, "_settingsButton", settingsButton);
            screenSO.ApplyModifiedPropertiesWithoutUndo();
        }

        private static SliderResult CreateSliderRow(Transform parent, string rowName, string labelText)
        {
            var rowGo = new GameObject(rowName);
            GameObjectUtility.SetParentAndAlign(rowGo, parent.gameObject);
            rowGo.AddComponent<RectTransform>();
            HorizontalLayoutGroup hlg = rowGo.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = SliderRowSpacing;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            LayoutElement rowLe = rowGo.AddComponent<LayoutElement>();
            rowLe.minHeight = 60f;
            rowLe.preferredHeight = 60f;

            var nameGo = new GameObject("Label");
            GameObjectUtility.SetParentAndAlign(nameGo, rowGo);
            nameGo.AddComponent<RectTransform>();
            TextMeshProUGUI nameTmp = nameGo.AddComponent<TextMeshProUGUI>();
            nameTmp.text = labelText;
            nameTmp.fontSize = SettingsLabelFontSize;
            nameTmp.color = Color.white;
            nameTmp.alignment = TextAlignmentOptions.MidlineLeft;
            LayoutElement nameLe = nameGo.AddComponent<LayoutElement>();
            nameLe.preferredWidth = LabelPreferredWidth;
            nameLe.flexibleWidth = 0f;

            Slider slider = CreateSlider(rowGo.transform);
            LayoutElement sliderLe = slider.gameObject.AddComponent<LayoutElement>();
            sliderLe.flexibleWidth = 1f;
            sliderLe.minWidth = SliderMinWidth;

            var valueGo = new GameObject("ValueLabel");
            GameObjectUtility.SetParentAndAlign(valueGo, rowGo);
            valueGo.AddComponent<RectTransform>();
            TextMeshProUGUI valueTmp = valueGo.AddComponent<TextMeshProUGUI>();
            valueTmp.text = "0.0";
            valueTmp.fontSize = SettingsValueFontSize;
            valueTmp.color = Color.white;
            valueTmp.alignment = TextAlignmentOptions.MidlineRight;
            LayoutElement valueLe = valueGo.AddComponent<LayoutElement>();
            valueLe.preferredWidth = ValueLabelPreferredWidth;
            valueLe.flexibleWidth = 0f;

            return new SliderResult(slider, valueTmp);
        }

        private static Slider CreateSlider(Transform parent)
        {
            var sliderGo = new GameObject("Slider");
            GameObjectUtility.SetParentAndAlign(sliderGo, parent.gameObject);
            sliderGo.AddComponent<RectTransform>();
            Slider slider = sliderGo.AddComponent<Slider>();

            var backgroundGo = new GameObject("Background");
            GameObjectUtility.SetParentAndAlign(backgroundGo, sliderGo);
            RectTransform bgRect = backgroundGo.AddComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0f, 0.25f);
            bgRect.anchorMax = new Vector2(1f, 0.75f);
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            Image bgImage = backgroundGo.AddComponent<Image>();
            bgImage.color = (Color)new Color32(40, 40, 50, 255);

            var fillAreaGo = new GameObject("Fill Area");
            GameObjectUtility.SetParentAndAlign(fillAreaGo, sliderGo);
            RectTransform fillAreaRect = fillAreaGo.AddComponent<RectTransform>();
            fillAreaRect.anchorMin = new Vector2(0f, 0.25f);
            fillAreaRect.anchorMax = new Vector2(1f, 0.75f);
            fillAreaRect.offsetMin = new Vector2(5f, 0f);
            fillAreaRect.offsetMax = new Vector2(-15f, 0f);

            var fillGo = new GameObject("Fill");
            GameObjectUtility.SetParentAndAlign(fillGo, fillAreaGo);
            RectTransform fillRect = fillGo.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = new Vector2(0f, 1f);
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            Image fillImage = fillGo.AddComponent<Image>();
            fillImage.color = (Color)new Color32(100, 180, 255, 255);

            var handleAreaGo = new GameObject("Handle Slide Area");
            GameObjectUtility.SetParentAndAlign(handleAreaGo, sliderGo);
            RectTransform handleAreaRect = handleAreaGo.AddComponent<RectTransform>();
            handleAreaRect.anchorMin = Vector2.zero;
            handleAreaRect.anchorMax = Vector2.one;
            handleAreaRect.offsetMin = new Vector2(10f, 0f);
            handleAreaRect.offsetMax = new Vector2(-10f, 0f);

            var handleGo = new GameObject("Handle");
            GameObjectUtility.SetParentAndAlign(handleGo, handleAreaGo);
            RectTransform handleRect = handleGo.AddComponent<RectTransform>();
            handleRect.anchorMin = new Vector2(0f, 0f);
            handleRect.anchorMax = new Vector2(0f, 1f);
            handleRect.offsetMin = new Vector2(-10f, 0f);
            handleRect.offsetMax = new Vector2(10f, 0f);
            Image handleImage = handleGo.AddComponent<Image>();
            handleImage.color = Color.white;
            handleImage.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");

            slider.fillRect = fillRect;
            slider.handleRect = handleRect;
            slider.targetGraphic = handleImage;
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 0.5f;

            return slider;
        }

        private static void CreateSectionLabel(Transform parent, string name, string text)
        {
            var go = new GameObject(name);
            GameObjectUtility.SetParentAndAlign(go, parent.gameObject);
            go.AddComponent<RectTransform>();
            TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = SettingsLabelFontSize;
            tmp.color = (Color)new Color32(200, 200, 200, 255);
            tmp.alignment = TextAlignmentOptions.MidlineLeft;
            tmp.fontStyle = FontStyles.Bold;
            LayoutElement le = go.AddComponent<LayoutElement>();
            le.minHeight = 40f;
            le.preferredHeight = 40f;
        }

        private static Button[] CreateColorButtonRow(Transform parent, string rowName, Color[] presets)
        {
            var rowGo = new GameObject(rowName);
            GameObjectUtility.SetParentAndAlign(rowGo, parent.gameObject);
            rowGo.AddComponent<RectTransform>();
            HorizontalLayoutGroup hlg = rowGo.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = ColorRowSpacing;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = false;
            LayoutElement rowLe = rowGo.AddComponent<LayoutElement>();
            rowLe.minHeight = ColorButtonPreferredHeight;
            rowLe.preferredHeight = ColorButtonPreferredHeight;

            var buttons = new Button[ColorButtonCount];
            for (int i = 0; i < ColorButtonCount; i++)
            {
                var btnGo = new GameObject($"ColorButton_{i}");
                GameObjectUtility.SetParentAndAlign(btnGo, rowGo);
                btnGo.AddComponent<RectTransform>();
                Image btnImage = btnGo.AddComponent<Image>();
                btnImage.color = presets[i];
                Button btn = btnGo.AddComponent<Button>();
                LayoutElement btnLe = btnGo.AddComponent<LayoutElement>();
                btnLe.preferredHeight = ColorButtonPreferredHeight;
                buttons[i] = btn;
            }

            return buttons;
        }

        private static Button CreateResetButton(Transform parent)
        {
            var go = new GameObject("ResetButton");
            GameObjectUtility.SetParentAndAlign(go, parent.gameObject);
            go.AddComponent<RectTransform>();
            Image bg = go.AddComponent<Image>();
            bg.color = ResetButtonBg;
            Button button = go.AddComponent<Button>();
            LayoutElement le = go.AddComponent<LayoutElement>();
            le.minHeight = 60f;
            le.preferredHeight = 60f;

            var labelGo = new GameObject("Label");
            GameObjectUtility.SetParentAndAlign(labelGo, go);
            EditorUIFactory.Stretch(labelGo.AddComponent<RectTransform>());
            TextMeshProUGUI tmp = labelGo.AddComponent<TextMeshProUGUI>();
            tmp.text = "Reset Defaults";
            tmp.fontSize = SettingsLabelFontSize;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontStyle = FontStyles.Bold;

            return button;
        }

        internal static void CreateCurrencyBadge(Transform parent, string name, string text,
            Vector2 anchorMin, Vector2 anchorMax, Color textColor)
        {
            var go = new GameObject(name);
            GameObjectUtility.SetParentAndAlign(go, parent.gameObject);
            RectTransform r = go.AddComponent<RectTransform>();
            r.anchorMin = anchorMin;
            r.anchorMax = anchorMax;
            r.offsetMin = new Vector2(4, -8);
            r.offsetMax = new Vector2(-4, -8);

            Image bg = go.AddComponent<Image>();
            bg.color = HudBarBg;

            HorizontalLayoutGroup layout = go.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 6;
            layout.padding = new RectOffset(16, 24, 0, 0);
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            var iconGo = new GameObject("Icon");
            GameObjectUtility.SetParentAndAlign(iconGo, go);
            iconGo.AddComponent<RectTransform>();
            Image iconImg = iconGo.AddComponent<Image>();
            iconImg.color = textColor;
            iconImg.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
            LayoutElement iconLe = iconGo.AddComponent<LayoutElement>();
            iconLe.minWidth = 50;
            iconLe.minHeight = 50;
            iconLe.preferredWidth = 50;
            iconLe.preferredHeight = 50;
            iconLe.flexibleWidth = 0;
            iconLe.flexibleHeight = 0;

            var labelGo = new GameObject("Label");
            GameObjectUtility.SetParentAndAlign(labelGo, go);
            labelGo.AddComponent<RectTransform>();
            TextMeshProUGUI tmp = labelGo.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = HudFontSize;
            tmp.color = textColor;
            tmp.alignment = TextAlignmentOptions.MidlineRight;
            tmp.fontStyle = FontStyles.Bold;
            LayoutElement labelLe = labelGo.AddComponent<LayoutElement>();
            labelLe.flexibleWidth = 1;
            labelLe.flexibleHeight = 1;
        }

        internal static void CreateHudBadge(Transform parent, string name, string text,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax,
            TextAlignmentOptions alignment, Color? textColor = null)
        {
            var go = new GameObject(name);
            GameObjectUtility.SetParentAndAlign(go, parent.gameObject);
            RectTransform r = go.AddComponent<RectTransform>();
            r.anchorMin = anchorMin;
            r.anchorMax = anchorMax;
            r.offsetMin = offsetMin;
            r.offsetMax = offsetMax;

            Image bg = go.AddComponent<Image>();
            bg.color = HudBarBg;

            var labelGo = new GameObject("Label");
            GameObjectUtility.SetParentAndAlign(labelGo, go);
            EditorUIFactory.Stretch(labelGo.AddComponent<RectTransform>());
            TextMeshProUGUI tmp = labelGo.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = HudFontSize;
            tmp.color = textColor ?? Color.white;
            tmp.alignment = alignment;
            tmp.fontStyle = FontStyles.Bold;
        }

        private readonly struct SliderResult
        {
            internal readonly Slider Slider;
            internal readonly TMP_Text ValueLabel;

            internal SliderResult(Slider slider, TMP_Text valueLabel)
            {
                Slider = slider;
                ValueLabel = valueLabel;
            }
        }
    }
}
