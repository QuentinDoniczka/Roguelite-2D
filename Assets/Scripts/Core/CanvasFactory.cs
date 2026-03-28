using UnityEngine;
using UnityEngine.UI;

namespace RogueliteAutoBattler.Core
{
    public static class CanvasFactory
    {
        public const int ReferenceWidth = 1080;
        public const int ReferenceHeight = 1920;
        public const float MatchWidthOrHeight = 0.5f;
        public const float PlaneDistance = 100f;
        public const string SortingLayerName = "UI";
        public const int SortingOrder = 0;

        public static GameObject Create(Camera cam)
        {
            if (cam == null)
                Debug.LogWarning("[CanvasFactory] Camera is null. Canvas will behave as ScreenSpaceOverlay.");

            var go = new GameObject("UICanvas");

            Canvas canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = cam;
            canvas.planeDistance = PlaneDistance;
            canvas.sortingLayerName = SortingLayerName;
            canvas.sortingOrder = SortingOrder;

            CanvasScaler scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(ReferenceWidth, ReferenceHeight);
            scaler.matchWidthOrHeight = MatchWidthOrHeight;

            go.AddComponent<GraphicRaycaster>();

            return go;
        }
    }
}
