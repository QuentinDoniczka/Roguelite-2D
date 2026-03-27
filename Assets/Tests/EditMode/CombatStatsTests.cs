using NUnit.Framework;
using RogueliteAutoBattler.Combat;
using UnityEngine;

namespace RogueliteAutoBattler.Tests.EditMode
{
    public class CombatStatsTests : EditModeTestBase
    {
        private GameObject _gameObject;
        private CombatStats _stats;

        [SetUp]
        public void SetUp()
        {
            _gameObject = Track(new GameObject("TestUnit"));
            _stats = _gameObject.AddComponent<CombatStats>();
            _stats.InitializeDirect(maxHp: 100, atk: 25, attackSpeed: 1.5f);
        }

        [Test]
        public void InitializeDirect_SetsAllFields()
        {
            Assert.AreEqual(100, _stats.MaxHp);
            Assert.AreEqual(100, _stats.CurrentHp);
            Assert.AreEqual(25, _stats.Atk);
            Assert.That(_stats.AttackSpeed, Is.EqualTo(1.5f).Within(0.01f));
            Assert.IsFalse(_stats.IsDead);
        }

        [Test]
        public void TakeDamage_ReducesCurrentHp()
        {
            _stats.TakeDamage(30);

            Assert.AreEqual(70, _stats.CurrentHp);
            Assert.IsFalse(_stats.IsDead);
        }

        [Test]
        public void TakeDamage_KillsAtZero_FiresOnDied()
        {
            bool diedFired = false;
            _stats.OnDied += () => diedFired = true;

            _stats.TakeDamage(100);

            Assert.AreEqual(0, _stats.CurrentHp);
            Assert.IsTrue(_stats.IsDead);
            Assert.IsTrue(diedFired);
        }

        [Test]
        public void TakeDamage_ClampsToZero_NeverNegative()
        {
            _stats.TakeDamage(999);

            Assert.AreEqual(0, _stats.CurrentHp);
            Assert.IsTrue(_stats.IsDead);
        }

        [Test]
        public void TakeDamage_WhenDead_DoesNotFireOnDiedAgain()
        {
            int diedCount = 0;
            _stats.OnDied += () => diedCount++;

            _stats.TakeDamage(100);
            _stats.TakeDamage(50);

            Assert.AreEqual(1, diedCount);
        }
    }
}
