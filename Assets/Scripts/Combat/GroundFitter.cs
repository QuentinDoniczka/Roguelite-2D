using UnityEngine;

namespace RogueliteAutoBattler.Combat
{
    /// <summary>
    /// Sizes the ground SpriteRenderer to match the GameArea (top portion of the screen).
    /// Adapts dynamically to camera orthoSize and screen ratio.
    /// </summary>
    public class GroundFitter : MonoBehaviour
    {
        [SerializeField] private float _gameAreaBottomRatio = 0.40f; // GameArea starts at 40% from bottom
        [SerializeField] private float _groundWidth = 200f;

        private SpriteRenderer _renderer;
        private Camera _camera;

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
            _camera = Camera.main;
            if (_renderer == null || _camera == null)
            {
                Debug.LogWarning($"[{nameof(GroundFitter)}] Missing SpriteRenderer or Camera. Disabling.", this);
                enabled = false;
                return;
            }
            FitToGameArea();
        }

        private void FitToGameArea()
        {
            float orthoSize = _camera.orthographicSize;
            float visibleHeight = orthoSize * 2f;
            float gameAreaBottom = -orthoSize + _gameAreaBottomRatio * visibleHeight;
            float gameAreaTop = orthoSize;
            float groundHeight = gameAreaTop - gameAreaBottom;
            float groundCenterY = (gameAreaBottom + gameAreaTop) * 0.5f;

            _renderer.size = new Vector2(_groundWidth, groundHeight);
            transform.localPosition = new Vector3(0f, groundCenterY, 0f);
        }
    }
}
