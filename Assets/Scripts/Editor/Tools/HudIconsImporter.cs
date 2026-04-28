using UnityEditor;
using UnityEngine;

namespace RogueliteAutoBattler.Editor.Tools
{
    internal static class HudIconsImporter
    {
        internal const string HudIconsFolder = "Assets/UI/Icons/HUD";
        internal const int HudIconMaxTextureSize = 256;

        private const string LogPrefix = "[HudIconsImporter]";
        private const string IconsFolder = "Assets/UI/Icons";
        private const string HudFolderName = "HUD";
        private const string SpritesRootFolder = "Assets/Sprites";

        private const FilterMode HudIconFilterMode = FilterMode.Bilinear;
        private const TextureImporterCompression HudIconCompression = TextureImporterCompression.CompressedHQ;
        private const TextureImporterNPOTScale HudIconNPOTScale = TextureImporterNPOTScale.None;
        private const TextureWrapMode HudIconWrapMode = TextureWrapMode.Clamp;

        internal static readonly string[] HudIconSlugs =
        {
            "gold",
            "diamant",
            "warrior",
            "arrow",
        };

        internal static string GetHudIconAssetPath(string slug) => $"{HudIconsFolder}/{slug}.png";

        internal static void EnsureHudIconsFolderExists()
        {
            if (!AssetDatabase.IsValidFolder(HudIconsFolder))
            {
                AssetDatabase.CreateFolder(IconsFolder, HudFolderName);
                Debug.Log($"{LogPrefix} Created {HudIconsFolder}");
            }
        }

        internal static void ApplyHudIconSettings(string assetPath)
        {
            var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null)
            {
                Debug.LogWarning($"{LogPrefix} No TextureImporter found at {assetPath}");
                return;
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 100f;
            importer.mipmapEnabled = false;
            importer.npotScale = HudIconNPOTScale;
            importer.wrapMode = HudIconWrapMode;
            importer.filterMode = HudIconFilterMode;
            importer.alphaIsTransparency = true;
            importer.sRGBTexture = true;
            importer.maxTextureSize = HudIconMaxTextureSize;
            importer.textureCompression = HudIconCompression;

            importer.SaveAndReimport();
        }

        [MenuItem("Tools/Roguelite/Move HUD Icons From Sprites Root")]
        private static void MoveFromSpritesRoot()
        {
            EnsureHudIconsFolderExists();

            foreach (string slug in HudIconSlugs)
            {
                string src = $"{SpritesRootFolder}/{slug}.png";
                string dst = GetHudIconAssetPath(slug);

                if (AssetDatabase.LoadAssetAtPath<Texture2D>(src) == null)
                {
                    Debug.Log($"{LogPrefix} {slug}: not found at {src}, skipping");
                    continue;
                }

                if (AssetDatabase.LoadAssetAtPath<Texture2D>(dst) != null)
                {
                    Debug.Log($"{LogPrefix} {slug}: already at {dst}, skipping move");
                    continue;
                }

                string error = AssetDatabase.MoveAsset(src, dst);
                if (!string.IsNullOrEmpty(error))
                {
                    Debug.LogError($"{LogPrefix} move {slug}: {error}");
                    continue;
                }

                ApplyHudIconSettings(dst);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"{LogPrefix} move-from-sprites-root complete");
        }

        [MenuItem("Tools/Roguelite/Import HUD Icons")]
        private static void ReimportAllHudIcons()
        {
            foreach (string slug in HudIconSlugs)
            {
                string path = GetHudIconAssetPath(slug);
                if (AssetDatabase.LoadAssetAtPath<Texture2D>(path) != null)
                {
                    ApplyHudIconSettings(path);
                }
                else
                {
                    Debug.LogWarning($"{LogPrefix} Asset not found: {path}");
                }
            }

            AssetDatabase.SaveAssets();
        }
    }
}
