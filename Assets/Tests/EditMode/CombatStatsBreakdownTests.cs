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
            Assert.AreEqual("AS", b.StatName);
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

        [Test]
        public void GetBreakdown_WithFlatModifier_AppendsTierTaggedEntry()
        {
            _stats.AddModifier(StatType.Atk, ModifierTier.Flat, "techtree", 10f);

            var b = _stats.GetBreakdown(StatType.Atk);

            Assert.AreEqual(2, b.Modifiers.Length);
            Assert.AreEqual("Base", b.Modifiers[0].Source);
            Assert.AreEqual(ModifierTier.Base, b.Modifiers[0].Tier);
            Assert.AreEqual("techtree", b.Modifiers[1].Source);
            Assert.AreEqual(ModifierTier.Flat, b.Modifiers[1].Tier);
            Assert.AreEqual("+10", b.Modifiers[1].Value);
            Assert.IsTrue(b.Modifiers[1].IsPositive);
        }

        [Test]
        public void GetBreakdown_WithPercentModifier_FormatsAsPercent()
        {
            _stats.AddModifier(StatType.Atk, ModifierTier.Percent, "item", 0.5f);

            var b = _stats.GetBreakdown(StatType.Atk);

            Assert.AreEqual(2, b.Modifiers.Length);
            Assert.AreEqual("+50%", b.Modifiers[1].Value);
            Assert.AreEqual(ModifierTier.Percent, b.Modifiers[1].Tier);
        }

        [Test]
        public void GetBreakdown_WithMultipleModifiers_PreservesInsertionOrder()
        {
            _stats.AddModifier(StatType.Atk, ModifierTier.Base, "src_base", 5f);
            _stats.AddModifier(StatType.Atk, ModifierTier.Percent, "src_pct", 0.25f);
            _stats.AddModifier(StatType.Atk, ModifierTier.Flat, "src_flat", 7f);

            var b = _stats.GetBreakdown(StatType.Atk);

            Assert.AreEqual(4, b.Modifiers.Length);
            Assert.AreEqual("Base", b.Modifiers[0].Source);
            Assert.AreEqual("src_base", b.Modifiers[1].Source);
            Assert.AreEqual(ModifierTier.Base, b.Modifiers[1].Tier);
            Assert.AreEqual("src_pct", b.Modifiers[2].Source);
            Assert.AreEqual(ModifierTier.Percent, b.Modifiers[2].Tier);
            Assert.AreEqual("src_flat", b.Modifiers[3].Source);
            Assert.AreEqual(ModifierTier.Flat, b.Modifiers[3].Tier);
        }

        [Test]
        public void GetBreakdown_NegativePercent_FormatsWithSignedPercent()
        {
            _stats.AddModifier(StatType.AttackSpeed, ModifierTier.Percent, "slow", -0.3f);

            var b = _stats.GetBreakdown(StatType.AttackSpeed);

            Assert.AreEqual(2, b.Modifiers.Length);
            Assert.AreEqual("-30%", b.Modifiers[1].Value);
            Assert.IsFalse(b.Modifiers[1].IsPositive);
            Assert.AreEqual(ModifierTier.Percent, b.Modifiers[1].Tier);
        }

        [Test]
        public void GetBreakdown_ZeroModifiers_StillSingleEntry()
        {
            var b = _stats.GetBreakdown(StatType.Atk);

            Assert.AreEqual(1, b.Modifiers.Length);
            Assert.AreEqual("Base", b.Modifiers[0].Source);
        }
    }
}
