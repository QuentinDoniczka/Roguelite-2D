using UnityEngine;

namespace RogueliteAutoBattler.Editor.Tools
{
    internal static class BranchPlacement
    {
        private const float DegenerateMagnitudeThreshold = 1e-6f;

        public static Vector2 ComputeBranchPosition(Vector2 parentPosition, float distance)
        {
            Vector2 dir = parentPosition;
            if (dir.sqrMagnitude < DegenerateMagnitudeThreshold)
            {
                dir = Vector2.right;
            }
            else
            {
                dir = dir.normalized;
            }
            return parentPosition + dir * distance;
        }

        public static Vector2 ComputeBranchPosition(Vector2 parentPosition, float distance, float angleDegrees)
        {
            float rad = angleDegrees * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Sin(rad), Mathf.Cos(rad));
            return parentPosition + dir * distance;
        }

        public static float ComputeDefaultAngle(Vector2 parentPosition)
        {
            if (parentPosition.sqrMagnitude < DegenerateMagnitudeThreshold)
                return 0f;

            float angle = Mathf.Atan2(parentPosition.x, parentPosition.y) * Mathf.Rad2Deg;
            return (angle % 360f + 360f) % 360f;
        }
    }
}
