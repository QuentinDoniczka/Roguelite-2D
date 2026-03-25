using System.Collections;
using NUnit.Framework;
using RogueliteAutoBattler.Combat;
using RogueliteAutoBattler.Tests;
using UnityEngine;
using UnityEngine.TestTools;

namespace RogueliteAutoBattler.Tests.PlayMode
{
    public class CombatFlowTests : PlayModeTestBase
    {
        private const float AttackReachTimeout = 2f;
        private const float HitDeliveryTimeout = 2f;
        private const float ReturnHomeTimeout = 2f;
        private const float PollInterval = 0.05f;

        [UnityTest]
        public IEnumerator EnemyDies_WhenAllyDealsEnoughDamage()
        {
            // --- Arrange ---
            var (combatWorld, teamContainer, enemiesContainer, teamAnchor, enemiesAnchor) =
                TestCharacterFactory.CreateCombatArena();
            Track(combatWorld);

            var allyGo = Track(TestCharacterFactory.CreateFullCombatCharacter(
                name: "Ally",
                maxHp: 100,
                atk: 50,
                attackSpeed: 2f,
                moveSpeed: 3f,
                attackRange: 0.5f,
                parent: teamContainer,
                position: new Vector2(-1f, 0f)));

            var enemyGo = Track(TestCharacterFactory.CreateFullCombatCharacter(
                name: "Enemy",
                maxHp: 100,
                atk: 10,
                attackSpeed: 1f,
                moveSpeed: 3f,
                attackRange: 0.5f,
                parent: enemiesContainer,
                position: new Vector2(2f, 0f)));

            // Wait one frame for Awake to run on all components.
            yield return null;

            var allyMover = allyGo.GetComponent<CharacterMover>();
            var allyController = allyGo.GetComponent<CombatController>();
            var enemyMover = enemyGo.GetComponent<CharacterMover>();
            var enemyController = enemyGo.GetComponent<CombatController>();
            var enemyStats = enemyGo.GetComponent<CombatStats>();

            allyMover.HomeAnchor = teamAnchor;
            enemyMover.HomeAnchor = enemiesAnchor;

            allyController.Target = enemyGo.transform;
            enemyController.Target = allyGo.transform;

            // --- Act: wait for ally to reach attack range ---
            float elapsed = 0f;
            while (allyController.State != CombatState.Attacking && elapsed < AttackReachTimeout)
            {
                yield return new WaitForSeconds(PollInterval);
                elapsed += PollInterval;
            }

            Assert.That(allyController.State, Is.EqualTo(CombatState.Attacking),
                "Ally should have reached attack range and entered Attacking state.");

            // Manually drive two attack cycles (50 atk x 2 = 100 hp).
            // Each cycle: wait for StartAttackSwing (_waitingForHit via FixedUpdate),
            // then call OnAnimationHit to deliver the blow.
            int hitsNeeded = 2;
            for (int i = 0; i < hitsNeeded; i++)
            {
                // Wait until FixedUpdate calls StartAttackSwing (sets _waitingForHit).
                // We detect this indirectly: after StartAttackSwing, OnAnimationHit will
                // actually deal damage. Poll until hit lands.
                int hpBefore = enemyStats.CurrentHp;
                float hitElapsed = 0f;

                while (enemyStats.CurrentHp == hpBefore && !enemyStats.IsDead && hitElapsed < HitDeliveryTimeout)
                {
                    // Call OnAnimationHit each poll — it no-ops when _waitingForHit is false.
                    allyController.OnAnimationHit();
                    yield return new WaitForFixedUpdate();
                    hitElapsed += Time.fixedDeltaTime;
                }
            }

            // --- Assert ---
            Assert.That(enemyStats.IsDead, Is.True,
                "Enemy should be dead after receiving 2 hits of 50 damage each (total 100 vs 100 HP).");
        }

        [UnityTest]
        public IEnumerator AllyReturnsToHome_AfterEnemyDies()
        {
            // --- Arrange ---
            var (combatWorld, teamContainer, enemiesContainer, teamAnchor, enemiesAnchor) =
                TestCharacterFactory.CreateCombatArena();
            Track(combatWorld);

            var allyGo = Track(TestCharacterFactory.CreateFullCombatCharacter(
                name: "Ally",
                maxHp: 100,
                atk: 50,
                attackSpeed: 2f,
                moveSpeed: 3f,
                attackRange: 0.5f,
                parent: teamContainer,
                position: new Vector2(-1f, 0f)));

            var enemyGo = Track(TestCharacterFactory.CreateFullCombatCharacter(
                name: "Enemy",
                maxHp: 100,
                atk: 10,
                attackSpeed: 1f,
                moveSpeed: 3f,
                attackRange: 0.5f,
                parent: enemiesContainer,
                position: new Vector2(2f, 0f)));

            // Wait one frame for Awake.
            yield return null;

            var allyMover = allyGo.GetComponent<CharacterMover>();
            var allyController = allyGo.GetComponent<CombatController>();
            var enemyStats = enemyGo.GetComponent<CombatStats>();
            var enemyMover = enemyGo.GetComponent<CharacterMover>();
            var enemyController = enemyGo.GetComponent<CombatController>();

            allyMover.HomeAnchor = teamAnchor;
            enemyMover.HomeAnchor = enemiesAnchor;

            allyController.Target = enemyGo.transform;
            enemyController.Target = allyGo.transform;

            float enemyOriginalX = enemyGo.transform.position.x;

            // Wait for ally to enter Attacking state — this subscribes _targetStats.OnDied
            // so HandleTargetDied fires when we kill the enemy.
            float elapsed = 0f;
            while (allyController.State != CombatState.Attacking && elapsed < AttackReachTimeout)
            {
                yield return new WaitForSeconds(PollInterval);
                elapsed += PollInterval;
            }

            Assert.That(allyController.State, Is.EqualTo(CombatState.Attacking),
                "Ally must reach Attacking state before we kill the enemy (needed for OnDied subscription).");

            // --- Act: kill enemy instantly ---
            enemyStats.TakeDamage(enemyStats.MaxHp);
            Assert.That(enemyStats.IsDead, Is.True, "Enemy should be dead after taking MaxHp damage.");

            // Wait for ally to move back toward home anchor.
            yield return new WaitForSeconds(ReturnHomeTimeout);

            // --- Assert ---
            float allyX = allyGo.transform.position.x;
            float homeX = teamAnchor.position.x;
            float distToHome = Mathf.Abs(allyX - homeX);
            float distToEnemyOriginal = Mathf.Abs(allyX - enemyOriginalX);

            Assert.That(distToHome, Is.LessThan(distToEnemyOriginal),
                $"Ally (x={allyX:F2}) should be closer to home anchor (x={homeX:F2}) " +
                $"than to enemy's original position (x={enemyOriginalX:F2}).");
        }

        [UnityTest]
        public IEnumerator FindNewTarget_RetargetsWhenCurrentTargetDies()
        {
            // --- Arrange ---
            var (combatWorld, teamContainer, enemiesContainer, teamAnchor, _) =
                TestCharacterFactory.CreateCombatArena();
            Track(combatWorld);

            var allyGo = Track(TestCharacterFactory.CreateFullCombatCharacter(
                name: "Ally",
                maxHp: 100,
                atk: 50,
                attackSpeed: 2f,
                moveSpeed: 3f,
                attackRange: 0.5f,
                parent: teamContainer,
                position: new Vector2(-1f, 0f)));

            var enemy1Go = Track(TestCharacterFactory.CreateFullCombatCharacter(
                name: "Enemy1",
                maxHp: 100,
                atk: 10,
                attackSpeed: 1f,
                moveSpeed: 3f,
                attackRange: 0.5f,
                parent: enemiesContainer,
                position: new Vector2(1.5f, 0f)));

            var enemy2Go = Track(TestCharacterFactory.CreateFullCombatCharacter(
                name: "Enemy2",
                maxHp: 100,
                atk: 10,
                attackSpeed: 1f,
                moveSpeed: 3f,
                attackRange: 0.5f,
                parent: enemiesContainer,
                position: new Vector2(2.5f, 0f)));

            // Wait one frame for Awake.
            yield return null;

            var allyMover = allyGo.GetComponent<CharacterMover>();
            var allyController = allyGo.GetComponent<CombatController>();
            var enemy1Stats = enemy1Go.GetComponent<CombatStats>();

            allyMover.HomeAnchor = teamAnchor;

            // Wire retargeting delegate — find closest alive enemy.
            allyController.FindNewTarget = () =>
                TargetFinder.Closest(enemiesContainer, allyGo.transform.position);

            allyController.Target = enemy1Go.transform;

            // Wait for ally to enter Attacking state — this subscribes _targetStats.OnDied
            // so HandleTargetDied fires and invokes FindNewTarget when Enemy1 dies.
            float elapsed = 0f;
            while (allyController.State != CombatState.Attacking && elapsed < AttackReachTimeout)
            {
                yield return new WaitForSeconds(PollInterval);
                elapsed += PollInterval;
            }

            Assert.That(allyController.State, Is.EqualTo(CombatState.Attacking),
                "Ally must reach Attacking state before we kill Enemy1 (needed for OnDied subscription).");

            // --- Act: kill Enemy1 directly ---
            // HandleTargetDied fires synchronously from OnDied, calls FindNewTarget,
            // and sets _mover.Target to Enemy2.
            enemy1Stats.TakeDamage(enemy1Stats.MaxHp);

            // Wait a frame for state to settle.
            yield return null;

            // --- Assert ---
            Assert.That(allyMover.Target, Is.EqualTo(enemy2Go.transform),
                "Ally should have retargeted to Enemy2 after Enemy1 died.");
        }
    }
}
