using System.Collections;
using System.Reflection;
using NUnit.Framework;
using RogueliteAutoBattler.UI.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RogueliteAutoBattler.Tests.PlayMode
{
    /// <summary>
    /// Shared base for NavigationManager integration tests.
    /// Creates the full UI hierarchy (Canvas, EventSystem, screens, tabs)
    /// and exposes helpers for screen assertions and reflection-based field setup.
    /// </summary>
    public abstract class NavigationTestBase : PlayModeTestBase
    {
        protected const int TabCount = 3;

        protected NavigationManager NavigationManager;
        protected UIScreen DefaultScreen;
        protected UIScreen DefaultInfoScreen;
        protected UIScreen[] RootScreens;
        protected UIScreen[] InfoScreens;
        protected TabButton[] TabButtons;

        /// <summary>
        /// Builds the NavigationManager with all screens and tabs.
        /// Call from [UnitySetUp] in derived classes.
        /// Pass a non-null cancelAction to wire up input-based cancel.
        /// Returns after one frame so Awake and Start have both executed.
        /// </summary>
        protected IEnumerator SetUpNavigation(Object cancelAction = null)
        {
            ClearSingleton();

            // Canvas + EventSystem to avoid UI warnings.
            var canvasGo = Track(new GameObject("TestCanvas"));
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<GraphicRaycaster>();

            var eventSystemGo = Track(new GameObject("TestEventSystem"));
            eventSystemGo.AddComponent<EventSystem>();

            // Create default screens.
            DefaultScreen = CreateUIScreen("DefaultScreen");
            DefaultInfoScreen = CreateUIScreen("DefaultInfoScreen");

            // Create tab root screens, info screens, and tab buttons.
            RootScreens = new UIScreen[TabCount];
            InfoScreens = new UIScreen[TabCount];
            TabButtons = new TabButton[TabCount];

            for (int i = 0; i < TabCount; i++)
            {
                RootScreens[i] = CreateUIScreen($"RootScreen_{i}");
                InfoScreens[i] = CreateUIScreen($"InfoScreen_{i}");
                TabButtons[i] = CreateTabButton($"TabButton_{i}", i, canvasGo.transform);
            }

            // Create NavigationManager INACTIVE so we control when Awake runs.
            var navGo = Track(new GameObject("NavigationManager"));
            navGo.SetActive(false);

            NavigationManager = navGo.AddComponent<NavigationManager>();

            // Set private serialized fields via reflection.
            SetPrivateField(NavigationManager, "_defaultScreen", DefaultScreen);
            SetPrivateField(NavigationManager, "_defaultInfoScreen", DefaultInfoScreen);
            SetPrivateField(NavigationManager, "_tabButtons", TabButtons);
            SetPrivateField(NavigationManager, "_rootScreens", RootScreens);
            SetPrivateField(NavigationManager, "_infoScreens", InfoScreens);
            SetPrivateField(NavigationManager, "_cancelAction", cancelAction);

            // Activate — triggers Awake and OnEnable, then Start next frame.
            navGo.SetActive(true);

            // Wait one frame so both Awake and Start execute.
            yield return null;
        }

        // ----- Helpers -----

        /// <summary>
        /// Creates a minimal UIScreen with a CanvasGroup on a tracked GameObject.
        /// </summary>
        protected UIScreen CreateUIScreen(string screenName)
        {
            var go = Track(new GameObject(screenName));
            go.AddComponent<CanvasGroup>();
            var screen = go.AddComponent<UIScreen>();
            return screen;
        }

        /// <summary>
        /// Creates a TabButton with the required Button component, parented under a Canvas.
        /// Sets the private _tabIndex field via reflection.
        /// </summary>
        protected TabButton CreateTabButton(string buttonName, int tabIndex, Transform parent)
        {
            var go = Track(new GameObject(buttonName));
            go.transform.SetParent(parent, false);
            go.AddComponent<Button>();
            var tab = go.AddComponent<TabButton>();
            SetPrivateField(tab, "_tabIndex", tabIndex);
            return tab;
        }

        /// <summary>
        /// Returns the CanvasGroup alpha of a UIScreen.
        /// </summary>
        protected static float GetAlpha(UIScreen screen)
        {
            return screen.GetComponent<CanvasGroup>().alpha;
        }

        /// <summary>
        /// Sets a private or serialized field via reflection.
        /// </summary>
        protected static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null,
                $"Field '{fieldName}' not found on {target.GetType().Name}.");
            field.SetValue(target, value);
        }

        /// <summary>
        /// Clears the NavigationManager singleton so a fresh instance can be created.
        /// Needed because Object.Destroy is deferred and OnDestroy may not have run yet.
        /// </summary>
        protected static void ClearSingleton()
        {
            var field = typeof(NavigationManager).GetField("<Instance>k__BackingField",
                BindingFlags.Static | BindingFlags.NonPublic);
            field?.SetValue(null, null);
        }
    }
}
