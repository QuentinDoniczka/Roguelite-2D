#if UNITY_EDITOR
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using RogueliteAutoBattler.UI.Toolkit.SkillTree;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace RogueliteAutoBattler.Tests.PlayMode
{
    public class SkillTreePanZoomManipulatorTests : PlayModeTestBase
    {
        private const string PanelSettingsPath = "Assets/UI/MainPanelSettings.asset";

        private const float MinimumZoom = 0.4f;
        private const float MaximumZoom = 2.5f;
        private const float WheelZoomStep = 0.1f;
        private const float ClickVersusDragThresholdPixels = 12f;
        private const float PanTolerancePixels = 0.5f;
        private const float ZoomToleranceUnits = 0.001f;

        private UIDocument _uiDocument;
        private VisualElement _viewport;
        private VisualElement _content;
        private SkillTreePanZoomManipulator _manipulator;

        [SetUp]
        public void SetUp()
        {
            PanelSettings panelSettings = AssetDatabase.LoadAssetAtPath<PanelSettings>(PanelSettingsPath);
            Assert.IsNotNull(panelSettings, $"MainPanelSettings not found at {PanelSettingsPath}");

            var documentGo = Track(new GameObject("TestPanZoomUIDocument"));
            documentGo.SetActive(false);
            _uiDocument = documentGo.AddComponent<UIDocument>();
            _uiDocument.panelSettings = panelSettings;
            documentGo.SetActive(true);
        }

        [Test]
        public void Constructor_InitialZoomIsOne()
        {
            var viewport = new VisualElement();
            var content = new VisualElement();
            var manipulator = new SkillTreePanZoomManipulator(viewport, content);

            Assert.AreEqual(1f, manipulator.CurrentZoom, ZoomToleranceUnits,
                "CurrentZoom must be 1 immediately after construction.");
        }

        [Test]
        public void Constructor_ExceededClickVersusDragThresholdIsFalseInitially()
        {
            var viewport = new VisualElement();
            var content = new VisualElement();
            var manipulator = new SkillTreePanZoomManipulator(viewport, content);

            Assert.IsFalse(manipulator.ExceededClickVersusDragThreshold,
                "ExceededClickVersusDragThreshold must be false before any gesture.");
        }

        [UnityTest]
        public IEnumerator WheelZoom_Increases_AndClampsToMaximum()
        {
            yield return SetUpHierarchy();
            if (_viewport == null) yield break;

            for (int i = 0; i < 50; i++)
            {
                SendWheelEvent(new Vector3(0f, -1f, 0f));
            }
            yield return null;

            Assert.LessOrEqual(_manipulator.CurrentZoom, MaximumZoom + ZoomToleranceUnits,
                "CurrentZoom must never exceed MaximumZoom after heavy zoom-in input.");
            Assert.Greater(_manipulator.CurrentZoom, 1f,
                "CurrentZoom must have increased above the initial value of 1.");
        }

        [UnityTest]
        public IEnumerator WheelZoom_Decreases_AndClampsToMinimum()
        {
            yield return SetUpHierarchy();
            if (_viewport == null) yield break;

            for (int i = 0; i < 50; i++)
            {
                SendWheelEvent(new Vector3(0f, 1f, 0f));
            }
            yield return null;

            Assert.GreaterOrEqual(_manipulator.CurrentZoom, MinimumZoom - ZoomToleranceUnits,
                "CurrentZoom must never drop below MinimumZoom after heavy zoom-out input.");
            Assert.Less(_manipulator.CurrentZoom, 1f,
                "CurrentZoom must have decreased below the initial value of 1.");
        }

        [UnityTest]
        public IEnumerator WheelZoom_AppliesStepPerTick()
        {
            yield return SetUpHierarchy();
            if (_viewport == null) yield break;

            float zoomBefore = _manipulator.CurrentZoom;
            SendWheelEvent(new Vector3(0f, -1f, 0f));
            yield return null;

            Assert.AreEqual(zoomBefore + WheelZoomStep, _manipulator.CurrentZoom, ZoomToleranceUnits,
                "A single wheel tick with delta.y = -1 must increase CurrentZoom by exactly WheelZoomStep.");
        }

        [UnityTest]
        public IEnumerator Pan_MovesContent_ByPointerDelta()
        {
            yield return SetUpHierarchy();
            if (_viewport == null) yield break;

            Vector3 contentPositionBefore = _content.transform.position;

            SendPointerDownEvent(new Vector2(100f, 100f), pointerId: 0);
            yield return null;
            SendPointerMoveEvent(new Vector2(150f, 150f), pointerId: 0);
            yield return null;

            Vector3 contentPositionAfter = _content.transform.position;
            Assert.AreEqual(contentPositionBefore.x + 50f, contentPositionAfter.x, PanTolerancePixels,
                "Content.x must shift by +50 after a 50-pixel horizontal pan delta.");
            Assert.AreEqual(contentPositionBefore.y + 50f, contentPositionAfter.y, PanTolerancePixels,
                "Content.y must shift by +50 after a 50-pixel vertical pan delta.");
        }

        [UnityTest]
        public IEnumerator Click_BelowThreshold_DoesNotSetDragExceeded()
        {
            yield return SetUpHierarchy();
            if (_viewport == null) yield break;

            SendPointerDownEvent(new Vector2(0f, 0f), pointerId: 0);
            yield return null;
            SendPointerMoveEvent(new Vector2(5f, 5f), pointerId: 0);
            yield return null;
            SendPointerUpEvent(new Vector2(5f, 5f), pointerId: 0);
            yield return null;

            Assert.IsFalse(_manipulator.ExceededClickVersusDragThreshold,
                "A movement under ClickVersusDragThresholdPixels must be treated as a click, not a drag.");
        }

        [UnityTest]
        public IEnumerator Drag_AboveThreshold_SetsDragExceeded()
        {
            yield return SetUpHierarchy();
            if (_viewport == null) yield break;

            SendPointerDownEvent(new Vector2(0f, 0f), pointerId: 0);
            yield return null;
            SendPointerMoveEvent(new Vector2(50f, 50f), pointerId: 0);
            yield return null;
            SendPointerUpEvent(new Vector2(50f, 50f), pointerId: 0);
            yield return null;

            Assert.IsTrue(_manipulator.ExceededClickVersusDragThreshold,
                "A movement well above ClickVersusDragThresholdPixels must be flagged as a drag after pointer-up.");
        }

        [UnityTest]
        public IEnumerator PointerDown_DoesNotCaptureAnyPointer()
        {
            yield return SetUpHierarchy();
            if (_viewport == null) yield break;

            const int pointerId = 0;
            SendPointerDownEvent(new Vector2(0f, 0f), pointerId);
            yield return null;

            Assert.IsFalse(_viewport.HasPointerCapture(pointerId),
                "PointerDown alone must not capture the pointer; capture must be lazy.");
        }

        [UnityTest]
        public IEnumerator Drag_AboveThreshold_CapturesPointer()
        {
            yield return SetUpHierarchy();
            if (_viewport == null) yield break;

            const int pointerId = 0;
            SendPointerDownEvent(new Vector2(0f, 0f), pointerId);
            yield return null;
            SendPointerMoveEvent(new Vector2(50f, 0f), pointerId);
            yield return null;

            Assert.IsTrue(_viewport.HasPointerCapture(pointerId),
                "Pointer must be captured once the click-vs-drag threshold is exceeded.");
        }

        [UnityTest]
        public IEnumerator Click_BelowThreshold_NeverCapturesPointer()
        {
            yield return SetUpHierarchy();
            if (_viewport == null) yield break;

            const int pointerId = 0;
            SendPointerDownEvent(new Vector2(0f, 0f), pointerId);
            yield return null;
            Assert.IsFalse(_viewport.HasPointerCapture(pointerId),
                "Capture must not be acquired on PointerDown.");

            SendPointerMoveEvent(new Vector2(5f, 0f), pointerId);
            yield return null;
            Assert.IsFalse(_viewport.HasPointerCapture(pointerId),
                "Capture must not be acquired on a tiny move below threshold.");

            SendPointerUpEvent(new Vector2(5f, 0f), pointerId);
            yield return null;
            Assert.IsFalse(_viewport.HasPointerCapture(pointerId),
                "Capture must not have been acquired at any point during a click gesture.");
        }

        [UnityTest]
        public IEnumerator Move_ExactlyAtThreshold_TriggersDrag()
        {
            yield return SetUpHierarchy();
            if (_viewport == null) yield break;

            const int pointerId = 0;
            SendPointerDownEvent(new Vector2(0f, 0f), pointerId);
            yield return null;
            SendPointerMoveEvent(new Vector2(ClickVersusDragThresholdPixels, 0f), pointerId);
            yield return null;
            SendPointerUpEvent(new Vector2(ClickVersusDragThresholdPixels, 0f), pointerId);
            yield return null;

            Assert.IsTrue(_manipulator.ExceededClickVersusDragThreshold,
                "A move exactly at the threshold distance must be classified as a drag.");
        }

        [UnityTest]
        public IEnumerator Move_JustBelowThreshold_StaysClick()
        {
            yield return SetUpHierarchy();
            if (_viewport == null) yield break;

            const int pointerId = 0;
            const float justBelowThreshold = ClickVersusDragThresholdPixels - 1f;
            SendPointerDownEvent(new Vector2(0f, 0f), pointerId);
            yield return null;
            SendPointerMoveEvent(new Vector2(justBelowThreshold, 0f), pointerId);
            yield return null;
            SendPointerUpEvent(new Vector2(justBelowThreshold, 0f), pointerId);
            yield return null;

            Assert.IsFalse(_manipulator.ExceededClickVersusDragThreshold,
                "A move just below the threshold distance must remain a click.");
        }

        [UnityTest]
        public IEnumerator KeyboardZoomIn_IncreasesZoom()
        {
            yield return SetUpHierarchy();
            if (_viewport == null) yield break;

            float zoomBefore = _manipulator.CurrentZoom;
            SendKeyDownEvent(KeyCode.Equals);
            yield return null;

            Assert.AreEqual(zoomBefore + WheelZoomStep, _manipulator.CurrentZoom, ZoomToleranceUnits,
                "Pressing Equals key must increase CurrentZoom by exactly WheelZoomStep.");
        }

        [UnityTest]
        public IEnumerator KeyboardZoomOut_DecreasesZoom()
        {
            yield return SetUpHierarchy();
            if (_viewport == null) yield break;

            float zoomBefore = _manipulator.CurrentZoom;
            SendKeyDownEvent(KeyCode.Minus);
            yield return null;

            Assert.AreEqual(zoomBefore - WheelZoomStep, _manipulator.CurrentZoom, ZoomToleranceUnits,
                "Pressing Minus key must decrease CurrentZoom by exactly WheelZoomStep.");
        }

        private IEnumerator SetUpHierarchy()
        {
            yield return null;
            yield return null;

            VisualElement root = _uiDocument.rootVisualElement;
            if (root == null)
            {
                Assert.Inconclusive("rootVisualElement is null - UIDocument failed to initialize in the test environment.");
                yield break;
            }

            _viewport = new VisualElement { name = "viewport" };
            _viewport.style.width = 800f;
            _viewport.style.height = 600f;
            root.Add(_viewport);

            _content = new VisualElement { name = "content" };
            _content.style.width = 2000f;
            _content.style.height = 2000f;
            _viewport.Add(_content);

            _manipulator = new SkillTreePanZoomManipulator(_viewport, _content);
            _viewport.AddManipulator(_manipulator);

            yield return null;
        }

        private void SendWheelEvent(Vector3 delta)
        {
            using (var wheelEvent = WheelEvent.GetPooled())
            {
                SetEventProperty(wheelEvent, "delta", delta);
                wheelEvent.target = _viewport;
                _viewport.SendEvent(wheelEvent);
            }
        }

        private void SendPointerDownEvent(Vector2 position, int pointerId)
        {
            using (var evt = PointerDownEvent.GetPooled())
            {
                PopulatePointerEvent(evt, position, pointerId);
                _viewport.SendEvent(evt);
            }
        }

        private void SendPointerMoveEvent(Vector2 position, int pointerId)
        {
            using (var evt = PointerMoveEvent.GetPooled())
            {
                PopulatePointerEvent(evt, position, pointerId);
                _viewport.SendEvent(evt);
            }
        }

        private void SendPointerUpEvent(Vector2 position, int pointerId)
        {
            using (var evt = PointerUpEvent.GetPooled())
            {
                PopulatePointerEvent(evt, position, pointerId);
                _viewport.SendEvent(evt);
            }
        }

        private void PopulatePointerEvent(EventBase evt, Vector2 position, int pointerId)
        {
            Vector3 position3 = new Vector3(position.x, position.y, 0f);
            SetEventProperty(evt, "position", position3);
            SetEventProperty(evt, "localPosition", position3);
            SetEventProperty(evt, "pointerId", pointerId);
            evt.target = _viewport;
        }

        private void SendKeyDownEvent(KeyCode keyCode)
        {
            using (var evt = KeyDownEvent.GetPooled('\0', keyCode, EventModifiers.None))
            {
                evt.target = _viewport;
                _viewport.SendEvent(evt);
            }
        }

        private static void SetEventProperty(EventBase evt, string propertyName, object value)
        {
            System.Type type = evt.GetType();
            while (type != null)
            {
                PropertyInfo property = type.GetProperty(
                    propertyName,
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                if (property != null && property.CanWrite)
                {
                    property.SetValue(evt, value);
                    return;
                }

                FieldInfo field = type.GetField(
                    propertyName,
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                if (field != null)
                {
                    field.SetValue(evt, value);
                    return;
                }

                type = type.BaseType;
            }
        }
    }
}
#endif
