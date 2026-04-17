using UnityEngine;

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

        [SerializeField] private float _infoAreaHeightPx = 280f;
        [SerializeField] private float _navBarHeightPx = 80f;

        private Camera _camera;
        private int _lastScreenHeight;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
        }

        private void Start()
        {
            ApplyViewportRect();
        }

        private void LateUpdate()
        {
            if (Screen.height == _lastScreenHeight) return;
            ApplyViewportRect();
        }

        private void ApplyViewportRect()
        {
            if (_camera == null) return;

            float totalUiHeightPx = _infoAreaHeightPx + _navBarHeightPx;
            float bottomNorm = Mathf.Clamp01(totalUiHeightPx / Screen.height);
            _camera.rect = new Rect(0f, bottomNorm, 1f, 1f - bottomNorm);

            _lastScreenHeight = Screen.height;
        }
    }
}
