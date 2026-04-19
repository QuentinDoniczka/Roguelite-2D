using System.Collections;
using NUnit.Framework;
using RogueliteAutoBattler.Combat.Core;
using RogueliteAutoBattler.Combat.Visuals;
using RogueliteAutoBattler.UI.Toolkit;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace RogueliteAutoBattler.Tests.PlayMode
{
    public class CombatWorldVisibilityNavTests : PlayModeTestBase
    {
        private GameObject _combatWorldGo;
        private CombatWorldVisibility _visibility;
        private Transform _teamContainer;
        private Transform _enemiesContainer;
        private SpriteRenderer _teamRenderer;
        private SpriteRenderer _enemyRenderer;
        private NavigationManager _navigationManager;
        private Button[] _tabButtons;
        private InMemoryScreen _defaultScreen;

        [SetUp]
        public void SetUp()
        {
            _combatWorldGo = Track(new GameObject("CombatWorld"));

            var teamGo = new GameObject(CombatSetupHelper.TeamContainerName);
            teamGo.transform.SetParent(_combatWorldGo.transform, false);
            _teamContainer = teamGo.transform;

            var enemiesGo = new GameObject(CombatSetupHelper.EnemiesContainerName);
            enemiesGo.transform.SetParent(_combatWorldGo.transform, false);
            _enemiesContainer = enemiesGo.transform;

            var teamUnitGo = new GameObject("TeamUnit");
            teamUnitGo.transform.SetParent(_teamContainer, false);
            _teamRenderer = teamUnitGo.AddComponent<SpriteRenderer>();

            var enemyUnitGo = new GameObject("EnemyUnit");
            enemyUnitGo.transform.SetParent(_enemiesContainer, false);
            _enemyRenderer = enemyUnitGo.AddComponent<SpriteRenderer>();

            _visibility = _combatWorldGo.AddComponent<CombatWorldVisibility>();
            _visibility.InitializeForTest(_teamContainer, _enemiesContainer);

            _tabButtons = new Button[5];
            for (int i = 0; i < _tabButtons.Length; i++)
                _tabButtons[i] = new Button { name = "tab-" + i };

            _defaultScreen = new InMemoryScreen();
            _navigationManager = new NavigationManager(_tabButtons, _defaultScreen);

            _visibility.WireNavigationForTest(_navigationManager);
        }

        public override void TearDown()
        {
            _navigationManager?.Dispose();
            _navigationManager = null;
            base.TearDown();
        }

        [UnityTest]
        public IEnumerator SetVisible_False_DisablesAllRenderersUnderCombatWorld()
        {
            yield return null;

            _visibility.SetVisible(false);

            Assert.IsFalse(_teamRenderer.enabled,
                "Team renderer must be disabled when CombatWorldVisibility is hidden.");
            Assert.IsFalse(_enemyRenderer.enabled,
                "Enemy renderer must be disabled when CombatWorldVisibility is hidden.");
            Assert.IsFalse(_visibility.Visible,
                "CombatWorldVisibility.Visible must be false after SetVisible(false).");
        }

        [UnityTest]
        public IEnumerator SetVisible_True_ReenablesAllRenderersUnderCombatWorld()
        {
            yield return null;

            _visibility.SetVisible(false);
            _visibility.SetVisible(true);

            Assert.IsTrue(_teamRenderer.enabled,
                "Team renderer must be re-enabled when CombatWorldVisibility is shown.");
            Assert.IsTrue(_enemyRenderer.enabled,
                "Enemy renderer must be re-enabled when CombatWorldVisibility is shown.");
            Assert.IsTrue(_visibility.Visible,
                "CombatWorldVisibility.Visible must be true after SetVisible(true).");
        }

        [UnityTest]
        public IEnumerator Nav_SwitchToVillage_HidesCombatWorld()
        {
            yield return null;

            Assert.IsTrue(_teamRenderer.enabled, "Pre-condition: team renderer should be visible at start (combat is the default screen).");
            Assert.IsTrue(_enemyRenderer.enabled, "Pre-condition: enemy renderer should be visible at start.");

            _navigationManager.SwitchTab(0);

            yield return null;

            Assert.IsFalse(_teamRenderer.enabled,
                "Switching to a tab (Village) must hide the combat team renderer via the OnTabChanged → SetVisible wire.");
            Assert.IsFalse(_enemyRenderer.enabled,
                "Switching to a tab (Village) must hide the combat enemy renderer via the OnTabChanged → SetVisible wire.");
            Assert.IsFalse(_visibility.Visible,
                "CombatWorldVisibility.Visible must be false after SwitchTab to a non-default tab.");
        }

        [UnityTest]
        public IEnumerator Nav_SwitchBackToCombat_ShowsCombatWorld()
        {
            yield return null;

            _navigationManager.SwitchTab(0);
            yield return null;

            Assert.IsFalse(_teamRenderer.enabled, "Pre-condition: combat must be hidden after switching to Village.");

            _navigationManager.ReturnToDefault();
            yield return null;

            Assert.IsTrue(_teamRenderer.enabled,
                "Returning to the default screen (Combat) must re-enable the team renderer via OnTabChanged(-1).");
            Assert.IsTrue(_enemyRenderer.enabled,
                "Returning to the default screen (Combat) must re-enable the enemy renderer via OnTabChanged(-1).");
            Assert.IsTrue(_visibility.Visible,
                "CombatWorldVisibility.Visible must be true after returning to the default screen.");
        }

        [UnityTest]
        public IEnumerator Nav_SwitchBetweenTabs_KeepsCombatHidden()
        {
            yield return null;

            _navigationManager.SwitchTab(0);
            yield return null;

            _navigationManager.SwitchTab(2);
            yield return null;

            Assert.IsFalse(_teamRenderer.enabled,
                "Switching between two non-default tabs (Village → Guild) must keep the combat world hidden.");
            Assert.IsFalse(_visibility.Visible,
                "CombatWorldVisibility.Visible must remain false when switching between non-default tabs.");
        }

        [UnityTest]
        public IEnumerator LateUpdate_HidesNewlySpawnedRenderers_WhileHidden()
        {
            yield return null;

            _navigationManager.SwitchTab(0);
            yield return null;

            var newUnitGo = new GameObject("LateSpawnedUnit");
            newUnitGo.transform.SetParent(_teamContainer, false);
            var newRenderer = newUnitGo.AddComponent<SpriteRenderer>();

            Assert.IsTrue(newRenderer.enabled,
                "Pre-condition: a freshly created SpriteRenderer is enabled by default.");

            yield return null;

            Assert.IsFalse(newRenderer.enabled,
                "CombatWorldVisibility.LateUpdate must disable renderers spawned while the combat world is hidden.");
        }

        private class InMemoryScreen : IScreen
        {
            public VisualElement Root { get; } = new VisualElement();
            public void OnShow() { }
            public void OnHide() { }
            public void OnPush() { }
            public void OnPop() { }
        }
    }
}
