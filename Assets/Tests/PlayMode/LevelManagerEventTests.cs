using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using RogueliteAutoBattler.Combat.Environment;
using RogueliteAutoBattler.Combat.Levels;
using RogueliteAutoBattler.Data;
using UnityEngine;
using UnityEngine.TestTools;

namespace RogueliteAutoBattler.Tests.PlayMode
{
    public class LevelManagerEventTests : PlayModeTestBase
    {
        private LevelManager _levelManager;
        private GameObject _teamContainer;
        private GameObject _enemiesContainer;
        private LevelDatabase _levelDatabase;

        private LevelManager CreateLevelManagerWithTwoLevels()
        {
            _levelDatabase = ScriptableObject.CreateInstance<LevelDatabase>();

            var emptyWave = new WaveData("EmptyWave", 0f, new List<EnemySpawnData>());
            var emptyStep = new StepData("Step0", new List<WaveData> { emptyWave });

            var level0 = new LevelData("Level0", new List<StepData> { emptyStep });
            var level1 = new LevelData("Level1", new List<StepData> { emptyStep });

            var stage = new StageData("Stage0", new List<LevelData> { level0, level1 });
            _levelDatabase.Stages.Add(stage);

            var levelManagerGo = new GameObject("TestLevelManager");
            levelManagerGo.AddComponent<WorldConveyor>();
            var levelManager = levelManagerGo.AddComponent<LevelManager>();
            levelManager.enabled = false;
            Track(levelManagerGo);

            _teamContainer = Track(new GameObject("Team"));
            _enemiesContainer = Track(new GameObject("Enemies"));

            levelManager.InitializeForTest(
                _teamContainer.transform,
                _enemiesContainer.transform,
                levelDatabase: _levelDatabase);

            return levelManager;
        }

        public override void TearDown()
        {
            base.TearDown();

            if (_levelDatabase != null)
                Object.DestroyImmediate(_levelDatabase);
        }

        [UnityTest]
        public IEnumerator OnLevelStarted_FiresWithCorrectIndices()
        {
            _levelManager = CreateLevelManagerWithTwoLevels();

            int firedStage = -1;
            int firedLevel = -1;
            bool alreadyCaptured = false;
            _levelManager.OnLevelStarted += (stage, level) =>
            {
                if (alreadyCaptured) return;
                alreadyCaptured = true;
                firedStage = stage;
                firedLevel = level;
            };

            _levelManager.ApplyStage(0);
            _levelManager.StartLevel(0);

            yield return null;

            Assert.AreEqual(0, firedStage, "OnLevelStarted should fire with stage index 0.");
            Assert.AreEqual(0, firedLevel, "OnLevelStarted should fire with level index 0.");
        }

        [UnityTest]
        public IEnumerator OnLevelStarted_FiresForSecondLevel()
        {
            _levelManager = CreateLevelManagerWithTwoLevels();

            int firedStage = -1;
            int firedLevel = -1;
            _levelManager.OnLevelStarted += (stage, level) =>
            {
                firedStage = stage;
                firedLevel = level;
            };

            _levelManager.ApplyStage(0);
            _levelManager.StartLevel(1);

            yield return null;

            Assert.AreEqual(0, firedStage, "OnLevelStarted should fire with stage index 0.");
            Assert.AreEqual(1, firedLevel, "OnLevelStarted should fire with level index 1.");
        }

        [UnityTest]
        public IEnumerator OnStageStarted_FiresWithCorrectIndices()
        {
            _levelManager = CreateLevelManagerWithTwoLevels();

            int firedStage = -1;
            int firedLevel = -1;
            _levelManager.OnStageStarted += (stage, level) =>
            {
                firedStage = stage;
                firedLevel = level;
            };

            _levelManager.ApplyStage(0);

            yield return null;

            Assert.AreEqual(0, firedStage, "OnStageStarted should fire with stage index 0.");
            Assert.AreEqual(0, firedLevel, "OnStageStarted should fire with current level index.");
        }

        [UnityTest]
        public IEnumerator CurrentStageIndex_ReflectsAppliedStage()
        {
            _levelManager = CreateLevelManagerWithTwoLevels();

            _levelManager.ApplyStage(0);

            yield return null;

            Assert.AreEqual(0, _levelManager.CurrentStageIndex,
                "CurrentStageIndex should be 0 after ApplyStage(0).");
        }

        [UnityTest]
        public IEnumerator OnWaveSpawned_FiresWithCorrectIndices()
        {
            _levelManager = CreateLevelManagerWithTwoLevels();

            int firedStage = -1;
            int firedLevel = -1;
            int firedWave = -1;
            bool alreadyCaptured = false;
            _levelManager.OnWaveSpawned += (stage, level, wave) =>
            {
                if (alreadyCaptured) return;
                alreadyCaptured = true;
                firedStage = stage;
                firedLevel = level;
                firedWave = wave;
            };

            _levelManager.ApplyStage(0);
            _levelManager.StartLevel(0);

            yield return null;

            Assert.AreEqual(0, firedStage, "OnWaveSpawned should fire with stage index 0.");
            Assert.AreEqual(0, firedLevel, "OnWaveSpawned should fire with level index 0.");
            Assert.AreEqual(0, firedWave, "OnWaveSpawned should fire with wave index 0.");
        }

        [UnityTest]
        public IEnumerator CurrentLevelIndex_ReflectsStartedLevel()
        {
            _levelManager = CreateLevelManagerWithTwoLevels();

            int capturedLevelIndex = -1;
            _levelManager.OnLevelStarted += (stage, level) =>
            {
                capturedLevelIndex = _levelManager.CurrentLevelIndex;
            };

            _levelManager.ApplyStage(0);
            _levelManager.StartLevel(1);

            yield return null;

            Assert.AreEqual(1, capturedLevelIndex,
                "CurrentLevelIndex should be 1 at the moment OnLevelStarted fires for StartLevel(1).");
        }
    }
}
