using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TestTools;

namespace RogueliteAutoBattler.Tests.PlayMode
{
    /// <summary>
    /// Integration tests for the Cancel (Escape) input action in NavigationManager.
    /// Uses InputTestFixture to simulate keyboard input.
    /// </summary>
    public class CancelActionTests : NavigationTestBase
    {
        private InputTestFixture _inputFixture;
        private Keyboard _keyboard;
        private InputActionAsset _inputAsset;
        private InputActionReference _cancelRef;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            // Set up InputTestFixture and virtual keyboard.
            _inputFixture = new InputTestFixture();
            _inputFixture.Setup();
            _keyboard = InputSystem.AddDevice<Keyboard>();

            // Create InputActionAsset with a Cancel action bound to Escape.
            _inputAsset = ScriptableObject.CreateInstance<InputActionAsset>();
            var map = _inputAsset.AddActionMap("UI");
            var cancelAction = map.AddAction("Cancel", type: InputActionType.Button);
            cancelAction.AddBinding("<Keyboard>/escape");

            _cancelRef = InputActionReference.Create(cancelAction);

            // Build the full NavigationManager hierarchy with cancel wired up.
            yield return SetUpNavigation(_cancelRef);
        }

        [TearDown]
        public void InputTearDown()
        {
            if (_cancelRef != null)
                Object.Destroy(_cancelRef);
            if (_inputAsset != null)
                Object.Destroy(_inputAsset);

            _inputFixture?.TearDown();
            _inputFixture = null;
            _keyboard = null;
        }

        [UnityTest]
        public IEnumerator Escape_ReturnsFromTabToDefault()
        {
            yield return null;

            NavigationManager.SwitchTab(0);

            Assert.That(NavigationManager.CurrentTab, Is.EqualTo(0),
                "CurrentTab should be 0 after SwitchTab(0).");

            PressAndReleaseEscape();
            yield return null;

            Assert.That(NavigationManager.CurrentTab, Is.EqualTo(-1),
                "CurrentTab should be -1 after pressing Escape from a tab.");
            Assert.That(GetAlpha(DefaultScreen), Is.EqualTo(1f).Within(0.01f),
                "Default screen should be visible after Escape returns to default.");
        }

        [UnityTest]
        public IEnumerator Escape_PopsSubScreen_BeforeReturningToDefault()
        {
            yield return null;

            NavigationManager.SwitchTab(0);

            // Push a sub-screen on top of the root screen for tab 0.
            var subScreen = CreateUIScreen("SubScreen");
            NavigationManager.PushScreen(subScreen);

            Assert.That(GetAlpha(subScreen), Is.EqualTo(1f).Within(0.01f),
                "Sub-screen should be visible after push.");

            // First Escape: pop the sub-screen, root screen returns.
            PressAndReleaseEscape();
            yield return null;

            Assert.That(GetAlpha(subScreen), Is.EqualTo(0f).Within(0.01f),
                "Sub-screen should be hidden after first Escape.");
            Assert.That(GetAlpha(RootScreens[0]), Is.EqualTo(1f).Within(0.01f),
                "Root screen 0 should be visible after popping sub-screen.");
            Assert.That(NavigationManager.CurrentTab, Is.EqualTo(0),
                "CurrentTab should still be 0 after popping sub-screen.");

            // Second Escape: return from tab to default.
            PressAndReleaseEscape();
            yield return null;

            Assert.That(NavigationManager.CurrentTab, Is.EqualTo(-1),
                "CurrentTab should be -1 after second Escape.");
            Assert.That(GetAlpha(DefaultScreen), Is.EqualTo(1f).Within(0.01f),
                "Default screen should be visible after second Escape.");
        }

        [UnityTest]
        public IEnumerator Escape_WhenAlreadyDefault_DoesNothing()
        {
            yield return null;

            Assert.That(NavigationManager.CurrentTab, Is.EqualTo(-1),
                "CurrentTab should be -1 at start.");

            PressAndReleaseEscape();
            yield return null;

            Assert.That(NavigationManager.CurrentTab, Is.EqualTo(-1),
                "CurrentTab should remain -1 after Escape when already on default.");
            Assert.That(GetAlpha(DefaultScreen), Is.EqualTo(1f).Within(0.01f),
                "Default screen should still be visible after Escape on default.");
        }

        // ----- Helpers -----

        private void PressAndReleaseEscape()
        {
            _inputFixture.Press(_keyboard.escapeKey);
            _inputFixture.Release(_keyboard.escapeKey);
        }
    }
}
