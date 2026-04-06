using System.Collections;
using NUnit.Framework;
using RogueliteAutoBattler.Combat.Visuals;
using UnityEngine;
using UnityEngine.TestTools;

namespace RogueliteAutoBattler.Tests.PlayMode
{
    public class FadeOverlayTests : PlayModeTestBase
    {
        private FadeOverlay _overlay;
        private CanvasGroup _canvasGroup;

        [SetUp]
        public void SetUp()
        {
            var go = new GameObject("TestFadeOverlay");
            Track(go);
            _canvasGroup = go.AddComponent<CanvasGroup>();
            _overlay = go.AddComponent<FadeOverlay>();
            _overlay.SetFadeDurationForTest(0.1f);
            _overlay.InitializeForTest();
        }

        [UnityTest]
        public IEnumerator FadeIn_AlphaReachesOne()
        {
            _overlay.FadeIn();

            yield return new WaitForSecondsRealtime(0.2f);

            Assert.AreEqual(1f, _overlay.Alpha, 0.01f);
        }

        [UnityTest]
        public IEnumerator FadeOut_AlphaReachesZero()
        {
            _canvasGroup.alpha = 1f;

            _overlay.FadeOut();

            yield return new WaitForSecondsRealtime(0.2f);

            Assert.AreEqual(0f, _overlay.Alpha, 0.01f);
        }

        [UnityTest]
        public IEnumerator FadeIn_BlocksRaycasts_WhenOpaque()
        {
            _overlay.FadeIn();

            yield return new WaitForSecondsRealtime(0.2f);

            Assert.IsTrue(_canvasGroup.blocksRaycasts);
        }

        [UnityTest]
        public IEnumerator FadeOut_UnblocksRaycasts_WhenTransparent()
        {
            _overlay.FadeIn();

            yield return new WaitForSecondsRealtime(0.2f);

            _overlay.FadeOut();

            yield return new WaitForSecondsRealtime(0.2f);

            Assert.IsFalse(_canvasGroup.blocksRaycasts);
        }

        [Test]
        public void InitialState_AlphaIsZero_AndRaycastsUnblocked()
        {
            Assert.AreEqual(0f, _overlay.Alpha);
            Assert.IsFalse(_canvasGroup.blocksRaycasts);
        }
    }
}
