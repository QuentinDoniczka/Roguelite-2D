using System.Collections;
using NUnit.Framework;
using RogueliteAutoBattler.Combat.Core;
using RogueliteAutoBattler.Combat.Visuals;
using RogueliteAutoBattler.Common;
using RogueliteAutoBattler.Core;
using UnityEngine;
using UnityEngine.TestTools;

namespace RogueliteAutoBattler.Tests.PlayMode
{
    public class UnitSelectionManagerTests : PlayModeTestBase
    {
        private Camera _camera;
        private UnitSelectionManager _manager;
        private GameObject _allyGo;
        private GameObject _enemyGo;
        private SelectionOutline _allyOutline;
        private SelectionOutline _enemyOutline;

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
            _manager = managerGo.AddComponent<UnitSelectionManager>();
            Track(managerGo);

            _allyGo = TestCharacterFactory.CreateSelectableCharacter(
                name: "Ally",
                isAlly: true,
                position: new Vector2(0f, 0f));
            Track(_allyGo);
            _allyOutline = _allyGo.GetComponent<SelectionOutline>();

            _enemyGo = TestCharacterFactory.CreateSelectableCharacter(
                name: "Enemy",
                isAlly: false,
                position: new Vector2(3f, 0f));
            Track(_enemyGo);
            _enemyOutline = _enemyGo.GetComponent<SelectionOutline>();
        }

        public override void TearDown()
        {
            base.TearDown();
            GameBootstrap.ResetForTest();
        }

        [UnityTest]
        public IEnumerator SelectAlly_SetsSelectedUnit()
        {
            yield return null;

            _manager.SimulateClickAtWorldPos(new Vector2(0f, 0f));
            yield return null;

            Assert.AreEqual(_allyGo, _manager.SelectedUnit);
        }

        [UnityTest]
        public IEnumerator SelectAlly_ShowsOutline()
        {
            yield return null;

            _manager.SimulateClickAtWorldPos(new Vector2(0f, 0f));
            yield return null;

            Assert.IsTrue(_allyOutline.IsOutlined);
        }

        [UnityTest]
        public IEnumerator SelectEnemy_ShowsOutline()
        {
            yield return null;

            _manager.SimulateClickAtWorldPos(new Vector2(3f, 0f));
            yield return null;

            Assert.IsTrue(_enemyOutline.IsOutlined);
        }

        [UnityTest]
        public IEnumerator ClickEmptySpace_DeselectsCurrentUnit()
        {
            yield return null;

            _manager.SimulateClickAtWorldPos(new Vector2(0f, 0f));
            yield return null;
            Assert.IsNotNull(_manager.SelectedUnit);

            _manager.SimulateClickAtWorldPos(new Vector2(100f, 100f));
            yield return null;

            Assert.IsNull(_manager.SelectedUnit);
        }

        [UnityTest]
        public IEnumerator SelectNewUnit_DeselectsPreviousUnit()
        {
            yield return null;

            _manager.SimulateClickAtWorldPos(new Vector2(0f, 0f));
            yield return null;
            Assert.IsTrue(_allyOutline.IsOutlined);

            _manager.SimulateClickAtWorldPos(new Vector2(3f, 0f));
            yield return null;

            Assert.IsFalse(_allyOutline.IsOutlined);
            Assert.IsTrue(_enemyOutline.IsOutlined);
            Assert.AreEqual(_enemyGo, _manager.SelectedUnit);
        }

        [UnityTest]
        public IEnumerator OnUnitSelected_FiresWithCorrectGameObject()
        {
            yield return null;

            GameObject selectedFromEvent = null;
            _manager.OnUnitSelected += go => selectedFromEvent = go;

            _manager.SimulateClickAtWorldPos(new Vector2(0f, 0f));
            yield return null;

            Assert.AreEqual(_allyGo, selectedFromEvent);
        }

        [UnityTest]
        public IEnumerator OnUnitDeselected_FiresWhenClickingEmptySpace()
        {
            yield return null;

            _manager.SimulateClickAtWorldPos(new Vector2(0f, 0f));
            yield return null;

            bool deselectedFired = false;
            _manager.OnUnitDeselected += () => deselectedFired = true;

            _manager.SimulateClickAtWorldPos(new Vector2(100f, 100f));
            yield return null;

            Assert.IsTrue(deselectedFired);
        }

        [UnityTest]
        public IEnumerator DestroyedUnit_AutoDeselectsInLateUpdate()
        {
            yield return null;

            _manager.SimulateClickAtWorldPos(new Vector2(0f, 0f));
            yield return null;
            Assert.AreEqual(_allyGo, _manager.SelectedUnit);

            Object.Destroy(_allyGo);
            yield return null;

            Assert.IsTrue(_manager.SelectedUnit == null);
        }

        [UnityTest]
        public IEnumerator ForceDeselect_ClearsSelectionAndOutline()
        {
            yield return null;

            _manager.SimulateClickAtWorldPos(new Vector2(0f, 0f));
            yield return null;
            Assert.IsTrue(_allyOutline.IsOutlined);

            _manager.ForceDeselect();
            yield return null;

            Assert.IsNull(_manager.SelectedUnit);
            Assert.IsFalse(_allyOutline.IsOutlined);
        }
    }
}
