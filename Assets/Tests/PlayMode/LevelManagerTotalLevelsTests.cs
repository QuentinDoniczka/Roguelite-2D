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
    public class LevelManagerTotalLevelsTests : PlayModeTestBase
    {
        private LevelDatabase _levelDatabase;

        public override void TearDown()
        {
            if (_levelDatabase != null)
                Object.DestroyImmediate(_levelDatabase);

            base.TearDown();
        }

        private LevelManager CreateLevelManagerWithThreeLevels()
        {
            _levelDatabase = ScriptableObject.CreateInstance<LevelDatabase>();

            var emptyWave = new WaveData("EmptyWave", 0f, new List<EnemySpawnData>());
            var emptyStep = new StepData("Step0", new List<WaveData> { emptyWave });

            var level0 = new LevelData("Level0", new List<StepData> { emptyStep });
            var level1 = new LevelData("Level1", new List<StepData> { emptyStep });
            var level2 = new LevelData("Level2", new List<StepData> { emptyStep });

            var stage = new StageData("Stage0", new List<LevelData> { level0, level1, level2 });
            _levelDatabase.Stages.Add(stage);

            var levelManagerGo = new GameObject("TestLevelManager");
            levelManagerGo.AddComponent<WorldConveyor>();
            var levelManager = levelManagerGo.AddComponent<LevelManager>();
            levelManager.enabled = false;
            Track(levelManagerGo);

            var teamContainer = Track(new GameObject("Team"));
            var enemiesContainer = Track(new GameObject("Enemies"));

            levelManager.InitializeForTest(
                teamContainer.transform,
                enemiesContainer.transform,
                levelDatabase: _levelDatabase);

            return levelManager;
        }

        [UnityTest]
        public IEnumerator TotalLevelsInCurrentStage_ReturnsCorrectCount()
        {
            var levelManager = CreateLevelManagerWithThreeLevels();

            levelManager.ApplyStage(0);
            yield return null;

            Assert.AreEqual(3, levelManager.TotalLevelsInCurrentStage);
        }

        [UnityTest]
        public IEnumerator TotalStepsInCurrentLevel_ReturnsCorrectCount()
        {
            var stepsDatabase = ScriptableObject.CreateInstance<LevelDatabase>();

            var emptyWave = new WaveData("EmptyWave", 0f, new List<EnemySpawnData>());
            var step0 = new StepData("Step0", new List<WaveData> { emptyWave });
            var step1 = new StepData("Step1", new List<WaveData> { emptyWave });
            var step2 = new StepData("Step2", new List<WaveData> { emptyWave });

            var level = new LevelData("Level0", new List<StepData> { step0, step1, step2 });
            var stage = new StageData("Stage0", new List<LevelData> { level });
            stepsDatabase.Stages.Add(stage);

            var levelManagerGo = new GameObject("TestStepsLevelManager");
            levelManagerGo.AddComponent<WorldConveyor>();
            var levelManager = levelManagerGo.AddComponent<LevelManager>();
            levelManager.enabled = false;
            Track(levelManagerGo);

            var teamContainer = Track(new GameObject("StepsTeam"));
            var enemiesContainer = Track(new GameObject("StepsEnemies"));

            levelManager.InitializeForTest(
                teamContainer.transform,
                enemiesContainer.transform,
                levelDatabase: stepsDatabase);

            levelManager.ApplyStage(0);
            yield return null;

            Assert.AreEqual(3, levelManager.TotalStepsInCurrentLevel);

            Object.DestroyImmediate(stepsDatabase);
        }

        [UnityTest]
        public IEnumerator TotalLevelsInCurrentStage_ReturnsZero_WhenNoDatabase()
        {
            var levelManagerGo = new GameObject("TestLevelManager");
            levelManagerGo.AddComponent<WorldConveyor>();
            var levelManager = levelManagerGo.AddComponent<LevelManager>();
            levelManager.enabled = false;
            Track(levelManagerGo);

            var teamContainer = Track(new GameObject("Team"));
            var enemiesContainer = Track(new GameObject("Enemies"));

            levelManager.InitializeForTest(
                teamContainer.transform,
                enemiesContainer.transform);

            yield return null;

            Assert.AreEqual(0, levelManager.TotalLevelsInCurrentStage);
        }
    }
}
