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
    }
}
