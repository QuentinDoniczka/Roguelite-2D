using System.Collections.Generic;
using NUnit.Framework;
using RogueliteAutoBattler.Combat;
using UnityEngine;

namespace RogueliteAutoBattler.Tests.EditMode
{
    public class CombatStatsDamageEventTests : EditModeTestBase
    {
        private CombatStats _stats;

        [SetUp]
        public void SetUp()
        {
            var go = Track(new GameObject("DamageEventTestUnit"));
            _stats = go.AddComponent<CombatStats>();
        }

        [Test]
        public void OnDamageTaken_FiresWithCorrectValues()
        {
            _stats.InitializeDirect(maxHp: 100, atk: 10, attackSpeed: 1f);

            int receivedDamage = 0;
            int receivedCurrentHp = 0;
            int fireCount = 0;
            _stats.OnDamageTaken += (damage, currentHp) =>
            {
                receivedDamage = damage;
                receivedCurrentHp = currentHp;
                fireCount++;
            };

            _stats.TakeDamage(30);

            Assert.AreEqual(1, fireCount);
            Assert.AreEqual(30, receivedDamage);
            Assert.AreEqual(70, receivedCurrentHp);
        }

        [Test]
        public void OnDamageTaken_FiresBeforeOnDied()
        {
            _stats.InitializeDirect(maxHp: 50, atk: 10, attackSpeed: 1f);

            var callOrder = new List<string>();
            int receivedDamage = 0;
            int receivedCurrentHp = 0;

            _stats.OnDamageTaken += (damage, currentHp) =>
            {
                receivedDamage = damage;
                receivedCurrentHp = currentHp;
                callOrder.Add("OnDamageTaken");
            };
            _stats.OnDied += () => callOrder.Add("OnDied");

            _stats.TakeDamage(50);

            Assert.AreEqual(2, callOrder.Count);
            Assert.AreEqual("OnDamageTaken", callOrder[0]);
            Assert.AreEqual("OnDied", callOrder[1]);
            Assert.AreEqual(50, receivedDamage);
            Assert.AreEqual(0, receivedCurrentHp);
        }

        [Test]
        public void OnDamageTaken_DoesNotFireWhenAlreadyDead()
        {
            _stats.InitializeDirect(maxHp: 100, atk: 10, attackSpeed: 1f);

            int fireCount = 0;
            _stats.OnDamageTaken += (damage, currentHp) => fireCount++;

            _stats.TakeDamage(100);
            Assert.AreEqual(1, fireCount);

            _stats.TakeDamage(10);
            Assert.AreEqual(1, fireCount);
        }
    }
}
