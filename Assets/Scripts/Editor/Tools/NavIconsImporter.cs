using UnityEditor;
using UnityEngine;

namespace RogueliteAutoBattler.Editor.Tools
{
    internal static class NavIconsImporter
    {
        internal const string NavIconsFolder = "Assets/UI/Icons/Nav";
        internal const int NavIconMaxTextureSize = 256;
        internal const string NavActiveStateIconSlug = "remove";

        private const string LogPrefix = "[NavIconsImporter]";
        private const string IconsParentFolder = "Assets/UI";
        private const string IconsFolderName = "Icons";
        private const string IconsFolder = "Assets/UI/Icons";
        private const string NavFolderName = "Nav";

        private const FilterMode NavIconFilterMode = FilterMode.Bilinear;
        private const TextureImporterCompression NavIconCompression = TextureImporterCompression.CompressedHQ;
        private const TextureImporterNPOTScale NavIconNPOTScale = TextureImporterNPOTScale.None;
        private const TextureWrapMode NavIconWrapMode = TextureWrapMode.Clamp;

        internal static readonly string[] NavIconSlugs =
        {
            "village",
            "skilltree",
            "map",
            "guilde",
            "shop",
        };

        internal static string GetNavIconAssetPath(string slug) => $"{NavIconsFolder}/{slug}.png";

        internal static void EnsureNavIconsFolderExists()
        {
            if (!AssetDatabase.IsValidFolder(IconsFolder))
            {
                AssetDatabase.CreateFolder(IconsParentFolder, IconsFolderName);
                Debug.Log($"{LogPrefix} Created {IconsFolder}");
            }

            if (!AssetDatabase.IsValidFolder(NavIconsFolder))
            {
                AssetDatabase.CreateFolder(IconsFolder, NavFolderName);
                Debug.Log($"{LogPrefix} Created {NavIconsFolder}");
            }
        }

        internal static void ApplyNavIconSettings(string assetPath)
        {
            var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null)
            {
                Debug.LogWarning($"{LogPrefix} No TextureImporter found at {assetPath}");
                return;
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.npotScale = NavIconNPOTScale;
            importer.mipmapEnabled = false;
            importer.wrapMode = NavIconWrapMode;
            importer.filterMode = NavIconFilterMode;
            importer.alphaIsTransparency = true;
            importer.maxTextureSize = NavIconMaxTextureSize;
            importer.textureCompression = NavIconCompression;

            importer.SaveAndReimport();
        }

        [MenuItem("Tools/Roguelite/Import Nav Icons")]
        private static void ReimportAllNavIcons()
        {
            int count = 0;
            try
            {
                AssetDatabase.StartAssetEditing();
                foreach (string slug in NavIconSlugs)
                {
                    if (TryApplyNavIconSettingsForSlug(slug))
                        count++;
                }

                if (TryApplyNavIconSettingsForSlug(NavActiveStateIconSlug))
                    count++;
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"{LogPrefix} Applied Sprite settings to {count} nav icons.");
        }

        private static bool TryApplyNavIconSettingsForSlug(string slug)
        {
            string path = GetNavIconAssetPath(slug);
            if (AssetDatabase.LoadAssetAtPath<Texture2D>(path) == null)
            {
                Debug.LogWarning($"{LogPrefix} Asset not found: {path}");
                return false;
            }
            ApplyNavIconSettings(path);
            return true;
        }
    }
}
