using System.Collections;
using NUnit.Framework;
using RogueliteAutoBattler.Combat.Core;
using RogueliteAutoBattler.Common;
using RogueliteAutoBattler.Core;
using RogueliteAutoBattler.UI.Widgets;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace RogueliteAutoBattler.Tests.PlayMode
{
    public class AllyStatsPanelTests : PlayModeTestBase
    {
        private Camera _camera;
        private UnitSelectionManager _selectionManager;
        private AllyStatsPanel _panel;
        private GameObject _allyGo;
        private CombatStats _allyCombatStats;
        private GameObject _enemyGo;
        private TMP_Text _emptyStateLabel;
        private CanvasGroup[] _statRowGroups;
        private TMP_Text[] _statValueLabels;
        private TMP_Text[] _statNameLabels;

        private const int STAT_ROW_COUNT = 6;

        [SetUp]
        public void SetUp()
        {
            if (PhysicsLayers.SelectionLayer < 0)
                Assert.Ignore("Selection layer not configured in this environment.");

            if (UnitSelectionManager.Instance != null)
                Object.DestroyImmediate(UnitSelectionManager.Instance.gameObject);

            var camGo = new GameObject("TestCamera");
            _camera = camGo.AddComponent<Camera>();
            _camera.orthographic = true;
            _camera.orthographicSize = 10f;
            _camera.tag = "MainCamera";
            Track(camGo);

            GameBootstrap.ResetForTest();
            var mainCameraProp = typeof(GameBootstrap).GetProperty("MainCamera",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            mainCameraProp.SetValue(null, _camera);

            var managerGo = new GameObject("UnitSelectionManager");
            _selectionManager = managerGo.AddComponent<UnitSelectionManager>();
            Track(managerGo);

            var canvasGo = new GameObject("TestCanvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<CanvasScaler>();
            Track(canvasGo);

            var panelGo = new GameObject("AllyStatsPanel");
            panelGo.transform.SetParent(canvasGo.transform, false);
            panelGo.AddComponent<RectTransform>();
            var canvasGroup = panelGo.AddComponent<CanvasGroup>();

            var emptyStateLabelGo = new GameObject("EmptyStateLabel");
            emptyStateLabelGo.transform.SetParent(canvasGo.transform, false);
            _emptyStateLabel = emptyStateLabelGo.AddComponent<TextMeshProUGUI>();

            _statValueLabels = new TMP_Text[STAT_ROW_COUNT];
            _statNameLabels = new TMP_Text[STAT_ROW_COUNT];
            _statRowGroups = new CanvasGroup[STAT_ROW_COUNT];
            var breakdownContainers = new GameObject[STAT_ROW_COUNT];
            var breakdownTexts = new TMP_Text[STAT_ROW_COUNT];

            for (int i = 0; i < STAT_ROW_COUNT; i++)
            {
                var rowGo = new GameObject($"StatRow_{i}");
                rowGo.transform.SetParent(panelGo.transform, false);
                rowGo.AddComponent<RectTransform>();
                _statRowGroups[i] = rowGo.AddComponent<CanvasGroup>();

                var nameGo = new GameObject($"StatName_{i}");
                nameGo.transform.SetParent(rowGo.transform, false);
                _statNameLabels[i] = nameGo.AddComponent<TextMeshProUGUI>();

                var valueGo = new GameObject($"StatValue_{i}");
                valueGo.transform.SetParent(rowGo.transform, false);
                _statValueLabels[i] = valueGo.AddComponent<TextMeshProUGUI>();

                var breakdownGo = new GameObject($"Breakdown_{i}");
                breakdownGo.transform.SetParent(rowGo.transform, false);
                breakdownGo.SetActive(false);
                breakdownContainers[i] = breakdownGo;

                var bdTextGo = new GameObject($"BreakdownText_{i}");
                bdTextGo.transform.SetParent(breakdownGo.transform, false);
                breakdownTexts[i] = bdTextGo.AddComponent<TextMeshProUGUI>();
            }

            var tabHeaderGo = new GameObject("TabHeader");
            tabHeaderGo.transform.SetParent(panelGo.transform, false);

            var tabContents = new GameObject[1];
            var tabButtonImages = new Image[1];

            var contentGo = new GameObject("TabContent_0");
            contentGo.transform.SetParent(panelGo.transform, false);
            tabContents[0] = contentGo;

            var btnGo = new GameObject("TabBtn_0");
            btnGo.transform.SetParent(tabHeaderGo.transform, false);
            tabButtonImages[0] = btnGo.AddComponent<Image>();

            _panel = panelGo.AddComponent<AllyStatsPanel>();
            _panel.InitializeForTest(
                _selectionManager,
                canvasGroup,
                PhysicsLayers.AllyLayer,
                _emptyStateLabel,
                _statValueLabels,
                _statNameLabels,
                _statRowGroups,
                breakdownContainers,
                breakdownTexts,
                tabHeaderGo,
                tabContents,
                tabButtonImages,
                Color.white,
                Color.gray);

            _allyGo = TestCharacterFactory.CreateSelectableCharacter(
                name: "Ally",
                isAlly: true,
                position: new Vector2(0f, 0f));
            Track(_allyGo);
            _allyCombatStats = _allyGo.GetComponent<CombatStats>();
            _allyCombatStats.InitializeDirect(100, 15, 1.2f);

            _enemyGo = TestCharacterFactory.CreateSelectableCharacter(
                name: "Enemy",
                isAlly: false,
                position: new Vector2(3f, 0f));
            Track(_enemyGo);
        }

        public override void TearDown()
        {
            base.TearDown();
            GameBootstrap.ResetForTest();
        }

        [UnityTest]
        public IEnumerator Panel_HiddenByDefault()
        {
            yield return null;

            Assert.IsFalse(_panel.IsVisible);
        }

        [UnityTest]
        public IEnumerator Panel_ShowsOnAllySelection()
        {
            yield return null;

            _selectionManager.SimulateClickAtWorldPos(new Vector2(0f, 0f));
            yield return null;

            Assert.IsTrue(_panel.IsVisible);
            Assert.AreEqual("100 / 100", _panel.StatValueText(0));
            Assert.AreEqual("15", _panel.StatValueText(1));
            Assert.AreEqual("1.2", _panel.StatValueText(3));
        }

        [UnityTest]
        public IEnumerator Panel_HidesOnDeselection()
        {
            yield return null;

            _selectionManager.SimulateClickAtWorldPos(new Vector2(0f, 0f));
            yield return null;
            Assert.IsTrue(_panel.IsVisible);

            _selectionManager.SimulateClickAtWorldPos(new Vector2(100f, 100f));
            yield return null;

            Assert.IsFalse(_panel.IsVisible);
        }

        [UnityTest]
        public IEnumerator Panel_HidesOnEnemySelection()
        {
            yield return null;

            _selectionManager.SimulateClickAtWorldPos(new Vector2(0f, 0f));
            yield return null;
            Assert.IsTrue(_panel.IsVisible);

            _selectionManager.SimulateClickAtWorldPos(new Vector2(3f, 0f));
            yield return null;

            Assert.IsFalse(_panel.IsVisible);
        }

        [UnityTest]
        public IEnumerator Panel_UpdatesHpOnDamage()
        {
            yield return null;

            _selectionManager.SimulateClickAtWorldPos(new Vector2(0f, 0f));
            yield return null;

            _allyCombatStats.TakeDamage(30);
            yield return null;

            Assert.AreEqual("70 / 100", _panel.StatValueText(0));
        }

        [UnityTest]
        public IEnumerator Panel_StaysVisibleOnTrackedAllyDeath()
        {
            yield return null;

            _selectionManager.SimulateClickAtWorldPos(new Vector2(0f, 0f));
            yield return null;
            Assert.IsTrue(_panel.IsVisible);

            _allyCombatStats.TakeDamage(200);
            yield return null;

            Assert.IsTrue(_panel.IsVisible);
            Assert.IsTrue(_panel.IsDisplayingDeadUnit);
        }

        [UnityTest]
        public IEnumerator Panel_SwitchesToNewAlly()
        {
            var secondAllyGo = TestCharacterFactory.CreateSelectableCharacter(
                name: "Ally2",
                isAlly: true,
                position: new Vector2(0f, 2f));
            Track(secondAllyGo);
            var secondAllyStats = secondAllyGo.GetComponent<CombatStats>();
            secondAllyStats.InitializeDirect(200, 25, 0.8f);

            yield return null;

            _selectionManager.SimulateClickAtWorldPos(new Vector2(0f, 0f));
            yield return null;
            Assert.AreEqual("100 / 100", _panel.StatValueText(0));
            Assert.AreEqual("15", _panel.StatValueText(1));

            _selectionManager.SimulateClickAtWorldPos(new Vector2(0f, 2f));
            yield return null;

            Assert.AreEqual("200 / 200", _panel.StatValueText(0));
            Assert.AreEqual("25", _panel.StatValueText(1));
        }

        [UnityTest]
        public IEnumerator EmptyStateLabel_VisibleWhenPanelHidden()
        {
            yield return null;

            Assert.IsTrue(_panel.IsEmptyStateLabelActive);
        }

        [UnityTest]
        public IEnumerator EmptyStateLabel_HidesOnAllySelection()
        {
            yield return null;

            _selectionManager.SimulateClickAtWorldPos(new Vector2(0f, 0f));
            yield return null;

            Assert.IsFalse(_panel.IsEmptyStateLabelActive);
        }

        [UnityTest]
        public IEnumerator EmptyStateLabel_ShowsOnDeselection()
        {
            yield return null;

            _selectionManager.SimulateClickAtWorldPos(new Vector2(0f, 0f));
            yield return null;
            Assert.IsFalse(_panel.IsEmptyStateLabelActive);

            _selectionManager.SimulateClickAtWorldPos(new Vector2(100f, 100f));
            yield return null;

            Assert.IsTrue(_panel.IsEmptyStateLabelActive);
        }

        [UnityTest]
        public IEnumerator StatRows_FadeInOnSelection()
        {
            yield return null;

            _selectionManager.SimulateClickAtWorldPos(new Vector2(0f, 0f));
            yield return new WaitForSeconds(2f);

            for (int i = 0; i < STAT_ROW_COUNT; i++)
                Assert.AreEqual(1f, _panel.StatRowAlpha(i), 0.01f, $"StatRow {i} should be fully visible");
        }

        [UnityTest]
        public IEnumerator StatRows_ResetToZeroOnHide()
        {
            yield return null;

            _selectionManager.SimulateClickAtWorldPos(new Vector2(0f, 0f));
            yield return new WaitForSeconds(0.5f);

            _selectionManager.SimulateClickAtWorldPos(new Vector2(100f, 100f));
            yield return null;

            for (int i = 0; i < STAT_ROW_COUNT; i++)
                Assert.AreEqual(0f, _panel.StatRowAlpha(i), 0.01f, $"StatRow {i} should be hidden");
        }
    }
}
