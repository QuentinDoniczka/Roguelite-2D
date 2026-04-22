using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace RogueliteAutoBattler.UI.Toolkit.SkillTree
{
    public sealed class SkillTreeEdgeLayer : VisualElement
    {
        private const string LayerClassName = "skill-tree-edge-layer";
        private const float DefaultEdgeThickness = 3f;

        private static readonly Color EdgeColorInactive = new Color(0.3f, 0.3f, 0.3f, 0.6f);
        private static readonly Color EdgeColorActive = new Color(0.6f, 0.85f, 0.3f, 1f);

        private readonly List<EdgeSpec> _edges = new();
        private float _unitToPixelScale = 1f;
        private float _edgeThickness = DefaultEdgeThickness;

        public SkillTreeEdgeLayer()
        {
            AddToClassList(LayerClassName);
            pickingMode = PickingMode.Ignore;
            generateVisualContent += OnGenerateVisualContent;
        }

        public int EdgeCount => _edges.Count;

        public void SetEdges(
            IReadOnlyList<(int fromId, int toId)> edges,
            IReadOnlyList<Vector2> nodePositionsByIndex,
            IReadOnlyList<int> nodeIds,
            IReadOnlyList<SkillTreeNodeVisualState> nodeStatesByIndex,
            float unitToPixelScale,
            float edgeThickness = DefaultEdgeThickness)
        {
            _edges.Clear();
            var idToIndex = BuildIdToIndexMap(nodeIds);
            for (var i = 0; i < edges.Count; i++)
            {
                var (fromId, toId) = edges[i];
                if (!idToIndex.TryGetValue(fromId, out var fromIndex)) continue;
                if (!idToIndex.TryGetValue(toId, out var toIndex)) continue;
                _edges.Add(new EdgeSpec
                {
                    From = nodePositionsByIndex[fromIndex],
                    To = nodePositionsByIndex[toIndex],
                    IsActive = IsEdgeActive(nodeStatesByIndex[fromIndex], nodeStatesByIndex[toIndex])
                });
            }
            _unitToPixelScale = unitToPixelScale;
            _edgeThickness = edgeThickness;
            MarkDirtyRepaint();
        }

        private static Dictionary<int, int> BuildIdToIndexMap(IReadOnlyList<int> nodeIds)
        {
            var map = new Dictionary<int, int>(nodeIds.Count);
            for (var i = 0; i < nodeIds.Count; i++)
                map[nodeIds[i]] = i;
            return map;
        }

        private static bool IsEdgeActive(SkillTreeNodeVisualState a, SkillTreeNodeVisualState b)
            => IsPurchasedOrMax(a) && IsPurchasedOrMax(b);

        private static bool IsPurchasedOrMax(SkillTreeNodeVisualState state)
            => state == SkillTreeNodeVisualState.Purchased || state == SkillTreeNodeVisualState.Max;

        private void OnGenerateVisualContent(MeshGenerationContext ctx)
        {
            if (_edges.Count == 0) return;
            var painter = ctx.painter2D;
            painter.lineWidth = _edgeThickness;
            for (var i = 0; i < _edges.Count; i++)
            {
                var edge = _edges[i];
                painter.strokeColor = edge.IsActive ? EdgeColorActive : EdgeColorInactive;
                painter.BeginPath();
                painter.MoveTo(edge.From * _unitToPixelScale);
                painter.LineTo(edge.To * _unitToPixelScale);
                painter.Stroke();
            }
        }

        private struct EdgeSpec
        {
            public Vector2 From;
            public Vector2 To;
            public bool IsActive;
        }
    }
}
