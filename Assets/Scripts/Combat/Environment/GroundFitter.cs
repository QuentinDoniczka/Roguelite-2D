using RogueliteAutoBattler.Data;
using UnityEngine;

namespace RogueliteAutoBattler.Combat.Environment
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class GroundFitter : MonoBehaviour
    {
        private const float HorizontalEdgePadding = 1f;
        private const float HorizontalTotalPadding = HorizontalEdgePadding * 2f;
        private const float UiBottomNormalizedHeight = 0.45f;

        [SerializeField] private float _groundWidth = 200f;
        [SerializeField] private BackgroundFit _fit = BackgroundFit.Tile;

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

        public void SetFitMode(BackgroundFit fit)
        {
            _fit = fit;
            if (_renderer == null)
                return;
            if (_camera == null)
                _camera = Camera.main;
            if (_camera == null)
                return;
            FitToGameArea();
        }

        private void FitToGameArea()
        {
            if (_renderer == null || _camera == null)
                return;

            float orthoSize = _camera.orthographicSize;
            float aspect = _camera.aspect;
            float totalHeight = orthoSize * 2f;
            float visibleWidth = totalHeight * aspect;

            float groundHeight = totalHeight * (1f - UiBottomNormalizedHeight);
            float groundTopY = orthoSize;
            float groundCenterY = groundTopY - groundHeight * 0.5f;

            float width = Mathf.Max(_groundWidth, visibleWidth + HorizontalTotalPadding);
            float visibleHalfWidth = visibleWidth * 0.5f;
            float anchorX = -(visibleHalfWidth + HorizontalEdgePadding) + width * 0.5f;

            if (_fit == BackgroundFit.Stretch)
            {
                _renderer.drawMode = SpriteDrawMode.Simple;
            }
            else
            {
                _renderer.drawMode = SpriteDrawMode.Tiled;
                _renderer.size = new Vector2(width, groundHeight);
            }

            transform.localPosition = new Vector3(anchorX, groundCenterY, 0f);

            _lastOrthoSize = orthoSize;
            _lastAspect = aspect;
        }
    }
}
