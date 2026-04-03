using System.Collections;
using System.Reflection;
using NUnit.Framework;
using RogueliteAutoBattler.Tests;
using RogueliteAutoBattler.UI.Core;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace RogueliteAutoBattler.Tests.PlayMode
{
    public class NavigationManagerTests : PlayModeTestBase
    {
        private UIScreen _defaultScreen;
        private UIScreen _rootScreen;
        private TabButton _tabButton;
        private NavigationManager _nav;

        [SetUp]
        public void SetUp()
        {
            _defaultScreen = CreateUIScreen("DefaultScreen");
            _rootScreen = CreateUIScreen("RootScreen0");
            _tabButton = CreateTabButton("Tab0");
            _nav = CreateNavigationManager(
                new[] { _rootScreen },
                _defaultScreen,
                new[] { _tabButton });
        }

        public override void TearDown()
        {
            if (NavigationManager.Instance != null)
            {
                Object.DestroyImmediate(NavigationManager.Instance.gameObject);
            }

            base.TearDown();
        }

        private UIScreen CreateUIScreen(string name = "TestScreen")
        {
            return Track(TestCharacterFactory.CreateUIScreen(name)).GetComponent<UIScreen>();
        }

        private TabButton CreateTabButton(string name = "TabButton")
        {
            var go = Track(new GameObject(name));
            go.AddComponent<Image>();
            return go.AddComponent<TabButton>();
        }

        private NavigationManager CreateNavigationManager(
            UIScreen[] rootScreens,
            UIScreen defaultScreen,
            TabButton[] tabButtons,
            UIScreen defaultInfoScreen = null,
            UIScreen[] infoScreens = null)
        {
            var go = new GameObject("NavigationManager");
            go.SetActive(false);
            Track(go);

            var nav = go.AddComponent<NavigationManager>();

            SetPrivateField(nav, "_defaultScreen", defaultScreen);
            SetPrivateField(nav, "_defaultInfoScreen", defaultInfoScreen);
            SetPrivateField(nav, "_tabButtons", tabButtons);
            SetPrivateField(nav, "_rootScreens", rootScreens);
            SetPrivateField(nav, "_infoScreens", infoScreens);
            SetPrivateField(nav, "_cancelAction", null);

            go.SetActive(true);

            return nav;
        }

        private static void SetPrivateField(object obj, string fieldName, object value)
        {
            var field = obj.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(obj, value);
        }

        [UnityTest]
        public IEnumerator SwitchTab_ChangesCurrentTab()
        {
            yield return null;

            _nav.SwitchTab(0);

            Assert.AreEqual(0, _nav.CurrentTab);
        }

        [UnityTest]
        public IEnumerator SwitchTab_ShowsRootScreen()
        {
            yield return null;

            _nav.SwitchTab(0);

            Assert.AreEqual(1f, _rootScreen.GetComponent<CanvasGroup>().alpha);
        }

        [UnityTest]
        public IEnumerator SwitchTab_SameTab_ReturnsToDefault()
        {
            yield return null;

            _nav.SwitchTab(0);
            _nav.SwitchTab(0);

            Assert.AreEqual(-1, _nav.CurrentTab);
        }

        [UnityTest]
        public IEnumerator ReturnToDefault_SetsTabToMinusOne()
        {
            yield return null;

            _nav.SwitchTab(0);
            _nav.ReturnToDefault();

            Assert.AreEqual(-1, _nav.CurrentTab);
        }

        [UnityTest]
        public IEnumerator ReturnToDefault_ShowsDefaultScreen()
        {
            yield return null;

            _nav.SwitchTab(0);
            _nav.ReturnToDefault();

            Assert.AreEqual(1f, _defaultScreen.GetComponent<CanvasGroup>().alpha);
        }

        [UnityTest]
        public IEnumerator OnTabChanged_FiresEvent()
        {
            yield return null;

            int receivedTabIndex = -99;
            _nav.OnTabChanged += index => receivedTabIndex = index;

            _nav.SwitchTab(0);

            Assert.AreEqual(0, receivedTabIndex);
        }

        [UnityTest]
        public IEnumerator PushScreen_AddsToCurrentStack()
        {
            var pushedScreen = CreateUIScreen("PushedScreen");

            yield return null;

            _nav.SwitchTab(0);
            _nav.PushScreen(pushedScreen);

            Assert.AreEqual(1f, pushedScreen.GetComponent<CanvasGroup>().alpha);
        }

        [UnityTest]
        public IEnumerator PopScreen_ReturnsToRootScreen()
        {
            var pushedScreen = CreateUIScreen("PushedScreen");

            yield return null;

            _nav.SwitchTab(0);
            _nav.PushScreen(pushedScreen);
            _nav.PopScreen();

            Assert.AreEqual(1f, _rootScreen.GetComponent<CanvasGroup>().alpha);
            Assert.AreEqual(0f, pushedScreen.GetComponent<CanvasGroup>().alpha);
        }
    }
}
