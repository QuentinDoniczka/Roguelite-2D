using System.Collections;
using System.Reflection;
using NUnit.Framework;
using RogueliteAutoBattler.UI.Toolkit;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace RogueliteAutoBattler.Tests.PlayMode
{
    public class NavigationHostTests : PlayModeTestBase
    {
        private PanelSettings _panelSettings;

        [SetUp]
        public void SetUp()
        {
            if (NavigationHost.Instance != null)
            {
                Object.DestroyImmediate(NavigationHost.Instance.gameObject);
            }

            ForceResetStaticInstance();
            _panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
        }

        [TearDown]
        public override void TearDown()
        {
            if (NavigationHost.Instance != null)
            {
                Object.DestroyImmediate(NavigationHost.Instance.gameObject);
            }

            ForceResetStaticInstance();

            if (_panelSettings != null)
            {
                Object.DestroyImmediate(_panelSettings);
            }

            base.TearDown();
        }

        [UnityTest]
        public IEnumerator Instance_IsSetAfterAwake()
        {
            CreateHostWithEmptyDocument();
            yield return null;

            Assert.IsNotNull(NavigationHost.Instance);
        }

        [UnityTest]
        public IEnumerator Instance_IsClearedOnDestroy()
        {
            NavigationHost host = CreateHostWithEmptyDocument();
            yield return null;

            Assert.IsNotNull(NavigationHost.Instance);

            Object.DestroyImmediate(host.gameObject);
            yield return null;

            Assert.IsNull(NavigationHost.Instance);
        }

        [UnityTest]
        public IEnumerator DuplicateInstance_IsDestroyed()
        {
            NavigationHost first = CreateHostWithEmptyDocument();
            yield return null;

            var secondGo = new GameObject("SecondNavigationHost");
            secondGo.SetActive(false);
            Track(secondGo);
            var secondDoc = secondGo.AddComponent<UIDocument>();
            secondDoc.panelSettings = _panelSettings;
            var secondHost = secondGo.AddComponent<NavigationHost>();
            SetPrivateField(secondHost, "_uiDocument", secondDoc);
            secondGo.SetActive(true);
            yield return null;

            Assert.AreEqual(first, NavigationHost.Instance);
            Assert.IsTrue(secondGo == null);
        }

        [UnityTest]
        public IEnumerator Navigation_IsNullWithoutUIDocument()
        {
            var go = Track(new GameObject("HostNoDocument"));
            go.SetActive(false);
            go.AddComponent<NavigationHost>();
            go.SetActive(true);

            LogAssert.Expect(LogType.Warning, "[NavigationHost] UIDocument is not assigned.");
            yield return null;

            Assert.IsNull(NavigationHost.Instance.Navigation);
        }

        [UnityTest]
        public IEnumerator Navigation_IsNullWhenUIDocumentHasNoUxml()
        {
            NavigationHost host = CreateHostWithEmptyDocument();
            yield return null;

            Assert.IsNull(host.Navigation);
        }

        [UnityTest]
        public IEnumerator Navigation_IsCreatedWithFullVisualTree()
        {
            NavigationHost host = CreateHostWithFullVisualTree();
            yield return null;

            Assert.IsNotNull(host.Navigation);
            Assert.AreEqual(-1, host.Navigation.CurrentTab);
        }

        private NavigationHost CreateHostWithEmptyDocument()
        {
            var go = Track(new GameObject("TestNavigationHost"));
            go.SetActive(false);

            var doc = go.AddComponent<UIDocument>();
            doc.panelSettings = _panelSettings;

            var host = go.AddComponent<NavigationHost>();
            SetPrivateField(host, "_uiDocument", doc);

            go.SetActive(true);
            return host;
        }

        private NavigationHost CreateHostWithFullVisualTree()
        {
            var go = Track(new GameObject("TestNavigationHostFull"));
            go.SetActive(false);

            var doc = go.AddComponent<UIDocument>();
            doc.panelSettings = _panelSettings;

            var host = go.AddComponent<NavigationHost>();
            SetPrivateField(host, "_uiDocument", doc);

            go.SetActive(true);

            VisualElement fakeRoot = new VisualElement();
            BuildVisualTree(fakeRoot);
            host.BuildNavigation(fakeRoot);

            return host;
        }

        private static void BuildVisualTree(VisualElement root)
        {
            var gameArea = new VisualElement { name = "game-area" };
            root.Add(gameArea);

            var screenDefault = new VisualElement { name = "screen-default" };
            gameArea.Add(screenDefault);

            string[] screenNames = { "screen-village", "screen-skilltree", "screen-autre", "screen-guilde", "screen-shop" };
            foreach (string screenName in screenNames)
            {
                var screen = new VisualElement { name = screenName };
                screen.AddToClassList("hidden");
                gameArea.Add(screen);
            }

            var navBar = new VisualElement { name = "nav-bar" };
            root.Add(navBar);

            string[] tabNames = { "tab-village", "tab-skilltree", "tab-autre", "tab-guilde", "tab-shop" };
            foreach (string tabName in tabNames)
            {
                var button = new Button { name = tabName };
                button.AddToClassList("tab-button");
                button.AddToClassList("tab-inactive");
                navBar.Add(button);
            }
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
