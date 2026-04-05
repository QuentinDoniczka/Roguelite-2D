using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace RogueliteAutoBattler.UI.Screens.SkillTree
{
    public class SkillTreeInputHandler : MonoBehaviour, IDragHandler, IScrollHandler, IPointerClickHandler
    {
        [SerializeField] private RectTransform _content;

        [Header("Zoom Limits")]
        [SerializeField] private float _minScale = 0.3f;
        [SerializeField] private float _maxScale = 3.0f;

        private const float ZoomPerNotch = 0.15f;
        private const float KeyboardZoomSpeed = 0.02f;

        private bool _isPinching;
        private float _lastPinchDistance;
        private Canvas _cachedCanvas;
        private CanvasGroup _cachedCanvasGroup;
        private InputAction _scrollAction;
        private InputAction _zoomInAction;
        private InputAction _zoomOutAction;

        public event Action OnVoidClicked;

        private void Awake()
        {
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
            HandleKeyboardZoom();
            HandlePinchZoom();
            DebugScrollRaw();
        }

        private void DebugScrollRaw()
        {
            if (Mouse.current == null) return;
            Vector2 scrollVal = Mouse.current.scroll.ReadValue();
            if (scrollVal != Vector2.zero)
                Debug.Log($"[SkillTree] Mouse.current.scroll raw = {scrollVal}");
        }

        private void OnScrollPerformed(InputAction.CallbackContext ctx)
        {
            Vector2 scrollValue = ctx.ReadValue<Vector2>();
            Debug.Log($"[SkillTree] InputAction scroll performed: {scrollValue}, visible={IsScreenVisible()}, mouse={Mouse.current != null}");

            if (!IsScreenVisible()) return;

            Vector2 screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            Vector2 mousePos = Mouse.current != null ? Mouse.current.position.ReadValue() : screenCenter;

            float normalizedDelta = scrollValue.y / 120f;
            float scaleFactor = 1f + normalizedDelta * ZoomPerNotch;
            Debug.Log($"[SkillTree] Applying zoom: normalizedDelta={normalizedDelta}, scaleFactor={scaleFactor}, currentScale={_content.localScale.x}");
            ApplyZoom(mousePos, scaleFactor);
            Debug.Log($"[SkillTree] After zoom: scale={_content.localScale.x}");
        }

        public void OnScroll(PointerEventData eventData)
        {
            Debug.Log($"[SkillTree] IScrollHandler.OnScroll fired: scrollDelta={eventData.scrollDelta}");
            float normalizedDelta = eventData.scrollDelta.y / 120f;
            float scaleFactor = 1f + normalizedDelta * ZoomPerNotch;
            ApplyZoom(eventData.position, scaleFactor);
        }

        private void HandleKeyboardZoom()
        {
            if (!IsScreenVisible()) return;

            float zoomDelta = 0f;
            if (_zoomInAction.IsPressed()) zoomDelta = KeyboardZoomSpeed;
            if (_zoomOutAction.IsPressed()) zoomDelta = -KeyboardZoomSpeed;

            if (Mathf.Abs(zoomDelta) < 0.001f) return;

            float scaleFactor = 1f + zoomDelta;
            Vector2 screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            ApplyZoom(screenCenter, scaleFactor);
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
