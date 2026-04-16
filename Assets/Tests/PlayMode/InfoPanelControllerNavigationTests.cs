using System.Collections;
using System.Reflection;
using NUnit.Framework;
using RogueliteAutoBattler.Combat.Core;
using RogueliteAutoBattler.Common;
using RogueliteAutoBattler.Core;
using RogueliteAutoBattler.UI.Toolkit;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace RogueliteAutoBattler.Tests.PlayMode
{
    public class InfoPanelControllerNavigationTests : PlayModeTestBase
    {
        private UnitSelectionManager _selectionManager;
        private InfoPanelController _controller;
        private GameObject _teamContainerGo;
        private GameObject _ally1;
        private GameObject _ally2;
        private GameObject _ally3;

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

        [UnitySetUp]
        public IEnumerator SetUp()
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

            _teamContainerGo = new GameObject("TeamContainer");
            Track(_teamContainerGo);

            _ally1 = TestCharacterFactory.CreateSelectableCharacter(
                name: "Ally1", isAlly: true, position: new Vector2(0, 0));
            _ally1.transform.SetParent(_teamContainerGo.transform, false);
            _ally1.GetComponent<CombatStats>().InitializeDirect(100, 10, 1f);
            Track(_ally1);

            _ally2 = TestCharacterFactory.CreateSelectableCharacter(
                name: "Ally2", isAlly: true, position: new Vector2(2, 0));
            _ally2.transform.SetParent(_teamContainerGo.transform, false);
            _ally2.GetComponent<CombatStats>().InitializeDirect(100, 10, 1f);
            Track(_ally2);

            _ally3 = TestCharacterFactory.CreateSelectableCharacter(
                name: "Ally3", isAlly: true, position: new Vector2(4, 0));
            _ally3.transform.SetParent(_teamContainerGo.transform, false);
            _ally3.GetComponent<CombatStats>().InitializeDirect(100, 10, 1f);
            Track(_ally3);

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

            _controller.InitializeForTest(_selectionManager, _teamContainerGo.transform);

            yield return null;
        }

        public override void TearDown()
        {
            _controller?.Dispose();
            base.TearDown();
            GameBootstrap.ResetForTest();
        }

        [UnityTest]
        public IEnumerator NavigateNextCyclesToSecondAlly()
        {
            Assert.AreEqual(0, _controller.CurrentRosterIndex);

            _controller.NavigateToNextAlly();
            yield return null;

            Assert.AreEqual(1, _controller.CurrentRosterIndex);
        }

        [UnityTest]
        public IEnumerator NavigatePrevWrapsAround()
        {
            Assert.AreEqual(0, _controller.CurrentRosterIndex);

            _controller.NavigateToPreviousAlly();
            yield return null;

            Assert.AreEqual(2, _controller.CurrentRosterIndex);
        }

        [Test]
        public void TeamPositionLabelShowsCorrectFormat()
        {
            Assert.AreEqual("1/3", _controller.TeamPosLabelText);
        }

        [Test]
        public void NameLabelShowsAllyName()
        {
            Assert.AreEqual("Ally1", _controller.NameLabelText);
        }

        [UnityTest]
        public IEnumerator NavigationUpdatesDisplayedStats()
        {
            _ally2.GetComponent<CombatStats>().InitializeDirect(200, 25, 2.0f);

            _controller.NavigateToNextAlly();
            yield return null;

            Assert.AreEqual("200 / 200", _controller.StatValueText(0));
        }
    }
}
