using System;
using NUnit.Framework;
using RogueliteAutoBattler.UI.Toolkit;
using UnityEngine.UIElements;

namespace RogueliteAutoBattler.Tests.EditMode
{
    [TestFixture]
    public class ToolkitNavigationManagerTests
    {
        private const int TAB_COUNT = 5;

        private Button[] _buttons;
        private StubScreen _defaultScreen;
        private NavigationManager _navigationManager;

        [SetUp]
        public void SetUp()
        {
            _buttons = new Button[TAB_COUNT];
            for (int i = 0; i < TAB_COUNT; i++)
            {
                _buttons[i] = new Button();
                _buttons[i].AddToClassList("tab-inactive");
            }

            _defaultScreen = new StubScreen();
            _navigationManager = new NavigationManager(_buttons, _defaultScreen);
        }

        [TearDown]
        public void TearDown()
        {
            _navigationManager.Dispose();
        }

        [Test]
        public void Constructor_DefaultTabIsMinusOne()
        {
            Assert.AreEqual(-1, _navigationManager.CurrentTab);
        }

        [Test]
        public void Constructor_ShowsDefaultScreen()
        {
            Assert.AreEqual(1, _defaultScreen.ShowCount);
        }

        [Test]
        public void SwitchTab_ChangesCurrentTab()
        {
            _navigationManager.RegisterTab(0, new StubScreen());

            _navigationManager.SwitchTab(0);

            Assert.AreEqual(0, _navigationManager.CurrentTab);
        }

        [Test]
        public void SwitchTab_HidesDefaultScreen()
        {
            _navigationManager.RegisterTab(0, new StubScreen());

            _navigationManager.SwitchTab(0);

            Assert.AreEqual(1, _defaultScreen.HideCount);
        }

        [Test]
        public void SwitchTab_ShowsTabScreen()
        {
            var screen = new StubScreen();
            _navigationManager.RegisterTab(0, screen);

            _navigationManager.SwitchTab(0);

            Assert.AreEqual(1, screen.ShowCount);
        }

        [Test]
        public void SwitchTab_ActivatesButton()
        {
            _navigationManager.RegisterTab(0, new StubScreen());

            _navigationManager.SwitchTab(0);

            Assert.IsTrue(_buttons[0].ClassListContains("tab-active"));
            Assert.IsFalse(_buttons[0].ClassListContains("tab-inactive"));
        }

        [Test]
        public void SwitchTab_DeactivatesPreviousButton()
        {
            _navigationManager.RegisterTab(0, new StubScreen());
            _navigationManager.RegisterTab(1, new StubScreen());

            _navigationManager.SwitchTab(0);
            _navigationManager.SwitchTab(1);

            Assert.IsTrue(_buttons[0].ClassListContains("tab-inactive"));
            Assert.IsFalse(_buttons[0].ClassListContains("tab-active"));
        }

        [Test]
        public void SwitchTab_SameIndex_ReturnsToDefault()
        {
            _navigationManager.RegisterTab(0, new StubScreen());

            _navigationManager.SwitchTab(0);
            _navigationManager.SwitchTab(0);

            Assert.AreEqual(-1, _navigationManager.CurrentTab);
        }

        [Test]
        public void ReturnToDefault_ShowsDefaultScreen()
        {
            _navigationManager.RegisterTab(0, new StubScreen());
            _navigationManager.SwitchTab(0);

            _navigationManager.ReturnToDefault();

            Assert.AreEqual(2, _defaultScreen.ShowCount);
        }

        [Test]
        public void ReturnToDefault_FiresEvent()
        {
            int receivedIndex = int.MaxValue;
            _navigationManager.OnTabChanged += index => receivedIndex = index;
            _navigationManager.RegisterTab(0, new StubScreen());
            _navigationManager.SwitchTab(0);

            _navigationManager.ReturnToDefault();

            Assert.AreEqual(-1, receivedIndex);
        }

        [Test]
        public void SwitchTab_FiresOnTabChanged()
        {
            int receivedIndex = -999;
            _navigationManager.OnTabChanged += index => receivedIndex = index;
            _navigationManager.RegisterTab(2, new StubScreen());

            _navigationManager.SwitchTab(2);

            Assert.AreEqual(2, receivedIndex);
        }

        [Test]
        public void PushScreen_DelegatesToStack()
        {
            var rootScreen = new StubScreen();
            var pushedScreen = new StubScreen();
            _navigationManager.RegisterTab(0, rootScreen);
            _navigationManager.SwitchTab(0);

            _navigationManager.PushScreen(pushedScreen);

            Assert.AreEqual(1, pushedScreen.ShowCount);
        }

        [Test]
        public void PopScreen_DelegatesToStack()
        {
            var rootScreen = new StubScreen();
            var pushedScreen = new StubScreen();
            _navigationManager.RegisterTab(0, rootScreen);
            _navigationManager.SwitchTab(0);
            _navigationManager.PushScreen(pushedScreen);

            IScreen popped = _navigationManager.PopScreen();

            Assert.AreEqual(pushedScreen, popped);
        }

        [Test]
        public void HandleCancel_PopsIfStackDeep()
        {
            var rootScreen = new StubScreen();
            var pushedScreen = new StubScreen();
            _navigationManager.RegisterTab(0, rootScreen);
            _navigationManager.SwitchTab(0);
            _navigationManager.PushScreen(pushedScreen);

            _navigationManager.HandleCancel();

            Assert.AreEqual(1, pushedScreen.HideCount);
        }

        [Test]
        public void HandleCancel_ReturnsToDefaultIfStackIsOne()
        {
            var rootScreen = new StubScreen();
            _navigationManager.RegisterTab(0, rootScreen);
            _navigationManager.SwitchTab(0);

            _navigationManager.HandleCancel();

            Assert.AreEqual(-1, _navigationManager.CurrentTab);
        }

        [Test]
        public void HandleCancel_DoesNothingAtDefault()
        {
            _navigationManager.HandleCancel();

            Assert.AreEqual(-1, _navigationManager.CurrentTab);
        }

        [Test]
        public void SwitchTab_UnregisteredTab_DoesNotCrash()
        {
            Assert.DoesNotThrow(() => _navigationManager.SwitchTab(3));
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
