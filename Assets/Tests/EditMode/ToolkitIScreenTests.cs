using NUnit.Framework;
using RogueliteAutoBattler.UI.Toolkit;
using UnityEngine.UIElements;

namespace RogueliteAutoBattler.Tests.EditMode
{
    [TestFixture]
    public class ToolkitIScreenTests
    {
        private StubScreen _stub;

        [SetUp]
        public void SetUp()
        {
            _stub = new StubScreen();
        }

        [Test]
        public void StubScreen_Root_ReturnsVisualElement()
        {
            Assert.IsNotNull(_stub.Root);
        }

        [Test]
        public void StubScreen_OnShow_RemovesHiddenClass()
        {
            _stub.Root.AddToClassList("hidden");

            _stub.OnShow();

            Assert.IsFalse(_stub.Root.ClassListContains("hidden"));
        }

        [Test]
        public void StubScreen_OnHide_AddsHiddenClass()
        {
            _stub.OnHide();

            Assert.IsTrue(_stub.Root.ClassListContains("hidden"));
        }

        [Test]
        public void StubScreen_OnPush_CallsOnHide()
        {
            _stub.OnPush();

            Assert.AreEqual(1, _stub.HideCount);
        }

        [Test]
        public void StubScreen_OnPop_CallsOnShow()
        {
            _stub.OnPop();

            Assert.AreEqual(1, _stub.ShowCount);
        }

        private class StubScreen : IScreen
        {
            public VisualElement Root { get; }
            public int ShowCount { get; private set; }
            public int HideCount { get; private set; }
            public int PushCount { get; private set; }
            public int PopCount { get; private set; }

            public StubScreen()
            {
                Root = new VisualElement();
            }

            public void OnShow()
            {
                ShowCount++;
                Root.RemoveFromClassList("hidden");
            }

            public void OnHide()
            {
                HideCount++;
                Root.AddToClassList("hidden");
            }

            public void OnPush()
            {
                PushCount++;
                OnHide();
            }

            public void OnPop()
            {
                PopCount++;
                OnShow();
            }
        }
    }
}
