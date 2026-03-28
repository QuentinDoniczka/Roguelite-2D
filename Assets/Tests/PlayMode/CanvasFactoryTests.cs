using System.Collections;
using NUnit.Framework;
using RogueliteAutoBattler.Core;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace RogueliteAutoBattler.Tests.PlayMode
{
    public class CanvasFactoryTests : PlayModeTestBase
    {
        private Camera _camera;

        [SetUp]
        public void SetUp()
        {
            _camera = Track(new GameObject("TestCamera")).AddComponent<Camera>();
            _camera.orthographic = true;
        }

        [UnityTest]
        public IEnumerator Create_ReturnsGameObjectWithCanvas()
        {
            var go = CanvasFactory.Create(_camera);
            Track(go);
            yield return null;

            Assert.IsNotNull(go);
            Assert.IsNotNull(go.GetComponent<Canvas>());
        }

        [UnityTest]
        public IEnumerator Create_SetsRenderModeScreenSpaceCamera()
        {
            var go = CanvasFactory.Create(_camera);
            Track(go);
            yield return null;

            var canvas = go.GetComponent<Canvas>();
            Assert.AreEqual(RenderMode.ScreenSpaceCamera, canvas.renderMode);
        }

        [UnityTest]
        public IEnumerator Create_AssignsWorldCamera()
        {
            var go = CanvasFactory.Create(_camera);
            Track(go);
            yield return null;

            var canvas = go.GetComponent<Canvas>();
            Assert.AreEqual(_camera, canvas.worldCamera);
        }

        [UnityTest]
        public IEnumerator Create_SetsPlaneDistance()
        {
            var go = CanvasFactory.Create(_camera);
            Track(go);
            yield return null;

            var canvas = go.GetComponent<Canvas>();
            Assert.AreEqual(CanvasFactory.PlaneDistance, canvas.planeDistance, 0.001f);
        }

        [UnityTest]
        public IEnumerator Create_SetsSortingLayer()
        {
            var go = CanvasFactory.Create(_camera);
            Track(go);
            yield return null;

            var canvas = go.GetComponent<Canvas>();
            Assert.AreEqual("UI", canvas.sortingLayerName);
        }

        [UnityTest]
        public IEnumerator Create_HasCanvasScalerWithCorrectConfig()
        {
            var go = CanvasFactory.Create(_camera);
            Track(go);
            yield return null;

            var scaler = go.GetComponent<CanvasScaler>();
            Assert.IsNotNull(scaler);
            Assert.AreEqual(CanvasScaler.ScaleMode.ScaleWithScreenSize, scaler.uiScaleMode);
            Assert.AreEqual(CanvasFactory.ReferenceWidth, scaler.referenceResolution.x, 0.001f);
            Assert.AreEqual(CanvasFactory.ReferenceHeight, scaler.referenceResolution.y, 0.001f);
            Assert.AreEqual(CanvasFactory.MatchWidthOrHeight, scaler.matchWidthOrHeight, 0.001f);
        }

        [UnityTest]
        public IEnumerator Create_HasGraphicRaycaster()
        {
            var go = CanvasFactory.Create(_camera);
            Track(go);
            yield return null;

            Assert.IsNotNull(go.GetComponent<GraphicRaycaster>());
        }

        [UnityTest]
        public IEnumerator Create_WithNullCamera_StillCreatesCanvas()
        {
            var go = CanvasFactory.Create(null);
            Track(go);
            yield return null;

            Assert.IsNotNull(go);
            var canvas = go.GetComponent<Canvas>();
            Assert.IsNotNull(canvas);
            Assert.IsNull(canvas.worldCamera);
        }
    }
}
