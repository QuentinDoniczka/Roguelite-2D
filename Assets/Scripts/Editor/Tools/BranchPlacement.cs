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

        /// <summary>
        /// Compute a branch child position offset from <paramref name="parentPosition"/>.
        /// Angle convention is clockwise from north: 0 = north, 90 = east, 180 = south, 270 = west.
        /// </summary>
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

        /// <summary>
        /// Compute the default branch angle pointing outward from the origin through
        /// <paramref name="parentPosition"/>. Returned value uses the clockwise-from-north
        /// convention (0 = north, 90 = east, 180 = south, 270 = west).
        /// </summary>
        public static float ComputeDefaultAngle(Vector2 parentPosition)
        {
            if (parentPosition.sqrMagnitude < DegenerateMagnitudeThreshold)
                return DefaultAngleForOriginParent;

            float clockwiseFromNorthDegrees = Mathf.Atan2(parentPosition.x, parentPosition.y) * Mathf.Rad2Deg;
            return NormalizeDegrees(clockwiseFromNorthDegrees);
        }

        /// <summary>
        /// Reflect <paramref name="angleDegrees"/> across the axis line defined by
        /// <paramref name="axisAngleDegrees"/>. Angles use the clockwise-from-north convention
        /// (0 = north, 90 = east, 180 = south, 270 = west). Formula:
        /// normalize(2 * normalize(axis) - normalize(angle)). Result is normalized to [0, 360).
        /// </summary>
        public static float MirrorAngle(float angleDegrees, float axisAngleDegrees)
        {
            return NormalizeDegrees(2f * NormalizeDegrees(axisAngleDegrees) - NormalizeDegrees(angleDegrees));
        }

        public static float ResolveAbsoluteAngle(float relative, float axis, bool isRelative)
        {
            if (!isRelative) return relative;
            return NormalizeDegrees(relative + axis);
        }

        internal static Vector2 ResolveMirrorSourcePosition(
            IReadOnlyList<SkillTreeData.SkillNodeEntry> nodes,
            int parentIndex,
            int sourceOverrideIndex)
        {
            if (nodes == null) return Vector2.zero;
            if (sourceOverrideIndex >= 0 && sourceOverrideIndex < nodes.Count)
                return nodes[sourceOverrideIndex].position;
            if (parentIndex >= 0 && parentIndex < nodes.Count)
                return nodes[parentIndex].position;
            return Vector2.zero;
        }

        internal static float NormalizeDegrees(float angleDegrees)
        {
            return (angleDegrees % FullCircleDegrees + FullCircleDegrees) % FullCircleDegrees;
        }
    }
}
