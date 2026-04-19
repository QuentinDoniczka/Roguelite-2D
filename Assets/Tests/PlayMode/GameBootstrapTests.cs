using System.Collections;
using System.Text.RegularExpressions;
using NUnit.Framework;
using RogueliteAutoBattler.Core;
using UnityEngine;
using UnityEngine.TestTools;

namespace RogueliteAutoBattler.Tests.PlayMode
{
    public class GameBootstrapTests : PlayModeTestBase
    {
        [SetUp]
        public void SetUp()
        {
            GameBootstrap.ResetForTest();
        }

        [TearDown]
        public override void TearDown()
        {
            GameBootstrap.ResetForTest();
            base.TearDown();
        }

        [UnityTest]
        public IEnumerator Initialize_FindsAllSceneRefs()
        {
            var canvasGo = Track(new GameObject("UICanvas"));
            canvasGo.AddComponent<Canvas>();

            var combatWorldGo = Track(new GameObject("CombatWorld"));

            var camGo = Track(new GameObject("MainCamera"));
            var cam = camGo.AddComponent<Camera>();
            cam.tag = "MainCamera";
            yield return null;

            LogAssert.Expect(LogType.Error, new Regex("navigation system"));

            GameBootstrap.Initialize();

            Assert.IsNotNull(GameBootstrap.Canvas);
            Assert.IsNotNull(GameBootstrap.CombatWorld);
            Assert.AreEqual(combatWorldGo.transform, GameBootstrap.CombatWorld);
            Assert.IsNotNull(GameBootstrap.MainCamera);
        }

        [UnityTest]
        public IEnumerator Initialize_MissingCombatWorld_LogsError()
        {
            var canvasGo = Track(new GameObject("UICanvas"));
            canvasGo.AddComponent<Canvas>();

            var camGo = Track(new GameObject("MainCamera"));
            var cam = camGo.AddComponent<Camera>();
            cam.tag = "MainCamera";
            yield return null;

            LogAssert.Expect(LogType.Error, new Regex("CombatWorld"));
            LogAssert.Expect(LogType.Error, new Regex("navigation system"));

            GameBootstrap.Initialize();
        }

        [UnityTest]
        public IEnumerator Initialize_NoCanvas_StillValidatesRefs()
        {
            yield return null;

            LogAssert.Expect(LogType.Error, new Regex("CombatWorld"));
            LogAssert.Expect(LogType.Error, new Regex("navigation system"));
            LogAssert.Expect(LogType.Error, new Regex("Main Camera"));

            GameBootstrap.Initialize();

            Assert.IsNull(GameBootstrap.Canvas);
        }

        [UnityTest]
        public IEnumerator ResetForTest_ClearsAllRefs()
        {
            var canvasGo = Track(new GameObject("UICanvas"));
            canvasGo.AddComponent<Canvas>();
            Track(new GameObject("CombatWorld"));

            var camGo = Track(new GameObject("MainCamera"));
            var cam = camGo.AddComponent<Camera>();
            cam.tag = "MainCamera";
            yield return null;

            LogAssert.Expect(LogType.Error, new Regex("navigation system"));

            GameBootstrap.Initialize();
            Assert.IsNotNull(GameBootstrap.Canvas);

            GameBootstrap.ResetForTest();

            Assert.IsNull(GameBootstrap.Canvas);
            Assert.IsNull(GameBootstrap.CombatWorld);
            Assert.IsNull(GameBootstrap.NavigationHost);
            Assert.IsNull(GameBootstrap.MainCamera);
        }
    }
}
