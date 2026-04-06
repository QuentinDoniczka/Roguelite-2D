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
        private CanvasGroup[] _statCardGroups;

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

            var hpLabelGo = new GameObject("HpLabel");
            hpLabelGo.transform.SetParent(panelGo.transform, false);
            var hpLabel = hpLabelGo.AddComponent<TextMeshProUGUI>();

            var atkLabelGo = new GameObject("AtkLabel");
            atkLabelGo.transform.SetParent(panelGo.transform, false);
            var atkLabel = atkLabelGo.AddComponent<TextMeshProUGUI>();

            var atkSpeedLabelGo = new GameObject("AtkSpeedLabel");
            atkSpeedLabelGo.transform.SetParent(panelGo.transform, false);
            var atkSpeedLabel = atkSpeedLabelGo.AddComponent<TextMeshProUGUI>();

            var canvasGroup = panelGo.AddComponent<CanvasGroup>();

            var emptyStateLabelGo = new GameObject("EmptyStateLabel");
            emptyStateLabelGo.transform.SetParent(canvasGo.transform, false);
            _emptyStateLabel = emptyStateLabelGo.AddComponent<TextMeshProUGUI>();

            _statCardGroups = new CanvasGroup[3];
            for (int i = 0; i < 3; i++)
            {
                var cardGo = new GameObject($"StatCard_{i}");
                cardGo.transform.SetParent(panelGo.transform, false);
                cardGo.AddComponent<RectTransform>();
                _statCardGroups[i] = cardGo.AddComponent<CanvasGroup>();
            }

            _panel = panelGo.AddComponent<AllyStatsPanel>();
            _panel.InitializeForTest(_selectionManager, hpLabel, atkLabel, atkSpeedLabel,
                canvasGroup, PhysicsLayers.AllyLayer, _emptyStateLabel, _statCardGroups);

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
            Assert.AreEqual("100 / 100", _panel.HpText);
            Assert.AreEqual("15", _panel.AtkText);
            Assert.AreEqual("1.2", _panel.AttackSpeedText);
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

            Assert.AreEqual("70 / 100", _panel.HpText);
        }

        [UnityTest]
        public IEnumerator Panel_HidesOnTrackedAllyDeath()
        {
            yield return null;

            _selectionManager.SimulateClickAtWorldPos(new Vector2(0f, 0f));
            yield return null;
            Assert.IsTrue(_panel.IsVisible);

            _allyCombatStats.TakeDamage(200);
            yield return null;

            Assert.IsFalse(_panel.IsVisible);
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
            Assert.AreEqual("100 / 100", _panel.HpText);
            Assert.AreEqual("15", _panel.AtkText);

            _selectionManager.SimulateClickAtWorldPos(new Vector2(0f, 2f));
            yield return null;

            Assert.AreEqual("200 / 200", _panel.HpText);
            Assert.AreEqual("25", _panel.AtkText);
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
        public IEnumerator StatCards_FadeInOnSelection()
        {
            yield return null;

            _selectionManager.SimulateClickAtWorldPos(new Vector2(0f, 0f));
            yield return new WaitForSeconds(0.5f);

            for (int i = 0; i < 3; i++)
                Assert.AreEqual(1f, _panel.StatCardAlpha(i), 0.01f, $"StatCard {i} should be fully visible");
        }

        [UnityTest]
        public IEnumerator StatCards_ResetToZeroOnHide()
        {
            yield return null;

            _selectionManager.SimulateClickAtWorldPos(new Vector2(0f, 0f));
            yield return new WaitForSeconds(0.5f);

            _selectionManager.SimulateClickAtWorldPos(new Vector2(100f, 100f));
            yield return null;

            for (int i = 0; i < 3; i++)
                Assert.AreEqual(0f, _panel.StatCardAlpha(i), 0.01f, $"StatCard {i} should be hidden");
        }
    }
}
