using System.Collections;
using NUnit.Framework;
using RogueliteAutoBattler.Combat;
using RogueliteAutoBattler.Tests;
using UnityEngine;
using UnityEngine.TestTools;

namespace RogueliteAutoBattler.Tests.PlayMode
{
    public class CombatControllerTests : PlayModeTestBase
    {
        [UnityTest]
        public IEnumerator EnteringAttackState_StopsMovementAndGoesIdle()
        {
            // Arrange — enemy close enough to enter Attacking immediately.
            var enemyGo = Track(TestCharacterFactory.CreateFullCombatCharacter(
                "Enemy", maxHp: 100, position: new Vector2(0.3f, 0f)));
            var heroGo = Track(TestCharacterFactory.CreateFullCombatCharacter(
                "Hero", maxHp: 100, atk: 10, attackSpeed: 1f, moveSpeed: 2f,
                position: new Vector2(0f, 0f)));

            yield return null;

            var heroController = heroGo.GetComponent<CombatController>();
            var heroMover = heroGo.GetComponent<CharacterMover>();
            var heroRb = heroGo.GetComponent<Rigidbody2D>();
            heroController.SetAttackRange(0.5f);
            heroController.Target = enemyGo.transform;

            // Wait for hero to enter Attacking state.
            float timeout = 2f;
            float elapsed = 0f;
            while (heroController.State != CombatState.Attacking && elapsed < timeout)
            {
                yield return new WaitForFixedUpdate();
                elapsed += Time.fixedDeltaTime;
            }

            Assert.AreEqual(CombatState.Attacking, heroController.State,
                "Hero should be in Attacking state (enemy within attack range).");

            // Assert — mover is disabled and velocity is zero (character stopped).
            Assert.IsFalse(heroMover.enabled,
                "CharacterMover should be disabled in Attacking state.");
            Assert.That(heroRb.linearVelocity.magnitude, Is.LessThan(0.1f),
                "Character velocity should be near zero in Attacking state.");
        }

        [UnityTest]
        public IEnumerator MovingState_SharedTargetDies_Retargets()
        {
            // Arrange — hero far from enemy A, so state will be Moving.
            var enemyA = Track(TestCharacterFactory.CreateFullCombatCharacter(
                "EnemyA", maxHp: 1, position: new Vector2(5f, 0f)));
            var enemyB = Track(TestCharacterFactory.CreateFullCombatCharacter(
                "EnemyB", maxHp: 100, position: new Vector2(6f, 0f)));
            var heroGo = Track(TestCharacterFactory.CreateFullCombatCharacter(
                "Hero", maxHp: 100, atk: 10, moveSpeed: 2f, position: new Vector2(0f, 0f)));

            // Wait a frame so Awake runs on all components.
            yield return null;

            var heroController = heroGo.GetComponent<CombatController>();
            heroController.Target = enemyA.transform;
            heroController.FindNewTarget = () => enemyB.transform;

            // Let FixedUpdate run so CombatController transitions from None to Moving.
            yield return new WaitForFixedUpdate();
            yield return null;

            Assert.AreEqual(CombatState.Moving, heroController.State,
                "Sanity: hero should be in Moving state (far from enemy A).");

            // Act — another source kills enemy A, firing OnDied synchronously.
            LogAssert.Expect(LogType.Log, $"[CombatStats] EnemyA died!");
            enemyA.GetComponent<CombatStats>().TakeDamage(100);

            // One frame for state to settle.
            yield return null;

            // Assert — hero retargeted to enemy B and is still moving.
            Assert.AreEqual(enemyB.transform, heroController.Target,
                "Hero should have retargeted to enemy B.");
            Assert.AreEqual(CombatState.Moving, heroController.State,
                "Hero should be in Moving state toward the new target.");
        }

        [UnityTest]
        public IEnumerator AttackingState_SharedTargetDies_Retargets()
        {
            // Arrange — enemy A very close to hero so state will be Attacking.
            var enemyA = Track(TestCharacterFactory.CreateFullCombatCharacter(
                "EnemyA", maxHp: 1, position: new Vector2(0.3f, 0f)));
            var enemyB = Track(TestCharacterFactory.CreateFullCombatCharacter(
                "EnemyB", maxHp: 100, position: new Vector2(3f, 0f)));
            var heroGo = Track(TestCharacterFactory.CreateFullCombatCharacter(
                "Hero", maxHp: 100, atk: 10, moveSpeed: 2f, position: new Vector2(0f, 0f)));

            // Wait a frame so Awake runs on all components.
            yield return null;

            var heroController = heroGo.GetComponent<CombatController>();
            heroController.SetAttackRange(0.5f);
            heroController.Target = enemyA.transform;
            heroController.FindNewTarget = () => enemyB.transform;

            // Wait for hero to enter Attacking state. May take several physics frames
            // because CircleCollider2D pushback can temporarily separate the characters.
            float timeout = 2f;
            float elapsed = 0f;
            while (heroController.State != CombatState.Attacking && elapsed < timeout)
            {
                yield return new WaitForFixedUpdate();
                elapsed += Time.fixedDeltaTime;
            }

            Assert.AreEqual(CombatState.Attacking, heroController.State,
                "Sanity: hero should be in Attacking state (enemy A within attack range).");

            // Act — another source kills enemy A, firing OnDied synchronously.
            LogAssert.Expect(LogType.Log, $"[CombatStats] EnemyA died!");
            enemyA.GetComponent<CombatStats>().TakeDamage(100);

            // One frame for state to settle.
            yield return null;

            // Assert — hero retargeted to enemy B and switched to Moving.
            Assert.AreEqual(enemyB.transform, heroController.Target,
                "Hero should have retargeted to enemy B.");
            Assert.AreEqual(CombatState.Moving, heroController.State,
                "Hero should be in Moving state toward the new (distant) target.");
        }
    }
}
