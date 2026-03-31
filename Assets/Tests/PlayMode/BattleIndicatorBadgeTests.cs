using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using RogueliteAutoBattler.Combat.Environment;
using RogueliteAutoBattler.Combat.Levels;
using RogueliteAutoBattler.Data;
using RogueliteAutoBattler.UI.Widgets;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;

namespace RogueliteAutoBattler.Tests.PlayMode
{
    public class BattleIndicatorBadgeTests : PlayModeTestBase
    {
        private LevelManager _levelManager;
        private BattleIndicatorBadge _badge;
        private TMP_Text _compactLabel;
        private TMP_Text _announcementLabel;
        private CanvasGroup _announcementGroup;
        private LevelDatabase _levelDatabase;

        [SetUp]
        public void SetUp()
        {
            _levelDatabase = ScriptableObject.CreateInstance<LevelDatabase>();
            var wave = new WaveData("W1", 0f, new List<EnemySpawnData>());
            var step = new StepData("Step0", new List<WaveData> { wave });
            var level1 = new LevelData("Level1", new List<StepData> { step });
            var level2 = new LevelData("Level2", new List<StepData> { step });
            var stage = new StageData("Stage1", null, new List<LevelData> { level1, level2 });
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

            var badgeGo = new GameObject("BattleIndicatorBadge");
            badgeGo.AddComponent<RectTransform>();
            badgeGo.transform.SetParent(canvasGo.transform);

            _compactLabel = new GameObject("CompactLabel").AddComponent<TextMeshProUGUI>();
            _compactLabel.transform.SetParent(badgeGo.transform);

            _announcementLabel = new GameObject("AnnouncementLabel").AddComponent<TextMeshProUGUI>();
            _announcementLabel.transform.SetParent(badgeGo.transform);

            var overlayGo = new GameObject("AnnouncementOverlay");
            overlayGo.AddComponent<RectTransform>();
            overlayGo.transform.SetParent(badgeGo.transform);
            _announcementGroup = overlayGo.AddComponent<CanvasGroup>();

            _badge = badgeGo.AddComponent<BattleIndicatorBadge>();
            _badge.InitializeForTest(_levelManager, _compactLabel, _announcementLabel, _announcementGroup);
        }

        public override void TearDown()
        {
            base.TearDown();

            if (_levelDatabase != null)
                Object.DestroyImmediate(_levelDatabase);
        }

        [UnityTest]
        public IEnumerator CompactLabel_ShowsInitialValue()
        {
            yield return null;

            Assert.AreEqual("1-1", _badge.CompactText);
        }

        [UnityTest]
        public IEnumerator CompactLabel_Updates_OnLevelStarted()
        {
            yield return null;

            _levelManager.StartLevel(1);
            yield return null;

            Assert.AreEqual("1-2", _badge.CompactText);
        }

        [UnityTest]
        public IEnumerator Announcement_Appears_OnLevelStarted()
        {
            yield return null;

            _levelManager.StartLevel(1);
            yield return new WaitForSeconds(0.3f);

            Assert.Greater(_badge.AnnouncementAlpha, 0.9f,
                "Announcement should be nearly fully visible after fade-in completes.");
            Assert.AreEqual("Stage 1 - Level 2", _badge.AnnouncementText);
        }

        [UnityTest]
        public IEnumerator Announcement_FadesOut_AfterHoldDuration()
        {
            yield return null;

            _levelManager.StartLevel(1);
            yield return new WaitForSeconds(2.0f);

            Assert.Less(_badge.AnnouncementAlpha, 0.05f,
                "Announcement should have faded out after total animation duration.");
        }

        [UnityTest]
        public IEnumerator CompactLabel_DisplaysOneIndexed()
        {
            yield return null;

            Assert.AreEqual("1-1", _badge.CompactText,
                "Display should be 1-indexed (1-1) not 0-indexed (0-0).");
        }
    }
}
