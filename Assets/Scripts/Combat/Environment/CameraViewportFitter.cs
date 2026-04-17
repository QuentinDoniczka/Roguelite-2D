using UnityEngine;
using UnityEngine.UIElements;

namespace RogueliteAutoBattler.Combat.Environment
{
    [RequireComponent(typeof(Camera))]
    public class CameraViewportFitter : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoAttach()
        {
            if (FindObjectsByType<CameraViewportFitter>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length > 0)
                return;

            Camera main = Camera.main;
            if (main != null)
                main.gameObject.AddComponent<CameraViewportFitter>();
        }

        [SerializeField] private bool _debugLog;

        private Camera _camera;
        private VisualElement _gameArea;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
        }

        private void Start()
        {
            UIDocument document = FindFirstObjectByType<UIDocument>(FindObjectsInactive.Include);
            if (document == null)
            {
                Debug.LogWarning("[CameraViewportFitter] No UIDocument found in scene.");
                return;
            }

            _gameArea = document.rootVisualElement.Q<VisualElement>("game-area");
            if (_gameArea == null)
            {
                Debug.LogWarning("[CameraViewportFitter] No VisualElement named 'game-area' found.");
                return;
            }

            _gameArea.RegisterCallback<GeometryChangedEvent>(OnGameAreaGeometryChanged);
        }

        private void OnDestroy()
        {
            _gameArea?.UnregisterCallback<GeometryChangedEvent>(OnGameAreaGeometryChanged);
        }

        private void OnGameAreaGeometryChanged(GeometryChangedEvent evt)
        {
            ApplyViewportRect();
        }

        private void ApplyViewportRect()
        {
            if (_camera == null || _gameArea == null) return;

            Rect panelBound = _gameArea.worldBound;
            Rect fullPanel = _gameArea.panel.visualTree.worldBound;

            if (fullPanel.width <= 0f || fullPanel.height <= 0f) return;

            float xNorm = panelBound.x / fullPanel.width;
            float wNorm = panelBound.width / fullPanel.width;
            float yNormFromBottom = 1f - (panelBound.y + panelBound.height) / fullPanel.height;
            float hNorm = panelBound.height / fullPanel.height;

            _camera.rect = new Rect(xNorm, yNormFromBottom, wNorm, hNorm);

            if (_debugLog)
            {
                Debug.Log($"[CameraViewportFitter] fullPanel={fullPanel} gameArea={panelBound} camera.rect={_camera.rect}");
            }
        }
    }
}
