using System.Collections;
using NUnit.Framework;
using RogueliteAutoBattler.Economy;
using UnityEngine;
using UnityEngine.TestTools;

namespace RogueliteAutoBattler.Tests.PlayMode
{
    public class GoldWalletTests : PlayModeTestBase
    {
        private GoldWallet _wallet;

        [SetUp]
        public void SetUp()
        {
            var go = Track(new GameObject("GoldWalletTest"));
            _wallet = go.AddComponent<GoldWallet>();
        }

        [UnityTest]
        public IEnumerator Add_IncreasesGold()
        {
            yield return null;

            _wallet.Add(100);

            Assert.AreEqual(100, _wallet.Gold);
        }

        [UnityTest]
        public IEnumerator Add_FiresOnGoldChangedWithNewTotal()
        {
            yield return null;

            int receivedTotal = -1;
            _wallet.OnGoldChanged += total => receivedTotal = total;

            _wallet.Add(50);

            Assert.AreEqual(50, receivedTotal);
        }

        [UnityTest]
        public IEnumerator Add_Zero_DoesNotChangeGold()
        {
            yield return null;

            _wallet.Add(100);
            int eventCount = 0;
            _wallet.OnGoldChanged += _ => eventCount++;

            _wallet.Add(0);

            Assert.AreEqual(100, _wallet.Gold);
            Assert.AreEqual(0, eventCount);
        }

        [UnityTest]
        public IEnumerator Add_NegativeAmount_DoesNotChangeGold()
        {
            yield return null;

            _wallet.Add(100);
            int eventCount = 0;
            _wallet.OnGoldChanged += _ => eventCount++;

            _wallet.Add(-50);

            Assert.AreEqual(100, _wallet.Gold);
            Assert.AreEqual(0, eventCount);
        }

        [UnityTest]
        public IEnumerator Add_MultipleCalls_AccumulatesCorrectly()
        {
            yield return null;

            _wallet.Add(100);
            _wallet.Add(200);
            _wallet.Add(50);

            Assert.AreEqual(350, _wallet.Gold);
        }

        [UnityTest]
        public IEnumerator Add_MultipleCalls_FiresEventEachTime()
        {
            yield return null;

            int eventCount = 0;
            int lastTotal = -1;
            _wallet.OnGoldChanged += total =>
            {
                eventCount++;
                lastTotal = total;
            };

            _wallet.Add(100);
            _wallet.Add(200);

            Assert.AreEqual(2, eventCount);
            Assert.AreEqual(300, lastTotal);
        }

        [UnityTest]
        public IEnumerator Reset_SetsGoldToZero()
        {
            yield return null;

            _wallet.Add(500);
            _wallet.ResetGold();

            Assert.AreEqual(0, _wallet.Gold);
        }

        [UnityTest]
        public IEnumerator Reset_FiresOnGoldChangedWithZero()
        {
            yield return null;

            _wallet.Add(500);

            int receivedTotal = -1;
            _wallet.OnGoldChanged += total => receivedTotal = total;

            _wallet.ResetGold();

            Assert.AreEqual(0, receivedTotal);
        }

        [UnityTest]
        public IEnumerator GoldStartsAtZero()
        {
            yield return null;

            Assert.AreEqual(0, _wallet.Gold);
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
            Assert.IsFalse(_wallet.CanAfford(1));
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
            Assert.AreEqual(60, _wallet.Gold);
        }

        [UnityTest]
        public IEnumerator Spend_NotEnough_ReturnsFalseNoChange()
        {
            yield return null;
            _wallet.Add(10);
            bool result = _wallet.Spend(50);
            Assert.IsFalse(result);
            Assert.AreEqual(10, _wallet.Gold);
        }

        [UnityTest]
        public IEnumerator Spend_ZeroOrNegative_ReturnsFalse()
        {
            yield return null;
            _wallet.Add(100);
            Assert.IsFalse(_wallet.Spend(0));
            Assert.IsFalse(_wallet.Spend(-5));
            Assert.AreEqual(100, _wallet.Gold);
        }

        [UnityTest]
        public IEnumerator Spend_FiresOnGoldChanged()
        {
            yield return null;
            _wallet.Add(100);
            int received = -1;
            _wallet.OnGoldChanged += v => received = v;
            _wallet.Spend(30);
            Assert.AreEqual(70, received);
        }
    }
}
