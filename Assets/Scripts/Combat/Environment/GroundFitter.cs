using UnityEngine;

namespace RogueliteAutoBattler.Combat.Environment
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class GroundFitter : MonoBehaviour
    {
        [SerializeField] private float _groundWidth = 200f;

        private SpriteRenderer _renderer;
        private Camera _camera;
        private float _lastOrthoSize;
        private float _lastAspect;

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
        }

        private void Start()
        {
            _camera = Camera.main;
            if (_renderer == null || _camera == null)
            {
                Debug.LogWarning($"[{nameof(GroundFitter)}] Missing SpriteRenderer or Camera. Disabling.", this);
                enabled = false;
                return;
            }
            FitToGameArea();
        }

        private void LateUpdate()
        {
            float ortho = _camera.orthographicSize;
            float aspect = _camera.aspect;
            if (Mathf.Approximately(ortho, _lastOrthoSize) && Mathf.Approximately(aspect, _lastAspect))
                return;
            FitToGameArea();
        }

        private void FitToGameArea()
        {
            float orthoSize = _camera.orthographicSize;
            float aspect = _camera.aspect;
            float visibleHeight = orthoSize * 2f;
            float visibleWidth = visibleHeight * aspect;

            float width = Mathf.Max(_groundWidth, visibleWidth + 2f);
            _renderer.size = new Vector2(width, visibleHeight);

            float visibleHalfWidth = visibleWidth * 0.5f;
            float anchorX = -(visibleHalfWidth + 1f) + width * 0.5f;
            transform.localPosition = new Vector3(anchorX, 0f, 0f);

            _lastOrthoSize = orthoSize;
            _lastAspect = aspect;
        }
    }
}
