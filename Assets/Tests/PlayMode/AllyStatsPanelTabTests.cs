using System.Collections;
using NUnit.Framework;
using RogueliteAutoBattler.Combat.Core;
using RogueliteAutoBattler.Core;
using RogueliteAutoBattler.UI.Widgets;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace RogueliteAutoBattler.Tests.PlayMode
{
    public class AllyStatsPanelTabTests : PlayModeTestBase
    {
        private UnitSelectionManager _selectionManager;
        private AllyStatsPanel _panel;
        private CanvasGroup[] _statRowGroups;
        private GameObject[] _tabContents;
        private Image[] _tabButtonImages;
        private Color _activeColor;
        private Color _inactiveColor;
        private GameObject _allyGo;
        private CombatStats _combatStats;

        private const int TabCount = 3;

        [SetUp]
        public void SetUp()
        {
            var fixture = AllyStatsPanelTestFixture.Create(Track, tabCount: TabCount);
            _selectionManager = fixture.SelectionManager;
            _panel = fixture.Panel;
            _statRowGroups = fixture.StatRowGroups;
            _tabContents = fixture.TabContents;
            _tabButtonImages = fixture.TabButtonImages;
            _activeColor = fixture.ActiveTabColor;
            _inactiveColor = fixture.InactiveTabColor;

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

            for (int i = 0; i < AllyStatsPanelTestFixture.StatRowCount; i++)
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
