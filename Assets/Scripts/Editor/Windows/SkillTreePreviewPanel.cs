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
        private const float ScaleFactor = 30f;
        private const float MinContainerSize = 200f;
        private const float ContainerPadding = 128f;
        private const float ContainerOffset = 64f;
        private const float NodeHalfSize = 32f;
        private const float DragThresholdPx = 4f;

        private readonly SkillTreeData _data;
        private readonly SkillNodePalette _palette;

        private VisualElement _root;
        private ScrollView _scrollView;

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
        private Vector2 _dragStartNodePosPx;
        private bool _dragThresholdExceeded;
        private readonly List<SkillTreeNodeElement> _nodeElements = new List<SkillTreeNodeElement>();

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
            _nodeElements.Clear();
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

            foreach (var element in _nodeElements)
            {
                if (element.NodeIndex == nodeIndex)
                {
                    element.SetState(next);
                    break;
                }
            }
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

            _scrollView = new ScrollView(ScrollViewMode.VerticalAndHorizontal);
            _scrollView.style.flexGrow = 1;
            _root.Add(_scrollView);

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

            var nodes = _data.Nodes;
            var (boundsMin, boundsMax) = ComputeNodeBounds(nodes);

            float containerWidth = Mathf.Max(MinContainerSize, (boundsMax.x - boundsMin.x) * ScaleFactor + ContainerPadding);
            float containerHeight = Mathf.Max(MinContainerSize, (boundsMax.y - boundsMin.y) * ScaleFactor + ContainerPadding);

            float offsetX = -boundsMin.x * ScaleFactor + ContainerOffset;
            float offsetY = -boundsMin.y * ScaleFactor + ContainerOffset;

            var container = new VisualElement();
            container.style.position = Position.Relative;
            container.style.width = containerWidth;
            container.style.height = containerHeight;
            container.style.flexShrink = 0;

            for (int i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];

                if (!_nodeStates.ContainsKey(i))
                    _nodeStates[i] = SkillTreeNodeVisualState.Locked;

                var element = new SkillTreeNodeElement(i);
                element.SetState(_nodeStates[i]);

                Color color = _palette != null
                    ? _palette.GetColor(node.colorTag)
                    : Color.white;
                element.SetColorTag(color);

                float pixelX = node.position.x * ScaleFactor + offsetX - NodeHalfSize;
                float pixelY = node.position.y * ScaleFactor + offsetY - NodeHalfSize;
                element.style.position = Position.Absolute;
                element.style.left = pixelX;
                element.style.top = pixelY;

                int capturedIndex = i;
                float capturedOffsetX = offsetX;
                float capturedOffsetY = offsetY;

                element.RegisterCallback<PointerDownEvent>(evt =>
                    OnPointerDown(evt, element, capturedIndex, capturedOffsetX, capturedOffsetY));
                element.RegisterCallback<PointerMoveEvent>(evt =>
                    OnPointerMove(evt, element, capturedIndex, capturedOffsetX, capturedOffsetY, containerWidth, containerHeight));
                element.RegisterCallback<PointerUpEvent>(evt =>
                    OnPointerUp(evt, element, capturedIndex, capturedOffsetX, capturedOffsetY));

                _nodeElements.Add(element);
                container.Add(element);
            }

            _scrollView.Add(container);
        }

        private void OnPointerDown(PointerDownEvent evt, SkillTreeNodeElement element, int nodeIndex,
            float offsetX, float offsetY)
        {
            _dragNodeIndex = nodeIndex;
            _dragStartPointerPos = evt.position;
            _dragStartNodePosPx = new Vector2(
                element.style.left.value.value,
                element.style.top.value.value);
            _dragThresholdExceeded = false;
            element.CapturePointer(evt.pointerId);
            evt.StopPropagation();
        }

        private void OnPointerMove(PointerMoveEvent evt, SkillTreeNodeElement element, int nodeIndex,
            float offsetX, float offsetY, float containerWidth, float containerHeight)
        {
            if (_dragNodeIndex != nodeIndex) return;

            Vector2 delta = (Vector2)evt.position - _dragStartPointerPos;

            if (!_dragThresholdExceeded && delta.magnitude >= DragThresholdPx)
                _dragThresholdExceeded = true;

            if (!_dragThresholdExceeded) return;

            float newLeft = Mathf.Clamp(_dragStartNodePosPx.x + delta.x, 0f, containerWidth - NodeHalfSize * 2f);
            float newTop = Mathf.Clamp(_dragStartNodePosPx.y + delta.y, 0f, containerHeight - NodeHalfSize * 2f);

            element.style.left = newLeft;
            element.style.top = newTop;

            evt.StopPropagation();
        }

        private void OnPointerUp(PointerUpEvent evt, SkillTreeNodeElement element, int nodeIndex,
            float offsetX, float offsetY)
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

                float nodeCenterPxX = finalLeft + NodeHalfSize;
                float nodeCenterPxY = finalTop + NodeHalfSize;

                Vector2 newDataPos = new Vector2(
                    (nodeCenterPxX - offsetX) / ScaleFactor,
                    (nodeCenterPxY - offsetY) / ScaleFactor);

                WriteNodePositionWithUndo(nodeIndex, newDataPos);
            }

            _dragNodeIndex = -1;
            _dragThresholdExceeded = false;
            evt.StopPropagation();
        }

        private static (Vector2 min, Vector2 max) ComputeNodeBounds(IReadOnlyList<SkillTreeData.SkillNodeEntry> nodes)
        {
            if (nodes == null || nodes.Count == 0)
                return (Vector2.zero, Vector2.zero);

            Vector2 boundsMin = nodes[0].position;
            Vector2 boundsMax = nodes[0].position;
            for (int i = 1; i < nodes.Count; i++)
            {
                boundsMin = Vector2.Min(boundsMin, nodes[i].position);
                boundsMax = Vector2.Max(boundsMax, nodes[i].position);
            }
            return (boundsMin, boundsMax);
        }
    }
}
