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

        public event Action OnVoidClicked;

        private void OnEnable()
        {
            EnhancedTouchSupport.Enable();
        }

        private void OnDisable()
        {
            EnhancedTouchSupport.Disable();
        }

        private void Update()
        {
            HandleMouseScroll();
            HandlePinchZoom();
        }

        private void HandleMouseScroll()
        {
            if (Mouse.current == null) return;

            float rawScroll = Mouse.current.scroll.ReadValue().y;
            if (Mathf.Abs(rawScroll) < 0.01f) return;

            float normalizedDelta = rawScroll / ScrollNormalization;
            float scaleFactor = 1f + normalizedDelta * ZoomPerNotch;
            ApplyZoom(Mouse.current.position.ReadValue(), scaleFactor);
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
