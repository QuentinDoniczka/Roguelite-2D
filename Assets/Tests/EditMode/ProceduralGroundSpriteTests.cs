using System.Reflection;
using NUnit.Framework;
using RogueliteAutoBattler.Combat.Visuals;
using UnityEngine;

namespace RogueliteAutoBattler.Tests.EditMode
{
    [TestFixture]
    public class ProceduralGroundSpriteTests
    {
        [TearDown]
        public void TearDown()
        {
            InvokeResetCacheForTests();
        }

        [Test]
        public void GetOrCreate_ReturnsNonNullSprite()
        {
            Sprite sprite = ProceduralGroundSprite.GetOrCreate();

            Assert.IsNotNull(sprite, "GetOrCreate should return a non-null sprite.");
            Assert.IsNotNull(sprite.texture, "Returned sprite must have a texture.");
        }

        [Test]
        public void GetOrCreate_ReturnsSameInstanceOnSecondCall()
        {
            Sprite first = ProceduralGroundSprite.GetOrCreate();
            Sprite second = ProceduralGroundSprite.GetOrCreate();

            Assert.AreSame(first, second,
                "GetOrCreate should cache and return the same Sprite instance on repeated calls.");
            Assert.AreSame(first.texture, second.texture,
                "Cached Sprite's backing Texture2D should also be the same instance.");
        }

        [Test]
        public void GetOrCreate_TextureIsCorrectSize()
        {
            Sprite sprite = ProceduralGroundSprite.GetOrCreate();

            Assert.AreEqual(ProceduralGroundSprite.TextureSize, sprite.texture.width,
                "Procedural ground texture width must equal TextureSize.");
            Assert.AreEqual(ProceduralGroundSprite.TextureSize, sprite.texture.height,
                "Procedural ground texture height must equal TextureSize.");
        }

        [Test]
        public void GetOrCreate_CheckerboardAlternates()
        {
            Sprite sprite = ProceduralGroundSprite.GetOrCreate();
            Color32[] pixels = sprite.texture.GetPixels32();

            Color32 colorAt00 = SampleAt(pixels, 0, 0);
            Color32 colorAtCellSize0 = SampleAt(pixels, ProceduralGroundSprite.CellSize, 0);
            Color32 colorAt0CellSize = SampleAt(pixels, 0, ProceduralGroundSprite.CellSize);
            Color32 colorAtCellCell = SampleAt(pixels, ProceduralGroundSprite.CellSize, ProceduralGroundSprite.CellSize);

            AssertColorEquals(ProceduralGroundSprite.ColorA, colorAt00,
                "Pixel at (0,0) should be ColorA (bottom-left cell).");
            AssertColorEquals(ProceduralGroundSprite.ColorB, colorAtCellSize0,
                "Pixel at (CellSize,0) should be ColorB (horizontally adjacent cell alternates).");
            AssertColorEquals(ProceduralGroundSprite.ColorB, colorAt0CellSize,
                "Pixel at (0,CellSize) should be ColorB (vertically adjacent cell alternates).");
            AssertColorEquals(ProceduralGroundSprite.ColorA, colorAtCellCell,
                "Pixel at (CellSize,CellSize) should be ColorA (diagonal cell matches origin).");
        }

        private static Color32 SampleAt(Color32[] pixels, int x, int y)
        {
            int index = y * ProceduralGroundSprite.TextureSize + x;
            return pixels[index];
        }

        private static void AssertColorEquals(Color32 expected, Color32 actual, string message)
        {
            Assert.AreEqual(expected.r, actual.r, message + " (r)");
            Assert.AreEqual(expected.g, actual.g, message + " (g)");
            Assert.AreEqual(expected.b, actual.b, message + " (b)");
            Assert.AreEqual(expected.a, actual.a, message + " (a)");
        }

        private static void InvokeResetCacheForTests()
        {
            MethodInfo resetMethod = typeof(ProceduralGroundSprite).GetMethod(
                "ResetCacheForTests",
                BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);

            Assert.IsNotNull(resetMethod,
                "ProceduralGroundSprite.ResetCacheForTests must exist for test isolation.");

            resetMethod.Invoke(null, null);
        }
    }
}
