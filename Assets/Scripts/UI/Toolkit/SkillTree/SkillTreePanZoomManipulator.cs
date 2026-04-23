using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace RogueliteAutoBattler.UI.Toolkit.SkillTree
{
    public sealed class SkillTreePanZoomManipulator : PointerManipulator
    {
        private const float MinimumZoom = 0.4f;
        private const float MaximumZoom = 2.5f;
        private const float WheelZoomStep = 0.1f;
        private const float ClickVersusDragThresholdPixels = 12f;
        private const float TwoDimensionalZ = 0f;
        private const float TwoDimensionalScaleZ = 1f;

        private readonly VisualElement _contentTarget;
        private readonly Dictionary<int, Vector2> _activePointerPositionsByPointerId = new();

        private Vector2 _initialPanPointerPosition;
        private Vector3 _initialPanContentPosition;
        private float _initialPinchDistance;
        private float _initialPinchScale;
        private Vector2 _pointerDownPosition;
        private bool _clickVsDragExceededThisGesture;

        public float CurrentZoom { get; private set; } = 1f;
        public bool ExceededClickVersusDragThreshold => _activePointerPositionsByPointerId.Count == 0 && _clickVsDragExceededThisGesture;

        public SkillTreePanZoomManipulator(VisualElement viewportTarget, VisualElement contentTarget)
        {
            target = viewportTarget;
            _contentTarget = contentTarget;
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.focusable = true;
            target.RegisterCallback<PointerDownEvent>(OnPointerDown);
            target.RegisterCallback<PointerMoveEvent>(OnPointerMove);
            target.RegisterCallback<PointerUpEvent>(OnPointerUp);
            target.RegisterCallback<PointerCancelEvent>(OnPointerCancel);
            target.RegisterCallback<WheelEvent>(OnWheel);
            target.RegisterCallback<KeyDownEvent>(OnKeyDown);
            target.RegisterCallback<PointerEnterEvent>(OnPointerEnter);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<PointerDownEvent>(OnPointerDown);
            target.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
            target.UnregisterCallback<PointerUpEvent>(OnPointerUp);
            target.UnregisterCallback<PointerCancelEvent>(OnPointerCancel);
            target.UnregisterCallback<WheelEvent>(OnWheel);
            target.UnregisterCallback<KeyDownEvent>(OnKeyDown);
            target.UnregisterCallback<PointerEnterEvent>(OnPointerEnter);
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            _activePointerPositionsByPointerId[evt.pointerId] = evt.position;

            if (_activePointerPositionsByPointerId.Count == 1)
            {
                _pointerDownPosition = evt.position;
                _clickVsDragExceededThisGesture = false;
                _initialPanPointerPosition = evt.position;
                _initialPanContentPosition = _contentTarget.transform.position;
            }
            else if (_activePointerPositionsByPointerId.Count == 2)
            {
                _initialPinchDistance = ComputeActivePinchDistance();
                _initialPinchScale = CurrentZoom;
                CaptureAllActivePointersIfNotAlreadyCaptured();
            }
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (!_activePointerPositionsByPointerId.ContainsKey(evt.pointerId))
                return;

            _activePointerPositionsByPointerId[evt.pointerId] = evt.position;

            if (_activePointerPositionsByPointerId.Count == 1)
            {
                Vector2 totalDelta = (Vector2)evt.position - _pointerDownPosition;
                if (!_clickVsDragExceededThisGesture && totalDelta.magnitude >= ClickVersusDragThresholdPixels)
                {
                    _clickVsDragExceededThisGesture = true;
                    CaptureAllActivePointersIfNotAlreadyCaptured();
                }

                Vector2 panDelta = (Vector2)evt.position - _initialPanPointerPosition;
                _contentTarget.transform.position = _initialPanContentPosition + new Vector3(panDelta.x, panDelta.y, TwoDimensionalZ);
            }
            else if (_activePointerPositionsByPointerId.Count == 2)
            {
                float currentDistance = ComputeActivePinchDistance();
                float newZoom = _initialPinchScale * (currentDistance / _initialPinchDistance);
                SetContentScale(Mathf.Clamp(newZoom, MinimumZoom, MaximumZoom));
            }
        }

        private void CaptureAllActivePointersIfNotAlreadyCaptured()
        {
            foreach (KeyValuePair<int, Vector2> entry in _activePointerPositionsByPointerId)
            {
                if (!target.HasPointerCapture(entry.Key))
                    target.CapturePointer(entry.Key);
            }
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            bool dragExceeded = _clickVsDragExceededThisGesture;

            _activePointerPositionsByPointerId.Remove(evt.pointerId);
            target.ReleasePointer(evt.pointerId);

            if (_activePointerPositionsByPointerId.Count == 1)
            {
                ResetPanTrackingToSingleRemainingPointer();
            }

            if (dragExceeded && _activePointerPositionsByPointerId.Count == 0)
                evt.StopPropagation();
        }

        private void OnPointerCancel(PointerCancelEvent evt)
        {
            _activePointerPositionsByPointerId.Remove(evt.pointerId);
            target.ReleasePointer(evt.pointerId);

            if (_activePointerPositionsByPointerId.Count == 1)
                ResetPanTrackingToSingleRemainingPointer();
        }

        private void OnWheel(WheelEvent evt)
        {
            float zoomDelta = -evt.delta.y * WheelZoomStep;
            SetContentScale(Mathf.Clamp(CurrentZoom + zoomDelta, MinimumZoom, MaximumZoom));
            evt.StopPropagation();
        }

        private void OnPointerEnter(PointerEnterEvent evt)
        {
            target.Focus();
        }

        private void OnKeyDown(KeyDownEvent evt)
        {
            bool isZoomIn = evt.keyCode == KeyCode.Plus
                || evt.keyCode == KeyCode.Equals
                || evt.keyCode == KeyCode.KeypadPlus
                || evt.character == '+';

            bool isZoomOut = evt.keyCode == KeyCode.Minus
                || evt.keyCode == KeyCode.KeypadMinus
                || evt.character == '-';

            if (isZoomIn)
            {
                ApplyKeyboardZoom(WheelZoomStep);
                evt.StopPropagation();
            }
            else if (isZoomOut)
            {
                ApplyKeyboardZoom(-WheelZoomStep);
                evt.StopPropagation();
            }
        }

        private void ApplyKeyboardZoom(float zoomDelta)
        {
            SetContentScale(Mathf.Clamp(CurrentZoom + zoomDelta, MinimumZoom, MaximumZoom));
        }

        private void ResetPanTrackingToSingleRemainingPointer()
        {
            foreach (KeyValuePair<int, Vector2> entry in _activePointerPositionsByPointerId)
            {
                _initialPanPointerPosition = entry.Value;
                _initialPanContentPosition = _contentTarget.transform.position;
                break;
            }
        }

        private float ComputeActivePinchDistance()
        {
            Vector2 first = default;
            Vector2 second = default;
            int index = 0;

            foreach (KeyValuePair<int, Vector2> entry in _activePointerPositionsByPointerId)
            {
                if (index == 0)
                    first = entry.Value;
                else
                    second = entry.Value;

                index++;
                if (index == 2)
                    break;
            }

            return Vector2.Distance(first, second);
        }

        private void SetContentScale(float newScale)
        {
            CurrentZoom = newScale;
            _contentTarget.transform.scale = new Vector3(newScale, newScale, TwoDimensionalScaleZ);
        }
    }
}
