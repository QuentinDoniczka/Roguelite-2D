using System.Reflection;
using NUnit.Framework;
using RogueliteAutoBattler.Combat.Core;
using RogueliteAutoBattler.Common;
using RogueliteAutoBattler.Core;
using RogueliteAutoBattler.UI.Toolkit;
using UnityEngine;
using UnityEngine.UIElements;

namespace RogueliteAutoBattler.Tests.PlayMode
{
    public class InfoPanelControllerTests : PlayModeTestBase
    {
        private UnitSelectionManager _selectionManager;
        private InfoPanelController _controller;
        private GameObject _allyGo;
        private GameObject _enemyGo;
        private CombatStats _allyCombatStats;

        private VisualElement _infoArea;
        private Label _emptyLabel;
        private VisualElement _header;
        private Label _nameLabel;
        private Label _positionLabel;
        private Button _prevButton;
        private Button _nextButton;
        private Button[] _tabButtons;
        private VisualElement _tabBar;
        private ScrollView _scrollView;
        private VisualElement _tabContent;

        [SetUp]
        public void SetUp()
        {
            if (PhysicsLayers.SelectionLayer < 0)
                Assert.Ignore("Selection layer not configured in this environment.");

            if (UnitSelectionManager.Instance != null)
                Object.DestroyImmediate(UnitSelectionManager.Instance.gameObject);

            var camGo = new GameObject("TestCamera");
            var camera = camGo.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 10f;
            camera.tag = "MainCamera";
            Track(camGo);

            GameBootstrap.ResetForTest();
            var mainCameraProp = typeof(GameBootstrap).GetProperty("MainCamera",
                BindingFlags.Public | BindingFlags.Static);
            mainCameraProp.SetValue(null, camera);

            var managerGo = new GameObject("UnitSelectionManager");
            _selectionManager = managerGo.AddComponent<UnitSelectionManager>();
            Track(managerGo);

            _allyGo = TestCharacterFactory.CreateSelectableCharacter(
                name: "Ally",
                isAlly: true,
                position: new Vector2(0f, 0f));
            _allyCombatStats = _allyGo.GetComponent<CombatStats>();
            _allyCombatStats.InitializeDirect(100, 15, 1.2f);
            Track(_allyGo);

            _enemyGo = TestCharacterFactory.CreateSelectableCharacter(
                name: "Enemy",
                isAlly: false,
                position: new Vector2(3f, 0f));
            Track(_enemyGo);

            _infoArea = new VisualElement();
            _emptyLabel = new Label("Select an ally");
            _header = new VisualElement();
            _nameLabel = new Label();
            _positionLabel = new Label();
            _prevButton = new Button();
            _nextButton = new Button();
            _tabButtons = new Button[] { new Button(), new Button(), new Button() };
            _tabBar = new VisualElement();
            _scrollView = new ScrollView();
            _tabContent = new VisualElement();

            _controller = new InfoPanelController(
                _infoArea,
                _emptyLabel,
                _header,
                _nameLabel,
                _positionLabel,
                _prevButton,
                _nextButton,
                _tabButtons,
                _tabBar,
                _scrollView,
                _tabContent,
                _selectionManager);

            _controller.InitializeForTest(_selectionManager);
        }

        public override void TearDown()
        {
            _controller?.Dispose();
            base.TearDown();
            GameBootstrap.ResetForTest();
        }

        [Test]
        public void PanelHiddenByDefault()
        {
            Assert.IsFalse(_controller.IsVisible);
        }

        [Test]
        public void PanelShowsOnAllySelection()
        {
            _selectionManager.ForceSelect(_allyGo);

            Assert.IsTrue(_controller.IsVisible);
        }

        [Test]
        public void PanelStaysVisibleOnDeselection()
        {
            _selectionManager.ForceSelect(_allyGo);
            Assert.IsTrue(_controller.IsVisible);

            _selectionManager.ForceDeselect();

            Assert.IsTrue(_controller.IsVisible);
        }

        [Test]
        public void PanelHidesOnEnemySelection()
        {
            _selectionManager.ForceSelect(_allyGo);
            Assert.IsTrue(_controller.IsVisible);

            _selectionManager.ForceSelect(_enemyGo);

            Assert.IsFalse(_controller.IsVisible);
        }

        [Test]
        public void EmptyLabelVisibleWhenPanelHidden()
        {
            Assert.IsTrue(_controller.IsEmptyStateLabelVisible);
        }

        [Test]
        public void EmptyLabelHidesOnAllySelection()
        {
            _selectionManager.ForceSelect(_allyGo);

            Assert.IsFalse(_controller.IsEmptyStateLabelVisible);
        }

        [Test]
        public void DisposeUnsubscribesFromEvents()
        {
            _controller.Dispose();

            _selectionManager.ForceSelect(_allyGo);

            Assert.IsFalse(_controller.IsVisible);
        }

        [Test]
        public void StatsTabShowsAllStatValues()
        {
            _selectionManager.ForceSelect(_allyGo);

            Assert.AreEqual("100 / 100", _controller.StatValueText(0));
            Assert.AreEqual("15", _controller.StatValueText(1));
            Assert.AreEqual("1.2", _controller.StatValueText(3));
        }

        [Test]
        public void StatsTabUpdatesOnDamage()
        {
            _selectionManager.ForceSelect(_allyGo);

            _allyCombatStats.TakeDamage(20);

            Assert.AreEqual("80 / 100", _controller.StatValueText(0));
        }

        [Test]
        public void StatRowsShowCorrectNames()
        {
            _selectionManager.ForceSelect(_allyGo);

            Assert.AreEqual("HP", _controller.StatNameText(0));
            Assert.AreEqual("ATK", _controller.StatNameText(1));
            Assert.AreEqual("DEF", _controller.StatNameText(2));
            Assert.AreEqual("SPD", _controller.StatNameText(3));
            Assert.AreEqual("REGEN", _controller.StatNameText(4));
            Assert.AreEqual("CRIT", _controller.StatNameText(5));
        }
    }
}
