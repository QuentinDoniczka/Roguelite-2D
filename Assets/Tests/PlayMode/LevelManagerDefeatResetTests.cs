using System.Collections;
using NUnit.Framework;
using RogueliteAutoBattler.Combat;
using RogueliteAutoBattler.Tests;
using UnityEngine;
using UnityEngine.TestTools;

namespace RogueliteAutoBattler.Tests.PlayMode
{
    public class LevelManagerDefeatResetTests : PlayModeTestBase
    {
        private GameObject _combatWorldGo;
        private LevelManager _levelManager;
        private CombatSpawnManager _spawnManager;
        private WorldConveyor _conveyor;
        private Transform _teamContainer;
        private Transform _enemiesContainer;
        private Transform _teamHomeAnchor;
        private Transform _enemiesHomeAnchor;

        private void CreateFullCombatSetup(int allyCount = 2, int enemyCount = 1)
        {
            _combatWorldGo = new GameObject("CombatWorld");
            Track(_combatWorldGo);

            var rb = _combatWorldGo.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;

            _conveyor = _combatWorldGo.AddComponent<WorldConveyor>();

            var teamContainerGo = new GameObject(CombatSetupHelper.TeamContainerName);
            teamContainerGo.transform.SetParent(_combatWorldGo.transform, false);
            _teamContainer = teamContainerGo.transform;

            var enemiesContainerGo = new GameObject(CombatSetupHelper.EnemiesContainerName);
            enemiesContainerGo.transform.SetParent(_combatWorldGo.transform, false);
            _enemiesContainer = enemiesContainerGo.transform;

            var teamHomeGo = new GameObject(CombatSetupHelper.TeamHomeAnchorName);
            teamHomeGo.transform.SetParent(_combatWorldGo.transform, false);
            teamHomeGo.transform.position = new Vector3(-3f, 0f, 0f);
            _teamHomeAnchor = teamHomeGo.transform;

            var enemiesHomeGo = new GameObject(CombatSetupHelper.EnemiesHomeAnchorName);
            enemiesHomeGo.transform.SetParent(_combatWorldGo.transform, false);
            enemiesHomeGo.transform.position = new Vector3(3f, 0f, 0f);
            _enemiesHomeAnchor = enemiesHomeGo.transform;

            var triggerZoneGo = new GameObject(CombatSetupHelper.CombatTriggerZoneName);
            triggerZoneGo.transform.SetParent(_combatWorldGo.transform, false);
            triggerZoneGo.transform.position = new Vector3(10f, 0f, 0f);
            triggerZoneGo.AddComponent<BoxCollider2D>();

            var groundGo = new GameObject("Ground");
            groundGo.transform.SetParent(_combatWorldGo.transform, false);
            groundGo.AddComponent<SpriteRenderer>();

            var allyPrefab = TestCharacterFactory.CreateCharacterPrefab("AllyPrefab");
            Track(allyPrefab);

            var enemyPrefab = TestCharacterFactory.CreateCharacterPrefab("EnemyPrefab");
            Track(enemyPrefab);

            var teamDb = TestCharacterFactory.CreateTeamDatabase(allyCount, allyPrefab);
            var levelDb = TestCharacterFactory.CreateLevelDatabase(enemyCount, enemyPrefab);

            _spawnManager = _combatWorldGo.AddComponent<CombatSpawnManager>();
            _spawnManager.enabled = false;
            _spawnManager.InitializeForTest(teamDb, _teamContainer, _teamHomeAnchor);

            _levelManager = _combatWorldGo.AddComponent<LevelManager>();
            _levelManager.enabled = false;
            _levelManager.InitializeForTest(
                _teamContainer,
                _enemiesContainer,
                _teamHomeAnchor,
                _enemiesHomeAnchor,
                levelDb);
            _levelManager.SetSpawnManagerForTest(_spawnManager);
        }

        private void SpawnInitialAlliesAndWire()
        {
            _spawnManager.SpawnAllies();
            _levelManager.WireAllyDeathTrackingForTest();
        }

        private void KillAllAllies()
        {
            for (int i = 0; i < _teamContainer.childCount; i++)
            {
                var ally = _teamContainer.GetChild(i);
                if (ally.TryGetComponent<CombatStats>(out var stats) && !stats.IsDead)
                    stats.TakeDamage(9999);
            }
        }

        [UnityTest]
        public IEnumerator DefeatReset_AfterDelay_OldEnemiesReplacedByNewWave()
        {
            CreateFullCombatSetup(allyCount: 2, enemyCount: 2);

            yield return null;

            SpawnInitialAlliesAndWire();

            yield return null;

            KillAllAllies();

            yield return new WaitForSeconds(_levelManager.DefeatResetDelay + 1f);

            Assert.AreEqual(2, _enemiesContainer.childCount,
                "Enemies container should have new wave enemies after defeat reset.");

            for (int i = 0; i < _enemiesContainer.childCount; i++)
            {
                var enemy = _enemiesContainer.GetChild(i);
                Assert.IsTrue(enemy.TryGetComponent<CombatStats>(out var stats),
                    $"New enemy {enemy.name} should have CombatStats.");
                Assert.AreEqual(stats.MaxHp, stats.CurrentHp,
                    $"New enemy {enemy.name} should have full HP.");
            }
        }

        [UnityTest]
        public IEnumerator DefeatReset_AfterDelay_AlliesAreRespawnedWithFullHp()
        {
            CreateFullCombatSetup(allyCount: 2, enemyCount: 1);

            yield return null;

            SpawnInitialAlliesAndWire();

            yield return null;

            KillAllAllies();

            yield return new WaitForSeconds(_levelManager.DefeatResetDelay + 1f);

            Assert.Greater(_teamContainer.childCount, 0,
                "Team container should have respawned allies after defeat reset.");

            for (int i = 0; i < _teamContainer.childCount; i++)
            {
                var ally = _teamContainer.GetChild(i);
                Assert.IsTrue(ally.TryGetComponent<CombatStats>(out var stats),
                    $"Respawned ally {ally.name} should have CombatStats.");
                Assert.AreEqual(stats.MaxHp, stats.CurrentHp,
                    $"Respawned ally {ally.name} should have full HP.");
            }
        }

        [UnityTest]
        public IEnumerator DefeatReset_ResetsStageAndLevelIndicesToZero()
        {
            CreateFullCombatSetup(allyCount: 2, enemyCount: 1);

            yield return null;

            SpawnInitialAlliesAndWire();

            yield return null;

            KillAllAllies();

            yield return new WaitForSeconds(_levelManager.DefeatResetDelay + 1f);

            Assert.AreEqual(0, _levelManager.CurrentStageIndex,
                "CurrentStageIndex should be 0 after defeat reset.");
            Assert.AreEqual(0, _levelManager.CurrentLevelIndex,
                "CurrentLevelIndex should be 0 after defeat reset.");
        }

        [UnityTest]
        public IEnumerator DefeatReset_LevelIsInProgressAfterReset()
        {
            CreateFullCombatSetup(allyCount: 2, enemyCount: 1);

            yield return null;

            SpawnInitialAlliesAndWire();

            yield return null;

            Assert.IsTrue(_levelManager.LevelInProgress,
                "Sanity: LevelInProgress should be true before defeat.");

            KillAllAllies();

            Assert.IsFalse(_levelManager.LevelInProgress,
                "LevelInProgress should be false immediately after all allies die.");

            yield return new WaitForSeconds(_levelManager.DefeatResetDelay + 1f);

            Assert.IsTrue(_levelManager.LevelInProgress,
                "LevelInProgress should be true after defeat reset completes.");
        }

        [UnityTest]
        public IEnumerator DefeatReset_GoldWalletNotReset()
        {
            CreateFullCombatSetup(allyCount: 2, enemyCount: 1);

            var walletGo = new GameObject("GoldWallet");
            Track(walletGo);
            var goldWallet = walletGo.AddComponent<GoldWallet>();
            goldWallet.Add(500);

            yield return null;

            SpawnInitialAlliesAndWire();

            yield return null;

            KillAllAllies();

            yield return new WaitForSeconds(_levelManager.DefeatResetDelay + 1f);

            Assert.AreEqual(500, goldWallet.Gold,
                "GoldWallet should retain its gold after defeat reset.");
        }
    }
}
