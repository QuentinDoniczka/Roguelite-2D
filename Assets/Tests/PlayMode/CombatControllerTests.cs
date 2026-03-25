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

            // Let FixedUpdate run so CombatController transitions to Attacking (distance 0.3 <= 0.5).
            yield return new WaitForFixedUpdate();
            yield return null;

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
