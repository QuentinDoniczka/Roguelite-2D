using System.Collections;
using NUnit.Framework;
using RogueliteAutoBattler.Combat.Environment;
using RogueliteAutoBattler.Data;
using UnityEngine;
using UnityEngine.TestTools;

namespace RogueliteAutoBattler.Tests.PlayMode
{
    public class GroundFitterFitModeTests : PlayModeTestBase
    {
        private const float SizeTolerance = 0.0001f;

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

        private (GroundFitter fitter, SpriteRenderer renderer) CreateGround()
        {
            var go = Track(new GameObject("TestGround"));
            var renderer = go.AddComponent<SpriteRenderer>();
            var fitter = go.AddComponent<GroundFitter>();
            return (fitter, renderer);
        }

        [UnityTest]
        public IEnumerator SetFitMode_Tile_SetsTiledDrawMode()
        {
            var (fitter, renderer) = CreateGround();
            yield return null;

            renderer.drawMode = SpriteDrawMode.Simple;

            fitter.SetFitMode(BackgroundFit.Tile);

            Assert.AreEqual(SpriteDrawMode.Tiled, renderer.drawMode,
                "SetFitMode(Tile) must switch the SpriteRenderer drawMode to Tiled.");
        }

        [UnityTest]
        public IEnumerator SetFitMode_Stretch_SetsSimpleDrawMode()
        {
            var (fitter, renderer) = CreateGround();
            yield return null;

            renderer.drawMode = SpriteDrawMode.Tiled;

            fitter.SetFitMode(BackgroundFit.Stretch);

            Assert.AreEqual(SpriteDrawMode.Simple, renderer.drawMode,
                "SetFitMode(Stretch) must switch the SpriteRenderer drawMode to Simple.");
        }

        [UnityTest]
        public IEnumerator SetFitMode_Stretch_DoesNotMutateRendererSize()
        {
            var (fitter, renderer) = CreateGround();
            yield return null;

            renderer.drawMode = SpriteDrawMode.Tiled;
            renderer.size = new Vector2(4f, 4f);

            fitter.SetFitMode(BackgroundFit.Stretch);

            Assert.AreEqual(4f, renderer.size.x, SizeTolerance,
                "Stretch mode must not overwrite the stored renderer.size.x value.");
            Assert.AreEqual(4f, renderer.size.y, SizeTolerance,
                "Stretch mode must not overwrite the stored renderer.size.y value.");
        }

        [UnityTest]
        public IEnumerator SetFitMode_Tile_OverwritesRendererSizeFromCamera()
        {
            var (fitter, renderer) = CreateGround();
            yield return null;

            renderer.drawMode = SpriteDrawMode.Tiled;
            renderer.size = new Vector2(1f, 1f);

            fitter.SetFitMode(BackgroundFit.Tile);

            Assert.Greater(renderer.size.x, 1f,
                "Tile mode must resize the renderer width from camera-derived bounds, larger than the preset 1.");
        }
    }
}
