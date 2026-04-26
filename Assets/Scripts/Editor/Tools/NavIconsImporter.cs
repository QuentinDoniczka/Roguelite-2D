using UnityEditor;
using UnityEngine;

namespace RogueliteAutoBattler.Editor.Tools
{
    internal static class NavIconsImporter
    {
        internal static void ApplyNavIconSettings(string assetPath)
        {
            var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null)
            {
                Debug.LogWarning($"[NavIconsImporter] No TextureImporter found at {assetPath}");
                return;
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.mipmapEnabled = false;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.filterMode = FilterMode.Bilinear;
            importer.alphaIsTransparency = true;
            importer.maxTextureSize = 256;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;

            importer.SaveAndReimport();
        }

        [MenuItem("Tools/Roguelite/Import Nav Icons")]
        private static void ImportNavIconsMenuItem()
        {
            string[] paths =
            {
                "Assets/UI/Icons/Nav/village.png",
                "Assets/UI/Icons/Nav/skilltree.png",
                "Assets/UI/Icons/Nav/map.png",
                "Assets/UI/Icons/Nav/guilde.png",
                "Assets/UI/Icons/Nav/shop.png",
            };

            int count = 0;
            foreach (string path in paths)
            {
                if (AssetDatabase.LoadAssetAtPath<Texture2D>(path) == null)
                {
                    Debug.LogWarning($"[NavIconsImporter] Asset not found: {path}");
                    continue;
                }
                ApplyNavIconSettings(path);
                count++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[NavIconsImporter] Applied Sprite settings to {count} nav icons.");
        }
    }
}
