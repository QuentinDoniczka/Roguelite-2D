using System.Collections;
using System.Reflection;
using NUnit.Framework;
using RogueliteAutoBattler.Combat.Core;
using RogueliteAutoBattler.Common;
using RogueliteAutoBattler.Core;
using RogueliteAutoBattler.Tests;
using RogueliteAutoBattler.UI.Toolkit;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace RogueliteAutoBattler.Tests.PlayMode
{
    public class AllyStatsPanelControllerTests : PlayModeTestBase
    {
        private AllyStatsPanelController _controller;
        private UnitSelectionManager _selectionManager;
        private Camera _camera;
        private MonoBehaviour _coroutineHost;

        [SetUp]
        public void SetUp()
        {
            if (PhysicsLayers.SelectionLayer < 0)
                Assert.Ignore("Selection layer not configured.");

            if (UnitSelectionManager.Instance != null)
                Object.DestroyImmediate(UnitSelectionManager.Instance.gameObject);

            var camGo = new GameObject("TestCamera");
            _camera = camGo.AddComponent<Camera>();
            _camera.orthographic = true;
            _camera.orthographicSize = 10f;
            Track(camGo);

            GameBootstrap.ResetForTest();
            typeof(GameBootstrap)
                .GetProperty("MainCamera", BindingFlags.Public | BindingFlags.Static)
                ?.SetValue(null, _camera);

            var managerGo = new GameObject("UnitSelectionManager");
            _selectionManager = managerGo.AddComponent<UnitSelectionManager>();
            Track(managerGo);

            var hostGo = new GameObject("CoroutineHost");
            _coroutineHost = hostGo.AddComponent<CoroutineHostStub>();
            Track(hostGo);

            _controller = BuildController();
            _controller.InitializeForTest(_selectionManager);
        }

        public override void TearDown()
        {
            _controller?.Dispose();
            GameBootstrap.ResetForTest();
            base.TearDown();
        }

        private AllyStatsPanelController BuildController()
        {
            var panelRoot = new VisualElement { name = "info-panel-root" };
            var emptyLabel = new Label("Select an ally") { name = "info-empty-label" };
            var contentContainer = new VisualElement { name = "info-content" };
            contentContainer.style.display = DisplayStyle.None;
            var nameLabel = new Label { name = "info-name-label" };
            var teamPosLabel = new Label { name = "info-team-pos-label" };
            var prevButton = new Button { name = "nav-prev-btn", text = "<" };
            var nextButton = new Button { name = "nav-next-btn", text = ">" };

            var tabButtons = new[]
            {
                new Button { name = "info-tab-stats", text = "Stats" },
                new Button { name = "info-tab-traits", text = "Traits" },
                new Button { name = "info-tab-loot", text = "Loot" }
            };

            var statsScrollView = new ScrollView { name = "info-tab-content-stats" };
            var tabContents = new VisualElement[]
            {
                statsScrollView,
                new VisualElement { name = "info-tab-content-traits" },
                new VisualElement { name = "info-tab-content-loot" }
            };

            return new AllyStatsPanelController(
                panelRoot, emptyLabel, contentContainer,
                nameLabel, teamPosLabel,
                prevButton, nextButton,
                tabButtons, tabContents,
                statsScrollView, _coroutineHost);
        }

        private AllyStatsPanelController CreateControllerWithTeam(out Transform teamContainer, int allyCount = 2)
        {
            var teamGo = new GameObject("Team");
            Track(teamGo);
            teamContainer = teamGo.transform;

            for (int i = 0; i < allyCount; i++)
            {
                var ally = TestCharacterFactory.CreateSelectableCharacter(
                    $"Ally_{i}", maxHp: 100 + i * 50, atk: 15 + i * 5, isAlly: true,
                    position: new Vector2(i * 2, 0));
                ally.transform.SetParent(teamContainer, false);
                ally.transform.position = new Vector2(i * 2, 0);
                Track(ally);
            }

            var controller = BuildController();
            controller.InitializeForTest(_selectionManager, teamContainer);
            return controller;
        }

        [Test]
        public void Panel_HiddenByDefault()
        {
            Assert.IsFalse(_controller.IsVisible);
        }

        [UnityTest]
        public IEnumerator Panel_ShowsOnAllySelection()
        {
            var ally = TestCharacterFactory.CreateSelectableCharacter("Ally", 100, 15, true, new Vector2(0, 0));
            Track(ally);

            _selectionManager.ForceSelect(ally);
            yield return null;

            Assert.IsTrue(_controller.IsVisible);
        }

        [UnityTest]
        public IEnumerator Panel_StaysVisibleOnDeselection()
        {
            var ally = TestCharacterFactory.CreateSelectableCharacter("Ally", 100, 15, true, new Vector2(0, 0));
            Track(ally);

            _selectionManager.ForceSelect(ally);
            yield return null;

            Assert.IsTrue(_controller.IsVisible);

            _selectionManager.ForceDeselect();
            yield return null;

            Assert.IsTrue(_controller.IsVisible);
        }

        [UnityTest]
        public IEnumerator Panel_HidesOnEnemySelection()
        {
            var ally = TestCharacterFactory.CreateSelectableCharacter("Ally", 100, 15, true, new Vector2(0, 0));
            Track(ally);
            var enemy = TestCharacterFactory.CreateSelectableCharacter("Enemy", 50, 5, false, new Vector2(5, 0));
            Track(enemy);

            _selectionManager.ForceSelect(ally);
            yield return null;
            Assert.IsTrue(_controller.IsVisible);

            _selectionManager.ForceSelect(enemy);
            yield return null;
            Assert.IsFalse(_controller.IsVisible);
        }

        [Test]
        public void EmptyStateLabel_VisibleWhenPanelHidden()
        {
            Assert.IsTrue(_controller.IsEmptyStateLabelActive);
        }

        [UnityTest]
        public IEnumerator EmptyStateLabel_HidesOnAllySelection()
        {
            var ally = TestCharacterFactory.CreateSelectableCharacter("Ally", 100, 15, true, new Vector2(0, 0));
            Track(ally);

            _selectionManager.ForceSelect(ally);
            yield return null;

            Assert.IsFalse(_controller.IsEmptyStateLabelActive);
        }

        [UnityTest]
        public IEnumerator StatsTab_ShowsAllStatValues()
        {
            var ally = TestCharacterFactory.CreateSelectableCharacter("Ally", 100, 15, true, new Vector2(0, 0));
            ally.GetComponent<CombatStats>().InitializeDirect(100, 15, 1.2f, 2.0f);
            Track(ally);

            _selectionManager.ForceSelect(ally);
            yield return null;

            Assert.AreEqual("100 / 100", _controller.StatValueText(0));
            Assert.AreEqual("15", _controller.StatValueText(1));
            Assert.AreEqual("0", _controller.StatValueText(2));
            Assert.AreEqual("1.2", _controller.StatValueText(3));
            Assert.AreEqual("2.0/s", _controller.StatValueText(4));
            Assert.AreEqual("0%", _controller.StatValueText(5));
        }

        [UnityTest]
        public IEnumerator Panel_UpdatesHpOnDamage()
        {
            var ally = TestCharacterFactory.CreateSelectableCharacter("Ally", 100, 15, true, new Vector2(0, 0));
            Track(ally);

            _selectionManager.ForceSelect(ally);
            yield return null;

            ally.GetComponent<CombatStats>().TakeDamage(30);
            yield return null;

            Assert.AreEqual("70 / 100", _controller.StatValueText(0));
        }

        [UnityTest]
        public IEnumerator Panel_SwitchesToNewAlly()
        {
            var ally1 = TestCharacterFactory.CreateSelectableCharacter("Ally1", 100, 15, true, new Vector2(0, 0));
            Track(ally1);
            var ally2 = TestCharacterFactory.CreateSelectableCharacter("Ally2", 200, 25, true, new Vector2(3, 0));
            Track(ally2);

            _selectionManager.ForceSelect(ally1);
            yield return null;
            Assert.AreEqual("100 / 100", _controller.StatValueText(0));

            _selectionManager.ForceSelect(ally2);
            yield return null;
            Assert.AreEqual("200 / 200", _controller.StatValueText(0));
            Assert.AreEqual("25", _controller.StatValueText(1));
        }

        [UnityTest]
        public IEnumerator StatsTab_ActiveByDefault()
        {
            var ally = TestCharacterFactory.CreateSelectableCharacter("Ally", 100, 15, true, new Vector2(0, 0));
            Track(ally);

            _selectionManager.ForceSelect(ally);
            yield return null;

            Assert.AreEqual(0, _controller.ActiveTabIndex);
        }

        [UnityTest]
        public IEnumerator SwitchTab_ShowsTargetContent()
        {
            var ally = TestCharacterFactory.CreateSelectableCharacter("Ally", 100, 15, true, new Vector2(0, 0));
            Track(ally);

            _selectionManager.ForceSelect(ally);
            yield return null;

            _controller.SwitchTab(1);

            Assert.AreEqual(1, _controller.ActiveTabIndex);
        }

        [UnityTest]
        public IEnumerator SwitchTab_ResetsOnReshow()
        {
            var ally = TestCharacterFactory.CreateSelectableCharacter("Ally", 100, 15, true, new Vector2(0, 0));
            Track(ally);
            var enemy = TestCharacterFactory.CreateSelectableCharacter("Enemy", 50, 5, false, new Vector2(5, 0));
            Track(enemy);

            _selectionManager.ForceSelect(ally);
            yield return null;
            _controller.SwitchTab(2);
            Assert.AreEqual(2, _controller.ActiveTabIndex);

            _selectionManager.ForceSelect(enemy);
            yield return null;
            _selectionManager.ForceSelect(ally);
            yield return null;

            Assert.AreEqual(0, _controller.ActiveTabIndex);
        }

        [UnityTest]
        public IEnumerator StatRow_ExpandsBreakdownOnToggle()
        {
            var ally = TestCharacterFactory.CreateSelectableCharacter("Ally", 100, 15, true, new Vector2(0, 0));
            Track(ally);

            _selectionManager.ForceSelect(ally);
            yield return null;

            _controller.ToggleBreakdown(0);

            Assert.IsTrue(_controller.IsBreakdownExpanded(0));
        }

        [UnityTest]
        public IEnumerator StatRow_CollapsesOnSecondToggle()
        {
            var ally = TestCharacterFactory.CreateSelectableCharacter("Ally", 100, 15, true, new Vector2(0, 0));
            Track(ally);

            _selectionManager.ForceSelect(ally);
            yield return null;

            _controller.ToggleBreakdown(0);
            Assert.IsTrue(_controller.IsBreakdownExpanded(0));

            _controller.ToggleBreakdown(0);
            Assert.IsFalse(_controller.IsBreakdownExpanded(0));
        }

        [UnityTest]
        public IEnumerator StatRow_BreakdownContainsBase()
        {
            var ally = TestCharacterFactory.CreateSelectableCharacter("Ally", 100, 15, true, new Vector2(0, 0));
            Track(ally);

            _selectionManager.ForceSelect(ally);
            yield return null;

            _controller.ToggleBreakdown(1);

            StringAssert.Contains("Base", _controller.BreakdownText(1));
        }

        [UnityTest]
        public IEnumerator StatRow_OnlyOneExpandedAtATime()
        {
            var ally = TestCharacterFactory.CreateSelectableCharacter("Ally", 100, 15, true, new Vector2(0, 0));
            Track(ally);

            _selectionManager.ForceSelect(ally);
            yield return null;

            _controller.ToggleBreakdown(0);
            Assert.IsTrue(_controller.IsBreakdownExpanded(0));

            _controller.ToggleBreakdown(1);
            Assert.IsFalse(_controller.IsBreakdownExpanded(0));
            Assert.IsTrue(_controller.IsBreakdownExpanded(1));
        }

        [UnityTest]
        public IEnumerator BreakdownText_ShowsTotal()
        {
            var ally = TestCharacterFactory.CreateSelectableCharacter("Ally", 100, 15, true, new Vector2(0, 0));
            Track(ally);

            _selectionManager.ForceSelect(ally);
            yield return null;

            _controller.ToggleBreakdown(0);

            StringAssert.Contains("Total", _controller.BreakdownText(0));
        }

        [UnityTest]
        public IEnumerator StatRows_FadeInOnSelection()
        {
            var ally = TestCharacterFactory.CreateSelectableCharacter("Ally", 100, 15, true, new Vector2(0, 0));
            Track(ally);

            _selectionManager.ForceSelect(ally);
            yield return new WaitForSeconds(2f);

            for (int i = 0; i < 6; i++)
                Assert.AreEqual(1f, _controller.StatRowOpacity(i), 0.01f, $"Row {i} should be fully visible");
        }

        [UnityTest]
        public IEnumerator Panel_StaysVisibleOnTrackedAllyDeath()
        {
            var ally = TestCharacterFactory.CreateSelectableCharacter("Ally", 100, 15, true, new Vector2(0, 0));
            Track(ally);

            _selectionManager.ForceSelect(ally);
            yield return null;

            ally.GetComponent<CombatStats>().TakeDamage(200);
            yield return null;

            Assert.IsTrue(_controller.IsVisible);
            Assert.IsTrue(_controller.IsDisplayingDeadUnit);
        }

        [UnityTest]
        public IEnumerator NavigateNext_CyclesToNextAlly()
        {
            _controller.Dispose();

            _controller = CreateControllerWithTeam(out _, 2);
            yield return null;

            Assert.AreEqual(0, _controller.CurrentRosterIndex);

            _controller.NavigateToNextAlly();
            yield return null;

            Assert.AreEqual(1, _controller.CurrentRosterIndex);
        }

        [UnityTest]
        public IEnumerator NavigatePrev_WrapsToLast()
        {
            _controller.Dispose();

            _controller = CreateControllerWithTeam(out _, 2);
            yield return null;

            Assert.AreEqual(0, _controller.CurrentRosterIndex);

            _controller.NavigateToPreviousAlly();
            yield return null;

            Assert.AreEqual(1, _controller.CurrentRosterIndex);
        }

        [UnityTest]
        public IEnumerator TeamPosLabel_UpdatesOnNavigation()
        {
            _controller.Dispose();

            _controller = CreateControllerWithTeam(out _, 3);
            yield return null;

            Assert.AreEqual("1/3", _controller.TeamPosLabelText);

            _controller.NavigateToNextAlly();
            yield return null;

            Assert.AreEqual("2/3", _controller.TeamPosLabelText);
        }
    }

    public class CoroutineHostStub : MonoBehaviour { }
}
