using System.Collections.Generic;
using RogueliteAutoBattler.Data;
using UnityEngine;
using UnityEngine.UIElements;

namespace RogueliteAutoBattler.Editor.Windows.SkillTreeDesigner
{
    internal static class SkillTreeCanvasMesh
    {
        internal static void DrawGrid(
            Painter2D p,
            Rect viewportPx,
            float zoom,
            Vector2 panPx,
            float gridSpacingPx,
            Color gridColor)
        {
            float spacing = gridSpacingPx * zoom;
            if (spacing < 4f) return;

            p.strokeColor = gridColor;
            p.lineWidth = 1f;

            float offsetX = ((panPx.x % spacing) + spacing) % spacing;
            float offsetY = ((panPx.y % spacing) + spacing) % spacing;

            for (float x = viewportPx.xMin + offsetX; x <= viewportPx.xMax; x += spacing)
            {
                p.BeginPath();
                p.MoveTo(new Vector2(x, viewportPx.yMin));
                p.LineTo(new Vector2(x, viewportPx.yMax));
                p.Stroke();
            }

            for (float y = viewportPx.yMin + offsetY; y <= viewportPx.yMax; y += spacing)
            {
                p.BeginPath();
                p.MoveTo(new Vector2(viewportPx.xMin, y));
                p.LineTo(new Vector2(viewportPx.xMax, y));
                p.Stroke();
            }
        }

        internal static void DrawEdges(
            Painter2D p,
            IReadOnlyList<SkillTreeData.SkillNodeEntry> nodes,
            Vector2 origin,
            float unitToPx,
            Vector2 panPx,
            float zoom,
            Color edgeColor)
        {
            if (nodes == null || nodes.Count == 0) return;

            var idToPos = BuildIdToScreenPos(nodes, origin, unitToPx, panPx, zoom);

            p.strokeColor = edgeColor;
            p.lineWidth = 2f;

            foreach (var node in nodes)
            {
                if (node.connectedNodeIds == null) continue;
                if (!idToPos.TryGetValue(node.id, out var fromScreen)) continue;

                foreach (int toId in node.connectedNodeIds)
                {
                    if (!idToPos.TryGetValue(toId, out var toScreen)) continue;
                    p.BeginPath();
                    p.MoveTo(fromScreen);
                    p.LineTo(toScreen);
                    p.Stroke();
                }
            }
        }

        internal static void DrawNodes(
            Painter2D p,
            IReadOnlyList<SkillTreeData.SkillNodeEntry> nodes,
            int? selectedId,
            Vector2 origin,
            float unitToPx,
            Vector2 panPx,
            float zoom,
            float nodeRadiusPx,
            Color nodeColor,
            Color selectedRingColor)
        {
            if (nodes == null || nodes.Count == 0) return;

            float scaledRadius = nodeRadiusPx * zoom;
            float ringRadius = scaledRadius + 4f * zoom;
            float ringThickness = 3f * zoom;

            foreach (var node in nodes)
            {
                var screen = DataToScreen(node.position, origin, unitToPx, panPx, zoom);

                p.fillColor = nodeColor;
                p.BeginPath();
                p.Arc(screen, scaledRadius, 0f, 360f);
                p.Fill();

                if (node.id == selectedId)
                {
                    p.strokeColor = selectedRingColor;
                    p.lineWidth = ringThickness;
                    p.BeginPath();
                    p.Arc(screen, ringRadius, 0f, 360f);
                    p.Stroke();
                }
            }
        }

        internal static Vector2 DataToScreen(Vector2 dataPos, Vector2 origin, float unitToPx, Vector2 panPx, float zoom)
        {
            return dataPos * unitToPx * zoom + panPx + origin;
        }

        private static Dictionary<int, Vector2> BuildIdToScreenPos(
            IReadOnlyList<SkillTreeData.SkillNodeEntry> nodes,
            Vector2 origin,
            float unitToPx,
            Vector2 panPx,
            float zoom)
        {
            var map = new Dictionary<int, Vector2>(nodes.Count);
            foreach (var node in nodes)
                map[node.id] = DataToScreen(node.position, origin, unitToPx, panPx, zoom);
            return map;
        }
    }
}
