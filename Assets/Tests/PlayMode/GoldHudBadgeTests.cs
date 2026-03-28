using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using RogueliteAutoBattler.Combat;
using RogueliteAutoBattler.UI.Widgets;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;

namespace RogueliteAutoBattler.Tests.PlayMode
{
    public class GoldHudBadgeTests
    {
        private readonly List<GameObject> _spawned = new();
        private GoldWallet _wallet;
        private TMP_Text _label;

        [SetUp]
        public void SetUp()
        {
            var walletGo = new GameObject("GoldWallet");
            _spawned.Add(walletGo);
            _wallet = walletGo.AddComponent<GoldWallet>();

            var canvasGo = new GameObject("TestCanvas");
            _spawned.Add(canvasGo);
            canvasGo.AddComponent<Canvas>();

            var badgeGo = new GameObject("GoldBadge");
            badgeGo.transform.SetParent(canvasGo.transform);

            var labelGo = new GameObject("Label");
            labelGo.transform.SetParent(badgeGo.transform);
            _label = labelGo.AddComponent<TextMeshProUGUI>();

            badgeGo.AddComponent<GoldHudBadge>();
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var go in _spawned)
            {
                if (go != null)
                    Object.DestroyImmediate(go);
            }

            _spawned.Clear();
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
    }
}
