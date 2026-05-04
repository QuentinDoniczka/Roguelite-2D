using System.Collections.Generic;
using RogueliteAutoBattler.Data;
using UnityEngine;

namespace RogueliteAutoBattler.Editor.Tools
{
    internal static class BranchPlacement
    {
        private const float DegenerateMagnitudeThreshold = 1e-6f;
        private const float FullCircleDegrees = 360f;
        private const float DefaultAngleForOriginParent = 0f;

        public const float PositionTolerance = 1f;

        public static Vector2 ComputeBranchPosition(Vector2 parentPosition, float distance)
        {
            Vector2 direction = parentPosition.sqrMagnitude < DegenerateMagnitudeThreshold
                ? Vector2.right
                : parentPosition.normalized;
            return parentPosition + direction * distance;
        }

        public static Vector2 ComputeBranchPosition(Vector2 parentPosition, float distance, float clockwiseFromNorthDegrees)
        {
            Vector2 direction = DirectionFromClockwiseNorthDegrees(clockwiseFromNorthDegrees);
            return parentPosition + direction * distance;
        }

        internal static Vector2 DirectionFromClockwiseNorthDegrees(float clockwiseFromNorthDegrees)
        {
            float radians = clockwiseFromNorthDegrees * Mathf.Deg2Rad;
            return new Vector2(Mathf.Sin(radians), Mathf.Cos(radians));
        }

        public static float ComputeDefaultAngle(Vector2 parentPosition)
        {
            if (parentPosition.sqrMagnitude < DegenerateMagnitudeThreshold)
                return DefaultAngleForOriginParent;

            float clockwiseFromNorthDegrees = Mathf.Atan2(parentPosition.x, parentPosition.y) * Mathf.Rad2Deg;
            return NormalizeDegrees(clockwiseFromNorthDegrees);
        }

        public static float MirrorAngle(float angleDegrees, float axisAngleDegrees)
        {
            return NormalizeDegrees(2f * NormalizeDegrees(axisAngleDegrees) - NormalizeDegrees(angleDegrees));
        }

        internal static (Vector2 parentPos, float resolvedAngle, float mirrorBranchAngle) ResolveBranchPlan(
            IReadOnlyList<SkillTreeData.SkillNodeEntry> nodes,
            int parentIndex,
            float angleDegrees,
            float mirrorAxisDegrees,
            bool mirrorEnabled)
        {
            if (nodes == null || parentIndex < 0 || parentIndex >= nodes.Count)
                return (Vector2.zero, 0f, 0f);

            Vector2 parentPos = nodes[parentIndex].position;
            float resolvedAngle = angleDegrees;
            float mirrorBranchAngle = mirrorEnabled ? MirrorAngle(angleDegrees, mirrorAxisDegrees) : angleDegrees;
            return (parentPos, resolvedAngle, mirrorBranchAngle);
        }

        private static float NormalizeDegrees(float angleDegrees)
        {
            return (angleDegrees % FullCircleDegrees + FullCircleDegrees) % FullCircleDegrees;
        }
    }
}
