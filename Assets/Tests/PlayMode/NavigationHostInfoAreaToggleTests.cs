#if UNITY_EDITOR
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using RogueliteAutoBattler.UI.Toolkit;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace RogueliteAutoBattler.Tests.PlayMode
{
    public class NavigationHostInfoAreaToggleTests : PlayModeTestBase
    {
        private const string PanelSettingsPath = "Assets/UI/MainPanelSettings.asset";
        private const string MainLayoutPath = "Assets/UI/Layouts/MainLayout.uxml";
        private const string UiDocumentFieldName = "_uiDocument";
        private const string HiddenClassName = "hidden";
        private const int SkillTreeTabIndex = 1;

        [SetUp]
        public void SetUp()
        {
            if (NavigationHost.Instance != null)
            {
                Object.DestroyImmediate(NavigationHost.Instance.gameObject);
            }

            ForceResetStaticInstance();
        }

        [TearDown]
        public override void TearDown()
        {
            if (NavigationHost.Instance != null)
            {
                Object.DestroyImmediate(NavigationHost.Instance.gameObject);
            }

            ForceResetStaticInstance();
            base.TearDown();
        }

        [UnityTest]
        public IEnumerator InfoArea_StartsVisible_OnDefaultScreen()
        {
            NavigationHost host = CreateHostWithMainLayout();
            yield return null;

            Assert.IsNotNull(host.InfoAreaElement,
                "InfoAreaElement must be cached after BuildNavigation runs against MainLayout.uxml.");
            Assert.IsFalse(host.InfoAreaElement.ClassListContains(HiddenClassName),
                "info-area must NOT have the hidden class on the default screen (no tab selected).");
        }

        [UnityTest]
        public IEnumerator SwitchTab_NonDefault_AddsHiddenClassToInfoArea()
        {
            NavigationHost host = CreateHostWithMainLayout();
            yield return null;

            host.Navigation.SwitchTab(SkillTreeTabIndex);
            yield return null;

            Assert.IsTrue(host.InfoAreaElement.ClassListContains(HiddenClassName),
                "info-area must have the hidden class once a non-default tab is active.");
        }

        [UnityTest]
        public IEnumerator ReturnToDefault_RemovesHiddenClassFromInfoArea()
        {
            NavigationHost host = CreateHostWithMainLayout();
            yield return null;

            host.Navigation.SwitchTab(SkillTreeTabIndex);
            yield return null;

            host.Navigation.ReturnToDefault();
            yield return null;

            Assert.IsFalse(host.InfoAreaElement.ClassListContains(HiddenClassName),
                "info-area must have the hidden class removed once navigation returns to the default screen.");
        }

        private NavigationHost CreateHostWithMainLayout()
        {
            PanelSettings panelSettings = AssetDatabase.LoadAssetAtPath<PanelSettings>(PanelSettingsPath);
            Assert.IsNotNull(panelSettings, $"MainPanelSettings must exist at {PanelSettingsPath}.");

            VisualTreeAsset visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(MainLayoutPath);
            Assert.IsNotNull(visualTree, $"MainLayout must exist at {MainLayoutPath}.");

            GameObject hostGo = Track(new GameObject("NavigationHostMainLayout"));
            hostGo.SetActive(false);

            UIDocument uiDocument = hostGo.AddComponent<UIDocument>();
            uiDocument.panelSettings = panelSettings;
            uiDocument.visualTreeAsset = visualTree;

            NavigationHost host = hostGo.AddComponent<NavigationHost>();
            SetPrivateField(host, UiDocumentFieldName, uiDocument);

            hostGo.SetActive(true);
            return host;
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            field?.SetValue(target, value);
        }

        private static void ForceResetStaticInstance()
        {
            PropertyInfo prop = typeof(NavigationHost).GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
            prop?.SetValue(null, null);
        }
    }
}
#endif
