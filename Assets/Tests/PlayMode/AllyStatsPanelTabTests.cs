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
    public class AllyStatsPanelTabTests : PlayModeTestBase
    {
        private Camera _camera;
        private UnitSelectionManager _selectionManager;
        private AllyStatsPanel _panel;
        private CanvasGroup _canvasGroup;
        private TMP_Text _emptyStateLabel;
        private TMP_Text[] _statValueLabels;
        private TMP_Text[] _statNameLabels;
        private CanvasGroup[] _statRowGroups;
        private GameObject[] _breakdownContainers;
        private TMP_Text[] _breakdownTexts;
        private GameObject _tabHeaderContainer;
        private GameObject[] _tabContents;
        private Image[] _tabButtonImages;
        private Color _activeColor;
        private Color _inactiveColor;
        private GameObject _allyGo;
        private CombatStats _combatStats;

        private const int STAT_ROW_COUNT = 6;
        private const int TAB_COUNT = 4;

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
            _canvasGroup = panelGo.AddComponent<CanvasGroup>();
            _panel = panelGo.AddComponent<AllyStatsPanel>();

            var emptyGo = new GameObject("EmptyLabel");
            emptyGo.transform.SetParent(panelGo.transform, false);
            _emptyStateLabel = emptyGo.AddComponent<TextMeshProUGUI>();

            _statValueLabels = new TMP_Text[STAT_ROW_COUNT];
            _statNameLabels = new TMP_Text[STAT_ROW_COUNT];
            _statRowGroups = new CanvasGroup[STAT_ROW_COUNT];
            _breakdownContainers = new GameObject[STAT_ROW_COUNT];
            _breakdownTexts = new TMP_Text[STAT_ROW_COUNT];

            for (int i = 0; i < STAT_ROW_COUNT; i++)
            {
                var rowGo = new GameObject($"StatRow_{i}");
                rowGo.transform.SetParent(panelGo.transform, false);
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
                _breakdownContainers[i] = breakdownGo;

                var bdTextGo = new GameObject($"BreakdownText_{i}");
                bdTextGo.transform.SetParent(breakdownGo.transform, false);
                _breakdownTexts[i] = bdTextGo.AddComponent<TextMeshProUGUI>();
            }

            _tabContents = new GameObject[TAB_COUNT];
            _tabButtonImages = new Image[TAB_COUNT];
            var tabHeaderGo = new GameObject("TabHeader");
            tabHeaderGo.transform.SetParent(panelGo.transform, false);
            _tabHeaderContainer = tabHeaderGo;

            for (int i = 0; i < TAB_COUNT; i++)
            {
                var contentGo = new GameObject($"TabContent_{i}");
                contentGo.transform.SetParent(panelGo.transform, false);
                _tabContents[i] = contentGo;

                var btnGo = new GameObject($"TabBtn_{i}");
                btnGo.transform.SetParent(tabHeaderGo.transform, false);
                _tabButtonImages[i] = btnGo.AddComponent<Image>();
            }

            _activeColor = Color.white;
            _inactiveColor = Color.gray;

            _panel.InitializeForTest(
                _selectionManager,
                _canvasGroup,
                PhysicsLayers.AllyLayer,
                _emptyStateLabel,
                _statValueLabels,
                _statNameLabels,
                _statRowGroups,
                _breakdownContainers,
                _breakdownTexts,
                _tabHeaderContainer,
                _tabContents,
                _tabButtonImages,
                _activeColor,
                _inactiveColor);

            _allyGo = TestCharacterFactory.CreateSelectableCharacter(
                name: "Ally",
                isAlly: true,
                position: new Vector2(0f, 0f));
            Track(_allyGo);
            _combatStats = _allyGo.GetComponent<CombatStats>();
            _combatStats.InitializeDirect(maxHp: 100, atk: 15, attackSpeed: 1.2f, regenHpPerSecond: 2f);
        }

        public override void TearDown()
        {
            base.TearDown();
            GameBootstrap.ResetForTest();
        }

        private void SelectAlly()
        {
            _selectionManager.SimulateClickAtWorldPos(new Vector2(0f, 0f));
        }

        private void DeselectAll()
        {
            _selectionManager.SimulateClickAtWorldPos(new Vector2(100f, 100f));
        }

        [UnityTest]
        public IEnumerator StatsTab_ActiveByDefault()
        {
            yield return null;

            SelectAlly();
            yield return null;

            Assert.AreEqual(0, _panel.ActiveTabIndex);
            Assert.IsTrue(_tabContents[0].activeSelf);
        }

        [UnityTest]
        public IEnumerator SwitchTab_ShowsTargetContent()
        {
            yield return null;

            SelectAlly();
            yield return null;

            _panel.SwitchTab(1);

            Assert.IsTrue(_tabContents[1].activeSelf);
            Assert.IsFalse(_tabContents[0].activeSelf);
        }

        [UnityTest]
        public IEnumerator SwitchTab_UpdatesButtonColors()
        {
            yield return null;

            SelectAlly();
            yield return null;

            _panel.SwitchTab(2);

            Assert.AreEqual(_activeColor, _tabButtonImages[2].color);
            Assert.AreEqual(_inactiveColor, _tabButtonImages[0].color);
        }

        [UnityTest]
        public IEnumerator StatsTab_ShowsAllStatValues()
        {
            yield return null;

            SelectAlly();
            yield return null;

            Assert.AreEqual("100 / 100", _panel.StatValueText(0));
            Assert.AreEqual("15", _panel.StatValueText(1));
            Assert.AreEqual("0", _panel.StatValueText(2));
            Assert.AreEqual("1.2", _panel.StatValueText(3));
            Assert.AreEqual("2.0/s", _panel.StatValueText(4));
            Assert.AreEqual("0%", _panel.StatValueText(5));
        }

        [UnityTest]
        public IEnumerator StatRow_ExpandsBreakdownOnToggle()
        {
            yield return null;

            SelectAlly();
            yield return null;

            _panel.ToggleBreakdown(0);

            Assert.IsTrue(_panel.IsBreakdownExpanded(0));
        }

        [UnityTest]
        public IEnumerator StatRow_CollapsesOnSecondToggle()
        {
            yield return null;

            SelectAlly();
            yield return null;

            _panel.ToggleBreakdown(0);
            _panel.ToggleBreakdown(0);

            Assert.IsFalse(_panel.IsBreakdownExpanded(0));
        }

        [UnityTest]
        public IEnumerator StatRow_BreakdownContainsBase()
        {
            yield return null;

            SelectAlly();
            yield return null;

            _panel.ToggleBreakdown(1);

            StringAssert.Contains("Base", _panel.BreakdownText(1));
        }

        [UnityTest]
        public IEnumerator StatRow_OnlyOneExpandedAtATime()
        {
            yield return null;

            SelectAlly();
            yield return null;

            _panel.ToggleBreakdown(0);
            Assert.IsTrue(_panel.IsBreakdownExpanded(0));

            _panel.ToggleBreakdown(1);
            Assert.IsFalse(_panel.IsBreakdownExpanded(0));
            Assert.IsTrue(_panel.IsBreakdownExpanded(1));
        }

        [UnityTest]
        public IEnumerator StatsTab_UpdatesOnDamage()
        {
            yield return null;

            SelectAlly();
            yield return null;

            Assert.AreEqual("100 / 100", _panel.StatValueText(0));

            _combatStats.TakeDamage(30);
            yield return null;

            Assert.AreEqual("70 / 100", _panel.StatValueText(0));
        }

        [UnityTest]
        public IEnumerator StatRow_StaggeredFadeIn()
        {
            yield return null;

            SelectAlly();
            yield return new WaitForSeconds(2f);

            for (int i = 0; i < STAT_ROW_COUNT; i++)
                Assert.AreEqual(1f, _statRowGroups[i].alpha, 0.01f, $"StatRow {i} should be fully visible after fade");
        }

        [UnityTest]
        public IEnumerator PlaceholderTab_IsActiveWhenSwitched()
        {
            yield return null;

            SelectAlly();
            yield return null;

            _panel.SwitchTab(1);

            Assert.IsTrue(_tabContents[1].activeSelf);
        }

        [UnityTest]
        public IEnumerator Hide_CollapsesBreakdown()
        {
            yield return null;

            SelectAlly();
            yield return null;

            _panel.ToggleBreakdown(0);
            Assert.IsTrue(_panel.IsBreakdownExpanded(0));

            DeselectAll();
            yield return null;

            Assert.IsFalse(_panel.IsBreakdownExpanded(0));
        }

        [UnityTest]
        public IEnumerator SwitchTab_ResetsOnReshow()
        {
            yield return null;

            SelectAlly();
            yield return null;

            _panel.SwitchTab(2);
            Assert.AreEqual(2, _panel.ActiveTabIndex);

            DeselectAll();
            yield return null;

            SelectAlly();
            yield return null;

            Assert.AreEqual(0, _panel.ActiveTabIndex);
        }

        [UnityTest]
        public IEnumerator BreakdownText_ShowsTotal()
        {
            yield return null;

            SelectAlly();
            yield return null;

            _panel.ToggleBreakdown(0);

            StringAssert.Contains("Total", _panel.BreakdownText(0));
        }
    }
}
