using System.Collections;
using NUnit.Framework;
using RogueliteAutoBattler.Economy;
using UnityEngine;
using UnityEngine.TestTools;

namespace RogueliteAutoBattler.Tests.PlayMode
{
    public class SkillPointWalletTests : PlayModeTestBase
    {
        private SkillPointWallet _wallet;

        [SetUp]
        public void SetUp()
        {
            var go = Track(new GameObject("SkillPointWalletTest"));
            _wallet = go.AddComponent<SkillPointWallet>();
        }

        [UnityTest]
        public IEnumerator PointsStartAtZero()
        {
            yield return null;
            Assert.AreEqual(0, _wallet.Points);
        }

        [UnityTest]
        public IEnumerator Add_IncreasesPoints()
        {
            yield return null;
            _wallet.Add(100);
            Assert.AreEqual(100, _wallet.Points);
        }

        [UnityTest]
        public IEnumerator Add_FiresEvent()
        {
            yield return null;
            int received = -1;
            _wallet.OnPointsChanged += v => received = v;
            _wallet.Add(50);
            Assert.AreEqual(50, received);
        }

        [UnityTest]
        public IEnumerator Add_ZeroOrNegative_NoChange()
        {
            yield return null;
            _wallet.Add(100);
            int eventCount = 0;
            _wallet.OnPointsChanged += _ => eventCount++;
            _wallet.Add(0);
            _wallet.Add(-10);
            Assert.AreEqual(100, _wallet.Points);
            Assert.AreEqual(0, eventCount);
        }

        [UnityTest]
        public IEnumerator CanAfford_ReturnsTrueWhenEnough()
        {
            yield return null;
            _wallet.Add(100);
            Assert.IsTrue(_wallet.CanAfford(100));
            Assert.IsTrue(_wallet.CanAfford(50));
        }

        [UnityTest]
        public IEnumerator CanAfford_ReturnsFalseWhenNotEnough()
        {
            yield return null;
            _wallet.Add(10);
            Assert.IsFalse(_wallet.CanAfford(50));
        }

        [UnityTest]
        public IEnumerator Spend_DeductsAndReturnsTrue()
        {
            yield return null;
            _wallet.Add(100);
            bool result = _wallet.Spend(40);
            Assert.IsTrue(result);
            Assert.AreEqual(60, _wallet.Points);
        }

        [UnityTest]
        public IEnumerator Spend_NotEnough_ReturnsFalseNoChange()
        {
            yield return null;
            _wallet.Add(10);
            bool result = _wallet.Spend(50);
            Assert.IsFalse(result);
            Assert.AreEqual(10, _wallet.Points);
        }

        [UnityTest]
        public IEnumerator Spend_ZeroOrNegative_ReturnsFalse()
        {
            yield return null;
            _wallet.Add(100);
            Assert.IsFalse(_wallet.Spend(0));
            Assert.IsFalse(_wallet.Spend(-5));
            Assert.AreEqual(100, _wallet.Points);
        }

        [UnityTest]
        public IEnumerator ResetPoints_SetsToZero()
        {
            yield return null;
            _wallet.Add(500);
            _wallet.ResetPoints();
            Assert.AreEqual(0, _wallet.Points);
        }
    }
}
