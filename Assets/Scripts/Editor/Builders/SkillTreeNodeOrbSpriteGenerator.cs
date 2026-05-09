using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace RogueliteAutoBattler.Editor.Builders
{
    internal enum OrbLayerKind { Core, Halo, Frame, Rim, InnerGlow, Sparkle }

    internal static class SkillTreeNodeOrbSpriteGenerator
    {
        internal const string AssetPath = "Assets/Resources/UI/SkillTreeNodeOrb.png";
        internal const string ResourcesLoadPath = "UI/SkillTreeNodeOrb";

        private const int TextureSize = 128;

        private const float CoreRadiusRatio = 0.18f;
        private const float FalloffExponent = 2.2f;

        private const float HaloCoreRadiusRatio = 0.05f;
        private const float HaloFalloffExponent = 2.6f;

        internal static string AssetPathFor(OrbLayerKind kind) =>
            kind == OrbLayerKind.Core
                ? "Assets/Resources/UI/SkillTreeNodeOrb.png"
                : $"Assets/Resources/UI/SkillTreeNodeOrb_{kind}.png";

        internal static string ResourcesLoadPathFor(OrbLayerKind kind) =>
            kind == OrbLayerKind.Core
                ? "UI/SkillTreeNodeOrb"
                : $"UI/SkillTreeNodeOrb_{kind}";

        [MenuItem("Tools/Skill Tree/Generate Node Orb Sprites")]
        private static void GenerateMenu()
        {
            GenerateAllLayers();
        }

        internal static void GenerateAllLayers()
        {
            foreach (OrbLayerKind kind in Enum.GetValues(typeof(OrbLayerKind)))
                GenerateOrLoad(kind, force: true);

            AssetDatabase.Refresh();
            Debug.Log("[SkillTreeOrb] Generated 6 layer PNGs.");
        }

        internal static Texture2D EnsureExists(OrbLayerKind kind = OrbLayerKind.Core)
        {
            return GenerateOrLoad(kind, force: false);
        }

        internal static Texture2D GenerateOrLoad(bool force = false)
        {
            return GenerateOrLoad(OrbLayerKind.Core, force);
        }

        internal static Texture2D GenerateOrLoad(OrbLayerKind kind, bool force)
        {
            string path = AssetPathFor(kind);

            if (!force)
            {
                Texture2D existing = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if (existing != null)
                    return existing;
            }

            EnsureResourcesUiFolderExists();

            byte[] pngBytes = BuildOrbPngBytes(kind);
            File.WriteAllBytes(path, pngBytes);

            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            ApplyTextureImporterSettings(path);

            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        private static byte[] BuildOrbPngBytes(OrbLayerKind kind)
        {
            Texture2D texture = new Texture2D(TextureSize, TextureSize, TextureFormat.RGBA32, false);

            float center = TextureSize * 0.5f;
            float maxRadius = TextureSize * 0.5f;

            Color32[] pixels = new Color32[TextureSize * TextureSize];
            for (int y = 0; y < TextureSize; y++)
            {
                for (int x = 0; x < TextureSize; x++)
                {
                    float px = x + 0.5f;
                    float py = y + 0.5f;
                    float nx = px / TextureSize;
                    float ny = py / TextureSize;
                    float dx = px - center;
                    float dy = py - center;
                    float r = Mathf.Sqrt(dx * dx + dy * dy) / maxRadius;
                    float alpha = ComputeAlphaForKind(kind, nx, ny, r);
                    byte alphaByte = (byte)Mathf.Clamp(Mathf.RoundToInt(alpha * 255f), 0, 255);
                    pixels[y * TextureSize + x] = new Color32(255, 255, 255, alphaByte);
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply();

            byte[] pngBytes = texture.EncodeToPNG();
            UnityEngine.Object.DestroyImmediate(texture);
            return pngBytes;
        }

        private static float ComputeAlphaForKind(OrbLayerKind kind, float nx, float ny, float r)
        {
            return kind switch
            {
                OrbLayerKind.Core => ComputeAlphaCore(r),
                OrbLayerKind.Halo => ComputeAlphaHalo(r),
                OrbLayerKind.Frame => ComputeAlphaFrame(r),
                OrbLayerKind.Rim => ComputeAlphaRim(r),
                OrbLayerKind.InnerGlow => ComputeAlphaInnerGlow(nx, ny),
                OrbLayerKind.Sparkle => ComputeAlphaSparkle(nx, ny),
                _ => ComputeAlphaCore(r)
            };
        }

        private static float ComputeAlphaCore(float normalizedDistance)
        {
            if (normalizedDistance <= CoreRadiusRatio)
                return 1f;
            if (normalizedDistance >= 1f)
                return 0f;

            float haloProgress = (normalizedDistance - CoreRadiusRatio) / (1f - CoreRadiusRatio);
            return Mathf.Pow(1f - haloProgress, FalloffExponent);
        }

        private static float ComputeAlphaHalo(float r)
        {
            if (r <= HaloCoreRadiusRatio)
                return 1f;

            return Mathf.Clamp01(Mathf.Pow(1f - (r - HaloCoreRadiusRatio) / (1f - HaloCoreRadiusRatio), HaloFalloffExponent));
        }

        private static float ComputeAlphaFrame(float r)
        {
            float band1 = Smoothstep(0.84f, 0.88f, r) * (1f - Smoothstep(0.95f, 0.99f, r));
            float band2 = 0.55f * (Smoothstep(0.78f, 0.81f, r) * (1f - Smoothstep(0.81f, 0.84f, r)));
            float band3 = 0.95f * (Smoothstep(0.72f, 0.75f, r) * (1f - Smoothstep(0.75f, 0.78f, r)));
            return Mathf.Clamp01(band1 + band2 + band3);
        }

        private static float ComputeAlphaRim(float r)
        {
            return Smoothstep(0.85f, 0.885f, r) * (1f - Smoothstep(0.885f, 0.92f, r));
        }

        private static float ComputeAlphaInnerGlow(float nx, float ny)
        {
            float dx = nx - 0.35f;
            float dy = ny - 0.65f;
            float d = Mathf.Sqrt(dx * dx + dy * dy);
            return Mathf.Clamp01(Mathf.Pow(1f - Mathf.Clamp01(d / 0.30f), 3.0f));
        }

        private static float ComputeAlphaSparkle(float nx, float ny)
        {
            const float PerpFalloff = 10f;      // controls lobe thinness (perpendicular axis)
            const float ParallelFalloff = 1.5f; // controls lobe length (parallel axis)
            const float PerpExponent = 1.5f;
            const float ParallelExponent = 2.0f;

            float dx = nx - 0.5f;
            float dy = ny - 0.5f;
            float lobeH = Mathf.Pow(Mathf.Max(0f, 1f - Mathf.Abs(dy) * PerpFalloff), PerpExponent)
                        * Mathf.Pow(Mathf.Max(0f, 1f - Mathf.Abs(dx) * ParallelFalloff), ParallelExponent);
            float lobeV = Mathf.Pow(Mathf.Max(0f, 1f - Mathf.Abs(dx) * PerpFalloff), PerpExponent)
                        * Mathf.Pow(Mathf.Max(0f, 1f - Mathf.Abs(dy) * ParallelFalloff), ParallelExponent);
            return Mathf.Clamp01(Mathf.Max(lobeH, lobeV));
        }

        private static float Smoothstep(float edge0, float edge1, float x)
        {
            float t = Mathf.Clamp01((x - edge0) / (edge1 - edge0));
            return t * t * (3f - 2f * t);
        }

        private static void EnsureResourcesUiFolderExists()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                AssetDatabase.CreateFolder("Assets", "Resources");
            if (!AssetDatabase.IsValidFolder("Assets/Resources/UI"))
                AssetDatabase.CreateFolder("Assets/Resources", "UI");

            EditorUIFactory.EnsureDirectoryExists(AssetPath);
        }

        private static void ApplyTextureImporterSettings(string assetPath)
        {
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null)
                return;

            importer.textureType = TextureImporterType.Default;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.isReadable = true;
            importer.filterMode = FilterMode.Bilinear;
            importer.textureCompression = TextureImporterCompression.Uncompressed;

            importer.SaveAndReimport();
        }
    }
}
