using System.Collections;
using System.Text.RegularExpressions;
using NUnit.Framework;
using RogueliteAutoBattler.Core;
using RogueliteAutoBattler.UI.Core;
using UnityEngine;
using UnityEngine.TestTools;

namespace RogueliteAutoBattler.Tests.PlayMode
{
    public class GameBootstrapTests : PlayModeTestBase
    {
        private Canvas _canvas;
        private Transform _combatWorld;
        private NavigationManager _navigationManager;
        private Camera _mainCamera;

        [SetUp]
        public void SetUp()
        {
            var canvasGo = Track(new GameObject("TestCanvas"));
            _canvas = canvasGo.AddComponent<Canvas>();

            _combatWorld = Track(new GameObject("TestCombatWorld")).transform;

            var navGo = new GameObject("TestNavigationManager");
            navGo.SetActive(false);
            Track(navGo);
            _navigationManager = navGo.AddComponent<NavigationManager>();

            var camGo = Track(new GameObject("TestCamera"));
            _mainCamera = camGo.AddComponent<Camera>();
            _mainCamera.orthographic = true;
        }

        [UnityTest]
        public IEnumerator Awake_AllRefsAssigned_NoErrors()
        {
            var go = new GameObject("Bootstrap");
            go.SetActive(false);
            Track(go);

            var bootstrap = go.AddComponent<GameBootstrap>();
            bootstrap.SetRefs(_canvas, _combatWorld, _navigationManager, _mainCamera);

            go.SetActive(true);
            yield return null;

            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator Awake_MissingCanvas_LogsError()
        {
            var go = new GameObject("Bootstrap");
            go.SetActive(false);
            Track(go);

            var bootstrap = go.AddComponent<GameBootstrap>();
            bootstrap.SetRefs(null, _combatWorld, _navigationManager, _mainCamera);

            LogAssert.Expect(LogType.Error, new Regex("_canvas"));

            go.SetActive(true);
            yield return null;
        }

        [UnityTest]
        public IEnumerator Awake_MissingCombatWorld_LogsError()
        {
            var go = new GameObject("Bootstrap");
            go.SetActive(false);
            Track(go);

            var bootstrap = go.AddComponent<GameBootstrap>();
            bootstrap.SetRefs(_canvas, null, _navigationManager, _mainCamera);

            LogAssert.Expect(LogType.Error, new Regex("_combatWorld"));

            go.SetActive(true);
            yield return null;
        }

        [UnityTest]
        public IEnumerator Awake_MissingNavigationManager_LogsError()
        {
            var go = new GameObject("Bootstrap");
            go.SetActive(false);
            Track(go);

            var bootstrap = go.AddComponent<GameBootstrap>();
            bootstrap.SetRefs(_canvas, _combatWorld, null, _mainCamera);

            LogAssert.Expect(LogType.Error, new Regex("_navigationManager"));

            go.SetActive(true);
            yield return null;
        }

        [UnityTest]
        public IEnumerator Awake_MissingCamera_LogsError()
        {
            var go = new GameObject("Bootstrap");
            go.SetActive(false);
            Track(go);

            var bootstrap = go.AddComponent<GameBootstrap>();
            bootstrap.SetRefs(_canvas, _combatWorld, _navigationManager, null);

            LogAssert.Expect(LogType.Error, new Regex("_mainCamera"));

            go.SetActive(true);
            yield return null;
        }

        [UnityTest]
        public IEnumerator Properties_ExposeSerializedFields()
        {
            var go = new GameObject("Bootstrap");
            go.SetActive(false);
            Track(go);

            var bootstrap = go.AddComponent<GameBootstrap>();
            bootstrap.SetRefs(_canvas, _combatWorld, _navigationManager, _mainCamera);

            go.SetActive(true);
            yield return null;

            Assert.AreEqual(_canvas, bootstrap.Canvas);
            Assert.AreEqual(_combatWorld, bootstrap.CombatWorld);
            Assert.AreEqual(_navigationManager, bootstrap.NavigationManager);
            Assert.AreEqual(_mainCamera, bootstrap.MainCamera);
        }
    }
}
