using System.Collections;
using NUnit.Framework;
using RogueliteAutoBattler.Combat.Core;
using UnityEngine.TestTools;

namespace RogueliteAutoBattler.Tests.PlayMode
{
    public class CombatStatsHealToFullTests : PlayModeTestBase
    {
        [UnityTest]
        public IEnumerator HealToFull_AfterMaxHpModifierAdded_LiftsCurrentHpToNewMax()
        {
            var charGo = Track(TestCharacterFactory.CreateCombatCharacter(
                name: "HealToFullModifierChar",
                maxHp: 100,
                atk: 10,
                attackSpeed: 1f));

            var stats = charGo.GetComponent<CombatStats>();

            yield return null;

            Assert.AreEqual(100, stats.CurrentHp, "CurrentHp should start at 100.");
            Assert.AreEqual(100, stats.MaxHp, "MaxHp should start at 100.");

            stats.AddModifier(StatType.Hp, ModifierTier.Flat, "test", 50f);

            Assert.AreEqual(150, stats.MaxHp, "MaxHp should rise to 150 after +50 Flat modifier.");
            Assert.AreEqual(100, stats.CurrentHp, "CurrentHp should remain at 100 until HealToFull is called.");

            stats.HealToFull();

            Assert.AreEqual(150, stats.MaxHp, "MaxHp should still be 150 after HealToFull.");
            Assert.AreEqual(150, stats.CurrentHp, "CurrentHp should be lifted to 150 after HealToFull.");
            Assert.AreEqual(stats.MaxHp, stats.CurrentHp, "CurrentHp should equal MaxHp after HealToFull.");
        }

        [UnityTest]
        public IEnumerator HealToFull_AtFullHealth_NoOp()
        {
            var charGo = Track(TestCharacterFactory.CreateCombatCharacter(
                name: "HealToFullNoOpChar",
                maxHp: 100,
                atk: 10,
                attackSpeed: 1f));

            var stats = charGo.GetComponent<CombatStats>();

            yield return null;

            stats.HealToFull();

            Assert.AreEqual(100, stats.CurrentHp, "CurrentHp should remain at 100.");
            Assert.AreEqual(100, stats.MaxHp, "MaxHp should remain at 100.");
            Assert.AreEqual(stats.MaxHp, stats.CurrentHp, "CurrentHp should equal MaxHp.");
        }

        [UnityTest]
        public IEnumerator HealToFull_DoesNotRaiseOnHealed()
        {
            var charGo = Track(TestCharacterFactory.CreateCombatCharacter(
                name: "HealToFullSilentChar",
                maxHp: 100,
                atk: 10,
                attackSpeed: 1f));

            var stats = charGo.GetComponent<CombatStats>();

            yield return null;

            stats.TakeDamage(50);
            Assert.AreEqual(50, stats.CurrentHp, "CurrentHp should be 50 after taking 50 damage.");

            int onHealedFireCount = 0;
            stats.OnHealed += (heal, currentHp) => onHealedFireCount++;

            stats.HealToFull();

            Assert.AreEqual(100, stats.CurrentHp, "CurrentHp should be lifted to 100 after HealToFull.");
            Assert.AreEqual(0, onHealedFireCount, "HealToFull should not raise OnHealed.");
        }
    }
}
