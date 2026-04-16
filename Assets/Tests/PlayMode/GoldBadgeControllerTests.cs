using System.Collections;
using NUnit.Framework;
using RogueliteAutoBattler.Economy;
using RogueliteAutoBattler.UI.Toolkit;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace RogueliteAutoBattler.Tests.PlayMode
{
    public class GoldBadgeControllerTests : PlayModeTestBase
    {
        private GoldWallet _wallet;
        private VisualElement _badgeRoot;
        private Label _goldLabel;
        private GoldBadgeController _controller;

        [SetUp]
        public void SetUp()
        {
            var walletGo = new GameObject("GoldWallet");
            Track(walletGo);
            _wallet = walletGo.AddComponent<GoldWallet>();

            _badgeRoot = new VisualElement();
            _goldLabel = new Label();

            _controller = new GoldBadgeController(_badgeRoot, _goldLabel, _wallet);
            _controller.InitializeForTest(_wallet);
        }

        [Test]
        public void GoldLabel_ShowsZero_OnInitialize()
        {
            Assert.AreEqual("0", _controller.DisplayText);
        }

        [Test]
        public void GoldLabel_Updates_WhenGoldAdded()
        {
            _wallet.Add(500);

            Assert.AreEqual("500", _controller.DisplayText);
        }

        [Test]
        public void GoldLabel_FormatsCompact_WhenGoldLarge()
        {
            _wallet.Add(1500);

            Assert.AreEqual("1.5K", _controller.DisplayText);
        }

        [Test]
        public void Dispose_UnsubscribesFromWallet()
        {
            _controller.Dispose();

            _wallet.Add(999);

            Assert.AreEqual("0", _controller.DisplayText);
        }

        [UnityTest]
        public IEnumerator Punch_ChangesScale()
        {
            _controller.Punch();

            yield return new WaitForSeconds(0.05f);

            Scale scaleValue = _badgeRoot.style.scale.value;
            Assert.Greater(scaleValue.value.x, 1.0f,
                "Badge root scale X should be greater than 1.0 during punch peak.");
        }
    }
}
