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
        private const float NeverSpawnDelay = 999f;

        private LevelManager _levelManager;
        private StepProgressBar _progressBar;
        private LevelDatabase _levelDatabase;

        [SetUp]
        public void SetUp()
        {
            (_levelDatabase, _levelManager, _progressBar) = BuildProgressBarSetup(3);
        }

        private (LevelDatabase database, LevelManager levelManager, StepProgressBar bar)
            BuildProgressBarSetup(int stepCount, string prefix = "", HashSet<int> specialStepIndices = null)
        {
            var database = ScriptableObject.CreateInstance<LevelDatabase>();
            var delayedWave = new WaveData("W1", NeverSpawnDelay, new List<EnemySpawnData>());

            var steps = new List<StepData>();
            for (int i = 0; i < stepCount; i++)
            {
                var stepType = specialStepIndices != null && specialStepIndices.Contains(i)
                    ? StepType.Special
                    : StepType.Normal;
                steps.Add(new StepData("Step" + i, new List<WaveData> { delayedWave }, stepType));
            }

            var level = new LevelData("Level0", steps);
            var stage = new StageData("Stage0", null, new List<LevelData> { level });
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

            var canvasGo = new GameObject(prefix + "Canvas");
            canvasGo.AddComponent<Canvas>();
            Track(canvasGo);

            var barGo = new GameObject(prefix + "StepProgressBar");
            barGo.AddComponent<RectTransform>();
            barGo.transform.SetParent(canvasGo.transform);

            var bar = barGo.AddComponent<StepProgressBar>();
            bar.InitializeForTest(levelManager);

            levelManager.ApplyStage(0);

            return (database, levelManager, bar);
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
        public IEnumerator ScrollDot_RemainsActive_WhenStepChanges()
        {
            yield return null;

            var conveyor = _levelManager.GetComponent<WorldConveyor>();
            conveyor.ScrollBy(2f, 10f, 20f);

            yield return null;
            Assert.IsTrue(_progressBar.IsScrollDotActive);

            _progressBar.SimulateStepChange(1);

            Assert.IsTrue(_progressBar.IsScrollDotActive,
                "ScrollDot should remain active after step change until scroll completes.");
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
        public IEnumerator DepartureSphere_StaysCompleted_DuringActiveScroll_AfterStepChange()
        {
            yield return null;

            _progressBar.SimulateScrollStart();
            yield return null;

            _progressBar.SimulateStepChange(1);
            yield return null;

            Assert.IsTrue(_progressBar.IsScrollDotActive,
                "ScrollDot should remain active after step change until scroll completes.");
            Assert.AreEqual(_progressBar.CompletedColor, _progressBar.GetSphereColor(0));
            Assert.AreEqual(_progressBar.CompletedColor, _progressBar.GetSphereColor(1),
                "Current sphere should show CompletedColor while scroll dot is active.");
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
            var (singleLevelDatabase, _, singleBar) = BuildProgressBarSetup(1, "Single");

            yield return null;

            Assert.AreEqual(1, singleBar.SphereCount);
            Assert.AreEqual(0, singleBar.LineCount);

            Object.DestroyImmediate(singleLevelDatabase);
        }

        [UnityTest]
        public IEnumerator SpecialStep_HasLargerSphere()
        {
            var specialIndices = new HashSet<int> { 1 };
            var (specialDb, _, specialBar) = BuildProgressBarSetup(3, "Special", specialIndices);

            yield return null;

            Assert.AreEqual(specialBar.SphereSize, specialBar.GetSpherePreferredWidth(0),
                "Normal step should use default sphere size.");
            Assert.AreEqual(specialBar.SpecialSphereSize, specialBar.GetSpherePreferredWidth(1),
                "Special step should use larger sphere size.");
            Assert.AreEqual(specialBar.SphereSize, specialBar.GetSpherePreferredWidth(2),
                "Normal step should use default sphere size.");

            Object.DestroyImmediate(specialDb);
        }
    }
}
