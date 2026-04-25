using UnityEditor;
using UnityEngine;

namespace RogueliteAutoBattler.Editor.Builders
{
    internal static class RoundedRectSpriteGenerator
    {
        private const string DefaultAssetPath = "Assets/Sprites/UI/rounded_rect_white.png";
        private const int TextureSize = 64;
        private const int CornerRadius = 12;
        private const float AntiAliasingHalfWidth = 0.5f;
        private static readonly Vector4 NineSliceBorder = new Vector4(12, 12, 12, 12);

        internal static Sprite GenerateOrLoad(string assetPath = DefaultAssetPath)
        {
            Sprite existing = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
            if (existing != null)
                return existing;

            Texture2D texture = new Texture2D(TextureSize, TextureSize, TextureFormat.RGBA32, false);

            Color32[] pixels = new Color32[TextureSize * TextureSize];
            for (int y = 0; y < TextureSize; y++)
            {
                for (int x = 0; x < TextureSize; x++)
                {
                    byte alpha = ComputeAlpha(x, y);
                    pixels[y * TextureSize + x] = new Color32(255, 255, 255, alpha);
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply();

            byte[] pngBytes = texture.EncodeToPNG();
            Object.DestroyImmediate(texture);

            EditorUIFactory.EnsureDirectoryExists(assetPath);
            System.IO.File.WriteAllBytes(assetPath, pngBytes);

            AssetDatabase.ImportAsset(assetPath);

            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spriteBorder = NineSliceBorder;
                importer.filterMode = FilterMode.Bilinear;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.alphaIsTransparency = true;
            }

            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);

            return AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        }

        private static byte ComputeAlpha(int x, int y)
        {
            int cornerX = CornerRadius - 1;
            int cornerY = CornerRadius - 1;

            int nearX = x < CornerRadius ? cornerX - x : x >= TextureSize - CornerRadius ? x - (TextureSize - CornerRadius) : -1;
            int nearY = y < CornerRadius ? cornerY - y : y >= TextureSize - CornerRadius ? y - (TextureSize - CornerRadius) : -1;

            if (nearX < 0 || nearY < 0)
                return 255;

            float distance = Mathf.Sqrt(nearX * nearX + nearY * nearY);
            float alpha = Mathf.Clamp01(CornerRadius - distance + AntiAliasingHalfWidth);
            return (byte)(alpha * 255f);
        }
    }
}
