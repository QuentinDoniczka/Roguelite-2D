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
    public class BattleIndicatorControllerTests : PlayModeTestBase
    {
        private LevelManager _levelManager;
        private LevelDatabase _levelDatabase;
        private Label _compactLabel;
        private VisualElement _announcementOverlay;
        private Label _announcementLabel;
        private BattleIndicatorController _controller;

        [SetUp]
        public void SetUp()
        {
            _levelDatabase = ScriptableObject.CreateInstance<LevelDatabase>();
            var wave = new WaveData("W1", 0f, new List<EnemySpawnData>());
            var step = new StepData("Step0", new List<WaveData> { wave });
            var level1 = new LevelData("Level1", new List<StepData> { step });
            var level2 = new LevelData("Level2", new List<StepData> { step });
            var stage = new StageData("Stage1", new List<LevelData> { level1, level2 });
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

            _compactLabel = new Label();
            _announcementOverlay = new VisualElement();
            _announcementLabel = new Label();

            _controller = new BattleIndicatorController(
                _compactLabel,
                _announcementOverlay,
                _announcementLabel,
                _levelManager);
            _controller.InitializeForTest(_levelManager);
        }

        public override void TearDown()
        {
            base.TearDown();

            if (_levelDatabase != null)
                Object.DestroyImmediate(_levelDatabase);
        }

        [Test]
        public void CompactLabel_ShowsInitialValue()
        {
            Assert.AreEqual("1-1", _controller.CompactText);
        }

        [Test]
        public void CompactLabel_Updates_OnLevelStarted()
        {
            _levelManager.StartLevel(1);

            Assert.AreEqual("1-2", _controller.CompactText);
        }

        [Test]
        public void AnnouncementText_Updates_OnLevelStarted()
        {
            _levelManager.StartLevel(1);

            Assert.AreEqual("Stage 1 - Level 2", _controller.AnnouncementText);
        }

        [Test]
        public void Dispose_UnsubscribesFromLevelManager()
        {
            _controller.Dispose();

            _levelManager.StartLevel(1);

            Assert.AreEqual("1-1", _controller.CompactText);
        }
    }
}
