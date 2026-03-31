using System.Collections;
using NUnit.Framework;
using RogueliteAutoBattler.Economy;
using RogueliteAutoBattler.UI.Widgets;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;

namespace RogueliteAutoBattler.Tests.PlayMode
{
    public class GoldHudBadgeTests : PlayModeTestBase
    {
        private GoldWallet _wallet;
        private TMP_Text _label;
        private GoldHudBadge _badge;

        [SetUp]
        public void SetUp()
        {
            var walletGo = new GameObject("GoldWallet");
            Track(walletGo);
            _wallet = walletGo.AddComponent<GoldWallet>();

            var canvasGo = new GameObject("TestCanvas");
            Track(canvasGo);
            canvasGo.AddComponent<Canvas>();

            var badgeGo = new GameObject("GoldBadge");
            badgeGo.AddComponent<RectTransform>();
            badgeGo.transform.SetParent(canvasGo.transform);

            var labelGo = new GameObject("Label");
            labelGo.transform.SetParent(badgeGo.transform);
            _label = labelGo.AddComponent<TextMeshProUGUI>();

            _badge = badgeGo.AddComponent<GoldHudBadge>();
        }

        [UnityTest]
        public IEnumerator Badge_DisplaysZero_OnStart()
        {
            yield return null;

            Assert.AreEqual("0", _label.text);
        }

        [UnityTest]
        public IEnumerator Badge_UpdatesDisplay_WhenGoldAdded()
        {
            yield return null;

            _wallet.Add(500);
            yield return null;

            Assert.AreEqual("500", _label.text);
        }

        [UnityTest]
        public IEnumerator Badge_FormatsCompact_WhenGoldLarge()
        {
            yield return null;

            _wallet.Add(1500);
            yield return null;

            Assert.AreEqual("1.5K", _label.text);
        }

        [UnityTest]
        public IEnumerator Punch_ScalesUpThenBack()
        {
            yield return null;

            _badge.Punch();

            yield return new WaitForSeconds(0.08f);

            RectTransform badgeRect = _badge.GetComponent<RectTransform>();
            Assert.Greater(badgeRect.localScale.x, 1.0f,
                "Scale should be greater than 1.0 during punch peak");

            yield return new WaitForSeconds(0.1f);

            Assert.AreEqual(1f, badgeRect.localScale.x, 0.05f);
            Assert.AreEqual(1f, badgeRect.localScale.y, 0.05f);
            Assert.AreEqual(1f, badgeRect.localScale.z, 0.05f);
        }
    }
}
