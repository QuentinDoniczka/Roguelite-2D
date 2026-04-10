using NUnit.Framework;
using RogueliteAutoBattler.Combat.Core;
using RogueliteAutoBattler.Tests.PlayMode;
using UnityEngine;

namespace RogueliteAutoBattler.Tests.EditMode
{
    [TestFixture]
    public class CombatStatsBreakdownTests : PlayModeTestBase
    {
        private CombatStats _stats;

        [SetUp]
        public void SetUp()
        {
            var go = Track(new GameObject("TestStats"));
            _stats = go.AddComponent<CombatStats>();
            _stats.InitializeDirect(maxHp: 100, atk: 15, attackSpeed: 1.2f, regenHpPerSecond: 2f);
        }

        [Test]
        public void GetBreakdown_Hp_ReturnsCorrectValues()
        {
            var b = _stats.GetBreakdown(StatType.Hp);
            Assert.AreEqual("HP", b.StatName);
            Assert.AreEqual("100 / 100", b.FinalValue);
            Assert.AreEqual(1, b.Modifiers.Length);
            Assert.AreEqual("Base", b.Modifiers[0].Source);
            Assert.AreEqual("100", b.Modifiers[0].Value);
        }

        [Test]
        public void GetBreakdown_Atk_ReturnsCorrectValues()
        {
            var b = _stats.GetBreakdown(StatType.Atk);
            Assert.AreEqual("ATK", b.StatName);
            Assert.AreEqual("15", b.FinalValue);
            Assert.AreEqual(1, b.Modifiers.Length);
            Assert.AreEqual("15", b.Modifiers[0].Value);
        }

        [Test]
        public void GetBreakdown_Def_ReturnsMockedZero()
        {
            var b = _stats.GetBreakdown(StatType.Def);
            Assert.AreEqual("DEF", b.StatName);
            Assert.AreEqual("0", b.FinalValue);
        }

        [Test]
        public void GetBreakdown_AttackSpeed_ReturnsFormattedValue()
        {
            var b = _stats.GetBreakdown(StatType.AttackSpeed);
            Assert.AreEqual("SPD", b.StatName);
            Assert.AreEqual("1.2", b.FinalValue);
        }

        [Test]
        public void GetBreakdown_RegenHp_ReturnsFormattedValue()
        {
            var b = _stats.GetBreakdown(StatType.RegenHp);
            Assert.AreEqual("REGEN", b.StatName);
            Assert.AreEqual("2.0/s", b.FinalValue);
        }

        [Test]
        public void GetBreakdown_CritRate_ReturnsMockedZero()
        {
            var b = _stats.GetBreakdown(StatType.CritRate);
            Assert.AreEqual("CRIT", b.StatName);
            Assert.AreEqual("0%", b.FinalValue);
        }

        [Test]
        public void GetBreakdown_Hp_UpdatesAfterDamage()
        {
            _stats.TakeDamage(30);
            var b = _stats.GetBreakdown(StatType.Hp);
            Assert.AreEqual("70 / 100", b.FinalValue);
        }

        [Test]
        public void GetBreakdown_AllModifiers_HaveBaseSource()
        {
            foreach (var statType in CombatStats.DisplayOrder)
            {
                var b = _stats.GetBreakdown(statType);
                Assert.IsTrue(b.Modifiers.Length >= 1, $"{statType} should have at least 1 modifier");
                Assert.AreEqual("Base", b.Modifiers[0].Source, $"{statType} first modifier should be Base");
                Assert.IsTrue(b.Modifiers[0].IsPositive, $"{statType} base modifier should be positive");
            }
        }
    }
}
