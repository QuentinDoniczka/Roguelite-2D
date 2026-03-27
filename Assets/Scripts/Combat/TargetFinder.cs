using UnityEngine;

namespace RogueliteAutoBattler.Combat
{
    /// <summary>
    /// Static utility for finding targets in a container by various criteria.
    /// Designed to be extensible for AI targeting (focus healer, lowest HP, etc.).
    /// </summary>
    public static class TargetFinder
    {
        private const float ContestPenalty = 1.0f;

        /// <summary>
        /// Finds the closest alive target on the X axis (side-scroller distance).
        /// <paramref name="maxWorldX"/> filters out targets whose world X position exceeds the limit.
        /// </summary>
        public static Transform Closest(Transform container, Vector3 from, float maxRange = float.MaxValue, float maxWorldX = float.MaxValue)
        {
            if (container == null)
                return null;

            Transform best = null;
            float bestDist = float.MaxValue;

            foreach (Transform child in container)
            {
                if (!IsAlive(child))
                    continue;

                if (child.position.x > maxWorldX)
                    continue;

                float dist = Vector2.Distance(from, child.position);
                if (dist < bestDist && dist <= maxRange)
                {
                    bestDist = dist;
                    best = child;
                }
            }

            return best;
        }

        /// <summary>
        /// Finds the least contested alive target, weighting by distance and attacker count.
        /// Prefers targets with fewer attackers. Falls back to closest when attacker counts are equal.
        /// </summary>
        public static Transform LeastContested(
            Transform container,
            Vector3 from,
            float maxRange = float.MaxValue,
            float maxWorldX = float.MaxValue)
        {
            if (container == null)
                return null;

            Transform best = null;
            float bestScore = float.MaxValue;

            foreach (Transform child in container)
            {
                if (!IsAlive(child))
                    continue;

                if (child.position.x > maxWorldX)
                    continue;

                float dist = Vector2.Distance(from, child.position);
                if (dist > maxRange)
                    continue;

                float score = dist + AttackSlotRegistry.AttackerCount(child) * ContestPenalty;
                if (score < bestScore)
                {
                    bestScore = score;
                    best = child;
                }
            }

            return best;
        }

        /// <summary>
        /// Finds the alive target with the lowest current HP in the given container.
        /// Useful for focus-fire strategies.
        /// </summary>
        public static Transform LowestHp(Transform container)
        {
            if (container == null)
                return null;

            Transform best = null;
            int bestHp = int.MaxValue;

            foreach (Transform child in container)
            {
                if (!child.TryGetComponent<CombatStats>(out var stats) || stats.IsDead)
                    continue;

                if (stats.CurrentHp < bestHp)
                {
                    bestHp = stats.CurrentHp;
                    best = child;
                }
            }

            return best;
        }

        /// <summary>
        /// Finds the alive target with the highest current HP in the given container.
        /// Useful for tank-targeting strategies.
        /// </summary>
        public static Transform HighestHp(Transform container)
        {
            if (container == null)
                return null;

            Transform best = null;
            int bestHp = int.MinValue;

            foreach (Transform child in container)
            {
                if (!child.TryGetComponent<CombatStats>(out var stats) || stats.IsDead)
                    continue;

                if (stats.CurrentHp > bestHp)
                {
                    bestHp = stats.CurrentHp;
                    best = child;
                }
            }

            return best;
        }

        /// <summary>
        /// Returns true if the Transform has a CombatStats component that is not dead.
        /// </summary>
        public static bool IsAlive(Transform t)
        {
            return t != null
                   && t.TryGetComponent<CombatStats>(out var stats)
                   && !stats.IsDead;
        }
    }
}
