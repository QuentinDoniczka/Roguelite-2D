using System.Collections;
using System.Reflection;
using NUnit.Framework;
using RogueliteAutoBattler.UI.Core;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace RogueliteAutoBattler.Tests.PlayMode
{
    public class NavigationManagerTests : PlayModeTestBase
    {
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
            var go = Track(new GameObject(name));
            go.AddComponent<CanvasGroup>();
            return go.AddComponent<UIScreen>();
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
            var defaultScreen = CreateUIScreen("DefaultScreen");
            var rootScreen = CreateUIScreen("RootScreen0");
            var tabButton = CreateTabButton("Tab0");

            var nav = CreateNavigationManager(
                new[] { rootScreen },
                defaultScreen,
                new[] { tabButton });

            yield return null;

            nav.SwitchTab(0);

            Assert.AreEqual(0, nav.CurrentTab);
        }

        [UnityTest]
        public IEnumerator SwitchTab_ShowsRootScreen()
        {
            var defaultScreen = CreateUIScreen("DefaultScreen");
            var rootScreen = CreateUIScreen("RootScreen0");
            var tabButton = CreateTabButton("Tab0");

            var nav = CreateNavigationManager(
                new[] { rootScreen },
                defaultScreen,
                new[] { tabButton });

            yield return null;

            nav.SwitchTab(0);

            Assert.AreEqual(1f, rootScreen.GetComponent<CanvasGroup>().alpha);
        }

        [UnityTest]
        public IEnumerator SwitchTab_SameTab_ReturnsToDefault()
        {
            var defaultScreen = CreateUIScreen("DefaultScreen");
            var rootScreen = CreateUIScreen("RootScreen0");
            var tabButton = CreateTabButton("Tab0");

            var nav = CreateNavigationManager(
                new[] { rootScreen },
                defaultScreen,
                new[] { tabButton });

            yield return null;

            nav.SwitchTab(0);
            nav.SwitchTab(0);

            Assert.AreEqual(-1, nav.CurrentTab);
        }

        [UnityTest]
        public IEnumerator ReturnToDefault_SetsTabToMinusOne()
        {
            var defaultScreen = CreateUIScreen("DefaultScreen");
            var rootScreen = CreateUIScreen("RootScreen0");
            var tabButton = CreateTabButton("Tab0");

            var nav = CreateNavigationManager(
                new[] { rootScreen },
                defaultScreen,
                new[] { tabButton });

            yield return null;

            nav.SwitchTab(0);
            nav.ReturnToDefault();

            Assert.AreEqual(-1, nav.CurrentTab);
        }

        [UnityTest]
        public IEnumerator ReturnToDefault_ShowsDefaultScreen()
        {
            var defaultScreen = CreateUIScreen("DefaultScreen");
            var rootScreen = CreateUIScreen("RootScreen0");
            var tabButton = CreateTabButton("Tab0");

            var nav = CreateNavigationManager(
                new[] { rootScreen },
                defaultScreen,
                new[] { tabButton });

            yield return null;

            nav.SwitchTab(0);
            nav.ReturnToDefault();

            Assert.AreEqual(1f, defaultScreen.GetComponent<CanvasGroup>().alpha);
        }

        [UnityTest]
        public IEnumerator OnTabChanged_FiresEvent()
        {
            var defaultScreen = CreateUIScreen("DefaultScreen");
            var rootScreen = CreateUIScreen("RootScreen0");
            var tabButton = CreateTabButton("Tab0");

            var nav = CreateNavigationManager(
                new[] { rootScreen },
                defaultScreen,
                new[] { tabButton });

            yield return null;

            int receivedTabIndex = -99;
            nav.OnTabChanged += index => receivedTabIndex = index;

            nav.SwitchTab(0);

            Assert.AreEqual(0, receivedTabIndex);
        }

        [UnityTest]
        public IEnumerator PushScreen_AddsToCurrentStack()
        {
            var defaultScreen = CreateUIScreen("DefaultScreen");
            var rootScreen = CreateUIScreen("RootScreen0");
            var tabButton = CreateTabButton("Tab0");
            var pushedScreen = CreateUIScreen("PushedScreen");

            var nav = CreateNavigationManager(
                new[] { rootScreen },
                defaultScreen,
                new[] { tabButton });

            yield return null;

            nav.SwitchTab(0);
            nav.PushScreen(pushedScreen);

            Assert.AreEqual(1f, pushedScreen.GetComponent<CanvasGroup>().alpha);
        }

        [UnityTest]
        public IEnumerator PopScreen_ReturnsToRootScreen()
        {
            var defaultScreen = CreateUIScreen("DefaultScreen");
            var rootScreen = CreateUIScreen("RootScreen0");
            var tabButton = CreateTabButton("Tab0");
            var pushedScreen = CreateUIScreen("PushedScreen");

            var nav = CreateNavigationManager(
                new[] { rootScreen },
                defaultScreen,
                new[] { tabButton });

            yield return null;

            nav.SwitchTab(0);
            nav.PushScreen(pushedScreen);
            nav.PopScreen();

            Assert.AreEqual(1f, rootScreen.GetComponent<CanvasGroup>().alpha);
            Assert.AreEqual(0f, pushedScreen.GetComponent<CanvasGroup>().alpha);
        }
    }
}
