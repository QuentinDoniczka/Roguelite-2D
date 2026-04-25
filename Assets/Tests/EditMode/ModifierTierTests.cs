using System;
using NUnit.Framework;
using RogueliteAutoBattler.Combat.Core;

namespace RogueliteAutoBattler.Tests.EditMode
{
    [TestFixture]
    public class ModifierTierTests
    {
        [Test]
        public void ModifierTier_HasExactlyThreeValues()
        {
            Assert.AreEqual(3, Enum.GetValues(typeof(ModifierTier)).Length);
        }

        [Test]
        public void ModifierTier_IndicesMatchSerializedContract()
        {
            Assert.AreEqual(0, (int)ModifierTier.Base);
            Assert.AreEqual(1, (int)ModifierTier.Percent);
            Assert.AreEqual(2, (int)ModifierTier.Flat);
        }

        [Test]
        public void ModifierTier_NamesAreStable()
        {
            Assert.AreEqual("Base", Enum.GetName(typeof(ModifierTier), 0));
            Assert.AreEqual("Percent", Enum.GetName(typeof(ModifierTier), 1));
            Assert.AreEqual("Flat", Enum.GetName(typeof(ModifierTier), 2));
        }
    }
}
