using System.Collections;
using NUnit.Framework;
using RogueliteAutoBattler.Combat.Core;
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

            yield return null;

            stats.TakeDamage(50);
            Assert.AreEqual(50, stats.CurrentHp, "HP should be 50 after taking 50 damage.");

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

            yield return null;

            stats.TakeDamage(10);
            Assert.AreEqual(90, stats.CurrentHp, "HP should be 90 after taking 10 damage.");

            yield return new WaitForSeconds(1f);

            Assert.AreEqual(100, stats.CurrentHp,
                "HP should be clamped at MaxHp after regen fully heals.");
        }

        [UnityTest]
        public IEnumerator OnHealed_FiresWhenRegenApplies()
        {
            var charGo = Track(TestCharacterFactory.CreateCombatCharacter(
                name: "OnHealedChar",
                maxHp: 100,
                atk: 10,
                attackSpeed: 1f,
                regenHpPerSecond: 20f));

            var stats = charGo.GetComponent<CombatStats>();

            yield return null;

            stats.TakeDamage(50);

            int fireCount = 0;
            stats.OnHealed += (heal, currentHp) => fireCount++;

            yield return new WaitForSeconds(1.5f);

            Assert.That(fireCount, Is.GreaterThan(0),
                "OnHealed should have fired at least once during regen.");
        }

        [UnityTest]
        public IEnumerator OnHealed_FiresWithCorrectValues()
        {
            var charGo = Track(TestCharacterFactory.CreateCombatCharacter(
                name: "OnHealedValuesChar",
                maxHp: 100,
                atk: 10,
                attackSpeed: 1f,
                regenHpPerSecond: 20f));

            var stats = charGo.GetComponent<CombatStats>();

            yield return null;

            stats.TakeDamage(50);

            int lastHealAmount = 0;
            int lastCurrentHp = 0;
            stats.OnHealed += (heal, currentHp) =>
            {
                lastHealAmount = heal;
                lastCurrentHp = currentHp;
            };

            yield return new WaitForSeconds(1.5f);

            Assert.That(lastHealAmount, Is.GreaterThan(0),
                "Heal amount should be positive.");
            Assert.AreEqual(stats.CurrentHp, lastCurrentHp,
                "Last reported currentHp should match actual CurrentHp.");
            Assert.That(lastCurrentHp, Is.GreaterThan(50),
                "CurrentHp reported by OnHealed should reflect healing above the damaged value.");
            Assert.That(lastCurrentHp, Is.LessThanOrEqualTo(100),
                "CurrentHp reported by OnHealed should not exceed MaxHp.");
        }

        [UnityTest]
        public IEnumerator OnHealed_DoesNotFireWhenHpIsAtMax()
        {
            var charGo = Track(TestCharacterFactory.CreateCombatCharacter(
                name: "OnHealedMaxHpChar",
                maxHp: 100,
                atk: 10,
                attackSpeed: 1f,
                regenHpPerSecond: 20f));

            var stats = charGo.GetComponent<CombatStats>();

            yield return null;

            Assert.AreEqual(100, stats.CurrentHp, "HP should start at max.");

            int fireCount = 0;
            stats.OnHealed += (heal, currentHp) => fireCount++;

            yield return new WaitForSeconds(1f);

            Assert.AreEqual(0, fireCount,
                "OnHealed should not fire when HP is already at max.");
        }
    }
}
