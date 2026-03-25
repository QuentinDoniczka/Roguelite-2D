using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace RogueliteAutoBattler.Tests.PlayMode
{
    /// <summary>
    /// Integration tests for NavigationManager tab/screen navigation.
    /// All UI is created programmatically — no scene dependency.
    /// </summary>
    public class NavigationManagerTests : NavigationTestBase
    {
        [UnitySetUp]
        public IEnumerator SetUp()
        {
            yield return SetUpNavigation();
        }

        [UnityTest]
        public IEnumerator DefaultScreen_IsVisible_OnStart()
        {
            yield return null;

            Assert.That(GetAlpha(DefaultScreen), Is.EqualTo(1f).Within(0.01f),
                "Default screen should be visible after Start.");

            for (int i = 0; i < TabCount; i++)
            {
                Assert.That(GetAlpha(RootScreens[i]), Is.EqualTo(0f).Within(0.01f),
                    $"Root screen {i} should be hidden on start.");
            }

            Assert.That(NavigationManager.CurrentTab, Is.EqualTo(-1),
                "CurrentTab should be -1 (no tab selected) on start.");
        }

        [UnityTest]
        public IEnumerator SwitchTab_ShowsCorrectScreen()
        {
            yield return null;

            NavigationManager.SwitchTab(0);

            Assert.That(GetAlpha(RootScreens[0]), Is.EqualTo(1f).Within(0.01f),
                "Root screen 0 should be visible after SwitchTab(0).");
            Assert.That(GetAlpha(DefaultScreen), Is.EqualTo(0f).Within(0.01f),
                "Default screen should be hidden after switching to a tab.");
            Assert.That(NavigationManager.CurrentTab, Is.EqualTo(0),
                "CurrentTab should be 0.");
        }

        [UnityTest]
        public IEnumerator SwitchTab_Toggle_ReturnToDefault()
        {
            yield return null;

            NavigationManager.SwitchTab(1);
            Assert.That(NavigationManager.CurrentTab, Is.EqualTo(1),
                "CurrentTab should be 1 after first SwitchTab(1).");

            // Toggle: clicking same tab again returns to default.
            NavigationManager.SwitchTab(1);

            Assert.That(GetAlpha(DefaultScreen), Is.EqualTo(1f).Within(0.01f),
                "Default screen should be visible after toggling tab off.");
            Assert.That(GetAlpha(RootScreens[1]), Is.EqualTo(0f).Within(0.01f),
                "Root screen 1 should be hidden after toggle.");
            Assert.That(NavigationManager.CurrentTab, Is.EqualTo(-1),
                "CurrentTab should be -1 after toggling tab off.");
        }

        [UnityTest]
        public IEnumerator SwitchTab_SwitchBetweenTabs()
        {
            yield return null;

            NavigationManager.SwitchTab(0);
            Assert.That(GetAlpha(RootScreens[0]), Is.EqualTo(1f).Within(0.01f),
                "Root screen 0 should be visible.");

            NavigationManager.SwitchTab(2);

            Assert.That(GetAlpha(RootScreens[2]), Is.EqualTo(1f).Within(0.01f),
                "Root screen 2 should be visible after switching.");
            Assert.That(GetAlpha(RootScreens[0]), Is.EqualTo(0f).Within(0.01f),
                "Root screen 0 should be hidden after switching to tab 2.");
            Assert.That(NavigationManager.CurrentTab, Is.EqualTo(2),
                "CurrentTab should be 2.");
        }

        [UnityTest]
        public IEnumerator PushAndPop_SubScreenNavigation()
        {
            yield return null;

            NavigationManager.SwitchTab(0);
            Assert.That(GetAlpha(RootScreens[0]), Is.EqualTo(1f).Within(0.01f),
                "Root screen 0 should be visible before push.");

            var subScreen = CreateUIScreen("SubScreen");

            NavigationManager.PushScreen(subScreen);

            Assert.That(GetAlpha(subScreen), Is.EqualTo(1f).Within(0.01f),
                "Sub-screen should be visible after push.");
            Assert.That(GetAlpha(RootScreens[0]), Is.EqualTo(0f).Within(0.01f),
                "Root screen 0 should be hidden after push (OnPush calls OnHide).");

            NavigationManager.PopScreen();

            Assert.That(GetAlpha(RootScreens[0]), Is.EqualTo(1f).Within(0.01f),
                "Root screen 0 should be visible after pop.");
            Assert.That(GetAlpha(subScreen), Is.EqualTo(0f).Within(0.01f),
                "Sub-screen should be hidden after pop.");
        }

        [UnityTest]
        public IEnumerator ReturnToDefault_DeselectsTabButton()
        {
            yield return null;

            NavigationManager.SwitchTab(0);
            Assert.That(TabButtons[0].IsSelected, Is.True,
                "Tab button 0 should be selected after SwitchTab(0).");

            NavigationManager.ReturnToDefault();

            Assert.That(TabButtons[0].IsSelected, Is.False,
                "Tab button 0 should be deselected after ReturnToDefault.");
            Assert.That(NavigationManager.CurrentTab, Is.EqualTo(-1),
                "CurrentTab should be -1 after ReturnToDefault.");
        }

        [UnityTest]
        public IEnumerator OnTabChanged_EventFires()
        {
            yield return null;

            int firedValue = int.MinValue;
            int fireCount = 0;
            NavigationManager.OnTabChanged += value =>
            {
                firedValue = value;
                fireCount++;
            };

            NavigationManager.SwitchTab(1);

            Assert.That(fireCount, Is.EqualTo(1),
                "OnTabChanged should have fired exactly once.");
            Assert.That(firedValue, Is.EqualTo(1),
                "OnTabChanged should have fired with index 1.");

            // Toggle off — should fire with -1.
            NavigationManager.SwitchTab(1);

            Assert.That(fireCount, Is.EqualTo(2),
                "OnTabChanged should have fired a second time on toggle.");
            Assert.That(firedValue, Is.EqualTo(-1),
                "OnTabChanged should have fired with -1 when returning to default.");
        }
    }
}
