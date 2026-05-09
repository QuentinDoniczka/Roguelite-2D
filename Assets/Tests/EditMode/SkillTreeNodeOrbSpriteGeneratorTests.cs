using System.IO;
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

        [Test]
        public void Generator_GenerateAllLayers_EmitsAllSixFiles_NonEmpty()
        {
            SkillTreeNodeOrbSpriteGenerator.GenerateAllLayers();

            foreach (OrbLayerKind kind in System.Enum.GetValues(typeof(OrbLayerKind)))
            {
                string path = SkillTreeNodeOrbSpriteGenerator.AssetPathFor(kind);
                Assert.IsTrue(File.Exists(path), $"PNG file must exist on disk for kind {kind}.");
                Assert.Greater(new FileInfo(path).Length, 0L, $"PNG file must be non-empty for kind {kind}.");
                Assert.IsNotNull(AssetDatabase.LoadAssetAtPath<Texture2D>(path),
                    $"AssetDatabase must be able to load Texture2D for kind {kind}.");
            }
        }

        [Test]
        public void Generator_Frame_HasTransparentCenter()
        {
            Texture2D tex = SkillTreeNodeOrbSpriteGenerator.GenerateOrLoad(OrbLayerKind.Frame, force: true);

            Assert.Less(tex.GetPixel(64, 64).a, 0.05f, "Frame center must be fully transparent.");
        }

        [Test]
        public void Generator_Frame_HasOpaqueOuterBand()
        {
            Texture2D tex = SkillTreeNodeOrbSpriteGenerator.GenerateOrLoad(OrbLayerKind.Frame, force: true);

            Assert.Greater(tex.GetPixel(64 + 56, 64).a, 0.5f, "Frame outer band at r≈0.875 must be opaque.");
        }

        [Test]
        public void Generator_Rim_HasTransparentCenterAndOutside()
        {
            Texture2D tex = SkillTreeNodeOrbSpriteGenerator.GenerateOrLoad(OrbLayerKind.Rim, force: true);

            Assert.Less(tex.GetPixel(64, 64).a, 0.05f, "Rim center must be transparent.");
            Assert.Less(tex.GetPixel(0, 0).a, 0.05f, "Rim corner (0,0) must be transparent.");
            Assert.Greater(tex.GetPixel(64 + 56, 64).a, 0.5f, "Rim band at r≈0.875 must be opaque.");
        }

        [Test]
        public void Generator_InnerGlow_PeaksOffCenterTopLeft()
        {
            Texture2D tex = SkillTreeNodeOrbSpriteGenerator.GenerateOrLoad(OrbLayerKind.InnerGlow, force: true);

            float alphaTopLeft = tex.GetPixel(45, 83).a;
            float alphaCenter = tex.GetPixel(64, 64).a;

            Assert.Greater(alphaTopLeft, alphaCenter, "InnerGlow top-left pixel must be brighter than center.");
        }

        [Test]
        public void Generator_Sparkle_HasFourLobes_NotDiagonals()
        {
            Texture2D tex = SkillTreeNodeOrbSpriteGenerator.GenerateOrLoad(OrbLayerKind.Sparkle, force: true);

            Assert.Greater(tex.GetPixel(64, 64 + 26).a, 0.4f, "Sparkle top cardinal lobe must be bright.");
            Assert.Greater(tex.GetPixel(64, 64 - 26).a, 0.4f, "Sparkle bottom cardinal lobe must be bright.");
            Assert.Greater(tex.GetPixel(64 + 26, 64).a, 0.4f, "Sparkle right cardinal lobe must be bright.");
            Assert.Greater(tex.GetPixel(64 - 26, 64).a, 0.4f, "Sparkle left cardinal lobe must be bright.");

            Assert.Less(tex.GetPixel(64 + 18, 64 + 18).a, 0.15f, "Sparkle top-right diagonal must be dark.");
            Assert.Less(tex.GetPixel(64 - 18, 64 - 18).a, 0.15f, "Sparkle bottom-left diagonal must be dark.");
            Assert.Less(tex.GetPixel(64 + 18, 64 - 18).a, 0.15f, "Sparkle bottom-right diagonal must be dark.");
            Assert.Less(tex.GetPixel(64 - 18, 64 + 18).a, 0.15f, "Sparkle top-left diagonal must be dark.");

            Assert.Greater(tex.GetPixel(64, 64).a, 0.9f, "Sparkle center must be near-opaque.");
        }
    }
}
