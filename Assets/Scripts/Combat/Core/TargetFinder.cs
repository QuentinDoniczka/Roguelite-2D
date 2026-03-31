using UnityEngine;

namespace RogueliteAutoBattler.Combat.Core
{
    public static class TargetFinder
    {
        private const float ContestPenalty = 1.0f;

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

        public static bool IsAlive(Transform t)
        {
            return t != null
                   && t.TryGetComponent<CombatStats>(out var stats)
                   && !stats.IsDead;
        }
    }
}
