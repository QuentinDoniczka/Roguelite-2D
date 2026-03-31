using RogueliteAutoBattler.Combat.Visuals;
using RogueliteAutoBattler.Economy;
using RogueliteAutoBattler.UI.Core;
using RogueliteAutoBattler.UI.Screens.Combat;
using RogueliteAutoBattler.UI.Widgets;
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
        private const int AnnouncementFontSize = 56;

        private static readonly Color HudBarBg = (Color)new Color32(0, 0, 0, 160);

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

            CreateCurrencyBadge(go.transform, "Gold", "0",
                new Vector2(0.55f, 0.92f), new Vector2(0.77f, 1f),
                (Color)new Color32(255, 215, 0, 255));

            var goldBadge = go.transform.Find("Gold");
            if (goldBadge != null)
                goldBadge.gameObject.AddComponent<GoldHudBadge>();

            var walletGo = new GameObject("GoldWallet");
            GameObjectUtility.SetParentAndAlign(walletGo, go);
            walletGo.AddComponent<GoldWallet>();

            var coinFlyPool = new GameObject("CoinFlyPool");
            GameObjectUtility.SetParentAndAlign(coinFlyPool, go);
            RectTransform coinFlyPoolRect = coinFlyPool.AddComponent<RectTransform>();
            coinFlyPoolRect.anchorMin = Vector2.zero;
            coinFlyPoolRect.anchorMax = Vector2.one;
            coinFlyPoolRect.offsetMin = Vector2.zero;
            coinFlyPoolRect.offsetMax = Vector2.zero;

            var bootstrap = go.AddComponent<CoinFlyBootstrap>();
            var bootstrapSO = new SerializedObject(bootstrap);
            var coinContainerProp = bootstrapSO.FindProperty("_coinContainer");
            if (coinContainerProp == null)
                Debug.LogError($"[{nameof(CombatHudBuilder)}] SerializedProperty '_coinContainer' not found on CoinFlyBootstrap.");
            else
                coinContainerProp.objectReferenceValue = coinFlyPoolRect;

            var targetBadgeProp = bootstrapSO.FindProperty("_targetBadge");
            if (targetBadgeProp == null)
                Debug.LogError($"[{nameof(CombatHudBuilder)}] SerializedProperty '_targetBadge' not found on CoinFlyBootstrap.");
            else
                targetBadgeProp.objectReferenceValue = goldBadge != null ? goldBadge.GetComponent<RectTransform>() : null;

            var coinSpriteProp = bootstrapSO.FindProperty("_coinSprite");
            if (coinSpriteProp == null)
                Debug.LogError($"[{nameof(CombatHudBuilder)}] SerializedProperty '_coinSprite' not found on CoinFlyBootstrap.");
            else
                coinSpriteProp.objectReferenceValue = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");

            bootstrapSO.ApplyModifiedProperties();

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
            BattleIndicatorBadge badge = battleGo.AddComponent<BattleIndicatorBadge>();

            var compactLabelGo = new GameObject("CompactLabel");
            GameObjectUtility.SetParentAndAlign(compactLabelGo, battleGo);
            EditorUIFactory.Stretch(compactLabelGo.AddComponent<RectTransform>());
            TextMeshProUGUI compactTmp = compactLabelGo.AddComponent<TextMeshProUGUI>();
            compactTmp.text = "1-1";
            compactTmp.fontSize = BattleFontSize;
            compactTmp.color = Color.white;
            compactTmp.alignment = TextAlignmentOptions.Center;
            compactTmp.fontStyle = FontStyles.Bold;

            var announcementOverlayGo = new GameObject("AnnouncementOverlay");
            GameObjectUtility.SetParentAndAlign(announcementOverlayGo, go);
            RectTransform announcementOverlayRect = announcementOverlayGo.AddComponent<RectTransform>();
            announcementOverlayRect.anchorMin = new Vector2(0.1f, 0.35f);
            announcementOverlayRect.anchorMax = new Vector2(0.9f, 0.65f);
            announcementOverlayRect.offsetMin = Vector2.zero;
            announcementOverlayRect.offsetMax = Vector2.zero;
            CanvasGroup announcementCanvasGroup = announcementOverlayGo.AddComponent<CanvasGroup>();
            announcementCanvasGroup.alpha = 0f;
            announcementCanvasGroup.blocksRaycasts = false;
            announcementCanvasGroup.interactable = false;

            var announcementLabelGo = new GameObject("AnnouncementLabel");
            GameObjectUtility.SetParentAndAlign(announcementLabelGo, announcementOverlayGo);
            EditorUIFactory.Stretch(announcementLabelGo.AddComponent<RectTransform>());
            TextMeshProUGUI announcementTmp = announcementLabelGo.AddComponent<TextMeshProUGUI>();
            announcementTmp.text = "";
            announcementTmp.fontSize = AnnouncementFontSize;
            announcementTmp.color = Color.white;
            announcementTmp.alignment = TextAlignmentOptions.Center;
            announcementTmp.fontStyle = FontStyles.Bold;

            var badgeSO = new SerializedObject(badge);

            var compactLabelProp = badgeSO.FindProperty("_compactLabel");
            if (compactLabelProp == null)
                Debug.LogError($"[{nameof(CombatHudBuilder)}] SerializedProperty '_compactLabel' not found on BattleIndicatorBadge.");
            else
                compactLabelProp.objectReferenceValue = compactTmp;

            var announcementLabelProp = badgeSO.FindProperty("_announcementLabel");
            if (announcementLabelProp == null)
                Debug.LogError($"[{nameof(CombatHudBuilder)}] SerializedProperty '_announcementLabel' not found on BattleIndicatorBadge.");
            else
                announcementLabelProp.objectReferenceValue = announcementTmp;

            var announcementGroupProp = badgeSO.FindProperty("_announcementGroup");
            if (announcementGroupProp == null)
                Debug.LogError($"[{nameof(CombatHudBuilder)}] SerializedProperty '_announcementGroup' not found on BattleIndicatorBadge.");
            else
                announcementGroupProp.objectReferenceValue = announcementCanvasGroup;

            badgeSO.ApplyModifiedProperties();

            return screen;
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
    }
}
