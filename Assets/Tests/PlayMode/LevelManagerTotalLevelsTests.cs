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

            var level0 = new LevelData("Level0", new List<WaveData> { emptyWave });
            var level1 = new LevelData("Level1", new List<WaveData> { emptyWave });
            var level2 = new LevelData("Level2", new List<WaveData> { emptyWave });

            var stage = new StageData("Stage0", null, new List<LevelData> { level0, level1, level2 });
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
