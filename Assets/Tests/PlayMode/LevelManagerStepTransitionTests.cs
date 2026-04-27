using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using RogueliteAutoBattler.Combat.Core;
using RogueliteAutoBattler.Combat.Environment;
using RogueliteAutoBattler.Combat.Levels;
using RogueliteAutoBattler.Data;
using UnityEngine;
using UnityEngine.TestTools;

namespace RogueliteAutoBattler.Tests.PlayMode
{
    public class LevelManagerStepTransitionTests : PlayModeTestBase
    {
        private GameObject _combatWorldGo;
        private LevelManager _levelManager;
        private CombatSpawnManager _spawnManager;
        private WorldConveyor _conveyor;
        private Transform _teamContainer;
        private Transform _enemiesContainer;
        private Transform _teamHomeAnchor;
        private Transform _enemiesHomeAnchor;
        private LevelDatabase _levelDatabase;

        private void CreateTwoStepCombatSetup(int allyCount = 1, int enemiesPerStep = 1)
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

            _levelDatabase = ScriptableObject.CreateInstance<LevelDatabase>();

            var step0Enemies = new List<EnemySpawnData>();
            for (int i = 0; i < enemiesPerStep; i++)
            {
                var enemy = new EnemySpawnData($"Step0_Enemy_{i}", 50, 5)
                {
                    Prefab = enemyPrefab,
                    AttackSpeed = 1f,
                    MoveSpeed = 2f,
                    AttackRange = 1f,
                    ColliderRadius = 0.3f,
                    GoldDrop = 0
                };
                step0Enemies.Add(enemy);
            }

            var step1Enemies = new List<EnemySpawnData>();
            for (int i = 0; i < enemiesPerStep; i++)
            {
                var enemy = new EnemySpawnData($"Step1_Enemy_{i}", 50, 5)
                {
                    Prefab = enemyPrefab,
                    AttackSpeed = 1f,
                    MoveSpeed = 2f,
                    AttackRange = 1f,
                    ColliderRadius = 0.3f,
                    GoldDrop = 0
                };
                step1Enemies.Add(enemy);
            }

            var wave0 = new WaveData("Wave_Step0", 0f, step0Enemies);
            var wave1 = new WaveData("Wave_Step1", 0f, step1Enemies);

            var step0 = new StepData("Step_0", new List<WaveData> { wave0 });
            var step1 = new StepData("Step_1", new List<WaveData> { wave1 });

            var level = new LevelData("Level_0", new List<StepData> { step0, step1 });
            var stage = new StageData("Stage_0", new List<LevelData> { level });
            _levelDatabase.Stages = new List<StageData> { stage };

            var teamRoster = _combatWorldGo.AddComponent<TeamRoster>();
            _spawnManager = _combatWorldGo.AddComponent<CombatSpawnManager>();
            _spawnManager.enabled = false;
            _spawnManager.InitializeForTest(teamDb, _teamContainer, _teamHomeAnchor, teamRoster);

            _levelManager = _combatWorldGo.AddComponent<LevelManager>();
            _levelManager.enabled = false;
            _levelManager.InitializeForTest(
                _teamContainer,
                _enemiesContainer,
                _teamHomeAnchor,
                _enemiesHomeAnchor,
                _levelDatabase);
            _levelManager.SetSpawnManagerForTest(_spawnManager);
        }

        private void SpawnAlliesAndStartLevel()
        {
            _spawnManager.SpawnAllies();
            _levelManager.WireAllyDeathTrackingForTest();
            _levelManager.ApplyStage(0);
            _levelManager.StartLevel(0);
        }

        private void KillAllEnemies()
        {
            for (int i = _enemiesContainer.childCount - 1; i >= 0; i--)
            {
                var enemy = _enemiesContainer.GetChild(i);
                if (enemy.TryGetComponent<CombatStats>(out var stats) && !stats.IsDead)
                    stats.TakeDamage(9999);
            }
        }

        public override void TearDown()
        {
            base.TearDown();

            if (_levelDatabase != null)
                Object.DestroyImmediate(_levelDatabase);
        }

        [UnityTest]
        public IEnumerator StepTransition_FiresOnStepStarted_WithCorrectIndex()
        {
            CreateTwoStepCombatSetup();

            yield return null;

            SpawnAlliesAndStartLevel();

            yield return null;

            int firedStepIndex = -1;
            _levelManager.OnStepStarted += (stepIndex) => firedStepIndex = stepIndex;

            KillAllEnemies();

            yield return new WaitForSeconds(3f);

            Assert.AreEqual(1, firedStepIndex,
                "OnStepStarted should have fired with step index 1 after killing all step 0 enemies.");
        }

        [UnityTest]
        public IEnumerator StepTransition_CurrentStepIndex_AdvancesAfterTransition()
        {
            CreateTwoStepCombatSetup();

            yield return null;

            SpawnAlliesAndStartLevel();

            yield return null;

            Assert.AreEqual(0, _levelManager.CurrentStepIndex,
                "CurrentStepIndex should be 0 at the start.");

            KillAllEnemies();

            yield return new WaitForSeconds(3f);

            Assert.AreEqual(1, _levelManager.CurrentStepIndex,
                "CurrentStepIndex should advance to 1 after step 0 enemies are cleared and scroll completes.");
        }

        [UnityTest]
        public IEnumerator StepTransition_LevelInProgressTrue_WhenOnStepStartedFires()
        {
            CreateTwoStepCombatSetup();

            yield return null;

            SpawnAlliesAndStartLevel();

            yield return null;

            Assert.IsTrue(_levelManager.LevelInProgress,
                "LevelInProgress should be true before killing enemies.");

            bool capturedLevelInProgressWhenStepStarted = false;
            _levelManager.OnStepStarted += (stepIndex) =>
            {
                capturedLevelInProgressWhenStepStarted = _levelManager.LevelInProgress;
            };

            KillAllEnemies();

            yield return new WaitForSeconds(3f);

            Assert.IsTrue(capturedLevelInProgressWhenStepStarted,
                "LevelInProgress should be true when OnStepStarted fires because _levelInProgress is restored before the spawn callback.");
        }

        [UnityTest]
        public IEnumerator StepTransition_LevelInProgressTrue_AfterScroll()
        {
            CreateTwoStepCombatSetup();

            yield return null;

            SpawnAlliesAndStartLevel();

            yield return null;

            KillAllEnemies();

            yield return new WaitForSeconds(3f);

            Assert.IsTrue(_levelManager.LevelInProgress,
                "LevelInProgress should be true after the scroll transition completes and step 1 begins.");
        }
    }
}
