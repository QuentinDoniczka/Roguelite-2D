using System.Collections;
using NUnit.Framework;
using RogueliteAutoBattler.Tests;
using RogueliteAutoBattler.UI.Core;
using UnityEngine;
using UnityEngine.TestTools;

namespace RogueliteAutoBattler.Tests.PlayMode
{
    public class ScreenStackTests : PlayModeTestBase
    {
        private ScreenStack _stack;
        private UIScreen _root;

        [SetUp]
        public void SetUp()
        {
            _root = CreateScreen("Root");
            _stack = new ScreenStack(_root);
        }

        [UnityTest]
        public IEnumerator Constructor_SetsRootAsCurrent()
        {
            yield return null;

            Assert.AreEqual(_root, _stack.Current);
            Assert.AreEqual(1, _stack.Count);
        }

        [UnityTest]
        public IEnumerator Push_NewScreenBecomesCurrent()
        {
            yield return null;

            var screen = CreateScreen("Pushed");

            _stack.Push(screen);

            Assert.AreEqual(screen, _stack.Current);
            Assert.AreEqual(2, _stack.Count);
        }

        [UnityTest]
        public IEnumerator Push_HidesPreviousScreen()
        {
            yield return null;

            var screen = CreateScreen("Pushed");
            _stack.Push(screen);

            var rootCanvasGroup = _root.GetComponent<CanvasGroup>();
            Assert.AreEqual(0f, rootCanvasGroup.alpha, 0.001f);
            Assert.IsFalse(rootCanvasGroup.blocksRaycasts);
            Assert.IsFalse(rootCanvasGroup.interactable);
        }

        [UnityTest]
        public IEnumerator Pop_ReturnsToPreviousScreen()
        {
            yield return null;

            var screen = CreateScreen("Pushed");
            _stack.Push(screen);

            UIScreen popped = _stack.Pop();

            Assert.AreEqual(screen, popped);
            Assert.AreEqual(_root, _stack.Current);
            Assert.AreEqual(1, _stack.Count);
        }

        [UnityTest]
        public IEnumerator Pop_OnRootOnly_ReturnsNull()
        {
            yield return null;

            UIScreen result = _stack.Pop();

            Assert.IsNull(result);
            Assert.AreEqual(1, _stack.Count);
            Assert.AreEqual(_root, _stack.Current);
        }

        [UnityTest]
        public IEnumerator Clear_RemovesAllButRoot()
        {
            yield return null;

            _stack.Push(CreateScreen("Screen1"));
            _stack.Push(CreateScreen("Screen2"));
            _stack.Push(CreateScreen("Screen3"));
            Assert.AreEqual(4, _stack.Count);

            _stack.Clear();

            Assert.AreEqual(1, _stack.Count);
            Assert.AreEqual(_root, _stack.Current);
        }

        [UnityTest]
        public IEnumerator ShowCurrent_SetsAlphaToOne()
        {
            yield return null;

            _stack.HideCurrent();
            _stack.ShowCurrent();

            var canvasGroup = _root.GetComponent<CanvasGroup>();
            Assert.AreEqual(1f, canvasGroup.alpha, 0.001f);
            Assert.IsTrue(canvasGroup.blocksRaycasts);
            Assert.IsTrue(canvasGroup.interactable);
        }

        [UnityTest]
        public IEnumerator HideCurrent_SetsAlphaToZero()
        {
            yield return null;

            _stack.HideCurrent();

            var canvasGroup = _root.GetComponent<CanvasGroup>();
            Assert.AreEqual(0f, canvasGroup.alpha, 0.001f);
            Assert.IsFalse(canvasGroup.blocksRaycasts);
            Assert.IsFalse(canvasGroup.interactable);
        }

        private UIScreen CreateScreen(string name = "Screen")
        {
            return Track(TestCharacterFactory.CreateUIScreen(name)).GetComponent<UIScreen>();
        }
    }
}
