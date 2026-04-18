using RogueliteAutoBattler.Combat.Visuals;
using RogueliteAutoBattler.UI.Toolkit;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace RogueliteAutoBattler.Editor
{
    internal static class CombatHudBuilder
    {
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
    }
}
