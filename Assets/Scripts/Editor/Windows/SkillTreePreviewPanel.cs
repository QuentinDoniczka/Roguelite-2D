using RogueliteAutoBattler.Data;
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
        private const string MainStylePath = "Assets/UI/Styles/MainStyle.uss";

        private readonly SkillTreeData _data;
        private readonly SkillNodePalette _palette;

        private VisualElement _root;
        private ScrollView _scrollView;

        private static readonly SkillTreeNodeVisualState[] AllStates =
        {
            SkillTreeNodeVisualState.Locked,
            SkillTreeNodeVisualState.Available,
            SkillTreeNodeVisualState.Purchased,
            SkillTreeNodeVisualState.Max
        };

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

            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(MainStylePath);
            if (styleSheet != null)
                _root.styleSheets.Add(styleSheet);

            _scrollView = new ScrollView(ScrollViewMode.Vertical);
            _scrollView.style.flexGrow = 1;
            _root.Add(_scrollView);

            BuildRows();

            return _root;
        }

        internal void Rebuild()
        {
            if (_root == null) return;
            _root.Clear();

            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(MainStylePath);
            if (styleSheet != null)
                _root.styleSheets.Add(styleSheet);

            _scrollView = new ScrollView(ScrollViewMode.Vertical);
            _scrollView.style.flexGrow = 1;
            _root.Add(_scrollView);

            BuildRows();
        }

        internal void SetVisible(bool visible)
        {
            if (_root == null) return;
            _root.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void BuildRows()
        {
            if (_data == null) return;

            var nodes = _data.Nodes;
            Vector2 boundsMin = Vector2.zero;
            Vector2 boundsMax = Vector2.zero;

            if (nodes.Count > 0)
            {
                boundsMin = nodes[0].position;
                boundsMax = nodes[0].position;
                for (int i = 1; i < nodes.Count; i++)
                {
                    boundsMin = Vector2.Min(boundsMin, nodes[i].position);
                    boundsMax = Vector2.Max(boundsMax, nodes[i].position);
                }
            }

            float containerWidth = Mathf.Max(200f, (boundsMax.x - boundsMin.x) * ScaleFactor + 128f);
            float containerHeight = Mathf.Max(MinRowHeight, (boundsMax.y - boundsMin.y) * ScaleFactor + 128f);

            foreach (var state in AllStates)
            {
                var row = BuildRow(state, nodes, boundsMin, containerWidth, containerHeight);
                _scrollView.Add(row);
            }
        }

        private VisualElement BuildRow(
            SkillTreeNodeVisualState state,
            System.Collections.Generic.IReadOnlyList<SkillTreeData.SkillNodeEntry> nodes,
            Vector2 boundsMin,
            float containerWidth,
            float containerHeight)
        {
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center;
            row.style.minHeight = MinRowHeight;
            row.style.borderBottomWidth = 1f;
            row.style.borderBottomColor = new StyleColor(new Color(0.3f, 0.3f, 0.3f, 1f));

            var header = new Label(state.ToString());
            header.style.width = 80f;
            header.style.flexShrink = 0;
            header.style.paddingLeft = 8f;
            header.style.unityFontStyleAndWeight = FontStyle.Bold;
            row.Add(header);

            var container = new VisualElement();
            container.style.position = Position.Relative;
            container.style.width = containerWidth;
            container.style.height = containerHeight;
            container.style.flexShrink = 0;

            float offsetX = -boundsMin.x * ScaleFactor + 64f;
            float offsetY = -boundsMin.y * ScaleFactor + 64f;

            for (int i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                var element = new SkillTreeNodeElement(i);
                element.SetState(state);

                Color color = _palette != null
                    ? _palette.GetColor(node.colorTag)
                    : Color.white;
                element.SetColorTag(color);

                var adjustedPosition = new Vector2(node.position.x * ScaleFactor + offsetX, node.position.y * ScaleFactor + offsetY);
                element.style.position = Position.Absolute;
                element.style.left = adjustedPosition.x - 32f;
                element.style.top = adjustedPosition.y - 32f;

                container.Add(element);
            }

            row.Add(container);
            return row;
        }
    }
}
