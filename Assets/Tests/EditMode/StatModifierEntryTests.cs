using NUnit.Framework;
using RogueliteAutoBattler.Combat.Core;

namespace RogueliteAutoBattler.Tests.EditMode
{
    [TestFixture]
    public class StatModifierEntryTests
    {
        [Test]
        public void LegacyCtor_DefaultsTierToFlat()
        {
            var entry = new StatModifierEntry("Base", "100", true);

            Assert.AreEqual(ModifierTier.Flat, entry.Tier);
        }

        [Test]
        public void NewCtor_StoresTier()
        {
            var percentEntry = new StatModifierEntry("Buff", "+10%", true, ModifierTier.Percent);
            Assert.AreEqual(ModifierTier.Percent, percentEntry.Tier);

            var baseEntry = new StatModifierEntry("Base", "50", true, ModifierTier.Base);
            Assert.AreEqual(ModifierTier.Base, baseEntry.Tier);

            var flatEntry = new StatModifierEntry("Equipment", "+5", true, ModifierTier.Flat);
            Assert.AreEqual(ModifierTier.Flat, flatEntry.Tier);
        }

        [Test]
        public void Source_Value_IsPositive_AreStored()
        {
            var entry = new StatModifierEntry("Equipment", "+12", true, ModifierTier.Flat);

            Assert.AreEqual("Equipment", entry.Source);
            Assert.AreEqual("+12", entry.Value);
            Assert.IsTrue(entry.IsPositive);

            var negativeEntry = new StatModifierEntry("Curse", "-5", false, ModifierTier.Percent);

            Assert.AreEqual("Curse", negativeEntry.Source);
            Assert.AreEqual("-5", negativeEntry.Value);
            Assert.IsFalse(negativeEntry.IsPositive);
        }
    }
}
