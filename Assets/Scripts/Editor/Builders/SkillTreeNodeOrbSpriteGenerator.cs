using System.IO;
using UnityEditor;
using UnityEngine;

namespace RogueliteAutoBattler.Editor.Builders
{
    internal static class SkillTreeNodeOrbSpriteGenerator
    {
        internal const string AssetPath = "Assets/Resources/UI/SkillTreeNodeOrb.png";
        internal const string ResourcesLoadPath = "UI/SkillTreeNodeOrb";

        private const int TextureSize = 128;
        private const float CoreRadiusRatio = 0.18f;
        private const float FalloffExponent = 2.2f;

        [MenuItem("Tools/Skill Tree/Generate Node Orb Sprite")]
        private static void GenerateMenu()
        {
            Texture2D texture = GenerateOrLoad(force: true);
            if (texture != null)
            {
                Debug.Log($"[SkillTreeNodeOrbSpriteGenerator] Generated orb sprite at {AssetPath}");
                Selection.activeObject = texture;
                EditorGUIUtility.PingObject(texture);
            }
        }

        internal static Texture2D EnsureExists()
        {
            return GenerateOrLoad(force: false);
        }

        internal static Texture2D GenerateOrLoad(bool force = false)
        {
            if (!force)
            {
                Texture2D existing = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetPath);
                if (existing != null)
                    return existing;
            }

            EnsureResourcesUiFolderExists();

            byte[] pngBytes = BuildOrbPngBytes();
            File.WriteAllBytes(AssetPath, pngBytes);

            AssetDatabase.ImportAsset(AssetPath, ImportAssetOptions.ForceUpdate);
            ApplyTextureImporterSettings(AssetPath);

            return AssetDatabase.LoadAssetAtPath<Texture2D>(AssetPath);
        }

        private static byte[] BuildOrbPngBytes()
        {
            Texture2D texture = new Texture2D(TextureSize, TextureSize, TextureFormat.RGBA32, false);

            float center = TextureSize * 0.5f;
            float maxRadius = TextureSize * 0.5f;

            Color32[] pixels = new Color32[TextureSize * TextureSize];
            for (int y = 0; y < TextureSize; y++)
            {
                for (int x = 0; x < TextureSize; x++)
                {
                    float dx = (x + 0.5f) - center;
                    float dy = (y + 0.5f) - center;
                    float normalizedDistance = Mathf.Sqrt(dx * dx + dy * dy) / maxRadius;
                    float alpha = ComputeAlpha(normalizedDistance);
                    byte alphaByte = (byte)Mathf.Clamp(Mathf.RoundToInt(alpha * 255f), 0, 255);
                    pixels[y * TextureSize + x] = new Color32(255, 255, 255, alphaByte);
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply();

            byte[] pngBytes = texture.EncodeToPNG();
            Object.DestroyImmediate(texture);
            return pngBytes;
        }

        private static float ComputeAlpha(float normalizedDistance)
        {
            if (normalizedDistance <= CoreRadiusRatio)
                return 1f;
            if (normalizedDistance >= 1f)
                return 0f;

            float haloProgress = (normalizedDistance - CoreRadiusRatio) / (1f - CoreRadiusRatio);
            return Mathf.Pow(1f - haloProgress, FalloffExponent);
        }

        private static void EnsureResourcesUiFolderExists()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                AssetDatabase.CreateFolder("Assets", "Resources");
            if (!AssetDatabase.IsValidFolder("Assets/Resources/UI"))
                AssetDatabase.CreateFolder("Assets/Resources", "UI");

            string directory = Path.GetDirectoryName(AssetPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);
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
