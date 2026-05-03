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

        public static Vector2 ReflectAcrossAxisThroughOrigin(Vector2 point, float axisAngleDegrees)
        {
            float doubleAngleRadians = axisAngleDegrees * 2f * Mathf.Deg2Rad;
            float cos2t = Mathf.Cos(doubleAngleRadians);
            float sin2t = Mathf.Sin(doubleAngleRadians);
            return new Vector2(
                point.x * cos2t + point.y * sin2t,
                point.x * sin2t - point.y * cos2t);
        }
    }
}
