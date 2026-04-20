using RogueliteAutoBattler.Combat.Visuals;
using RogueliteAutoBattler.Economy;
using RogueliteAutoBattler.UI.Toolkit;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace RogueliteAutoBattler.Editor
{
    internal static class CombatHudBuilder
    {
        private const string CoinFlyOverlayGameObjectName = "CoinFlyOverlay";
        private const string CoinFlyPoolGameObjectName = "CoinFlyPool";
        private const string BuiltinKnobSpritePath = "UI/Skin/Knob.psd";
        private const int CoinFlyCanvasSortingOrder = 100;
        private const float CoinFlyReferenceResolutionX = 1080f;
        private const float CoinFlyReferenceResolutionY = 1920f;
        private const float CoinFlyMatchWidthOrHeight = 0.5f;

        internal static void SetupToolkitCombatHud(GameObject navigationHostGo, GoldWallet goldWallet)
        {
            UIDocument uiDocument = navigationHostGo.GetComponent<UIDocument>();

            CombatHudController combatHud = navigationHostGo.AddComponent<CombatHudController>();
            var combatHudSo = new SerializedObject(combatHud);
            EditorUIFactory.SetObj(combatHudSo, "_uiDocument", uiDocument);
            EditorUIFactory.SetObj(combatHudSo, "_goldWallet", goldWallet);
            combatHudSo.ApplyModifiedProperties();

            var coinFlyOverlayGo = new GameObject(CoinFlyOverlayGameObjectName);
            GameObjectUtility.SetParentAndAlign(coinFlyOverlayGo, navigationHostGo);

            Canvas canvas = coinFlyOverlayGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = CoinFlyCanvasSortingOrder;

            CanvasScaler scaler = coinFlyOverlayGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(CoinFlyReferenceResolutionX, CoinFlyReferenceResolutionY);
            scaler.matchWidthOrHeight = CoinFlyMatchWidthOrHeight;

            coinFlyOverlayGo.AddComponent<GraphicRaycaster>();

            var coinFlyPoolGo = new GameObject(CoinFlyPoolGameObjectName);
            GameObjectUtility.SetParentAndAlign(coinFlyPoolGo, coinFlyOverlayGo);
            RectTransform coinFlyPoolRect = coinFlyPoolGo.AddComponent<RectTransform>();
            EditorUIFactory.Stretch(coinFlyPoolRect);

            CoinFlyBootstrap bootstrap = coinFlyOverlayGo.AddComponent<CoinFlyBootstrap>();
            var bootstrapSo = new SerializedObject(bootstrap);
            EditorUIFactory.SetObj(bootstrapSo, "_coinContainer", coinFlyPoolRect);
            EditorUIFactory.SetObj(bootstrapSo, "_targetBadge", null);
            EditorUIFactory.SetObj(bootstrapSo, "_coinSprite",
                AssetDatabase.GetBuiltinExtraResource<Sprite>(BuiltinKnobSpritePath));
            bootstrapSo.ApplyModifiedProperties();
        }
    }
}
