using System.Collections;
using System.Text.RegularExpressions;
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
        private const int SmallGoldAmount = 500;
        private const int LargeGoldAmount = 1500;
        private const string LargeGoldFormatted = "1.5K";
        private const string ZeroGoldFormatted = "0";
        private const float PunchWarmupSeconds = 0.05f;

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
            _controller.Initialize(_wallet);
        }

        [Test]
        public void GoldLabel_ShowsZero_OnInitialize()
        {
            Assert.AreEqual(ZeroGoldFormatted, _controller.DisplayText);
        }

        [Test]
        public void GoldLabel_Updates_WhenGoldAdded()
        {
            _wallet.Add(SmallGoldAmount);

            Assert.AreEqual(SmallGoldAmount.ToString(), _controller.DisplayText);
        }

        [Test]
        public void GoldLabel_FormatsCompact_WhenGoldLarge()
        {
            _wallet.Add(LargeGoldAmount);

            Assert.AreEqual(LargeGoldFormatted, _controller.DisplayText);
        }

        [Test]
        public void Dispose_UnsubscribesFromWallet()
        {
            _controller.Dispose();

            _wallet.Add(999);

            Assert.AreEqual(ZeroGoldFormatted, _controller.DisplayText);
        }

        [UnityTest]
        public IEnumerator Punch_ChangesScale()
        {
            _controller.Punch();

            yield return new WaitForSeconds(PunchWarmupSeconds);

            Scale scaleValue = _badgeRoot.style.scale.value;
            Assert.Greater(scaleValue.value.x, 1.0f);
        }

        [Test]
        public void Initialize_WithNullWallet_LogsWarning_AndDoesNotSubscribe()
        {
            var freshWalletGo = Track(new GameObject("FreshGoldWallet"));
            var freshWallet = freshWalletGo.AddComponent<GoldWallet>();

            var freshBadgeRoot = new VisualElement();
            var freshGoldLabel = new Label(ZeroGoldFormatted);
            var freshController = new GoldBadgeController(freshBadgeRoot, freshGoldLabel, freshWallet);

            LogAssert.Expect(LogType.Warning, new Regex("GoldBadgeController.*null wallet"));

            freshController.Initialize(null);

            freshWallet.Add(200);

            Assert.AreEqual(ZeroGoldFormatted, freshController.DisplayText,
                "Badge must not update when initialized with a null wallet.");
        }

        [UnityTest]
        public IEnumerator Initialize_WithWallet_UpdatesLabelOnSubsequentAdd()
        {
            var localWalletGo = Track(new GameObject("LocalGoldWallet"));
            var localWallet = localWalletGo.AddComponent<GoldWallet>();

            var localBadgeRoot = new VisualElement();
            var localGoldLabel = new Label();
            var localController = new GoldBadgeController(localBadgeRoot, localGoldLabel, localWallet);

            localController.Initialize(localWallet);

            localWallet.Add(250);

            yield return null;

            Assert.AreEqual("250", localController.DisplayText,
                "Badge must subscribe to wallet events when Initialize(wallet) is called and reflect new totals (regression guard for #215).");
        }
    }
}
