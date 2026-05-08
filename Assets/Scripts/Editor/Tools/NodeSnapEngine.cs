using System;
using System.Collections.Generic;
using RogueliteAutoBattler.Data;
using UnityEngine;

namespace RogueliteAutoBattler.Editor.Tools
{
    internal static class NodeSnapEngine
    {
        public enum SnapAxis { None, X, Y, LineCardinal, LineCollinear }

        private const int InRadiusBufferCapacity = 256;

        public readonly struct SnapResult
        {
            public readonly Vector2 ResolvedPosition;
            public readonly SnapAxis SnappedAxis;
            public readonly int TargetNodeIndex;
            public readonly int SecondaryTargetNodeIndex;

            public SnapResult(Vector2 resolved, SnapAxis axis, int targetIndex)
            {
                ResolvedPosition = resolved;
                SnappedAxis = axis;
                TargetNodeIndex = targetIndex;
                SecondaryTargetNodeIndex = -1;
            }

            public SnapResult(Vector2 resolved, SnapAxis axis, int targetIndex, int secondaryTargetIndex)
            {
                ResolvedPosition = resolved;
                SnappedAxis = axis;
                TargetNodeIndex = targetIndex;
                SecondaryTargetNodeIndex = secondaryTargetIndex;
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

        public static SnapResult Resolve(
            Vector2 candidate,
            int draggedNodeIndex,
            IReadOnlyList<SkillTreeData.SkillNodeEntry> nodes,
            float legacyThresholdUnits,
            float alignmentRadiusUnits)
        {
            var legacyResult = Resolve(candidate, draggedNodeIndex, nodes, legacyThresholdUnits);
            if (legacyResult.SnappedAxis != SnapAxis.None) return legacyResult;

            if (nodes == null || alignmentRadiusUnits <= 0f) return SnapResult.NoSnap(candidate);
            if (legacyThresholdUnits <= 0f) return SnapResult.NoSnap(candidate);

            Span<int> inRadiusIndices = stackalloc int[InRadiusBufferCapacity];
            int inRadiusCount = CollectInRadiusIndices(candidate, draggedNodeIndex, nodes, alignmentRadiusUnits, inRadiusIndices);
            if (inRadiusCount == 0) return SnapResult.NoSnap(candidate);

            var tier1 = TryResolveCardinalAlignment(candidate, nodes, inRadiusIndices, inRadiusCount, legacyThresholdUnits);
            if (tier1.SnappedAxis != SnapAxis.None) return tier1;

            return TryResolveCollinearAlignment(candidate, nodes, inRadiusIndices, inRadiusCount, legacyThresholdUnits);
        }

        public static SnapResult Resolve(
            Vector2 candidate,
            int draggedNodeIndex,
            IReadOnlyList<SkillTreeData.SkillNodeEntry> nodes,
            float legacyThresholdUnits,
            float alignmentRadiusUnits,
            SnapResult previousSnap)
        {
            if (TryHoldPreviousSnap(candidate, draggedNodeIndex, nodes, legacyThresholdUnits, previousSnap, out var heldResult))
                return heldResult;

            return Resolve(candidate, draggedNodeIndex, nodes, legacyThresholdUnits, alignmentRadiusUnits);
        }

        private static bool TryHoldPreviousSnap(
            Vector2 candidate,
            int draggedNodeIndex,
            IReadOnlyList<SkillTreeData.SkillNodeEntry> nodes,
            float legacyThresholdUnits,
            SnapResult previousSnap,
            out SnapResult heldResult)
        {
            heldResult = SnapResult.NoSnap(candidate);
            if (previousSnap.SnappedAxis == SnapAxis.None) return false;
            if (nodes == null || legacyThresholdUnits <= 0f) return false;

            int targetIndex = previousSnap.TargetNodeIndex;
            if (targetIndex < 0 || targetIndex >= nodes.Count) return false;
            if (targetIndex == draggedNodeIndex) return false;

            if (previousSnap.SnappedAxis == SnapAxis.LineCollinear)
            {
                int secondaryIndex = previousSnap.SecondaryTargetNodeIndex;
                if (secondaryIndex < 0 || secondaryIndex >= nodes.Count) return false;
                if (secondaryIndex == draggedNodeIndex) return false;
            }

            Vector2 targetPos = nodes[targetIndex].position;

            switch (previousSnap.SnappedAxis)
            {
                case SnapAxis.X:
                {
                    if (Mathf.Abs(candidate.x - targetPos.x) >= legacyThresholdUnits) return false;
                    heldResult = new SnapResult(new Vector2(targetPos.x, candidate.y), SnapAxis.X, targetIndex);
                    return true;
                }
                case SnapAxis.Y:
                {
                    if (Mathf.Abs(candidate.y - targetPos.y) >= legacyThresholdUnits) return false;
                    heldResult = new SnapResult(new Vector2(candidate.x, targetPos.y), SnapAxis.Y, targetIndex);
                    return true;
                }
                case SnapAxis.LineCardinal:
                {
                    bool previousAxisIsX = Mathf.Abs(targetPos.x - previousSnap.ResolvedPosition.x)
                                           <= Mathf.Abs(targetPos.y - previousSnap.ResolvedPosition.y);
                    float residual = previousAxisIsX
                        ? Mathf.Abs(candidate.x - targetPos.x)
                        : Mathf.Abs(candidate.y - targetPos.y);
                    if (residual >= legacyThresholdUnits) return false;
                    Vector2 resolvedRaw = previousAxisIsX
                        ? new Vector2(targetPos.x, candidate.y)
                        : new Vector2(candidate.x, targetPos.y);
                    heldResult = new SnapResult(SkillTreeGrid.Quantize(resolvedRaw), SnapAxis.LineCardinal, targetIndex, -1);
                    return true;
                }
                case SnapAxis.LineCollinear:
                {
                    Vector2 pa = targetPos;
                    Vector2 pb = nodes[previousSnap.SecondaryTargetNodeIndex].position;
                    Vector2 ab = pb - pa;
                    float abLengthSqr = ab.x * ab.x + ab.y * ab.y;
                    if (abLengthSqr <= Mathf.Epsilon) return false;

                    Vector2 ac = candidate - pa;
                    float t = (ac.x * ab.x + ac.y * ab.y) / abLengthSqr;
                    Vector2 foot = pa + ab * t;
                    float fx = candidate.x - foot.x;
                    float fy = candidate.y - foot.y;
                    float perpResidual = Mathf.Sqrt(fx * fx + fy * fy);
                    if (perpResidual >= legacyThresholdUnits) return false;

                    Vector2 midpoint = (pa + pb) * 0.5f;
                    float midpointDistance = Vector2.Distance(candidate, midpoint);
                    Vector2 resolvedRaw = midpointDistance < legacyThresholdUnits ? midpoint : foot;
                    heldResult = new SnapResult(
                        SkillTreeGrid.Quantize(resolvedRaw),
                        SnapAxis.LineCollinear,
                        targetIndex,
                        previousSnap.SecondaryTargetNodeIndex);
                    return true;
                }
                default:
                    return false;
            }
        }

        private static int CollectInRadiusIndices(
            Vector2 candidate,
            int draggedNodeIndex,
            IReadOnlyList<SkillTreeData.SkillNodeEntry> nodes,
            float radiusUnits,
            Span<int> destination)
        {
            float radiusSqr = radiusUnits * radiusUnits;
            int count = 0;
            int capacity = destination.Length;
            for (int i = 0; i < nodes.Count; i++)
            {
                if (i == draggedNodeIndex) continue;
                if (count >= capacity) break;
                Vector2 p = nodes[i].position;
                float dx = p.x - candidate.x;
                float dy = p.y - candidate.y;
                if (dx * dx + dy * dy <= radiusSqr)
                {
                    destination[count] = i;
                    count++;
                }
            }
            return count;
        }

        private static SnapResult TryResolveCardinalAlignment(
            Vector2 candidate,
            IReadOnlyList<SkillTreeData.SkillNodeEntry> nodes,
            ReadOnlySpan<int> inRadiusIndices,
            int inRadiusCount,
            float legacyThresholdUnits)
        {
            int qualifyingCount = 0;
            int winningNeighbor = -1;
            bool winningAxisIsX = false;
            float winningResidual = float.PositiveInfinity;

            for (int k = 0; k < inRadiusCount; k++)
            {
                int neighborIndex = inRadiusIndices[k];
                Vector2 p = nodes[neighborIndex].position;
                float dx = Mathf.Abs(p.x - candidate.x);
                float dy = Mathf.Abs(p.y - candidate.y);
                float minResidual = Mathf.Min(dx, dy);
                if (minResidual >= legacyThresholdUnits) continue;

                qualifyingCount++;
                if (minResidual < winningResidual)
                {
                    winningResidual = minResidual;
                    winningNeighbor = neighborIndex;
                    winningAxisIsX = dx <= dy;
                }
            }

            if (qualifyingCount != 1 || winningNeighbor < 0) return SnapResult.NoSnap(candidate);

            Vector2 neighborPos = nodes[winningNeighbor].position;
            Vector2 resolvedRaw = winningAxisIsX
                ? new Vector2(neighborPos.x, candidate.y)
                : new Vector2(candidate.x, neighborPos.y);
            Vector2 resolvedQuantized = SkillTreeGrid.Quantize(resolvedRaw);
            return new SnapResult(resolvedQuantized, SnapAxis.LineCardinal, winningNeighbor, -1);
        }

        private static SnapResult TryResolveCollinearAlignment(
            Vector2 candidate,
            IReadOnlyList<SkillTreeData.SkillNodeEntry> nodes,
            ReadOnlySpan<int> inRadiusIndices,
            int inRadiusCount,
            float legacyThresholdUnits)
        {
            if (inRadiusCount < 2) return SnapResult.NoSnap(candidate);

            int bestI = -1, bestJ = -1;
            float bestPerpResidual = float.PositiveInfinity;
            Vector2 bestProjectedFoot = Vector2.zero;
            Vector2 bestMidpoint = Vector2.zero;

            for (int a = 0; a < inRadiusCount; a++)
            {
                int indexA = inRadiusIndices[a];
                Vector2 pa = nodes[indexA].position;
                for (int b = a + 1; b < inRadiusCount; b++)
                {
                    int indexB = inRadiusIndices[b];
                    Vector2 pb = nodes[indexB].position;
                    Vector2 ab = pb - pa;
                    float abLengthSqr = ab.x * ab.x + ab.y * ab.y;
                    if (abLengthSqr <= Mathf.Epsilon) continue;

                    Vector2 ac = candidate - pa;
                    float t = (ac.x * ab.x + ac.y * ab.y) / abLengthSqr;
                    Vector2 foot = pa + ab * t;
                    float fx = candidate.x - foot.x;
                    float fy = candidate.y - foot.y;
                    float perpResidual = Mathf.Sqrt(fx * fx + fy * fy);

                    if (perpResidual < bestPerpResidual)
                    {
                        bestPerpResidual = perpResidual;
                        bestI = indexA;
                        bestJ = indexB;
                        bestProjectedFoot = foot;
                        bestMidpoint = (pa + pb) * 0.5f;
                    }
                }
            }

            if (bestI < 0 || bestPerpResidual > legacyThresholdUnits) return SnapResult.NoSnap(candidate);

            float midpointDistance = Vector2.Distance(candidate, bestMidpoint);
            Vector2 resolvedRaw = midpointDistance < legacyThresholdUnits
                ? bestMidpoint
                : bestProjectedFoot;
            Vector2 resolvedQuantized = SkillTreeGrid.Quantize(resolvedRaw);
            return new SnapResult(resolvedQuantized, SnapAxis.LineCollinear, bestI, bestJ);
        }
    }
}
