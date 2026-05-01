using RogueliteAutoBattler.Data;
using UnityEngine;
using UnityEngine.UIElements;

namespace RogueliteAutoBattler.Editor.Windows.SkillTreeDesigner
{
    [UxmlElement]
    internal sealed partial class SkillTreeCanvasElement : VisualElement
    {
        private const float MinZoom = 0.25f;
        private const float MaxZoom = 4f;
        private const float ZoomStep = 0.1f;
        private const float UnitToPx = 64f;
        private const float NodeRadiusPx = 18f;
        private const float GridSpacingPx = 32f;

        private static readonly Color GridColor = new Color(0.25f, 0.25f, 0.25f, 0.8f);
        private static readonly Color NodeColor = new Color(0.35f, 0.55f, 0.85f, 1f);
        private static readonly Color EdgeColor = new Color(0.5f, 0.5f, 0.5f, 0.9f);
        private static readonly Color SelectedRingColor = new Color(1f, 0.85f, 0.1f, 1f);

        private SkillTreeData _data;
        private int? _selectedNodeId;
        private float _zoom = 1f;
        private Vector2 _pan = Vector2.zero;
        private bool _isPanning;
        private Vector2 _lastMousePos;
        private int _markDirtyCount;

        public SkillTreeCanvasElement()
        {
            style.flexGrow = 1;
            generateVisualContent += OnGenerateVisualContent;
            RegisterCallback<WheelEvent>(OnWheel);
            RegisterCallback<MouseDownEvent>(OnMouseDown);
            RegisterCallback<MouseMoveEvent>(OnMouseMove);
            RegisterCallback<MouseUpEvent>(OnMouseUp);
        }

        internal float Zoom => _zoom;
        internal Vector2 Pan => _pan;
        internal int MarkDirtyRepaintCount => _markDirtyCount;

        internal void SetData(SkillTreeData data, int? selectedId)
        {
            _data = data;
            _selectedNodeId = selectedId;
            MarkDirty();
        }

        internal void SetZoomForTest(float z)
        {
            _zoom = Mathf.Clamp(z, MinZoom, MaxZoom);
        }

        internal void SetPanForTest(Vector2 p)
        {
            _pan = p;
        }

        internal Vector2 DataToScreen(Vector2 dataPos)
        {
            return SkillTreeCanvasMesh.DataToScreen(dataPos, contentRect.center, UnitToPx, _pan, _zoom);
        }

        internal int? HitTest(Vector2 localPx)
        {
            if (_data == null) return null;

            var nodes = _data.Nodes;
            float radiusSq = (NodeRadiusPx * _zoom) * (NodeRadiusPx * _zoom);
            int? result = null;

            for (int i = 0; i < nodes.Count; i++)
            {
                var screen = DataToScreen(nodes[i].position);
                var delta = localPx - screen;
                if (delta.x * delta.x + delta.y * delta.y <= radiusSq)
                    result = nodes[i].id;
            }

            return result;
        }

        internal void SimulateClickAt(Vector2 localPx)
        {
            var hit = HitTest(localPx);
            if (!hit.HasValue) return;
            using var e = NodeClickedEvent.GetPooled(hit.Value);
            e.target = this;
            SendEvent(e);
        }

        private void MarkDirty()
        {
            MarkDirtyRepaint();
            _markDirtyCount++;
        }

        private void OnWheel(WheelEvent evt)
        {
            _zoom = Mathf.Clamp(_zoom - evt.delta.y * ZoomStep, MinZoom, MaxZoom);
            MarkDirty();
            evt.StopPropagation();
        }

        private void OnMouseDown(MouseDownEvent evt)
        {
            bool isPanGesture = evt.button == 2 || (evt.button == 0 && evt.altKey);

            if (isPanGesture)
            {
                _isPanning = true;
                _lastMousePos = evt.localMousePosition;
                this.CaptureMouse();
                evt.StopPropagation();
                return;
            }

            if (evt.button == 0)
            {
                var hit = HitTest(evt.localMousePosition);
                if (hit.HasValue)
                {
                    using var e = NodeClickedEvent.GetPooled(hit.Value);
                    e.target = this;
                    SendEvent(e);
                }
            }
        }

        private void OnMouseMove(MouseMoveEvent evt)
        {
            if (!_isPanning) return;
            _pan += evt.localMousePosition - _lastMousePos;
            _lastMousePos = evt.localMousePosition;
            MarkDirty();
        }

        private void OnMouseUp(MouseUpEvent evt)
        {
            _isPanning = false;
            if (this.HasMouseCapture())
                this.ReleaseMouse();
        }

        private void OnGenerateVisualContent(MeshGenerationContext mgc)
        {
            if (_data == null) return;

            var painter = mgc.painter2D;
            var viewport = contentRect;
            var origin = contentRect.center;

            SkillTreeCanvasMesh.DrawGrid(painter, viewport, _zoom, _pan, GridSpacingPx, GridColor);
            SkillTreeCanvasMesh.DrawEdges(painter, _data.Nodes, origin, UnitToPx, _pan, _zoom, EdgeColor);
            SkillTreeCanvasMesh.DrawNodes(painter, _data.Nodes, _selectedNodeId, origin, UnitToPx, _pan, _zoom, NodeRadiusPx, NodeColor, SelectedRingColor);
        }
    }
}
