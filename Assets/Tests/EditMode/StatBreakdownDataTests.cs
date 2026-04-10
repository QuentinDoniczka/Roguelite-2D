using NUnit.Framework;
using RogueliteAutoBattler.Combat.Core;

namespace RogueliteAutoBattler.Tests.EditMode
{
    [TestFixture]
    public class StatBreakdownDataTests
    {
        [Test]
        public void StatModifierEntry_ConstructsCorrectly()
        {
            var entry = new StatModifierEntry("Base", "45", true);
            Assert.AreEqual("Base", entry.Source);
            Assert.AreEqual("45", entry.Value);
            Assert.IsTrue(entry.IsPositive);
        }

        [Test]
        public void StatModifierEntry_NegativeModifier()
        {
            var entry = new StatModifierEntry("Trait: Faible", "-10%", false);
            Assert.AreEqual("Trait: Faible", entry.Source);
            Assert.AreEqual("-10%", entry.Value);
            Assert.IsFalse(entry.IsPositive);
        }

        [Test]
        public void StatBreakdownData_ConstructsCorrectly()
        {
            var modifiers = new[] { new StatModifierEntry("Base", "100", true) };
            var data = new StatBreakdownData("HP", "80 / 100", modifiers);
            Assert.AreEqual("HP", data.StatName);
            Assert.AreEqual("80 / 100", data.FinalValue);
            Assert.AreEqual(1, data.Modifiers.Length);
            Assert.AreEqual("Base", data.Modifiers[0].Source);
        }

        [Test]
        public void StatBreakdownData_MultipleModifiers()
        {
            var modifiers = new[]
            {
                new StatModifierEntry("Base", "45", true),
                new StatModifierEntry("Epee +12", "+12", true),
                new StatModifierEntry("Trait", "+10%", true)
            };
            var data = new StatBreakdownData("ATK", "57", modifiers);
            Assert.AreEqual(3, data.Modifiers.Length);
            Assert.AreEqual("57", data.FinalValue);
        }
    }
}
