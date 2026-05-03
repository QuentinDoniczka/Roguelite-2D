using System.Collections.Generic;
using RogueliteAutoBattler.Data;
using UnityEngine;

namespace RogueliteAutoBattler.Editor.Tools
{
    internal static class NodeSnapEngine
    {
        public enum SnapAxis { None, X, Y }

        public readonly struct SnapResult
        {
            public readonly Vector2 ResolvedPosition;
            public readonly SnapAxis SnappedAxis;
            public readonly int TargetNodeIndex;

            public SnapResult(Vector2 resolved, SnapAxis axis, int targetIndex)
            {
                ResolvedPosition = resolved;
                SnappedAxis = axis;
                TargetNodeIndex = targetIndex;
            }

            public static SnapResult NoSnap(Vector2 position) => new SnapResult(position, SnapAxis.None, -1);
        }

        public static SnapResult Resolve(
            Vector2 candidatePosition,
            int draggedNodeIndex,
            IReadOnlyList<SkillTreeData.SkillNodeEntry> nodes,
            float thresholdUnits)
        {
            if (nodes == null || thresholdUnits <= 0f) return SnapResult.NoSnap(candidatePosition);

            int bestX = -1, bestY = -1;
            float bestDX = thresholdUnits, bestDY = thresholdUnits;
            for (int i = 0; i < nodes.Count; i++)
            {
                if (i == draggedNodeIndex) continue;
                Vector2 p = nodes[i].position;
                float dx = Mathf.Abs(p.x - candidatePosition.x);
                float dy = Mathf.Abs(p.y - candidatePosition.y);
                if (dx < bestDX) { bestDX = dx; bestX = i; }
                if (dy < bestDY) { bestDY = dy; bestY = i; }
            }

            if (bestX < 0 && bestY < 0) return SnapResult.NoSnap(candidatePosition);

            if (bestX >= 0 && bestY >= 0)
            {
                if (bestDX <= bestDY)
                    return new SnapResult(new Vector2(nodes[bestX].position.x, candidatePosition.y), SnapAxis.X, bestX);
                return new SnapResult(new Vector2(candidatePosition.x, nodes[bestY].position.y), SnapAxis.Y, bestY);
            }

            if (bestX >= 0)
                return new SnapResult(new Vector2(nodes[bestX].position.x, candidatePosition.y), SnapAxis.X, bestX);
            return new SnapResult(new Vector2(candidatePosition.x, nodes[bestY].position.y), SnapAxis.Y, bestY);
        }
    }
}
