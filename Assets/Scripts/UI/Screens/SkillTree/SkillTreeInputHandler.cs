using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace RogueliteAutoBattler.UI.Screens.SkillTree
{
    public class SkillTreeInputHandler : MonoBehaviour, IDragHandler, IPointerClickHandler
    {
        [SerializeField] private RectTransform _content;

        [Header("Zoom Limits")]
        [SerializeField] private float _minScale = 0.3f;
        [SerializeField] private float _maxScale = 3.0f;

        private const float ScrollNormalization = 120f;
        private const float ZoomPerNotch = 0.15f;

        private bool _isPinching;
        private float _lastPinchDistance;
        private Canvas _cachedCanvas;
        private CanvasGroup _cachedCanvasGroup;
        private InputAction _scrollAction;

        public event Action OnVoidClicked;

        private void Awake()
        {
            _scrollAction = new InputAction("SkillTreeScroll", InputActionType.PassThrough, "<Mouse>/scroll");
        }

        private void OnEnable()
        {
            EnhancedTouchSupport.Enable();
            _scrollAction.Enable();
            _scrollAction.performed += OnScrollPerformed;
        }

        private void OnDisable()
        {
            _scrollAction.performed -= OnScrollPerformed;
            _scrollAction.Disable();
            EnhancedTouchSupport.Disable();
        }

        private void OnDestroy()
        {
            _scrollAction?.Dispose();
        }

        private void Update()
        {
            HandlePinchZoom();
        }

        private void OnScrollPerformed(InputAction.CallbackContext ctx)
        {
            if (!IsScreenVisible()) return;
            if (Mouse.current == null) return;

            Vector2 scrollValue = ctx.ReadValue<Vector2>();
            float normalizedDelta = scrollValue.y / ScrollNormalization;
            float scaleFactor = 1f + normalizedDelta * ZoomPerNotch;
            ApplyZoom(Mouse.current.position.ReadValue(), scaleFactor);
        }

        private bool IsScreenVisible()
        {
            if (_cachedCanvasGroup == null)
                _cachedCanvasGroup = GetComponentInParent<CanvasGroup>();
            return _cachedCanvasGroup != null && _cachedCanvasGroup.alpha > 0f;
        }

        private void HandlePinchZoom()
        {
            var activeTouches = Touch.activeTouches;

            if (activeTouches.Count == 2)
            {
                _isPinching = true;

                Vector2 firstPosition = activeTouches[0].screenPosition;
                Vector2 secondPosition = activeTouches[1].screenPosition;
                float currentDistance = Vector2.Distance(firstPosition, secondPosition);

                if (_lastPinchDistance > 0f)
                {
                    float scaleFactor = currentDistance / _lastPinchDistance;
                    Vector2 midpoint = (firstPosition + secondPosition) * 0.5f;
                    ApplyZoom(midpoint, scaleFactor);
                }

                _lastPinchDistance = currentDistance;
            }
            else
            {
                _isPinching = false;
                _lastPinchDistance = 0f;
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_isPinching) return;

            _content.anchoredPosition += eventData.delta;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            OnVoidClicked?.Invoke();
        }

        internal void ApplyZoom(Vector2 screenPivot, float scaleFactor)
        {
            Canvas canvas = GetCachedCanvas();
            if (canvas == null) return;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _content, screenPivot, canvas.worldCamera, out Vector2 localPivot);

            float currentScale = _content.localScale.x;
            float newScale = Mathf.Clamp(currentScale * scaleFactor, _minScale, _maxScale);

            _content.anchoredPosition = localPivot - (newScale / currentScale) * (localPivot - _content.anchoredPosition);
            _content.localScale = Vector3.one * newScale;
        }

        private Canvas GetCachedCanvas()
        {
            if (_cachedCanvas == null)
            {
                _cachedCanvas = GetComponentInParent<Canvas>();
            }

            return _cachedCanvas;
        }
    }
}
