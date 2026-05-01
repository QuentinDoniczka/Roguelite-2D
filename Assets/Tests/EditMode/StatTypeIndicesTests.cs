using System;
using System.Collections.Generic;
using NUnit.Framework;
using RogueliteAutoBattler.Combat.Core;

namespace RogueliteAutoBattler.Tests.EditMode
{
    [TestFixture]
    public class StatTypeIndicesTests
    {
        [Test]
        public void StatType_HasExactlyNineValues()
        {
            Assert.AreEqual(9, Enum.GetValues(typeof(StatType)).Length);
        }

        [Test]
        public void StatType_IndicesMatchSerializedContract()
        {
            Assert.AreEqual(0, (int)StatType.Hp);
            Assert.AreEqual(1, (int)StatType.RegenHp);
            Assert.AreEqual(2, (int)StatType.Atk);
            Assert.AreEqual(3, (int)StatType.Def);
            Assert.AreEqual(4, (int)StatType.Mana);
            Assert.AreEqual(5, (int)StatType.Power);
            Assert.AreEqual(6, (int)StatType.AttackSpeed);
            Assert.AreEqual(7, (int)StatType.CritRate);
            Assert.AreEqual(8, (int)StatType.None);
        }

        [Test]
        public void StatType_NamesMatchExpectedSequence()
        {
            string[] expected = { "Hp", "RegenHp", "Atk", "Def", "Mana", "Power", "AttackSpeed", "CritRate", "None" };
            for (int i = 0; i < expected.Length; i++)
            {
                Assert.AreEqual(expected[i], Enum.GetName(typeof(StatType), i));
            }
        }

        [Test]
        public void StatType_None_HasIndex8()
        {
            Assert.AreEqual(8, (int)StatType.None);
            Assert.AreEqual("None", Enum.GetName(typeof(StatType), 8));
        }

        [Test]
        public void StatType_HasNoDuplicateNumericValues()
        {
            var values = Enum.GetValues(typeof(StatType));
            var unique = new HashSet<int>();
            foreach (StatType v in values)
            {
                unique.Add((int)v);
            }
            Assert.AreEqual(Enum.GetValues(typeof(StatType)).Length, unique.Count);
        }
    }
}
