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

        private const float ZoomPerNotch = 0.2f;
        private const float KeyboardZoomSpeed = 0.04f;

        private bool _isPinching;
        private float _lastPinchDistance;
        private Canvas _cachedCanvas;
        private CanvasGroup _cachedCanvasGroup;
        private InputAction _scrollAction;
        private InputAction _zoomInAction;
        private InputAction _zoomOutAction;

        public event Action OnVoidClicked;

        private static Vector2 ScreenCenter => new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);

        private void Awake()
        {
            _cachedCanvasGroup = GetComponentInParent<CanvasGroup>();
            _cachedCanvas = GetComponentInParent<Canvas>();

            _scrollAction = new InputAction("SkillTreeScroll", InputActionType.PassThrough, "<Mouse>/scroll");

            _zoomInAction = new InputAction("SkillTreeZoomIn", InputActionType.Button);
            _zoomInAction.AddBinding("<Keyboard>/equals");
            _zoomInAction.AddBinding("<Keyboard>/numpadPlus");

            _zoomOutAction = new InputAction("SkillTreeZoomOut", InputActionType.Button);
            _zoomOutAction.AddBinding("<Keyboard>/minus");
            _zoomOutAction.AddBinding("<Keyboard>/numpadMinus");
        }

        private void OnEnable()
        {
            EnhancedTouchSupport.Enable();
            _scrollAction.Enable();
            _scrollAction.performed += OnScrollPerformed;
            _zoomInAction.Enable();
            _zoomOutAction.Enable();
        }

        private void OnDisable()
        {
            _scrollAction.performed -= OnScrollPerformed;
            _scrollAction.Disable();
            _zoomInAction.Disable();
            _zoomOutAction.Disable();
            EnhancedTouchSupport.Disable();
        }

        private void OnDestroy()
        {
            _scrollAction?.Dispose();
            _zoomInAction?.Dispose();
            _zoomOutAction?.Dispose();
        }

        private void Update()
        {
            if (!IsScreenVisible()) return;

            HandleKeyboardZoom();
            HandlePinchZoom();
        }

        private void OnScrollPerformed(InputAction.CallbackContext ctx)
        {
            if (!IsScreenVisible()) return;

            Vector2 scrollValue = ctx.ReadValue<Vector2>();
            float normalizedDelta = Mathf.Sign(scrollValue.y);
            float scaleFactor = 1f + normalizedDelta * ZoomPerNotch;

            Vector2 pivot = Mouse.current != null ? Mouse.current.position.ReadValue() : ScreenCenter;
            ApplyZoom(pivot, scaleFactor);
        }

        private void HandleKeyboardZoom()
        {
            float zoomDelta = 0f;
            if (_zoomInAction.IsPressed()) zoomDelta = KeyboardZoomSpeed;
            if (_zoomOutAction.IsPressed()) zoomDelta = -KeyboardZoomSpeed;

            if (Mathf.Abs(zoomDelta) < 0.001f) return;

            ApplyZoom(ScreenCenter, 1f + zoomDelta);
        }

        private void HandlePinchZoom()
        {
            var activeTouches = Touch.activeTouches;

            if (activeTouches.Count != 2)
            {
                _isPinching = false;
                _lastPinchDistance = 0f;
                return;
            }

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

        public void OnDrag(PointerEventData eventData)
        {
            if (_isPinching) return;
            _content.anchoredPosition += eventData.delta;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            OnVoidClicked?.Invoke();
        }

        private bool IsScreenVisible()
        {
            return _cachedCanvasGroup != null && _cachedCanvasGroup.alpha > 0f;
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
            return _cachedCanvas;
        }
    }
}
