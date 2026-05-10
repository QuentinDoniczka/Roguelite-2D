using System.Collections.Generic;
using RogueliteAutoBattler.Data;
using RogueliteAutoBattler.UI.Toolkit;
using RogueliteAutoBattler.UI.Toolkit.SkillTree;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RogueliteAutoBattler.Editor.Windows
{
    internal sealed class SkillTreePreviewPanel
    {
        private const float DragThresholdPx = 4f;

        private readonly SkillTreeData _data;
        private readonly SkillNodePalette _palette;

        private VisualElement _root;
        private VisualElement _nodesLayer;
        private SkillTreeEdgeLayer _edgeLayer;
        private SkillTreeRenderer _renderer;

        private readonly Dictionary<int, SkillTreeNodeVisualState> _nodeStates =
            new Dictionary<int, SkillTreeNodeVisualState>();

        private static readonly SkillTreeNodeVisualState[] CycleOrder =
        {
            SkillTreeNodeVisualState.Locked,
            SkillTreeNodeVisualState.Available,
            SkillTreeNodeVisualState.Purchased,
            SkillTreeNodeVisualState.Max
        };

        private int _dragNodeIndex = -1;
        private Vector2 _dragStartPointerPos;
        private Vector2 _dragStartDataPos;
        private bool _dragThresholdExceeded;

        internal SkillTreePreviewPanel(SkillTreeData data, SkillNodePalette palette)
        {
            _data = data;
            _palette = palette;
        }

        internal VisualElement BuildRoot()
        {
            _root = new VisualElement();
            _root.style.flexGrow = 1;
            _root.style.flexDirection = FlexDirection.Column;

            BuildContent();
            return _root;
        }

        internal void Rebuild()
        {
            if (_root == null) return;
            _root.Clear();
            BuildContent();
        }

        internal void SetVisible(bool visible)
        {
            if (_root == null) return;
            _root.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        internal void CycleNodeState(int nodeIndex)
        {
            if (_data == null || nodeIndex < 0 || nodeIndex >= _data.Nodes.Count) return;

            if (!_nodeStates.TryGetValue(nodeIndex, out var current))
                current = SkillTreeNodeVisualState.Locked;

            int currentIndex = System.Array.IndexOf(CycleOrder, current);
            int nextIndex = (currentIndex + 1) % CycleOrder.Length;
            var next = CycleOrder[nextIndex];

            _nodeStates[nodeIndex] = next;

            if (_renderer != null)
            {
                foreach (var element in _renderer.NodeElements)
                {
                    if (element.NodeIndex == nodeIndex)
                    {
                        element.SetState(next);
                        break;
                    }
                }
            }

            RefreshEdges();
        }

        internal void WriteNodePositionWithUndo(int nodeIndex, Vector2 newDataPosition)
        {
            if (_data == null || nodeIndex < 0 || nodeIndex >= _data.Nodes.Count) return;

            Undo.RegisterCompleteObjectUndo(_data, "Drag Skill Tree Node");
            var entry = _data.Nodes[nodeIndex];
            entry.position = newDataPosition;
            _data.SetNode(nodeIndex, entry);
            EditorUtility.SetDirty(_data);
        }

        private void BuildContent()
        {
            AttachMainStyleSheet();
            BuildEditableCanvas();
        }

        internal void AttachMainStyleSheet()
        {
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(UIStylePaths.MainStyleSheet);
            if (styleSheet != null)
                _root.styleSheets.Add(styleSheet);
        }

        private void BuildEditableCanvas()
        {
            if (_data == null) return;

            var viewport = new VisualElement();
            viewport.style.flexGrow = 1;
            viewport.style.overflow = Overflow.Hidden;
            _root.Add(viewport);

            var content = new VisualElement();
            content.style.position = Position.Absolute;
            content.style.left = 0f;
            content.style.top = 0f;
            viewport.Add(content);

            _edgeLayer = new SkillTreeEdgeLayer();
            content.Add(_edgeLayer);

            _nodesLayer = new VisualElement();
            _nodesLayer.style.position = Position.Absolute;
            _nodesLayer.style.left = 0f;
            _nodesLayer.style.top = 0f;
            content.Add(_nodesLayer);

            _renderer = new SkillTreeRenderer(_palette);

            var statesList = BuildStatesList();
            _renderer.RenderNodes(_data, statesList, _nodesLayer, null);

            foreach (var element in _renderer.NodeElements)
            {
                RegisterNodePointerCallbacks(element, element.NodeIndex);
            }

            var idToIndexMap = BuildIdToIndexMap();
            _renderer.RenderEdges(_data, idToIndexMap, statesList, _edgeLayer);

            viewport.RegisterCallback<GeometryChangedEvent>(evt => CenterContent(viewport, content));
        }

        private void CenterContent(VisualElement viewport, VisualElement content)
        {
            if (viewport.layout.width > 0f && viewport.layout.height > 0f)
            {
                content.style.left = viewport.layout.width * 0.5f;
                content.style.top = viewport.layout.height * 0.5f;
            }
        }

        private void RegisterNodePointerCallbacks(SkillTreeNodeElement element, int nodeIndex)
        {
            element.RegisterCallback<PointerDownEvent>(evt => OnPointerDown(evt, element, nodeIndex));
            element.RegisterCallback<PointerMoveEvent>(evt => OnPointerMove(evt, element, nodeIndex));
            element.RegisterCallback<PointerUpEvent>(evt => OnPointerUp(evt, element, nodeIndex));
        }

        private void OnPointerDown(PointerDownEvent evt, SkillTreeNodeElement element, int nodeIndex)
        {
            _dragNodeIndex = nodeIndex;
            _dragStartPointerPos = evt.position;
            _dragStartDataPos = _data.Nodes[nodeIndex].position;
            _dragThresholdExceeded = false;
            element.CapturePointer(evt.pointerId);
            evt.StopPropagation();
        }

        private void OnPointerMove(PointerMoveEvent evt, SkillTreeNodeElement element, int nodeIndex)
        {
            if (_dragNodeIndex != nodeIndex) return;

            Vector2 delta = (Vector2)evt.position - _dragStartPointerPos;

            if (!_dragThresholdExceeded && delta.magnitude >= DragThresholdPx)
                _dragThresholdExceeded = true;

            if (!_dragThresholdExceeded) return;

            var newDataPos = _dragStartDataPos + delta / SkillTreeRenderer.UnitToPixelScale;
            float newLeft = newDataPos.x * SkillTreeRenderer.UnitToPixelScale - SkillTreeRenderer.NodeHalfSize;
            float newTop = newDataPos.y * SkillTreeRenderer.UnitToPixelScale - SkillTreeRenderer.NodeHalfSize;

            element.style.left = newLeft;
            element.style.top = newTop;

            evt.StopPropagation();
        }

        private void OnPointerUp(PointerUpEvent evt, SkillTreeNodeElement element, int nodeIndex)
        {
            if (_dragNodeIndex != nodeIndex) return;

            element.ReleasePointer(evt.pointerId);

            if (!_dragThresholdExceeded)
            {
                CycleNodeState(nodeIndex);
            }
            else
            {
                float finalLeft = element.style.left.value.value;
                float finalTop = element.style.top.value.value;

                var newDataPos = new Vector2(
                    (finalLeft + SkillTreeRenderer.NodeHalfSize) / SkillTreeRenderer.UnitToPixelScale,
                    (finalTop + SkillTreeRenderer.NodeHalfSize) / SkillTreeRenderer.UnitToPixelScale);

                WriteNodePositionWithUndo(nodeIndex, newDataPos);
            }

            _dragNodeIndex = -1;
            _dragThresholdExceeded = false;
            evt.StopPropagation();
        }

        private void RefreshEdges()
        {
            if (_renderer == null || _edgeLayer == null || _data == null) return;
            var statesList = BuildStatesList();
            var idToIndexMap = BuildIdToIndexMap();
            _renderer.RenderEdges(_data, idToIndexMap, statesList, _edgeLayer);
        }

        private List<SkillTreeNodeVisualState> BuildStatesList()
        {
            var states = new List<SkillTreeNodeVisualState>(_data.Nodes.Count);
            for (var i = 0; i < _data.Nodes.Count; i++)
            {
                states.Add(_nodeStates.TryGetValue(i, out var s) ? s : SkillTreeNodeVisualState.Locked);
            }
            return states;
        }

        private Dictionary<int, int> BuildIdToIndexMap()
        {
            var map = new Dictionary<int, int>(_data.Nodes.Count);
            for (var i = 0; i < _data.Nodes.Count; i++)
                map[_data.Nodes[i].id] = i;
            return map;
        }
    }
}
