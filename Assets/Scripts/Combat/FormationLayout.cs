using UnityEngine;

namespace RogueliteAutoBattler.Combat
{
    /// <summary>
    /// Calculates formation positions for a group of units.
    /// Returns offsets relative to an anchor point.
    /// </summary>
    public static class FormationLayout
    {
        private const float DefaultYSpacing = 0.5f;
        private const float DefaultColumnSpacing = 0.5f;
        private const int MaxPerColumn = 5;

        /// <summary>
        /// Returns world-space positions for <paramref name="count"/> units.
        /// <paramref name="facingRight"/>: true for allies (back column at -X), false for enemies (back column at +X).
        /// </summary>
        public static Vector2[] GetPositions(
            Vector2 anchor,
            int count,
            bool facingRight,
            float ySpacing = DefaultYSpacing,
            float columnSpacing = DefaultColumnSpacing)
        {
            if (count <= 0) return System.Array.Empty<Vector2>();

            var positions = new Vector2[count];

            if (count <= MaxPerColumn)
            {
                // Single column
                FillColumn(positions, 0, count, anchor.x, anchor.y, ySpacing);
            }
            else
            {
                // Two columns
                int frontCount = Mathf.CeilToInt(count / 2f);
                int backCount = count - frontCount;

                float backOffsetX = facingRight ? -columnSpacing : columnSpacing;

                // Front column at anchor.x
                FillColumn(positions, 0, frontCount, anchor.x, anchor.y, ySpacing);
                // Back column offset on X
                FillColumn(positions, frontCount, backCount, anchor.x + backOffsetX, anchor.y, ySpacing);
            }

            return positions;
        }

        private static void FillColumn(Vector2[] positions, int startIndex, int count, float centerX, float centerY, float ySpacing)
        {
            float totalHeight = (count - 1) * ySpacing;
            float startY = centerY + totalHeight * 0.5f;

            for (int i = 0; i < count; i++)
            {
                positions[startIndex + i] = new Vector2(centerX, startY - i * ySpacing);
            }
        }
    }
}
