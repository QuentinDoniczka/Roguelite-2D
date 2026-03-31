using System.Collections;
using NUnit.Framework;
using RogueliteAutoBattler.Combat.Core;
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

            float timeout = 2f;
            float elapsed = 0f;
            while (heroController.State != CombatState.Attacking && elapsed < timeout)
            {
                yield return new WaitForFixedUpdate();
                elapsed += Time.fixedDeltaTime;
            }

            Assert.AreEqual(CombatState.Attacking, heroController.State,
                "Hero should be in Attacking state (enemy within attack range).");

            Assert.IsFalse(heroMover.enabled,
                "CharacterMover should be disabled in Attacking state.");
            Assert.That(heroRb.linearVelocity.magnitude, Is.LessThan(0.1f),
                "Character velocity should be near zero in Attacking state.");
        }

        [UnityTest]
        public IEnumerator MovingState_SharedTargetDies_Retargets()
        {
            var enemyA = Track(TestCharacterFactory.CreateFullCombatCharacter(
                "EnemyA", maxHp: 1, position: new Vector2(5f, 0f)));
            var enemyB = Track(TestCharacterFactory.CreateFullCombatCharacter(
                "EnemyB", maxHp: 100, position: new Vector2(6f, 0f)));
            var heroGo = Track(TestCharacterFactory.CreateFullCombatCharacter(
                "Hero", maxHp: 100, atk: 10, moveSpeed: 2f, position: new Vector2(0f, 0f)));

            yield return null;

            var heroController = heroGo.GetComponent<CombatController>();
            heroController.Target = enemyA.transform;
            heroController.FindNewTarget = () => enemyB.transform;

            yield return new WaitForFixedUpdate();
            yield return null;

            Assert.AreEqual(CombatState.Moving, heroController.State,
                "Sanity: hero should be in Moving state (far from enemy A).");

            LogAssert.Expect(LogType.Log, $"[CombatStats] EnemyA died!");
            enemyA.GetComponent<CombatStats>().TakeDamage(100);

            yield return null;

            Assert.AreEqual(enemyB.transform, heroController.Target,
                "Hero should have retargeted to enemy B.");
            Assert.AreEqual(CombatState.Moving, heroController.State,
                "Hero should be in Moving state toward the new target.");
        }

        [UnityTest]
        public IEnumerator AttackingState_SharedTargetDies_Retargets()
        {
            var enemyA = Track(TestCharacterFactory.CreateFullCombatCharacter(
                "EnemyA", maxHp: 1, position: new Vector2(0.3f, 0f)));
            var enemyB = Track(TestCharacterFactory.CreateFullCombatCharacter(
                "EnemyB", maxHp: 100, position: new Vector2(5f, 0f)));
            var heroGo = Track(TestCharacterFactory.CreateFullCombatCharacter(
                "Hero", maxHp: 100, atk: 10, moveSpeed: 2f, position: new Vector2(0f, 0f)));

            yield return null;

            var heroController = heroGo.GetComponent<CombatController>();
            heroController.SetAttackRange(1.5f);
            heroController.Target = enemyA.transform;
            heroController.FindNewTarget = () => enemyB.transform;

            float timeout = 2f;
            float elapsed = 0f;
            while (heroController.State != CombatState.Attacking && elapsed < timeout)
            {
                yield return new WaitForFixedUpdate();
                elapsed += Time.fixedDeltaTime;
            }

            Assert.AreEqual(CombatState.Attacking, heroController.State,
                "Sanity: hero should be in Attacking state (enemy A within attack range).");

            LogAssert.Expect(LogType.Log, $"[CombatStats] EnemyA died!");
            enemyA.GetComponent<CombatStats>().TakeDamage(100);

            yield return null;

            Assert.AreEqual(enemyB.transform, heroController.Target,
                "Hero should have retargeted to enemy B.");
            Assert.AreEqual(CombatState.Moving, heroController.State,
                "Hero should be in Moving state toward the new (distant) target.");
        }
    }
}
