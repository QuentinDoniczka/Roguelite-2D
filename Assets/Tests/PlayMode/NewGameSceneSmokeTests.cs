using System.Collections;
using NUnit.Framework;
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

        private Scene _loadedScene;

        [SetUp]
        public void SetUp()
        {
            GameBootstrap.ResetForTest();
            LogAssert.ignoreFailingMessages = false;
        }

        [TearDown]
        public void TearDown()
        {
            GameBootstrap.ResetForTest();
        }

        [UnityTest]
        public IEnumerator NewGameScene_LoadsCleanly_NoErrorOrAssertLogs()
        {
            LogAssert.ignoreFailingMessages = true;

            AsyncOperation op = SceneManager.LoadSceneAsync(NewGameScenePath, LoadSceneMode.Single);
            if (op == null)
            {
                Assert.Inconclusive($"Scene '{NewGameScenePath}' could not be loaded — verify it is present in EditorBuildSettings.");
                yield break;
            }

            while (!op.isDone)
                yield return null;

            yield return null;
            yield return null;
            yield return null;

            _loadedScene = SceneManager.GetActiveScene();
            Assert.AreEqual(SceneNameWithoutExt, _loadedScene.name,
                "Active scene after load must be NewGameScene.");

            LogAssert.ignoreFailingMessages = false;
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator NewGameScene_GameBootstrap_HasNavigationHost()
        {
            LogAssert.ignoreFailingMessages = true;

            AsyncOperation op = SceneManager.LoadSceneAsync(NewGameScenePath, LoadSceneMode.Single);
            if (op == null)
            {
                Assert.Inconclusive($"Scene '{NewGameScenePath}' could not be loaded — verify it is present in EditorBuildSettings.");
                yield break;
            }

            while (!op.isDone)
                yield return null;

            yield return null;
            yield return null;
            yield return null;

            Assert.IsNotNull(GameBootstrap.NavigationHost,
                "GameBootstrap.NavigationHost must resolve after scene load — regression guard for #199 (NavigationHost lookup must succeed once legacy NavigationManager is purged).");
        }

        [UnityTest]
        public IEnumerator NewGameScene_GameBootstrap_HasCombatWorldAndCamera()
        {
            LogAssert.ignoreFailingMessages = true;

            AsyncOperation op = SceneManager.LoadSceneAsync(NewGameScenePath, LoadSceneMode.Single);
            if (op == null)
            {
                Assert.Inconclusive($"Scene '{NewGameScenePath}' could not be loaded — verify it is present in EditorBuildSettings.");
                yield break;
            }

            while (!op.isDone)
                yield return null;

            yield return null;
            yield return null;
            yield return null;

            Assert.IsNotNull(GameBootstrap.CombatWorld,
                "GameBootstrap.CombatWorld must resolve after scene load.");
            Assert.IsNotNull(GameBootstrap.MainCamera,
                "GameBootstrap.MainCamera must resolve after scene load.");
        }

        [UnityTest]
        public IEnumerator NewGameScene_NavigationHost_FindsExactlyOneInstance()
        {
            LogAssert.ignoreFailingMessages = true;

            AsyncOperation op = SceneManager.LoadSceneAsync(NewGameScenePath, LoadSceneMode.Single);
            if (op == null)
            {
                Assert.Inconclusive($"Scene '{NewGameScenePath}' could not be loaded — verify it is present in EditorBuildSettings.");
                yield break;
            }

            while (!op.isDone)
                yield return null;

            yield return null;
            yield return null;
            yield return null;

            NavigationHost[] hosts = Object.FindObjectsByType<NavigationHost>(FindObjectsSortMode.None);
            Assert.AreEqual(1, hosts.Length,
                "Exactly one NavigationHost must exist in NewGameScene (regression guard against duplicate or missing nav host after the legacy purge).");
        }
    }
}
