using System.Collections;
using NUnit.Framework;
using RogueliteAutoBattler.Combat.Core;
using RogueliteAutoBattler.Core;
using RogueliteAutoBattler.UI.Widgets;
using UnityEngine;
using UnityEngine.TestTools;

namespace RogueliteAutoBattler.Tests.PlayMode
{
    public class AllyStatsPanelTests : PlayModeTestBase
    {
        private UnitSelectionManager _selectionManager;
        private AllyStatsPanel _panel;
        private CombatStats _allyCombatStats;

        [SetUp]
        public void SetUp()
        {
            var fixture = AllyStatsPanelTestFixture.Create(Track, tabCount: 1);
            _selectionManager = fixture.SelectionManager;
            _panel = fixture.Panel;

            var allyGo = TestCharacterFactory.CreateSelectableCharacter(
                name: "Ally",
                isAlly: true,
                position: new Vector2(0f, 0f));
            Track(allyGo);
            _allyCombatStats = allyGo.GetComponent<CombatStats>();
            _allyCombatStats.InitializeDirect(100, 15, 1.2f);

            var enemyGo = TestCharacterFactory.CreateSelectableCharacter(
                name: "Enemy",
                isAlly: false,
                position: new Vector2(3f, 0f));
            Track(enemyGo);
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
        public IEnumerator Panel_StaysVisibleOnDeselection()
        {
            yield return null;

            _selectionManager.SimulateClickAtWorldPos(new Vector2(0f, 0f));
            yield return null;
            Assert.IsTrue(_panel.IsVisible);

            _selectionManager.SimulateClickAtWorldPos(new Vector2(100f, 100f));
            yield return null;

            Assert.IsTrue(_panel.IsVisible);
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
        public IEnumerator EmptyStateLabel_StaysHiddenOnDeselection()
        {
            yield return null;

            _selectionManager.SimulateClickAtWorldPos(new Vector2(0f, 0f));
            yield return null;
            Assert.IsFalse(_panel.IsEmptyStateLabelActive);

            _selectionManager.SimulateClickAtWorldPos(new Vector2(100f, 100f));
            yield return null;

            Assert.IsFalse(_panel.IsEmptyStateLabelActive);
        }

        [UnityTest]
        public IEnumerator StatRows_FadeInOnSelection()
        {
            yield return null;

            _selectionManager.SimulateClickAtWorldPos(new Vector2(0f, 0f));
            yield return new WaitForSeconds(2f);

            for (int i = 0; i < AllyStatsPanelTestFixture.StatRowCount; i++)
                Assert.AreEqual(1f, _panel.StatRowAlpha(i), 0.01f, $"StatRow {i} should be fully visible");
        }

        [UnityTest]
        public IEnumerator StatRows_StayVisibleOnDeselection()
        {
            yield return null;

            _selectionManager.SimulateClickAtWorldPos(new Vector2(0f, 0f));
            yield return new WaitForSeconds(2f);

            for (int i = 0; i < AllyStatsPanelTestFixture.StatRowCount; i++)
                Assert.AreEqual(1f, _panel.StatRowAlpha(i), 0.01f, $"StatRow {i} should be fully visible before deselect");

            _selectionManager.SimulateClickAtWorldPos(new Vector2(100f, 100f));
            yield return null;

            for (int i = 0; i < AllyStatsPanelTestFixture.StatRowCount; i++)
                Assert.AreEqual(1f, _panel.StatRowAlpha(i), 0.01f, $"StatRow {i} should stay visible after deselect");
        }
    }
}
