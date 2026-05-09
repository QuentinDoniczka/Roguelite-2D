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
        private const float MinRowHeight = 200f;
        private const float MinContainerWidth = 200f;
        private const float ContainerPadding = 128f;
        private const float ContainerOffset = 64f;
        private const float NodeHalfSize = 32f;
        private const float HeaderWidth = 80f;
        private const float HeaderPaddingLeft = 8f;
        private const float RowBorderThickness = 1f;
        private static readonly Color RowBorderColor = new Color(0.3f, 0.3f, 0.3f, 1f);

        private static readonly SkillTreeNodeVisualState[] AllStates =
        {
            SkillTreeNodeVisualState.Locked,
            SkillTreeNodeVisualState.Available,
            SkillTreeNodeVisualState.Purchased,
            SkillTreeNodeVisualState.Max
        };

        private readonly SkillTreeData _data;
        private readonly SkillNodePalette _palette;

        private VisualElement _root;
        private ScrollView _scrollView;

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

        private void BuildContent()
        {
            AttachMainStyleSheet();

            _scrollView = new ScrollView(ScrollViewMode.Vertical);
            _scrollView.style.flexGrow = 1;
            _root.Add(_scrollView);

            BuildStateComparisonRows();
        }

        private void AttachMainStyleSheet()
        {
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(UIStylePaths.MainStyleSheet);
            if (styleSheet != null)
                _root.styleSheets.Add(styleSheet);
        }

        private void BuildStateComparisonRows()
        {
            if (_data == null) return;

            var nodes = _data.Nodes;
            var (boundsMin, boundsMax) = ComputeNodeBounds(nodes);

            float containerWidth = Mathf.Max(MinContainerWidth, (boundsMax.x - boundsMin.x) * ScaleFactor + ContainerPadding);
            float containerHeight = Mathf.Max(MinRowHeight, (boundsMax.y - boundsMin.y) * ScaleFactor + ContainerPadding);

            foreach (var state in AllStates)
            {
                var row = BuildRow(state, nodes, boundsMin, containerWidth, containerHeight);
                _scrollView.Add(row);
            }
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

        private VisualElement BuildRow(
            SkillTreeNodeVisualState state,
            IReadOnlyList<SkillTreeData.SkillNodeEntry> nodes,
            Vector2 boundsMin,
            float containerWidth,
            float containerHeight)
        {
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center;
            row.style.minHeight = MinRowHeight;
            row.style.borderBottomWidth = RowBorderThickness;
            row.style.borderBottomColor = new StyleColor(RowBorderColor);

            var header = new Label(state.ToString());
            header.style.width = HeaderWidth;
            header.style.flexShrink = 0;
            header.style.paddingLeft = HeaderPaddingLeft;
            header.style.unityFontStyleAndWeight = FontStyle.Bold;
            row.Add(header);

            var container = new VisualElement();
            container.style.position = Position.Relative;
            container.style.width = containerWidth;
            container.style.height = containerHeight;
            container.style.flexShrink = 0;

            float offsetX = -boundsMin.x * ScaleFactor + ContainerOffset;
            float offsetY = -boundsMin.y * ScaleFactor + ContainerOffset;

            for (int i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                var element = new SkillTreeNodeElement(i);
                element.SetState(state);

                Color color = _palette != null
                    ? _palette.GetColor(node.colorTag)
                    : Color.white;
                element.SetColorTag(color);

                float adjustedX = node.position.x * ScaleFactor + offsetX;
                float adjustedY = node.position.y * ScaleFactor + offsetY;
                element.style.position = Position.Absolute;
                element.style.left = adjustedX - NodeHalfSize;
                element.style.top = adjustedY - NodeHalfSize;

                container.Add(element);
            }

            row.Add(container);
            return row;
        }
    }
}
