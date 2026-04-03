using System;
using RogueliteAutoBattler.Combat.Core;
using UnityEngine;

namespace RogueliteAutoBattler.Combat.Levels
{
    internal class AllyTargetManager
    {
        private readonly Transform _teamContainer;
        private readonly Transform _enemiesContainer;
        private readonly Func<float> _combatZoneXProvider;

        internal AllyTargetManager(
            Transform teamContainer,
            Transform enemiesContainer,
            Func<float> combatZoneXProvider)
        {
            _teamContainer = teamContainer;
            _enemiesContainer = enemiesContainer;
            _combatZoneXProvider = combatZoneXProvider;
        }

        internal void AssignAllyTargetsInZone()
        {
            if (_teamContainer == null || _enemiesContainer == null) return;

            float combatZoneX = _combatZoneXProvider();

            for (int i = 0; i < _teamContainer.childCount; i++)
            {
                var ally = _teamContainer.GetChild(i);
                if (!ally.TryGetComponent<CombatController>(out var controller)) continue;
                if (controller.Target != null) continue;
                if (!ally.TryGetComponent<CombatStats>(out var stats) || stats.IsDead) continue;

                Transform target = TargetFinder.LeastContested(_enemiesContainer, ally.position, float.MaxValue, combatZoneX);
                if (target != null)
                {
                    controller.Target = target;
                    WireAllyRetarget(ally);
                }
            }
        }

        internal void SetAllyTarget(Transform firstEnemy)
        {
            if (_teamContainer == null)
                return;

            float combatZoneX = _combatZoneXProvider();

            for (int i = 0; i < _teamContainer.childCount; i++)
            {
                var allyTransform = _teamContainer.GetChild(i);
                if (!allyTransform.TryGetComponent<CombatStats>(out var allyStats) || allyStats.IsDead)
                    continue;

                if (!allyTransform.TryGetComponent<CharacterMover>(out var allyMover))
                    continue;

                bool needsTarget = allyMover.Target == null;
                if (!needsTarget && allyMover.Target.TryGetComponent<CombatStats>(out var targetStats))
                    needsTarget = targetStats.IsDead;

                if (needsTarget)
                {
                    bool inZone = firstEnemy.position.x <= combatZoneX;
                    if (inZone && allyTransform.TryGetComponent<CombatController>(out var allyController))
                        allyController.Target = firstEnemy;
                }

                WireAllyRetarget(allyTransform);
            }
        }

        internal void ClearAllyTargets() => DisengageAll(_teamContainer);

        internal void ClearEnemyTargets() => DisengageAll(_enemiesContainer);

        private void WireAllyRetarget(Transform ally)
        {
            if (!ally.TryGetComponent<CombatController>(out var controller)) return;
            if (controller.FindNewTarget != null) return;

            var allyRef = ally;
            controller.FindNewTarget = () => TargetFinder.LeastContested(_enemiesContainer, allyRef.position, float.MaxValue, _combatZoneXProvider());
        }

        private static void DisengageAll(Transform container)
        {
            if (container == null) return;

            for (int i = 0; i < container.childCount; i++)
            {
                var child = container.GetChild(i);
                if (child.TryGetComponent<CombatController>(out var controller))
                {
                    controller.Disengage();
                }
            }
        }
    }
}
