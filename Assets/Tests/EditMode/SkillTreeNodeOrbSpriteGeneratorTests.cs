using NUnit.Framework;
using RogueliteAutoBattler.Editor.Builders;
using UnityEditor;
using UnityEngine;

namespace RogueliteAutoBattler.Tests.EditMode
{
    [TestFixture]
    public class SkillTreeNodeOrbSpriteGeneratorTests
    {
        private const int ExpectedTextureSize = 128;
        private const int CenterPixelCoord = 64;
        private const float FullAlphaThreshold = 0.99f;
        private const float ZeroAlphaThreshold = 0.01f;

        [Test]
        public void GenerateOrLoad_ProducesNonNullTexture_WithExpectedDimensions()
        {
            Texture2D texture = SkillTreeNodeOrbSpriteGenerator.GenerateOrLoad(force: true);

            Assert.IsNotNull(texture, "GenerateOrLoad should return a valid Texture2D.");
            Assert.AreEqual(ExpectedTextureSize, texture.width, "Texture width must equal TextureSize.");
            Assert.AreEqual(ExpectedTextureSize, texture.height, "Texture height must equal TextureSize.");
        }

        [Test]
        public void GenerateOrLoad_CenterPixel_HasFullAlpha()
        {
            Texture2D texture = SkillTreeNodeOrbSpriteGenerator.GenerateOrLoad(force: true);

            Color centerPixel = texture.GetPixel(CenterPixelCoord, CenterPixelCoord);

            Assert.GreaterOrEqual(centerPixel.a, FullAlphaThreshold, "Center pixel alpha must be near 1.");
        }

        [Test]
        public void GenerateOrLoad_BorderPixel_HasZeroAlpha()
        {
            Texture2D texture = SkillTreeNodeOrbSpriteGenerator.GenerateOrLoad(force: true);

            Color cornerPixel = texture.GetPixel(0, 0);
            Color oppositeCornerPixel = texture.GetPixel(ExpectedTextureSize - 1, ExpectedTextureSize - 1);

            Assert.LessOrEqual(cornerPixel.a, ZeroAlphaThreshold, "Top-left corner alpha must be near 0.");
            Assert.LessOrEqual(oppositeCornerPixel.a, ZeroAlphaThreshold, "Bottom-right corner alpha must be near 0.");
        }

        [Test]
        public void GenerateOrLoad_HaloFalloff_IsMonotonicallyDecreasing()
        {
            Texture2D texture = SkillTreeNodeOrbSpriteGenerator.GenerateOrLoad(force: true);

            float alphaCenter = texture.GetPixel(CenterPixelCoord, CenterPixelCoord).a;
            float alphaMidRadius = texture.GetPixel(CenterPixelCoord + 32, CenterPixelCoord + 32).a;
            float alphaNearEdge = texture.GetPixel(CenterPixelCoord + 60, CenterPixelCoord + 60).a;

            Assert.Greater(alphaCenter, alphaMidRadius, "Center alpha must exceed mid-radius alpha.");
            Assert.Greater(alphaMidRadius, alphaNearEdge, "Mid-radius alpha must exceed near-edge alpha.");
        }

        [Test]
        public void EnsureExists_IsIdempotent()
        {
            SkillTreeNodeOrbSpriteGenerator.GenerateOrLoad(force: true);
            string firstGuid = AssetDatabase.AssetPathToGUID(SkillTreeNodeOrbSpriteGenerator.AssetPath);

            SkillTreeNodeOrbSpriteGenerator.EnsureExists();
            string secondGuid = AssetDatabase.AssetPathToGUID(SkillTreeNodeOrbSpriteGenerator.AssetPath);

            Assert.IsFalse(string.IsNullOrEmpty(firstGuid), "Asset GUID must be valid after first generation.");
            Assert.AreEqual(firstGuid, secondGuid, "EnsureExists must not regenerate the asset when it already exists.");
        }
    }
}
