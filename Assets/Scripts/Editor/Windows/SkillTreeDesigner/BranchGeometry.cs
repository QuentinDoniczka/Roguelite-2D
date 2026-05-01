using UnityEngine;

namespace RogueliteAutoBattler.Editor.Windows.SkillTreeDesigner
{
    // Clockwise convention, 0° = up on screen.
    // SO positions use Y-down (positive Y = lower on screen, matching UI Toolkit style.top).
    // Therefore "up on screen" = negative Y delta: x = sin(θ), y = -cos(θ).
    internal static class BranchGeometry
    {
        internal static Vector2 ComputeBranchPosition(Vector2 parent, float distance, float angleDeg)
        {
            float radians = angleDeg * Mathf.Deg2Rad;
            float dx = Mathf.Sin(radians) * distance;
            float dy = -Mathf.Cos(radians) * distance;
            return new Vector2(parent.x + dx, parent.y + dy);
        }
    }
}
