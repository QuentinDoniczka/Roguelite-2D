using UnityEngine;

namespace RogueliteAutoBattler.Combat
{
    /// <summary>
    /// Positions this GameObject at a screen-relative (viewport) position in world space.
    /// Place at scene root (outside CombatWorld) so it stays fixed regardless of world scroll.
    /// </summary>
    public class ScreenAnchor : MonoBehaviour
    {
        [Tooltip("Viewport position (0,0 = bottom-left, 1,1 = top-right).")]
        [SerializeField] private Vector2 _viewportPosition = new Vector2(0.5f, 0.5f);

        private Camera _camera;
        private float _lastOrthoSize;
        private float _lastAspect;

        private void Start()
        {
            _camera = Camera.main;
            if (_camera == null)
            {
                Debug.LogWarning($"[{nameof(ScreenAnchor)}] No main camera found. Disabling.", this);
                enabled = false;
                return;
            }
            UpdatePosition();
        }

        private void LateUpdate()
        {
            if (_camera == null) return;

            float ortho = _camera.orthographicSize;
            float aspect = _camera.aspect;
            if (Mathf.Approximately(ortho, _lastOrthoSize) && Mathf.Approximately(aspect, _lastAspect))
                return;

            UpdatePosition();
        }

        private void UpdatePosition()
        {
            Vector3 worldPos = _camera.ViewportToWorldPoint(new Vector3(_viewportPosition.x, _viewportPosition.y, 0f));
            transform.position = new Vector3(worldPos.x, worldPos.y, 0f);
            _lastOrthoSize = _camera.orthographicSize;
            _lastAspect = _camera.aspect;
        }
    }
}
