using UnityEngine;

namespace RogueliteAutoBattler.Editor.Tools
{
    internal static class MirrorAxisGeometry
    {
        public static void ComputeAxisEndpoints(
            Vector2 origin,
            float axisAngleDegrees,
            float halfSpan,
            out Vector2 start,
            out Vector2 end)
        {
            Vector2 axisDir = BranchPlacement.DirectionFromClockwiseNorthDegrees(axisAngleDegrees);
            start = origin - axisDir * halfSpan;
            end = origin + axisDir * halfSpan;
        }
    }
}
