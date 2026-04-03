using System.Collections;
using NUnit.Framework;
using RogueliteAutoBattler.UI.Core;
using UnityEngine;
using UnityEngine.TestTools;

namespace RogueliteAutoBattler.Tests.PlayMode
{
    public class UIScreenTests : PlayModeTestBase
    {
        private UIScreen CreateUIScreen(string name = "TestScreen")
        {
            var go = Track(new GameObject(name));
            go.AddComponent<CanvasGroup>();
            return go.AddComponent<UIScreen>();
        }

        [UnityTest]
        public IEnumerator OnShow_SetsAlphaToOne()
        {
            var screen = CreateUIScreen();
            yield return null;

            screen.OnShow();

            Assert.AreEqual(1f, screen.GetComponent<CanvasGroup>().alpha);
        }

        [UnityTest]
        public IEnumerator OnShow_EnablesBlocksRaycasts()
        {
            var screen = CreateUIScreen();
            yield return null;

            screen.OnShow();

            Assert.IsTrue(screen.GetComponent<CanvasGroup>().blocksRaycasts);
        }

        [UnityTest]
        public IEnumerator OnShow_EnablesInteractable()
        {
            var screen = CreateUIScreen();
            yield return null;

            screen.OnShow();

            Assert.IsTrue(screen.GetComponent<CanvasGroup>().interactable);
        }

        [UnityTest]
        public IEnumerator OnHide_SetsAlphaToZero()
        {
            var screen = CreateUIScreen();
            yield return null;

            screen.OnHide();

            Assert.AreEqual(0f, screen.GetComponent<CanvasGroup>().alpha);
        }

        [UnityTest]
        public IEnumerator OnHide_DisablesBlocksRaycasts()
        {
            var screen = CreateUIScreen();
            yield return null;

            screen.OnHide();

            Assert.IsFalse(screen.GetComponent<CanvasGroup>().blocksRaycasts);
        }

        [UnityTest]
        public IEnumerator OnHide_DisablesInteractable()
        {
            var screen = CreateUIScreen();
            yield return null;

            screen.OnHide();

            Assert.IsFalse(screen.GetComponent<CanvasGroup>().interactable);
        }

        [UnityTest]
        public IEnumerator OnPush_HidesScreen()
        {
            var screen = CreateUIScreen();
            yield return null;

            screen.OnShow();
            screen.OnPush();

            var canvasGroup = screen.GetComponent<CanvasGroup>();
            Assert.AreEqual(0f, canvasGroup.alpha);
            Assert.IsFalse(canvasGroup.blocksRaycasts);
            Assert.IsFalse(canvasGroup.interactable);
        }

        [UnityTest]
        public IEnumerator OnPop_ShowsScreen()
        {
            var screen = CreateUIScreen();
            yield return null;

            screen.OnHide();
            screen.OnPop();

            var canvasGroup = screen.GetComponent<CanvasGroup>();
            Assert.AreEqual(1f, canvasGroup.alpha);
            Assert.IsTrue(canvasGroup.blocksRaycasts);
            Assert.IsTrue(canvasGroup.interactable);
        }
    }
}
