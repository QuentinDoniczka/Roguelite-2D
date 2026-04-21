using System.Collections;
using System.Text.RegularExpressions;
using NUnit.Framework;
using RogueliteAutoBattler.Combat.Core;
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
            var combatWorldGo = Track(new GameObject("CombatWorld"));

            var camGo = Track(new GameObject("MainCamera"));
            var cam = camGo.AddComponent<Camera>();
            cam.tag = "MainCamera";
            yield return null;

            LogAssert.Expect(LogType.Error, new Regex("TeamRoster"));
            LogAssert.Expect(LogType.Error, new Regex("navigation system"));

            GameBootstrap.Initialize();

            Assert.IsNotNull(GameBootstrap.CombatWorld);
            Assert.AreEqual(combatWorldGo.transform, GameBootstrap.CombatWorld);
            Assert.IsNotNull(GameBootstrap.MainCamera);
        }

        [UnityTest]
        public IEnumerator Initialize_MissingCombatWorld_LogsError()
        {
            var camGo = Track(new GameObject("MainCamera"));
            var cam = camGo.AddComponent<Camera>();
            cam.tag = "MainCamera";
            yield return null;

            LogAssert.Expect(LogType.Error, new Regex("CombatWorld"));
            LogAssert.Expect(LogType.Error, new Regex("TeamRoster"));
            LogAssert.Expect(LogType.Error, new Regex("navigation system"));

            GameBootstrap.Initialize();
        }

        [UnityTest]
        public IEnumerator Initialize_NoSceneRefs_ValidatesAllMissingRefs()
        {
            yield return null;

            LogAssert.Expect(LogType.Error, new Regex("CombatWorld"));
            LogAssert.Expect(LogType.Error, new Regex("TeamRoster"));
            LogAssert.Expect(LogType.Error, new Regex("navigation system"));
            LogAssert.Expect(LogType.Error, new Regex("Main Camera"));

            GameBootstrap.Initialize();

            Assert.IsNull(GameBootstrap.CombatWorld);
            Assert.IsNull(GameBootstrap.MainCamera);
        }

        [UnityTest]
        public IEnumerator ResetForTest_ClearsAllRefs()
        {
            Track(new GameObject("CombatWorld"));

            var camGo = Track(new GameObject("MainCamera"));
            var cam = camGo.AddComponent<Camera>();
            cam.tag = "MainCamera";
            yield return null;

            LogAssert.Expect(LogType.Error, new Regex("TeamRoster"));
            LogAssert.Expect(LogType.Error, new Regex("navigation system"));

            GameBootstrap.Initialize();
            Assert.IsNotNull(GameBootstrap.CombatWorld);

            GameBootstrap.ResetForTest();

            Assert.IsNull(GameBootstrap.CombatWorld);
            Assert.IsNull(GameBootstrap.NavigationHost);
            Assert.IsNull(GameBootstrap.MainCamera);
        }

        [UnityTest]
        public IEnumerator Initialize_SetsTeamRoster_WhenCombatWorldHasComponent()
        {
            var combatWorldGo = Track(new GameObject("CombatWorld"));
            var expectedRoster = combatWorldGo.AddComponent<TeamRoster>();

            var camGo = Track(new GameObject("MainCamera"));
            var cam = camGo.AddComponent<Camera>();
            cam.tag = "MainCamera";
            yield return null;

            LogAssert.Expect(LogType.Error, new Regex("navigation system"));

            GameBootstrap.Initialize();

            Assert.IsNotNull(GameBootstrap.TeamRoster);
            Assert.AreSame(expectedRoster, GameBootstrap.TeamRoster);
        }

        [UnityTest]
        public IEnumerator Initialize_LogsError_WhenCombatWorldLacksTeamRoster()
        {
            Track(new GameObject("CombatWorld"));

            var camGo = Track(new GameObject("MainCamera"));
            var cam = camGo.AddComponent<Camera>();
            cam.tag = "MainCamera";
            yield return null;

            LogAssert.Expect(LogType.Error, "[GameBootstrap] TeamRoster not found on CombatWorld root.");
            LogAssert.Expect(LogType.Error, new Regex("navigation system"));

            GameBootstrap.Initialize();

            Assert.IsNull(GameBootstrap.TeamRoster);
        }

        [UnityTest]
        public IEnumerator ResetForTest_ClearsTeamRoster()
        {
            var combatWorldGo = Track(new GameObject("CombatWorld"));
            combatWorldGo.AddComponent<TeamRoster>();

            var camGo = Track(new GameObject("MainCamera"));
            var cam = camGo.AddComponent<Camera>();
            cam.tag = "MainCamera";
            yield return null;

            LogAssert.Expect(LogType.Error, new Regex("navigation system"));

            GameBootstrap.Initialize();
            Assert.IsNotNull(GameBootstrap.TeamRoster);

            GameBootstrap.ResetForTest();

            Assert.IsNull(GameBootstrap.TeamRoster);
        }
    }
}
