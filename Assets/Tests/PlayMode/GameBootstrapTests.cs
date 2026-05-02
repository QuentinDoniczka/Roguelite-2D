using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using RogueliteAutoBattler.Combat.Core;
using RogueliteAutoBattler.Core;
using RogueliteAutoBattler.Data;
using RogueliteAutoBattler.Economy;
using UnityEngine;
using UnityEngine.TestTools;

namespace RogueliteAutoBattler.Tests.PlayMode
{
    public class GameBootstrapTests : PlayModeTestBase
    {
        private static readonly Regex GoldWalletErrorRegex = new Regex("GoldWallet not found");
        private static readonly Regex SkillPointWalletErrorRegex = new Regex("SkillPointWallet not found");
        private static readonly Regex ProgressionLoaderErrorRegex = new Regex("ProgressionLoader not initialized");
        private static readonly Regex AllyStatBonusServiceErrorRegex = new Regex("AllyStatBonusService not initialized");

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

        private static void ExpectMissingProgressionRefsErrors()
        {
            LogAssert.Expect(LogType.Error, GoldWalletErrorRegex);
            LogAssert.Expect(LogType.Error, SkillPointWalletErrorRegex);
            LogAssert.Expect(LogType.Error, ProgressionLoaderErrorRegex);
            LogAssert.Expect(LogType.Error, AllyStatBonusServiceErrorRegex);
        }

        [UnityTest]
        public IEnumerator Initialize_FindsAllSceneRefs()
        {
            var combatWorldGo = Track(new GameObject(GameBootstrap.CombatWorldName));

            var camGo = Track(new GameObject("MainCamera"));
            var cam = camGo.AddComponent<Camera>();
            cam.tag = "MainCamera";
            yield return null;

            LogAssert.Expect(LogType.Error, new Regex("TeamRoster"));
            LogAssert.Expect(LogType.Error, new Regex("navigation system"));
            ExpectMissingProgressionRefsErrors();

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
            ExpectMissingProgressionRefsErrors();

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
            ExpectMissingProgressionRefsErrors();

            GameBootstrap.Initialize();

            Assert.IsNull(GameBootstrap.CombatWorld);
            Assert.IsNull(GameBootstrap.MainCamera);
        }

        [UnityTest]
        public IEnumerator ResetForTest_ClearsAllRefs()
        {
            Track(new GameObject(GameBootstrap.CombatWorldName));

            var camGo = Track(new GameObject("MainCamera"));
            var cam = camGo.AddComponent<Camera>();
            cam.tag = "MainCamera";
            yield return null;

            LogAssert.Expect(LogType.Error, new Regex("TeamRoster"));
            LogAssert.Expect(LogType.Error, new Regex("navigation system"));
            ExpectMissingProgressionRefsErrors();

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
            var combatWorldGo = Track(new GameObject(GameBootstrap.CombatWorldName));
            var expectedRoster = combatWorldGo.AddComponent<TeamRoster>();

            var camGo = Track(new GameObject("MainCamera"));
            var cam = camGo.AddComponent<Camera>();
            cam.tag = "MainCamera";
            yield return null;

            LogAssert.Expect(LogType.Error, new Regex("navigation system"));
            ExpectMissingProgressionRefsErrors();

            GameBootstrap.Initialize();

            Assert.IsNotNull(GameBootstrap.TeamRoster);
            Assert.AreSame(expectedRoster, GameBootstrap.TeamRoster);
        }

        [UnityTest]
        public IEnumerator Initialize_LogsError_WhenCombatWorldLacksTeamRoster()
        {
            Track(new GameObject(GameBootstrap.CombatWorldName));

            var camGo = Track(new GameObject("MainCamera"));
            var cam = camGo.AddComponent<Camera>();
            cam.tag = "MainCamera";
            yield return null;

            LogAssert.Expect(LogType.Error, "[GameBootstrap] TeamRoster not found on CombatWorld root.");
            LogAssert.Expect(LogType.Error, new Regex("navigation system"));
            ExpectMissingProgressionRefsErrors();

            GameBootstrap.Initialize();

            Assert.IsNull(GameBootstrap.TeamRoster);
        }

        [UnityTest]
        public IEnumerator ResetForTest_ClearsTeamRoster()
        {
            var combatWorldGo = Track(new GameObject(GameBootstrap.CombatWorldName));
            combatWorldGo.AddComponent<TeamRoster>();

            var camGo = Track(new GameObject("MainCamera"));
            var cam = camGo.AddComponent<Camera>();
            cam.tag = "MainCamera";
            yield return null;

            LogAssert.Expect(LogType.Error, new Regex("navigation system"));
            ExpectMissingProgressionRefsErrors();

            GameBootstrap.Initialize();
            Assert.IsNotNull(GameBootstrap.TeamRoster);

            GameBootstrap.ResetForTest();

            Assert.IsNull(GameBootstrap.TeamRoster);
        }

        [UnityTest]
        public IEnumerator Initialize_WithSkillTreeAssetsInjected_CreatesAllyStatBonusServiceAndLoader()
        {
            var combatWorldGo = Track(new GameObject(GameBootstrap.CombatWorldName));
            combatWorldGo.AddComponent<TeamRoster>();

            var camGo = Track(new GameObject("MainCamera"));
            var cam = camGo.AddComponent<Camera>();
            cam.tag = "MainCamera";

            var walletGo = Track(new GameObject("GoldWallet"));
            var wallet = walletGo.AddComponent<GoldWallet>();

            var spWalletGo = Track(new GameObject("SkillPointWallet"));
            var spWallet = spWalletGo.AddComponent<SkillPointWallet>();

            var data = ScriptableObject.CreateInstance<SkillTreeData>();
            data.InitializeForTest(new List<SkillTreeData.SkillNodeEntry>
            {
                new SkillTreeData.SkillNodeEntry
                {
                    id = 0,
                    maxLevel = 5,
                    statModifierType = StatType.Hp,
                    statModifierMode = SkillTreeData.StatModifierMode.Flat,
                    statModifierValuePerLevel = 5f,
                    connectedNodeIds = new List<int>()
                }
            });
            var progress = ScriptableObject.CreateInstance<SkillTreeProgress>();

            string tempDirectory = Path.Combine(Path.GetTempPath(), "roguelite-tests", Guid.NewGuid().ToString());
            string tempFilePath = Path.Combine(tempDirectory, "progression.json");

            GameBootstrap.SkillTreeDataAssetForTest = data;
            GameBootstrap.SkillTreeProgressAssetForTest = progress;
            GameBootstrap.GoldWalletForTest = wallet;
            GameBootstrap.SkillPointWalletForTest = spWallet;
            GameBootstrap.ProgressionFilePathForTest = tempFilePath;

            yield return null;

            LogAssert.Expect(LogType.Error, new Regex("navigation system"));

            try
            {
                GameBootstrap.Initialize();

                Assert.IsNotNull(GameBootstrap.GoldWallet, "GoldWallet should be resolved from injection.");
                Assert.AreSame(wallet, GameBootstrap.GoldWallet);
                Assert.IsNotNull(GameBootstrap.SkillPointWallet, "SkillPointWallet should be resolved from injection.");
                Assert.AreSame(spWallet, GameBootstrap.SkillPointWallet);
                Assert.IsNotNull(GameBootstrap.ProgressionLoader, "ProgressionLoader should be created.");
                Assert.IsNotNull(GameBootstrap.AllyStatBonusService, "AllyStatBonusService should be created.");
            }
            finally
            {
                GameBootstrap.ResetForTest();
                UnityEngine.Object.DestroyImmediate(data);
                UnityEngine.Object.DestroyImmediate(progress);
                if (Directory.Exists(tempDirectory))
                    Directory.Delete(tempDirectory, true);
            }
        }

        [UnityTest]
        public IEnumerator Initialize_MissingSkillTreeAssets_LogsErrorAndServiceNull()
        {
            var combatWorldGo = Track(new GameObject(GameBootstrap.CombatWorldName));
            combatWorldGo.AddComponent<TeamRoster>();

            var camGo = Track(new GameObject("MainCamera"));
            var cam = camGo.AddComponent<Camera>();
            cam.tag = "MainCamera";

            yield return null;

            LogAssert.Expect(LogType.Error, new Regex("navigation system"));
            ExpectMissingProgressionRefsErrors();

            GameBootstrap.Initialize();

            Assert.IsNull(GameBootstrap.AllyStatBonusService);
            Assert.IsNull(GameBootstrap.ProgressionLoader);
        }

        [UnityTest]
        public IEnumerator ResetForTest_DisposesProgressionLoaderAndService()
        {
            var combatWorldGo = Track(new GameObject(GameBootstrap.CombatWorldName));
            combatWorldGo.AddComponent<TeamRoster>();

            var camGo = Track(new GameObject("MainCamera"));
            var cam = camGo.AddComponent<Camera>();
            cam.tag = "MainCamera";

            var walletGo = Track(new GameObject("GoldWallet"));
            var wallet = walletGo.AddComponent<GoldWallet>();

            var spWalletGo = Track(new GameObject("SkillPointWallet"));
            var spWallet = spWalletGo.AddComponent<SkillPointWallet>();

            var data = ScriptableObject.CreateInstance<SkillTreeData>();
            data.InitializeForTest(new List<SkillTreeData.SkillNodeEntry>
            {
                new SkillTreeData.SkillNodeEntry
                {
                    id = 0,
                    maxLevel = 5,
                    statModifierType = StatType.Hp,
                    statModifierMode = SkillTreeData.StatModifierMode.Flat,
                    statModifierValuePerLevel = 5f,
                    connectedNodeIds = new List<int>()
                }
            });
            var progress = ScriptableObject.CreateInstance<SkillTreeProgress>();

            string tempDirectory = Path.Combine(Path.GetTempPath(), "roguelite-tests", Guid.NewGuid().ToString());
            string tempFilePath = Path.Combine(tempDirectory, "progression.json");

            GameBootstrap.SkillTreeDataAssetForTest = data;
            GameBootstrap.SkillTreeProgressAssetForTest = progress;
            GameBootstrap.GoldWalletForTest = wallet;
            GameBootstrap.SkillPointWalletForTest = spWallet;
            GameBootstrap.ProgressionFilePathForTest = tempFilePath;

            yield return null;

            LogAssert.Expect(LogType.Error, new Regex("navigation system"));

            try
            {
                GameBootstrap.Initialize();

                Assert.IsNotNull(GameBootstrap.ProgressionLoader);
                Assert.IsNotNull(GameBootstrap.AllyStatBonusService);

                GameBootstrap.ResetForTest();

                Assert.IsNull(GameBootstrap.ProgressionLoader);
                Assert.IsNull(GameBootstrap.AllyStatBonusService);

                if (File.Exists(tempFilePath))
                    File.Delete(tempFilePath);

                progress.SetLevel(0, 7);

                Assert.IsFalse(File.Exists(tempFilePath),
                    "Save handler should have been unsubscribed by Dispose; no file should be written.");
            }
            finally
            {
                GameBootstrap.ResetForTest();
                UnityEngine.Object.DestroyImmediate(data);
                UnityEngine.Object.DestroyImmediate(progress);
                if (Directory.Exists(tempDirectory))
                    Directory.Delete(tempDirectory, true);
            }
        }
    }
}
