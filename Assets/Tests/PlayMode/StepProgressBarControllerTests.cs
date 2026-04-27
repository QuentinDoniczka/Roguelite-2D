using System.Collections.Generic;
using NUnit.Framework;
using RogueliteAutoBattler.Combat.Environment;
using RogueliteAutoBattler.Combat.Levels;
using RogueliteAutoBattler.Data;
using RogueliteAutoBattler.UI.Toolkit;
using UnityEngine;
using UnityEngine.UIElements;

namespace RogueliteAutoBattler.Tests.PlayMode
{
    public class StepProgressBarControllerTests : PlayModeTestBase
    {
        private const float NeverSpawnDelay = 999f;

        private LevelManager _levelManager;
        private LevelDatabase _levelDatabase;
        private VisualElement _container;
        private StepProgressBarController _controller;

        [SetUp]
        public void SetUp()
        {
            (_levelDatabase, _levelManager, _controller, _container) = BuildSetup(3);
        }

        private (LevelDatabase database, LevelManager levelManager, StepProgressBarController controller, VisualElement container)
            BuildSetup(int stepCount, string prefix = "")
        {
            var database = ScriptableObject.CreateInstance<LevelDatabase>();
            var delayedWave = new WaveData("W1", NeverSpawnDelay, new List<EnemySpawnData>());

            var steps = new List<StepData>();
            for (int i = 0; i < stepCount; i++)
            {
                steps.Add(new StepData("Step" + i, new List<WaveData> { delayedWave }));
            }

            var level = new LevelData("Level0", steps);
            var stage = new StageData("Stage0", new List<LevelData> { level });
            database.Stages.Add(stage);

            var levelManagerGo = new GameObject(prefix + "LevelManager");
            levelManagerGo.AddComponent<WorldConveyor>();
            var levelManager = levelManagerGo.AddComponent<LevelManager>();
            levelManager.enabled = false;
            Track(levelManagerGo);

            var teamContainer = Track(new GameObject(prefix + "Team"));
            var enemiesContainer = Track(new GameObject(prefix + "Enemies"));
            levelManager.InitializeForTest(
                teamContainer.transform,
                enemiesContainer.transform,
                levelDatabase: database);

            var container = new VisualElement();
            var controller = new StepProgressBarController(container);
            controller.InitializeForTest(levelManager);

            return (database, levelManager, controller, container);
        }

        public override void TearDown()
        {
            base.TearDown();

            if (_levelDatabase != null)
                Object.DestroyImmediate(_levelDatabase);
        }

        [Test]
        public void Rebuild_CreatesCorrectSphereCount()
        {
            Assert.AreEqual(3, _controller.SphereCount);
        }

        [Test]
        public void Rebuild_CreatesCorrectLineCount()
        {
            Assert.AreEqual(2, _controller.LineCount);
        }

        [Test]
        public void FirstSphere_HasCurrentClass()
        {
            Assert.IsTrue(_controller.SphereHasClass(0, "step-sphere--current"));
        }

        [Test]
        public void OnStepStarted_UpdatesSphereClasses()
        {
            _controller.SimulateStepChange(1);

            Assert.IsTrue(_controller.SphereHasClass(0, "step-sphere--completed"));
            Assert.IsTrue(_controller.SphereHasClass(1, "step-sphere--current"));
            Assert.IsTrue(_controller.SphereHasClass(2, "step-sphere--upcoming"));
        }

        [Test]
        public void ScrollDot_BecomesActive_OnScrollStart()
        {
            _controller.SimulateScrollStart();

            Assert.IsTrue(_controller.IsScrollDotActive);
        }

        [Test]
        public void SingleStepLevel_ShowsOneSphereNoLines()
        {
            var (singleDatabase, _, singleController, _) = BuildSetup(1, "Single");

            Assert.AreEqual(1, singleController.SphereCount);
            Assert.AreEqual(0, singleController.LineCount);

            Object.DestroyImmediate(singleDatabase);
        }

        [Test]
        public void Dispose_UnsubscribesFromEvents()
        {
            var database = ScriptableObject.CreateInstance<LevelDatabase>();
            var delayedWave = new WaveData("W1", NeverSpawnDelay, new List<EnemySpawnData>());

            var threeStepLevel = new LevelData("Level0", new List<StepData>
            {
                new StepData("S0", new List<WaveData> { delayedWave }),
                new StepData("S1", new List<WaveData> { delayedWave }),
                new StepData("S2", new List<WaveData> { delayedWave })
            });
            var twoStepLevel = new LevelData("Level1", new List<StepData>
            {
                new StepData("S0", new List<WaveData> { delayedWave }),
                new StepData("S1", new List<WaveData> { delayedWave })
            });
            var stage = new StageData("Stage0", new List<LevelData> { threeStepLevel, twoStepLevel });
            database.Stages.Add(stage);

            var levelManagerGo = new GameObject("DisposeLevelManager");
            levelManagerGo.AddComponent<WorldConveyor>();
            var levelManager = levelManagerGo.AddComponent<LevelManager>();
            levelManager.enabled = false;
            Track(levelManagerGo);

            var teamContainer = Track(new GameObject("DisposeTeam"));
            var enemiesContainer = Track(new GameObject("DisposeEnemies"));
            levelManager.InitializeForTest(
                teamContainer.transform,
                enemiesContainer.transform,
                levelDatabase: database);

            var container = new VisualElement();
            var controller = new StepProgressBarController(container);
            controller.InitializeForTest(levelManager);

            Assert.AreEqual(3, controller.SphereCount);

            controller.Dispose();
            levelManager.StartLevel(1);

            Assert.AreEqual(3, controller.SphereCount);

            Object.DestroyImmediate(database);
        }
    }
}
