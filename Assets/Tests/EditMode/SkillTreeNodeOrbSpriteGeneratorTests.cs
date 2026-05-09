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
        public void Generator_GenerateAllLayers_EmitsAllFiveFiles_NonEmpty()
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
        public void Generator_HaloOuter_HasFullCenter_TransparentEdge()
        {
            Texture2D tex = SkillTreeNodeOrbSpriteGenerator.GenerateOrLoad(OrbLayerKind.HaloOuter, force: true);

            Assert.Greater(tex.GetPixel(64, 64).a, 0.95f, "HaloOuter center must be near-opaque.");
            Assert.Less(tex.GetPixel(0, 0).a, 0.05f, "HaloOuter corner must be near-transparent.");
        }

        [Test]
        public void Generator_HaloInner_HasFullCenter_TransparentBeyondCutoff()
        {
            Texture2D tex = SkillTreeNodeOrbSpriteGenerator.GenerateOrLoad(OrbLayerKind.HaloInner, force: true);

            Assert.Greater(tex.GetPixel(64, 64).a, 0.95f, "HaloInner center must be near-opaque.");
            int outerPixelY = 64 + 40;
            Assert.Less(tex.GetPixel(64, outerPixelY).a, 0.05f, "HaloInner alpha at r~0.625 must be 0 (beyond cutoff 0.55).");
        }

        [Test]
        public void Generator_Rays_HasTransparentCenter()
        {
            Texture2D tex = SkillTreeNodeOrbSpriteGenerator.GenerateOrLoad(OrbLayerKind.Rays, force: true);

            Assert.Less(tex.GetPixel(64, 64).a, 0.05f, "Rays center must be transparent (inner radius hole).");
        }

        [Test]
        public void Generator_Rays_HasTransparentOutside()
        {
            Texture2D tex = SkillTreeNodeOrbSpriteGenerator.GenerateOrLoad(OrbLayerKind.Rays, force: true);

            Assert.Less(tex.GetPixel(0, 0).a, 0.05f, "Rays corner (0,0) must be transparent.");
        }

        [Test]
        public void Generator_Rays_HasTwelveLobes()
        {
            Texture2D tex = SkillTreeNodeOrbSpriteGenerator.GenerateOrLoad(OrbLayerKind.Rays, force: true);

            float r = 0.6f;
            int numRays = 12;
            float angularStep = 360f / numRays;

            for (int i = 0; i < numRays; i++)
            {
                float thetaDeg = i * angularStep;
                float thetaRad = thetaDeg * Mathf.Deg2Rad;
                int px = 64 + (int)(r * 0.5f * 128f * Mathf.Cos(thetaRad));
                int py = 64 + (int)(r * 0.5f * 128f * Mathf.Sin(thetaRad));
                px = Mathf.Clamp(px, 0, 127);
                py = Mathf.Clamp(py, 0, 127);
                Assert.Greater(tex.GetPixel(px, py).a, 0.4f,
                    $"Streak at theta={thetaDeg}° (px={px},{py}) must be bright (streak peak).");
            }

            for (int i = 0; i < numRays; i++)
            {
                float thetaDeg = i * angularStep + 15f;
                float thetaRad = thetaDeg * Mathf.Deg2Rad;
                int px = 64 + (int)(r * 0.5f * 128f * Mathf.Cos(thetaRad));
                int py = 64 + (int)(r * 0.5f * 128f * Mathf.Sin(thetaRad));
                px = Mathf.Clamp(px, 0, 127);
                py = Mathf.Clamp(py, 0, 127);
                Assert.Less(tex.GetPixel(px, py).a, 0.15f,
                    $"Gap at theta={thetaDeg}° (px={px},{py}) must be dark (between streaks).");
            }
        }
    }
}
