using UnityEngine;

namespace RogueliteAutoBattler.Editor.Tools
{
    internal static class AlignmentOverlayGeometry
    {
        public static float ComputeRadiusCircleScreenRadius(float radiusUnits, float unitSize, float zoom)
        {
            return radiusUnits * unitSize * zoom;
        }

        public static Vector2 ComputeRadiusCircleCenterScreen(Vector2 nodePosUnits, Vector2 origin, float scaledUnit)
        {
            return origin + nodePosUnits * scaledUnit;
        }
    }
}
