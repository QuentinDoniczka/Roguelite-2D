using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace RogueliteAutoBattler.Editor.Builders
{
    internal enum OrbLayerKind { Core, Halo, HaloOuter, HaloInner, Rays }

    internal static class SkillTreeNodeOrbSpriteGenerator
    {
        internal const string AssetPath = "Assets/Resources/UI/SkillTreeNodeOrb.png";
        internal const string ResourcesLoadPath = "UI/SkillTreeNodeOrb";

        private const int TextureSize = 128;

        private const float CoreRadiusRatio = 0.18f;
        private const float FalloffExponent = 2.2f;

        private const float HaloCoreRadiusRatio = 0.05f;
        private const float HaloFalloffExponent = 2.6f;

        private const float HaloOuterFalloffExponent = 3.5f;

        private const float HaloInnerCoreRadiusRatio = 0.30f;
        private const float HaloInnerFalloffExponent = 2.0f;
        private const float HaloInnerOuterCutoff = 0.55f;

        private const int RaysCount = 12;
        private const float RaysAngularExtent = 0.10f;
        private const float RaysParallelExponent = 1.5f;
        private const float RaysInnerRadius = 0.40f;
        private const float RaysOuterRadius = 0.95f;
        private const float RaysFeather = 0.05f;

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
            Debug.Log("[SkillTreeOrb] Generated 5 layer PNGs.");
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
                OrbLayerKind.HaloOuter => ComputeAlphaHaloOuter(r),
                OrbLayerKind.HaloInner => ComputeAlphaHaloInner(r),
                OrbLayerKind.Rays => ComputeAlphaRays(nx, ny),
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

        private static float ComputeAlphaHaloOuter(float r)
        {
            return Mathf.Clamp01(Mathf.Pow(1f - r, HaloOuterFalloffExponent));
        }

        private static float ComputeAlphaHaloInner(float r)
        {
            if (r <= HaloInnerCoreRadiusRatio) return 1f;
            if (r >= HaloInnerOuterCutoff) return 0f;
            var t = (r - HaloInnerCoreRadiusRatio) / (HaloInnerOuterCutoff - HaloInnerCoreRadiusRatio);
            return Mathf.Clamp01(Mathf.Pow(1f - t, HaloInnerFalloffExponent));
        }

        private static float ComputeAlphaRays(float nx, float ny)
        {
            float dx = nx - 0.5f;
            float dy = ny - 0.5f;
            float r = Mathf.Sqrt(dx * dx + dy * dy) * 2f;
            if (r <= RaysInnerRadius - RaysFeather || r >= RaysOuterRadius + RaysFeather) return 0f;
            float angle = Mathf.Atan2(dy, dx);
            float angularStep = (Mathf.PI * 2f) / RaysCount;
            float modAngle = angle - Mathf.Floor(angle / angularStep) * angularStep;
            float distToNearest = Mathf.Min(modAngle, angularStep - modAngle);
            float streakStrength = Mathf.Pow(Mathf.Max(0f, 1f - distToNearest / RaysAngularExtent), RaysParallelExponent);
            float innerMask = Smoothstep(RaysInnerRadius - RaysFeather, RaysInnerRadius, r);
            float outerMask = 1f - Smoothstep(RaysOuterRadius - RaysFeather, RaysOuterRadius + RaysFeather, r);
            return Mathf.Clamp01(streakStrength * innerMask * outerMask);
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
