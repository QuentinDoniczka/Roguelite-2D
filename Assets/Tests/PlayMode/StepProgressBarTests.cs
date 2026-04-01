using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using RogueliteAutoBattler.Combat.Environment;
using RogueliteAutoBattler.Combat.Levels;
using RogueliteAutoBattler.Data;
using RogueliteAutoBattler.UI.Widgets;
using UnityEngine;
using UnityEngine.TestTools;

namespace RogueliteAutoBattler.Tests.PlayMode
{
    public class StepProgressBarTests : PlayModeTestBase
    {
        private LevelManager _levelManager;
        private StepProgressBar _progressBar;
        private LevelDatabase _levelDatabase;

        [SetUp]
        public void SetUp()
        {
            _levelDatabase = ScriptableObject.CreateInstance<LevelDatabase>();
            var delayedWave = new WaveData("W1", 999f, new List<EnemySpawnData>());
            var step0 = new StepData("Step0", new List<WaveData> { delayedWave });
            var step1 = new StepData("Step1", new List<WaveData> { delayedWave });
            var step2 = new StepData("Step2", new List<WaveData> { delayedWave });
            var level = new LevelData("Level0", new List<StepData> { step0, step1, step2 });
            var stage = new StageData("Stage0", null, new List<LevelData> { level });
            _levelDatabase.Stages.Add(stage);

            var levelManagerGo = new GameObject("LevelManager");
            levelManagerGo.AddComponent<WorldConveyor>();
            _levelManager = levelManagerGo.AddComponent<LevelManager>();
            _levelManager.enabled = false;
            Track(levelManagerGo);

            var teamContainer = Track(new GameObject("Team"));
            var enemiesContainer = Track(new GameObject("Enemies"));
            _levelManager.InitializeForTest(
                teamContainer.transform,
                enemiesContainer.transform,
                levelDatabase: _levelDatabase);

            var canvasGo = new GameObject("TestCanvas");
            canvasGo.AddComponent<Canvas>();
            Track(canvasGo);

            var barGo = new GameObject("StepProgressBar");
            barGo.AddComponent<RectTransform>();
            barGo.transform.SetParent(canvasGo.transform);

            _progressBar = barGo.AddComponent<StepProgressBar>();
            _progressBar.InitializeForTest(_levelManager);

            _levelManager.ApplyStage(0);
        }

        public override void TearDown()
        {
            base.TearDown();

            if (_levelDatabase != null)
                Object.DestroyImmediate(_levelDatabase);
        }

        [UnityTest]
        public IEnumerator Rebuild_CreatesCorrectSphereCount()
        {
            yield return null;

            Assert.AreEqual(3, _progressBar.SphereCount);
        }

        [UnityTest]
        public IEnumerator Rebuild_CreatesCorrectLineCount()
        {
            yield return null;

            Assert.AreEqual(2, _progressBar.LineCount);
        }

        [UnityTest]
        public IEnumerator InitialState_FirstSphereIsCurrent()
        {
            yield return null;

            Assert.AreEqual(_progressBar.CurrentColor, _progressBar.GetSphereColor(0));
            Assert.AreEqual(_progressBar.UpcomingColor, _progressBar.GetSphereColor(1));
            Assert.AreEqual(_progressBar.UpcomingColor, _progressBar.GetSphereColor(2));
        }

        [UnityTest]
        public IEnumerator OnStepStarted_UpdatesSphereColors()
        {
            _progressBar.SimulateStepChange(1);
            yield return null;

            Assert.AreEqual(_progressBar.CompletedColor, _progressBar.GetSphereColor(0));
            Assert.AreEqual(_progressBar.CurrentColor, _progressBar.GetSphereColor(1));
            Assert.AreEqual(_progressBar.UpcomingColor, _progressBar.GetSphereColor(2));
        }

        [UnityTest]
        public IEnumerator OnStepStarted_UpdatesLineColors()
        {
            _progressBar.SimulateStepChange(1);
            yield return null;

            Assert.AreEqual(_progressBar.CompletedColor, _progressBar.GetLineColor(0));
            Assert.AreEqual(_progressBar.UpcomingColor, _progressBar.GetLineColor(1));
        }

        [UnityTest]
        public IEnumerator Rebuild_CreatesScrollDot_Inactive()
        {
            yield return null;

            Assert.IsFalse(_progressBar.IsScrollDotActive,
                "ScrollDot should be inactive after Rebuild.");

            var dotTransform = _progressBar.transform.Find("ScrollDot");
            Assert.IsNotNull(dotTransform, "A child named ScrollDot should exist.");
        }

        [UnityTest]
        public IEnumerator ScrollDot_BecomesActive_WhenConveyorScrollStarts()
        {
            yield return null;

            var conveyor = _levelManager.GetComponent<WorldConveyor>();
            conveyor.ScrollBy(2f, 10f, 20f);

            yield return null;

            Assert.IsTrue(_progressBar.IsScrollDotActive,
                "ScrollDot should be active when conveyor starts scrolling.");
        }

        [UnityTest]
        public IEnumerator ScrollDot_BecomesInactive_WhenStepChanges()
        {
            yield return null;

            var conveyor = _levelManager.GetComponent<WorldConveyor>();
            conveyor.ScrollBy(2f, 10f, 20f);

            yield return null;
            Assert.IsTrue(_progressBar.IsScrollDotActive);

            _progressBar.SimulateStepChange(1);

            Assert.IsFalse(_progressBar.IsScrollDotActive,
                "ScrollDot should be inactive after step change.");
        }

        [UnityTest]
        public IEnumerator ScrollDot_BecomesInactive_WhenConveyorScrollCompletes()
        {
            yield return null;

            var conveyor = _levelManager.GetComponent<WorldConveyor>();
            conveyor.ScrollBy(2f, 10f, 20f);

            yield return null;
            Assert.IsTrue(_progressBar.IsScrollDotActive);

            yield return new WaitForSeconds(2f);

            Assert.IsFalse(_progressBar.IsScrollDotActive,
                "ScrollDot should be inactive after scroll completes.");
        }

        [UnityTest]
        public IEnumerator DepartureSphere_BecomesCompletedColor_WhenScrollDotActive()
        {
            yield return null;

            _progressBar.SimulateScrollStart();
            yield return null;

            Assert.AreEqual(_progressBar.CompletedColor, _progressBar.GetSphereColor(0));
            Assert.AreEqual(_progressBar.UpcomingColor, _progressBar.GetSphereColor(1));
            Assert.AreEqual(_progressBar.UpcomingColor, _progressBar.GetSphereColor(2));
            Assert.IsTrue(_progressBar.IsScrollDotActive);
        }

        [UnityTest]
        public IEnumerator DepartureSphere_ReturnsToCurrent_AfterStepChange()
        {
            yield return null;

            _progressBar.SimulateScrollStart();
            yield return null;

            _progressBar.SimulateStepChange(1);
            yield return null;

            Assert.IsFalse(_progressBar.IsScrollDotActive);
            Assert.AreEqual(_progressBar.CompletedColor, _progressBar.GetSphereColor(0));
            Assert.AreEqual(_progressBar.CurrentColor, _progressBar.GetSphereColor(1));
            Assert.AreEqual(_progressBar.UpcomingColor, _progressBar.GetSphereColor(2));
        }

        [UnityTest]
        public IEnumerator DepartureSphere_BecomesCompletedColor_WhenScrollingFromMiddleStep()
        {
            _progressBar.SimulateStepChange(1);
            yield return null;

            _progressBar.SimulateScrollStart();
            yield return null;

            Assert.AreEqual(_progressBar.CompletedColor, _progressBar.GetSphereColor(0));
            Assert.AreEqual(_progressBar.CompletedColor, _progressBar.GetSphereColor(1));
            Assert.AreEqual(_progressBar.UpcomingColor, _progressBar.GetSphereColor(2));
            Assert.IsTrue(_progressBar.IsScrollDotActive);
        }

        [UnityTest]
        public IEnumerator SingleStepLevel_ShowsOneSphereNoLines()
        {
            var singleLevelDatabase = ScriptableObject.CreateInstance<LevelDatabase>();
            var delayedWave = new WaveData("W1", 999f, new List<EnemySpawnData>());
            var singleStep = new StepData("Step0", new List<WaveData> { delayedWave });
            var singleLevel = new LevelData("Level0", new List<StepData> { singleStep });
            var singleStage = new StageData("Stage0", null, new List<LevelData> { singleLevel });
            singleLevelDatabase.Stages.Add(singleStage);

            var singleLevelManagerGo = new GameObject("SingleLevelManager");
            singleLevelManagerGo.AddComponent<WorldConveyor>();
            var singleLevelManager = singleLevelManagerGo.AddComponent<LevelManager>();
            singleLevelManager.enabled = false;
            Track(singleLevelManagerGo);

            var teamContainer = Track(new GameObject("SingleTeam"));
            var enemiesContainer = Track(new GameObject("SingleEnemies"));
            singleLevelManager.InitializeForTest(
                teamContainer.transform,
                enemiesContainer.transform,
                levelDatabase: singleLevelDatabase);

            var canvasGo = new GameObject("SingleCanvas");
            canvasGo.AddComponent<Canvas>();
            Track(canvasGo);

            var barGo = new GameObject("SingleStepProgressBar");
            barGo.AddComponent<RectTransform>();
            barGo.transform.SetParent(canvasGo.transform);

            var singleBar = barGo.AddComponent<StepProgressBar>();
            singleBar.InitializeForTest(singleLevelManager);

            singleLevelManager.ApplyStage(0);

            yield return null;

            Assert.AreEqual(1, singleBar.SphereCount);
            Assert.AreEqual(0, singleBar.LineCount);

            Object.DestroyImmediate(singleLevelDatabase);
        }
    }
}
