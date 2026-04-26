using NUnit.Framework;
using RogueliteAutoBattler.Combat.Core;
using UnityEngine;

namespace RogueliteAutoBattler.Tests.EditMode
{
    [TestFixture]
    public class CombatStatsModifierPipelineTests
    {
        private GameObject _gameObject;
        private CombatStats _stats;

        [SetUp]
        public void SetUp()
        {
            _gameObject = new GameObject("ModifierPipelineTestUnit");
            _stats = _gameObject.AddComponent<CombatStats>();
            _stats.InitializeDirect(maxHp: 100, atk: 15, attackSpeed: 1.2f, regenHpPerSecond: 2f);
        }

        [TearDown]
        public void TearDown()
        {
            if (_gameObject != null)
                Object.DestroyImmediate(_gameObject);
        }

        [Test]
        public void AddModifier_BasePlusFlat_AppliesAdditively_OnAtk()
        {
            _stats.AddModifier(StatType.Atk, ModifierTier.Base, "test", 10f);

            Assert.That(_stats.Atk, Is.EqualTo(25));
        }

        [Test]
        public void AddModifier_PercentMultipliesBaseSum_OnAtk()
        {
            _stats.AddModifier(StatType.Atk, ModifierTier.Base, "tt", 5f);
            _stats.AddModifier(StatType.Atk, ModifierTier.Percent, "item", 0.5f);

            Assert.That(_stats.Atk, Is.EqualTo(30));
        }

        [Test]
        public void AddModifier_FlatAppliedAfterPercent_OnAtk()
        {
            _stats.AddModifier(StatType.Atk, ModifierTier.Base, "tt", 5f);
            _stats.AddModifier(StatType.Atk, ModifierTier.Percent, "item", 0.5f);
            _stats.AddModifier(StatType.Atk, ModifierTier.Flat, "buff", 10f);

            Assert.That(_stats.Atk, Is.EqualTo(40));
        }

        [Test]
        public void RemoveModifiersFromSource_Idempotent_ReturnsRemovedCount()
        {
            _stats.AddModifier(StatType.Atk, ModifierTier.Base, "techtree", 1f);
            _stats.AddModifier(StatType.Hp, ModifierTier.Flat, "techtree", 10f);
            _stats.AddModifier(StatType.AttackSpeed, ModifierTier.Percent, "techtree", 0.1f);
            _stats.AddModifier(StatType.Atk, ModifierTier.Flat, "item", 5f);

            Assert.That(_stats.RemoveModifiersFromSource("techtree"), Is.EqualTo(3));
            Assert.That(_stats.RemoveModifiersFromSource("techtree"), Is.EqualTo(0));
            Assert.That(_stats.RemoveModifiersFromSource("item"), Is.EqualTo(1));
        }

        [Test]
        public void AddModifier_Hp_ClampsCurrentHpToNewMax()
        {
            _stats.AddModifier(StatType.Hp, ModifierTier.Flat, "shrink", -50f);

            Assert.That(_stats.MaxHp, Is.EqualTo(50));
            Assert.That(_stats.CurrentHp, Is.LessThanOrEqualTo(50));
        }

        [Test]
        public void RemoveModifiersFromSource_RecomputesStatsBack()
        {
            _stats.AddModifier(StatType.Atk, ModifierTier.Base, "x", 20f);
            Assert.That(_stats.Atk, Is.EqualTo(35));

            int removed = _stats.RemoveModifiersFromSource("x");

            Assert.That(removed, Is.EqualTo(1));
            Assert.That(_stats.Atk, Is.EqualTo(15));
        }

        [Test]
        public void AddModifier_PercentNegative_ReducesStat()
        {
            _stats.AddModifier(StatType.AttackSpeed, ModifierTier.Percent, "slow", -0.5f);

            Assert.That(_stats.AttackSpeed, Is.EqualTo(0.6f).Within(0.001f));
        }

        [Test]
        public void AddModifier_DefStat_NoBackingField_StillReturnsCorrectValue()
        {
            _stats.AddModifier(StatType.Def, ModifierTier.Flat, "armor", 12f);

            Assert.That(_stats.GetStatValue(StatType.Def), Is.EqualTo(12f).Within(0.001f));
        }

        [Test]
        public void AddModifier_Atk_NonRoundResult_RoundsToNearestInt()
        {
            _stats.AddModifier(StatType.Atk, ModifierTier.Base, "tt", 90f);
            _stats.AddModifier(StatType.Atk, ModifierTier.Percent, "item", 0.1f);

            Assert.That(_stats.Atk, Is.EqualTo(116));
        }

        [Test]
        public void Breakdown_PercentTwoLevelsOfFivePercent_FormatsAsTenNotNine()
        {
            float perLevelDecimal = 5f * 0.01f;
            float twoLevels = perLevelDecimal * 2f;
            _stats.AddModifier(StatType.Hp, ModifierTier.Percent, "techtree:node0", twoLevels);

            var breakdown = _stats.GetBreakdown(StatType.Hp);

            Assert.AreEqual(2, breakdown.Modifiers.Length);
            Assert.AreEqual("+10%", breakdown.Modifiers[1].Value,
                "Float imprecision (5*0.01*2*100 = 9.99999976) must round, not truncate.");
        }

        [Test]
        public void Breakdown_ManaWithPercentMod_IncludesModifierLineEvenWhenBaseIsZero()
        {
            _stats.AddModifier(StatType.Mana, ModifierTier.Percent, "techtree:node0", 0.05f);

            var breakdown = _stats.GetBreakdown(StatType.Mana);

            Assert.AreEqual(2, breakdown.Modifiers.Length, "Should include base entry + techtree modifier");
            Assert.AreEqual("+5%", breakdown.Modifiers[1].Value,
                "Modifier line for Mana (Percent +5%) must be visible even when base Mana is 0.");
        }

        [Test]
        public void Breakdown_PowerWithPercentMod_IncludesModifierLineEvenWhenBaseIsZero()
        {
            _stats.AddModifier(StatType.Power, ModifierTier.Percent, "techtree:node0", 0.05f);

            var breakdown = _stats.GetBreakdown(StatType.Power);

            Assert.AreEqual(2, breakdown.Modifiers.Length);
            Assert.AreEqual("+5%", breakdown.Modifiers[1].Value);
        }

        [Test]
        public void Breakdown_AtkWithPercentMod_ShowsModifierLine()
        {
            _stats.AddModifier(StatType.Atk, ModifierTier.Percent, "techtree:node0", 0.05f);

            var breakdown = _stats.GetBreakdown(StatType.Atk);

            Assert.AreEqual(2, breakdown.Modifiers.Length);
            Assert.AreEqual("+5%", breakdown.Modifiers[1].Value);
        }
    }
}
