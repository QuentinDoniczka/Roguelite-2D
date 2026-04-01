using UnityEngine;

namespace RogueliteAutoBattler.Combat.Core
{
    public static class FormationLayout
    {
        private const float DefaultYSpacing = 0.5f;
        private const float DefaultColumnSpacing = 0.5f;
        private const int MaxPerColumn = 5;

        public static Vector2[] GetPositions(
            Vector2 anchor,
            int count,
            bool facingRight,
            float ySpacing = DefaultYSpacing,
            float columnSpacing = DefaultColumnSpacing,
            float characterScale = 1f)
        {
            if (count <= 0) return System.Array.Empty<Vector2>();

            float scaledYSpacing = ySpacing * characterScale;
            float scaledColumnSpacing = columnSpacing * characterScale;

            var positions = new Vector2[count];

            if (count <= MaxPerColumn)
            {
                FillColumn(positions, 0, count, anchor.x, anchor.y, scaledYSpacing);
            }
            else
            {
                int frontCount = Mathf.CeilToInt(count / 2f);
                int backCount = count - frontCount;

                float backOffsetX = facingRight ? -scaledColumnSpacing : scaledColumnSpacing;

                FillColumn(positions, 0, frontCount, anchor.x, anchor.y, scaledYSpacing);
                FillColumn(positions, frontCount, backCount, anchor.x + backOffsetX, anchor.y, scaledYSpacing);
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
