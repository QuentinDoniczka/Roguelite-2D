using System.Collections;
using System.Reflection;
using NUnit.Framework;
using RogueliteAutoBattler.Combat.Environment;
using UnityEngine;
using UnityEngine.TestTools;

namespace RogueliteAutoBattler.Tests.PlayMode
{
    public class ScreenAnchorTests : PlayModeTestBase
    {
        private const float PositionTolerance = 0.001f;

        private Camera _camera;

        [SetUp]
        public void SetUp()
        {
            var camGo = Track(new GameObject("MainCamera"));
            _camera = camGo.AddComponent<Camera>();
            _camera.tag = "MainCamera";
            _camera.orthographic = true;
            _camera.orthographicSize = 5f;
            _camera.transform.position = new Vector3(0f, 0f, -10f);
        }

        private ScreenAnchor CreateAnchor(Vector2 viewportPosition, Vector2 worldOffset)
        {
            var go = new GameObject("TestScreenAnchor");
            go.SetActive(false);
            Track(go);

            var anchor = go.AddComponent<ScreenAnchor>();
            SetPrivateField(anchor, "_viewportPosition", viewportPosition);
            SetPrivateField(anchor, "_worldOffset", worldOffset);

            go.SetActive(true);
            return anchor;
        }

        [UnityTest]
        public IEnumerator UpdatePosition_AppliesWorldOffset()
        {
            ScreenAnchor anchor = CreateAnchor(
                viewportPosition: new Vector2(0.5f, 0.5f),
                worldOffset: new Vector2(0f, 0.5f));

            yield return null;

            Assert.AreEqual(0f, anchor.transform.position.x, PositionTolerance,
                "Viewport center X maps to world X 0 with camera at origin; offset.x is 0 so result should remain 0.");
            Assert.AreEqual(0.5f, anchor.transform.position.y, PositionTolerance,
                "Viewport center Y (world 0) plus worldOffset.y (0.5) must equal 0.5 on the transform.");
            Assert.AreEqual(0f, anchor.transform.position.z, PositionTolerance,
                "ScreenAnchor must always zero the Z component for 2D.");
        }

        [UnityTest]
        public IEnumerator UpdatePosition_ZeroOffset_NoShift()
        {
            ScreenAnchor anchor = CreateAnchor(
                viewportPosition: new Vector2(0.5f, 0.5f),
                worldOffset: Vector2.zero);

            yield return null;

            Assert.AreEqual(0f, anchor.transform.position.x, PositionTolerance,
                "Viewport center with zero offset must land on world origin X.");
            Assert.AreEqual(0f, anchor.transform.position.y, PositionTolerance,
                "Viewport center with zero offset must land on world origin Y.");
        }

        [UnityTest]
        public IEnumerator UpdatePosition_RespondsToCameraResize()
        {
            ScreenAnchor anchor = CreateAnchor(
                viewportPosition: new Vector2(0.5f, 1f),
                worldOffset: new Vector2(0f, 0.5f));

            yield return null;

            float expectedInitialY = 5f + 0.5f;
            Assert.AreEqual(expectedInitialY, anchor.transform.position.y, PositionTolerance,
                "With ortho size 5, viewport y=1 maps to world y=5; adding offset.y=0.5 gives y=5.5.");

            _camera.orthographicSize = 3f;
            yield return null;
            yield return null;

            float expectedAfterResizeY = 3f + 0.5f;
            Assert.AreEqual(expectedAfterResizeY, anchor.transform.position.y, PositionTolerance,
                "After the camera ortho size shrinks to 3, the anchor must re-resolve: viewport y=1 -> world y=3, plus offset.y=0.5 = 3.5.");
        }

        private static void SetPrivateField(object obj, string fieldName, object value)
        {
            FieldInfo field = obj.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(field, $"Private field '{fieldName}' not found on {obj.GetType().Name}.");
            field.SetValue(obj, value);
        }
    }
}
