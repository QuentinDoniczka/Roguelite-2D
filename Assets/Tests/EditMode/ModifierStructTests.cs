using System;
using NUnit.Framework;
using RogueliteAutoBattler.Combat.Core;

namespace RogueliteAutoBattler.Tests.EditMode
{
    [TestFixture]
    public class ModifierStructTests
    {
        [Test]
        public void Ctor_StoresAllFields()
        {
            var modifier = new Modifier(StatType.Hp, ModifierTier.Base, "techtree", 5f);

            Assert.AreEqual(StatType.Hp, modifier.Stat);
            Assert.AreEqual(ModifierTier.Base, modifier.Tier);
            Assert.AreEqual("techtree", modifier.Source);
            Assert.AreEqual(5f, modifier.Value);
        }

        [Test]
        public void Ctor_AcceptsAllStatTypes()
        {
            foreach (StatType statType in Enum.GetValues(typeof(StatType)))
            {
                var modifier = new Modifier(statType, ModifierTier.Flat, "test", 1f);
                Assert.AreEqual(statType, modifier.Stat);
            }
        }

        [Test]
        public void Ctor_AcceptsAllTiers()
        {
            foreach (ModifierTier tier in Enum.GetValues(typeof(ModifierTier)))
            {
                var modifier = new Modifier(StatType.Atk, tier, "test", 1f);
                Assert.AreEqual(tier, modifier.Tier);
            }
        }

        [Test]
        public void Ctor_AcceptsNegativeValue()
        {
            var modifier = new Modifier(StatType.Def, ModifierTier.Flat, "curse", -10f);

            Assert.AreEqual(-10f, modifier.Value);
        }
    }
}
