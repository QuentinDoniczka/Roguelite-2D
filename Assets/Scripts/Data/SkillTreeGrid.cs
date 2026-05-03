using UnityEngine;

namespace RogueliteAutoBattler.Data
{
    public static class SkillTreeGrid
    {
        public const float Step = 0.01f;
        private const float DisplayMultiplier = 1f / Step;

        public static Vector2 Quantize(Vector2 v) =>
            new(Mathf.Round(v.x / Step) * Step, Mathf.Round(v.y / Step) * Step);

        public static (int x, int y) ToDisplay(Vector2 v) =>
            (Mathf.RoundToInt(v.x * DisplayMultiplier), Mathf.RoundToInt(v.y * DisplayMultiplier));

        public static int DistanceDisplay(Vector2 a, Vector2 b) =>
            Mathf.RoundToInt(Vector2.Distance(a, b) * DisplayMultiplier);
    }
}
