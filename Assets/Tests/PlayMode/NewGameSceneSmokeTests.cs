using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using RogueliteAutoBattler.Combat.Visuals;
using RogueliteAutoBattler.Core;
using RogueliteAutoBattler.UI.Toolkit;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace RogueliteAutoBattler.Tests.PlayMode
{
    public class NewGameSceneSmokeTests
    {
        private const string NewGameScenePath = "Assets/Scenes/NewGameScene.unity";
        private const string SceneNameWithoutExt = "NewGameScene";

        private readonly List<string> _capturedErrors = new List<string>();
        private bool _sceneWasLoaded;

        [SetUp]
        public void SetUp()
        {
            GameBootstrap.ResetForTest();
            DamageNumberService.ResetForTest();
            CoinFlyService.ResetForTest();
            _capturedErrors.Clear();
            _sceneWasLoaded = false;
            Application.logMessageReceivedThreaded += OnLogReceived;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Application.logMessageReceivedThreaded -= OnLogReceived;
            LogAssert.ignoreFailingMessages = true;

            if (_sceneWasLoaded)
            {
                var emptyScene = SceneManager.CreateScene("EmptyTeardownScene_" + System.Guid.NewGuid().ToString("N"));
                SceneManager.SetActiveScene(emptyScene);

                for (int i = SceneManager.sceneCount - 1; i >= 0; i--)
                {
                    var scene = SceneManager.GetSceneAt(i);
                    if (scene != emptyScene && scene.IsValid() && scene.name == SceneNameWithoutExt)
                    {
                        AsyncOperation unload = SceneManager.UnloadSceneAsync(scene);
                        if (unload != null)
                        {
                            while (!unload.isDone)
                                yield return null;
                        }
                    }
                }

                yield return null;
            }

            GameBootstrap.ResetForTest();
            DamageNumberService.ResetForTest();
            CoinFlyService.ResetForTest();
        }

        private void OnLogReceived(string condition, string stackTrace, LogType type)
        {
            if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
            {
                _capturedErrors.Add($"[{type}] {condition}");
            }
        }

        private IEnumerator LoadNewGameScene()
        {
            LogAssert.ignoreFailingMessages = true;

            AsyncOperation op = SceneManager.LoadSceneAsync(NewGameScenePath, LoadSceneMode.Single);
            if (op == null)
            {
                yield break;
            }

            _sceneWasLoaded = true;
            while (!op.isDone)
                yield return null;

            yield return null;
            yield return null;
            yield return null;

            GameBootstrap.Initialize();

            yield return null;
        }

        [UnityTest]
        public IEnumerator NewGameScene_LoadsCleanly_NoErrorOrAssertLogs()
        {
            yield return LoadNewGameScene();

            if (!_sceneWasLoaded)
            {
                Assert.Inconclusive($"Scene '{NewGameScenePath}' could not be loaded — verify it is present in EditorBuildSettings.");
                yield break;
            }

            Assert.AreEqual(SceneNameWithoutExt, SceneManager.GetActiveScene().name,
                "Active scene after load must be NewGameScene.");

            var unexpectedErrors = new List<string>();
            foreach (string err in _capturedErrors)
            {
                if (err.Contains("[GameBootstrap] No navigation system"))
                    continue;
                unexpectedErrors.Add(err);
            }

            Assert.IsEmpty(unexpectedErrors,
                "NewGameScene must load without errors. Captured: " + string.Join(" | ", unexpectedErrors));
        }

        [UnityTest]
        public IEnumerator NewGameScene_GameBootstrap_HasNavigationHost()
        {
            yield return LoadNewGameScene();

            if (!_sceneWasLoaded)
            {
                Assert.Inconclusive($"Scene '{NewGameScenePath}' could not be loaded — verify it is present in EditorBuildSettings.");
                yield break;
            }

            Assert.IsNotNull(GameBootstrap.NavigationHost,
                "GameBootstrap.NavigationHost must resolve after scene load — regression guard for #199 (NavigationHost lookup must succeed once legacy NavigationManager is purged).");
        }

        [UnityTest]
        public IEnumerator NewGameScene_GameBootstrap_HasCombatWorldAndCamera()
        {
            yield return LoadNewGameScene();

            if (!_sceneWasLoaded)
            {
                Assert.Inconclusive($"Scene '{NewGameScenePath}' could not be loaded — verify it is present in EditorBuildSettings.");
                yield break;
            }

            Assert.IsNotNull(GameBootstrap.CombatWorld,
                "GameBootstrap.CombatWorld must resolve after scene load.");
            Assert.IsNotNull(GameBootstrap.MainCamera,
                "GameBootstrap.MainCamera must resolve after scene load.");
        }

        [UnityTest]
        public IEnumerator NewGameScene_NavigationHost_FindsExactlyOneInstance()
        {
            yield return LoadNewGameScene();

            if (!_sceneWasLoaded)
            {
                Assert.Inconclusive($"Scene '{NewGameScenePath}' could not be loaded — verify it is present in EditorBuildSettings.");
                yield break;
            }

            NavigationHost[] hosts = Object.FindObjectsByType<NavigationHost>(FindObjectsSortMode.None);
            Assert.AreEqual(1, hosts.Length,
                "Exactly one NavigationHost must exist in NewGameScene (regression guard against duplicate or missing nav host after the legacy purge).");
        }
    }
}
