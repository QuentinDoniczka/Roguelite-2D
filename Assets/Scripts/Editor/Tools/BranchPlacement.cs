using UnityEngine;

namespace RogueliteAutoBattler.Editor.Tools
{
    internal static class BranchPlacement
    {
        private const float DegenerateMagnitudeThreshold = 1e-6f;
        private const float FullCircleDegrees = 360f;
        private const float DefaultAngleForOriginParent = 0f;

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
            float radians = clockwiseFromNorthDegrees * Mathf.Deg2Rad;
            Vector2 direction = new Vector2(Mathf.Sin(radians), Mathf.Cos(radians));
            return parentPosition + direction * distance;
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
            return (clockwiseFromNorthDegrees % FullCircleDegrees + FullCircleDegrees) % FullCircleDegrees;
        }
    }
}
