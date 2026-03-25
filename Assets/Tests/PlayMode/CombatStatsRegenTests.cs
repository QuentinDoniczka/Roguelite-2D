using System.Collections;
using NUnit.Framework;
using RogueliteAutoBattler.Combat;
using RogueliteAutoBattler.Tests;
using UnityEngine;
using UnityEngine.TestTools;

namespace RogueliteAutoBattler.Tests.PlayMode
{
    public class CombatStatsRegenTests : PlayModeTestBase
    {
        [UnityTest]
        public IEnumerator Regen_OverTime_HealsCharacter()
        {
            var charGo = Track(TestCharacterFactory.CreateCombatCharacter(
                name: "RegenChar",
                maxHp: 100,
                atk: 10,
                attackSpeed: 1f,
                regenHpPerSecond: 20f));

            var stats = charGo.GetComponent<CombatStats>();

            // Wait a frame so Awake runs.
            yield return null;

            // Deal 50 damage, bringing HP to 50.
            stats.TakeDamage(50);
            Assert.AreEqual(50, stats.CurrentHp, "HP should be 50 after taking 50 damage.");

            // Wait for regen to heal. At 20 HP/s, after 1.5s, ~30 HP should be healed.
            yield return new WaitForSeconds(1.5f);

            Assert.That(stats.CurrentHp, Is.GreaterThan(50),
                "HP should have increased from regen over time.");
            Assert.That(stats.CurrentHp, Is.LessThanOrEqualTo(100),
                "HP should not exceed MaxHp.");
        }

        [UnityTest]
        public IEnumerator Regen_StopsAtMaxHp()
        {
            var charGo = Track(TestCharacterFactory.CreateCombatCharacter(
                name: "RegenCapChar",
                maxHp: 100,
                atk: 10,
                attackSpeed: 1f,
                regenHpPerSecond: 50f));

            var stats = charGo.GetComponent<CombatStats>();

            // Wait a frame so Awake runs.
            yield return null;

            // Deal only 10 damage. At 50 HP/s regen, the 10 HP gap should be filled in ~0.2s.
            stats.TakeDamage(10);
            Assert.AreEqual(90, stats.CurrentHp, "HP should be 90 after taking 10 damage.");

            yield return new WaitForSeconds(1f);

            Assert.AreEqual(100, stats.CurrentHp,
                "HP should be clamped at MaxHp after regen fully heals.");
        }
    }
}
