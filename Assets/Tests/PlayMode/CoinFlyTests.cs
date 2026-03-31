using System.Collections;
using NUnit.Framework;
using RogueliteAutoBattler.Combat.Visuals;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace RogueliteAutoBattler.Tests.PlayMode
{
    public class CoinFlyTests : PlayModeTestBase
    {
        private Canvas _canvas;
        private GameObject _coinFlyGo;
        private CoinFly _coinFly;

        [SetUp]
        public void SetUp()
        {
            var canvasGo = Track(new GameObject("TestCanvas"));
            _canvas = canvasGo.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<CanvasScaler>();

            _coinFlyGo = new GameObject("CoinFly");
            _coinFlyGo.transform.SetParent(canvasGo.transform, false);
            _coinFlyGo.AddComponent<Image>();
            _coinFly = _coinFlyGo.AddComponent<CoinFly>();
            _coinFlyGo.SetActive(false);

            _coinFly.Initialize(_ => { });
        }

        [UnityTest]
        public IEnumerator Play_ActivatesGameObject()
        {
            yield return null;

            _coinFly.Play(Vector2.zero, new Vector2(200f, 200f), 0.5f, null);
            yield return null;

            Assert.IsTrue(_coinFlyGo.activeSelf);
        }

        [UnityTest]
        public IEnumerator Play_DeactivatesAfterDuration()
        {
            yield return null;

            float duration = 0.3f;
            _coinFly.Play(Vector2.zero, new Vector2(200f, 200f), duration, null);
            yield return null;

            Assert.IsTrue(_coinFlyGo.activeSelf);

            yield return new WaitForSeconds(duration + 0.2f);

            Assert.IsFalse(_coinFlyGo.activeSelf);
        }

        [UnityTest]
        public IEnumerator Play_InvokesReturnCallback()
        {
            bool callbackInvoked = false;
            _coinFly.Initialize(coin => callbackInvoked = true);

            yield return null;

            _coinFly.Play(Vector2.zero, new Vector2(100f, 100f), 0.3f, null);

            yield return new WaitForSeconds(0.3f + 0.2f);

            Assert.IsTrue(callbackInvoked);
        }

        [UnityTest]
        public IEnumerator Play_InvokesOnArriveCallback()
        {
            bool onArriveCalled = false;

            yield return null;

            _coinFly.Play(Vector2.zero, new Vector2(100f, 100f), 0.3f, () => onArriveCalled = true);

            yield return new WaitForSeconds(0.3f + 0.2f);

            Assert.IsTrue(onArriveCalled);
        }

        [UnityTest]
        public IEnumerator Play_MovesFromStartToTarget()
        {
            yield return null;

            Vector2 start = Vector2.zero;
            Vector2 target = new Vector2(200f, 200f);
            _coinFly.Play(start, target, 0.5f, null);
            yield return null;

            RectTransform rt = _coinFlyGo.GetComponent<RectTransform>();
            Vector2 posAfterOneFrame = rt.anchoredPosition;

            Assert.AreNotEqual(start, posAfterOneFrame,
                "Position should have moved from start after one frame");

            yield return new WaitForSeconds(0.5f + 0.2f);

            Assert.AreEqual(target.x, rt.anchoredPosition.x, 0.1f);
            Assert.AreEqual(target.y, rt.anchoredPosition.y, 0.1f);
        }
    }
}
