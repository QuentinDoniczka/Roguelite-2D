using RogueliteAutoBattler.UI.Core;
using RogueliteAutoBattler.UI.Screens.Combat;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace RogueliteAutoBattler.Editor
{
    /// <summary>
    /// Builds the combat HUD panel (transparent overlay revealing the 2D world behind the Canvas).
    /// </summary>
    internal static class CombatHudBuilder
    {
        private const int HudFontSize = 28;
        private const int BattleFontSize = 40;
        private static readonly Color HudBarBg = (Color)new Color32(0, 0, 0, 160);

        internal static UIScreen CreateCombatPanel(Transform parent)
        {
            var go = new GameObject("CombatPanel");
            GameObjectUtility.SetParentAndAlign(go, parent.gameObject);
            EditorUIFactory.Stretch(go.AddComponent<RectTransform>());

            // No Image — transparent, reveals the 2D world behind
            EditorUIFactory.SetupCanvasGroup(go, true);

            UIScreen screen = go.AddComponent<CombatScreen>();

            // --- Top left: Reset timer (pill badge) ---
            CreateHudBadge(go.transform, "ResetTimer", "Reset: 3j 14h",
                new Vector2(0, 0.92f), new Vector2(0.35f, 1f),
                new Vector2(12, -8), new Vector2(-8, -8),
                TextAlignmentOptions.Center);

            // --- Top right: Gold + Diamond side by side (start at 55%) ---
            // Gold badge with icon
            CreateCurrencyBadge(go.transform, "Gold", "1.93m",
                new Vector2(0.55f, 0.92f), new Vector2(0.77f, 1f),
                (Color)new Color32(255, 215, 0, 255));

            // Diamond badge with icon
            CreateCurrencyBadge(go.transform, "Diamond", "211",
                new Vector2(0.77f, 0.92f), new Vector2(1f, 1f),
                (Color)new Color32(185, 242, 255, 255));

            // --- Center: Battle indicator (below top row) ---
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

            return screen;
        }

        /// <summary>Creates a currency badge: dark bg + small icon (placeholder) + colored text, using HorizontalLayoutGroup.</summary>
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

            // Layout: icon + text side by side — childControl forces sizes from LayoutElement
            HorizontalLayoutGroup layout = go.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 6;
            layout.padding = new RectOffset(16, 24, 0, 0);
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            // Icon — fixed 50x50 square
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

            // Text — fills remaining space
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

        /// <summary>Creates a small HUD badge: dark semi-transparent bg + text.</summary>
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

            // Dark semi-transparent rounded-ish background
            Image bg = go.AddComponent<Image>();
            bg.color = HudBarBg;

            // Text
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
    }
}
