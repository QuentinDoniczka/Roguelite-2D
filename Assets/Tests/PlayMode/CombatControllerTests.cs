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

        private const float FadeOutDuration = 0.25f;
        private const float FadeOutMarginSeconds = 0.1f;

        [UnityTest]
        public IEnumerator FadeOutAndDeactivate_DeactivatesGameObject_AfterFadeDuration()
        {
            var heroGo = Track(TestCharacterFactory.CreateFullCombatCharacter(
                "HeroToKill", maxHp: 10, position: new Vector2(0f, 0f)));

            yield return null;

            LogAssert.Expect(LogType.Log, "[CombatStats] HeroToKill died!");
            heroGo.GetComponent<CombatStats>().TakeDamage(999999);

            yield return new WaitForSeconds(FadeOutDuration + FadeOutMarginSeconds);

            Assert.IsFalse(heroGo.activeSelf,
                "GameObject should be deactivated after the fade-out duration.");
        }

        [UnityTest]
        public IEnumerator FadeOutAndDeactivate_DoesNotDestroyGameObject()
        {
            var heroGo = Track(TestCharacterFactory.CreateFullCombatCharacter(
                "HeroSurvivesDestroy", maxHp: 10, position: new Vector2(0f, 0f)));

            yield return null;

            LogAssert.Expect(LogType.Log, "[CombatStats] HeroSurvivesDestroy died!");
            heroGo.GetComponent<CombatStats>().TakeDamage(999999);

            yield return new WaitForSeconds(FadeOutDuration + FadeOutMarginSeconds);

            Assert.IsTrue(heroGo != null,
                "GameObject reference should still be valid (not Destroy'd).");
        }

        [UnityTest]
        public IEnumerator ResetFromDeath_RestoresAlphaOnAllSpriteRenderers()
        {
            var heroGo = Track(TestCharacterFactory.CreateFullCombatCharacter(
                "HeroAlphaReset", maxHp: 10, position: new Vector2(0f, 0f)));

            yield return null;

            LogAssert.Expect(LogType.Log, "[CombatStats] HeroAlphaReset died!");
            heroGo.GetComponent<CombatStats>().TakeDamage(999999);

            yield return new WaitForSeconds(FadeOutDuration + FadeOutMarginSeconds);

            heroGo.SetActive(true);
            heroGo.GetComponent<CombatController>().ResetFromDeath();

            var renderers = heroGo.GetComponentsInChildren<SpriteRenderer>(includeInactive: true);
            Assert.IsTrue(renderers.Length > 0,
                "Sanity: the test character should expose at least one SpriteRenderer.");
            foreach (var renderer in renderers)
            {
                Assert.AreEqual(1f, renderer.color.a, 0.001f,
                    $"SpriteRenderer on '{renderer.name}' should have alpha 1 after ResetFromDeath.");
            }
        }

        [UnityTest]
        public IEnumerator ResetFromDeath_ResetsStateToNone()
        {
            var heroGo = Track(TestCharacterFactory.CreateFullCombatCharacter(
                "HeroStateReset", maxHp: 10, position: new Vector2(0f, 0f)));

            yield return null;

            LogAssert.Expect(LogType.Log, "[CombatStats] HeroStateReset died!");
            heroGo.GetComponent<CombatStats>().TakeDamage(999999);

            yield return new WaitForSeconds(FadeOutDuration + FadeOutMarginSeconds);

            heroGo.SetActive(true);
            heroGo.GetComponent<CombatStats>().InitializeDirect(100, 10, 1f);
            heroGo.GetComponent<CombatController>().ResetFromDeath();

            Assert.IsFalse(heroGo.GetComponent<CombatController>().IsDead,
                "Controller should report IsDead == false after ResetFromDeath (stats re-initialized).");
            Assert.AreEqual(CombatState.None, heroGo.GetComponent<CombatController>().State,
                "Controller state should be reset to None after ResetFromDeath.");
        }

        [UnityTest]
        public IEnumerator ResetFromDeath_StopsOngoingFadeCoroutine()
        {
            var heroGo = Track(TestCharacterFactory.CreateFullCombatCharacter(
                "HeroFadeStop", maxHp: 10, position: new Vector2(0f, 0f)));

            yield return null;

            LogAssert.Expect(LogType.Log, "[CombatStats] HeroFadeStop died!");
            heroGo.GetComponent<CombatStats>().TakeDamage(999999);

            yield return new WaitForSeconds(0.05f);

            heroGo.GetComponent<CombatStats>().InitializeDirect(100, 10, 1f);
            heroGo.GetComponent<CombatController>().ResetFromDeath();

            yield return new WaitForSeconds(FadeOutDuration + FadeOutMarginSeconds);

            Assert.IsTrue(heroGo.activeSelf,
                "GameObject should still be active (fade coroutine was stopped by ResetFromDeath).");
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
