using System;
using System.Linq;
using NUnit.Framework;
using RogueliteAutoBattler.Combat.Core;
using UnityEngine;

namespace RogueliteAutoBattler.Tests.EditMode
{
    [TestFixture]
    public class CombatStatsBreakdownAllStatsTests
    {
        private GameObject _go;
        private CombatStats _stats;

        [SetUp]
        public void SetUp()
        {
            _go = new GameObject("TestStatsAllStats");
            _stats = _go.AddComponent<CombatStats>();
            _stats.InitializeDirect(maxHp: 100, atk: 10, attackSpeed: 1f, regenHpPerSecond: 0f);
        }

        [TearDown]
        public void TearDown()
        {
            if (_go != null)
                UnityEngine.Object.DestroyImmediate(_go);
        }

        [Test]
        public void GetBreakdown_EveryStatType_ReturnsNonEmptyStatName()
        {
            foreach (StatType value in Enum.GetValues(typeof(StatType)))
            {
                var bd = _stats.GetBreakdown(value);
                Assert.IsNotEmpty(bd.StatName, $"GetBreakdown({value}) returned empty StatName");
            }
        }

        [Test]
        public void GetBreakdown_EveryStatType_ReturnsAtLeastOneModifier()
        {
            foreach (StatType value in Enum.GetValues(typeof(StatType)))
            {
                var bd = _stats.GetBreakdown(value);
                Assert.IsNotNull(bd.Modifiers, $"GetBreakdown({value}) returned null Modifiers");
                Assert.GreaterOrEqual(bd.Modifiers.Length, 1, $"GetBreakdown({value}) returned no modifiers");
            }
        }

        [Test]
        public void DisplayOrder_ContainsAllStatTypeValues()
        {
            var enumValues = Enum.GetValues(typeof(StatType)).Cast<StatType>().ToHashSet();
            var displayOrderSet = CombatStats.DisplayOrder.ToHashSet();

            Assert.AreEqual(Enum.GetValues(typeof(StatType)).Length, displayOrderSet.Count, "DisplayOrder must have one unique entry per StatType value");
            Assert.IsTrue(displayOrderSet.SetEquals(enumValues), "DisplayOrder must contain every StatType value exactly once");
        }

        [Test]
        public void GetBreakdown_NewStatTypes_HaveCorrectPlaceholderLabels()
        {
            Assert.AreEqual("MANA", _stats.GetBreakdown(StatType.Mana).StatName);
            Assert.AreEqual("POWER", _stats.GetBreakdown(StatType.Power).StatName);
        }
    }
}
