using UnityEngine;

namespace RogueliteAutoBattler.Combat
{
    /// <summary>
    /// Static utility for finding targets in a container by various criteria.
    /// Designed to be extensible for AI targeting (focus healer, lowest HP, etc.).
    /// </summary>
    public static class TargetFinder
    {
        /// <summary>
        /// Finds the closest alive target to <paramref name="from"/> in the given container.
        /// </summary>
        /// <summary>
        /// Finds the closest alive target on the X axis (side-scroller distance).
        /// </summary>
        public static Transform Closest(Transform container, Vector3 from)
        {
            if (container == null)
                return null;

            Transform best = null;
            float bestDist = float.MaxValue;

            foreach (Transform child in container)
            {
                if (!IsAlive(child))
                    continue;

                float dist = Mathf.Abs(child.position.x - from.x);
                if (dist < bestDist)
                {
                    bestDist = dist;
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
