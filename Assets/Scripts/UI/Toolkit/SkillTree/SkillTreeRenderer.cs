using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using RogueliteAutoBattler.Data;

namespace RogueliteAutoBattler.UI.Toolkit.SkillTree
{
    public sealed class SkillTreeRenderer
    {
        public const float UnitToPixelScale = 40f;
        public const float NodeHalfSize = 32f;

        private readonly SkillNodePalette _palette;
        private readonly List<SkillTreeNodeElement> _nodeElements = new();
        private readonly List<Vector2> _positionsCache = new();
        private readonly List<SkillTreeNodeVisualState> _statesCache = new();

        public SkillTreeRenderer(SkillNodePalette palette)
        {
            _palette = palette;
        }

        public IReadOnlyList<SkillTreeNodeElement> NodeElements => _nodeElements;

        public void RenderNodes(
            SkillTreeData data,
            IReadOnlyList<SkillTreeNodeVisualState> statesByIndex,
            VisualElement nodesLayer,
            Action<int> onNodeClicked = null)
        {
            nodesLayer.Clear();
            _nodeElements.Clear();
            bool hasPalette = _palette != null;
            for (var i = 0; i < data.Nodes.Count; i++)
            {
                var node = data.Nodes[i];
                var nodeElement = new SkillTreeNodeElement(i);
                nodeElement.SetDataPosition(node.position, UnitToPixelScale);
                var color = hasPalette ? _palette.GetColor(node.colorTag) : Color.white;
                nodeElement.SetColorTag(color);
                nodeElement.SetState(statesByIndex[i]);
                if (onNodeClicked != null) nodeElement.Clicked += onNodeClicked;
                nodesLayer.Add(nodeElement);
                _nodeElements.Add(nodeElement);
            }
        }

        public void RenderEdges(
            SkillTreeData data,
            IReadOnlyDictionary<int, int> idToIndexMap,
            IReadOnlyList<SkillTreeNodeVisualState> statesByIndex,
            SkillTreeEdgeLayer edgeLayer)
        {
            var edges = data.GetEdges();
            _positionsCache.Clear();
            _statesCache.Clear();
            for (var i = 0; i < data.Nodes.Count; i++)
            {
                _positionsCache.Add(data.Nodes[i].position);
                _statesCache.Add(statesByIndex[i]);
            }
            edgeLayer.SetEdges(edges, _positionsCache, idToIndexMap, _statesCache, UnitToPixelScale);
        }
    }
}
