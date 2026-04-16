using RogueliteAutoBattler.Combat.Visuals;
using RogueliteAutoBattler.Economy;
using RogueliteAutoBattler.UI.Core;
using RogueliteAutoBattler.UI.Screens.Combat;
using RogueliteAutoBattler.UI.Toolkit;
using RogueliteAutoBattler.UI.Widgets;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UIDocument = UnityEngine.UIElements.UIDocument;

namespace RogueliteAutoBattler.Editor
{
    internal static class CombatHudBuilder
    {
        private const int HudFontSize = 28;
        private const int BattleFontSize = 40;
        private const int AnnouncementFontSize = 56;
        private const int CurrencyIconSize = 50;
        private const int CurrencyBadgeSpacing = 6;
        private const int CurrencyBadgePaddingLeft = 16;
        private const int CurrencyBadgePaddingRight = 24;

        private static readonly Vector2 BadgeOffsetInnerLeft = new Vector2(4, -8);
        private static readonly Vector2 BadgeOffsetInnerRight = new Vector2(-4, -8);
        private static readonly Vector2 BadgeOffsetOuterRight = new Vector2(-8, -8);

        private static readonly Color HudBarBg = new Color32(0, 0, 0, 160);

        internal static UIScreen CreateCombatPanel(Transform parent)
        {
            TMP_FontAsset bangersFont = EditorUIFactory.LoadBangersFont(nameof(CombatHudBuilder));

            var go = new GameObject("CombatPanel");
            GameObjectUtility.SetParentAndAlign(go, parent.gameObject);
            EditorUIFactory.Stretch(go.AddComponent<RectTransform>());

            EditorUIFactory.SetupCanvasGroup(go, true);

            UIScreen screen = go.AddComponent<CombatScreen>();

            CreateCurrencyBadge(go.transform, "Gold", "0",
                new Vector2(0.56f, 0.92f), new Vector2(0.78f, 1f),
                new Color32(255, 215, 0, 255),
                bangersFont,
                BadgeOffsetInnerLeft, BadgeOffsetInnerRight);

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
            EditorUIFactory.SetObj(bootstrapSO, "_coinContainer", coinFlyPoolRect);
            EditorUIFactory.SetObj(bootstrapSO, "_targetBadge",
                goldBadge != null ? goldBadge.GetComponent<RectTransform>() : null);
            EditorUIFactory.SetObj(bootstrapSO, "_coinSprite",
                AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd"));
            bootstrapSO.ApplyModifiedProperties();

            CreateCurrencyBadge(go.transform, "Diamond", "211",
                new Vector2(0.78f, 0.92f), new Vector2(1f, 1f),
                new Color32(185, 242, 255, 255),
                bangersFont,
                BadgeOffsetInnerLeft, BadgeOffsetOuterRight);

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
            RectTransform compactLabelRect = compactLabelGo.AddComponent<RectTransform>();
            compactLabelRect.anchorMin = new Vector2(0f, 0.4f);
            compactLabelRect.anchorMax = new Vector2(1f, 1f);
            compactLabelRect.offsetMin = Vector2.zero;
            compactLabelRect.offsetMax = Vector2.zero;
            TextMeshProUGUI compactTmp = compactLabelGo.AddComponent<TextMeshProUGUI>();
            compactTmp.text = "1-1";
            compactTmp.fontSize = BattleFontSize;
            compactTmp.color = Color.white;
            compactTmp.alignment = TextAlignmentOptions.Center;
            compactTmp.fontStyle = FontStyles.Bold;
            EditorUIFactory.ApplyFont(compactTmp, bangersFont);

            var stepBarGo = new GameObject("StepProgressBar");
            stepBarGo.transform.SetParent(battleGo.transform, false);
            RectTransform stepBarRect = stepBarGo.AddComponent<RectTransform>();
            stepBarRect.anchorMin = new Vector2(0.1f, 0.05f);
            stepBarRect.anchorMax = new Vector2(0.9f, 0.35f);
            stepBarRect.offsetMin = Vector2.zero;
            stepBarRect.offsetMax = Vector2.zero;

            HorizontalLayoutGroup stepBarLayout = stepBarGo.AddComponent<HorizontalLayoutGroup>();
            stepBarLayout.childAlignment = TextAnchor.MiddleCenter;
            stepBarLayout.childControlWidth = true;
            stepBarLayout.childControlHeight = true;
            stepBarLayout.childForceExpandWidth = false;
            stepBarLayout.childForceExpandHeight = false;
            stepBarLayout.spacing = 0;

            StepProgressBar stepBar = stepBarGo.AddComponent<StepProgressBar>();
            var stepBarSo = new SerializedObject(stepBar);
            EditorUIFactory.SetObj(stepBarSo, "_sphereSprite",
                AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd"));
            stepBarSo.ApplyModifiedPropertiesWithoutUndo();

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
            EditorUIFactory.ApplyFont(announcementTmp, bangersFont);

            var badgeSO = new SerializedObject(badge);
            EditorUIFactory.SetObj(badgeSO, "_compactLabel", compactTmp);
            EditorUIFactory.SetObj(badgeSO, "_announcementLabel", announcementTmp);
            EditorUIFactory.SetObj(badgeSO, "_announcementGroup", announcementCanvasGroup);
            badgeSO.ApplyModifiedProperties();

            return screen;
        }

        internal static void SetupToolkitCombatHud(GameObject navigationHostGo)
        {
            UIDocument uiDocument = navigationHostGo.GetComponent<UIDocument>();

            CombatHudController combatHud = navigationHostGo.AddComponent<CombatHudController>();
            var combatHudSo = new SerializedObject(combatHud);
            EditorUIFactory.SetObj(combatHudSo, "_uiDocument", uiDocument);
            combatHudSo.ApplyModifiedProperties();

            var coinFlyOverlayGo = new GameObject("CoinFlyOverlay");
            GameObjectUtility.SetParentAndAlign(coinFlyOverlayGo, navigationHostGo);

            Canvas canvas = coinFlyOverlayGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            CanvasScaler scaler = coinFlyOverlayGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;

            coinFlyOverlayGo.AddComponent<GraphicRaycaster>();

            var coinFlyPoolGo = new GameObject("CoinFlyPool");
            GameObjectUtility.SetParentAndAlign(coinFlyPoolGo, coinFlyOverlayGo);
            RectTransform coinFlyPoolRect = coinFlyPoolGo.AddComponent<RectTransform>();
            EditorUIFactory.Stretch(coinFlyPoolRect);

            CoinFlyBootstrap bootstrap = coinFlyOverlayGo.AddComponent<CoinFlyBootstrap>();
            var bootstrapSo = new SerializedObject(bootstrap);
            EditorUIFactory.SetObj(bootstrapSo, "_coinContainer", coinFlyPoolRect);
            EditorUIFactory.SetObj(bootstrapSo, "_targetBadge", null);
            EditorUIFactory.SetObj(bootstrapSo, "_coinSprite",
                AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd"));
            bootstrapSo.ApplyModifiedProperties();
        }

        private static void CreateCurrencyBadge(Transform parent, string name, string text,
            Vector2 anchorMin, Vector2 anchorMax, Color textColor,
            TMP_FontAsset font,
            Vector2? offsetMin = null, Vector2? offsetMax = null)
        {
            var go = new GameObject(name);
            GameObjectUtility.SetParentAndAlign(go, parent.gameObject);
            RectTransform r = go.AddComponent<RectTransform>();
            r.anchorMin = anchorMin;
            r.anchorMax = anchorMax;
            r.offsetMin = offsetMin ?? BadgeOffsetInnerLeft;
            r.offsetMax = offsetMax ?? BadgeOffsetInnerRight;

            Image bg = go.AddComponent<Image>();
            bg.color = HudBarBg;

            HorizontalLayoutGroup layout = go.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = CurrencyBadgeSpacing;
            layout.padding = new RectOffset(CurrencyBadgePaddingLeft, CurrencyBadgePaddingRight, 0, 0);
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
            iconLe.minWidth = CurrencyIconSize;
            iconLe.minHeight = CurrencyIconSize;
            iconLe.preferredWidth = CurrencyIconSize;
            iconLe.preferredHeight = CurrencyIconSize;
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
            EditorUIFactory.ApplyFont(tmp, font);
            LayoutElement labelLe = labelGo.AddComponent<LayoutElement>();
            labelLe.flexibleWidth = 1;
            labelLe.flexibleHeight = 1;
        }
    }
}
