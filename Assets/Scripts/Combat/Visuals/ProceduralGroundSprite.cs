using UnityEngine;

namespace RogueliteAutoBattler.Combat.Visuals
{
    public static class ProceduralGroundSprite
    {
        public const int TextureSize = 64;
        public const int CellSize = 8;
        public const int PixelsPerUnit = 64;

        public static readonly Color32 ColorA = new Color32(45, 90, 39, 255);
        public static readonly Color32 ColorB = new Color32(61, 122, 55, 255);

        private static Sprite _cached;
        private static Texture2D _cachedTexture;

        public static Sprite GetOrCreate()
        {
            if (_cached != null)
                return _cached;

            var texture = new Texture2D(TextureSize, TextureSize, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Repeat
            };

            var pixels = new Color32[TextureSize * TextureSize];
            for (int y = 0; y < TextureSize; y++)
            {
                for (int x = 0; x < TextureSize; x++)
                {
                    int cellX = x / CellSize;
                    int cellY = y / CellSize;
                    pixels[y * TextureSize + x] = ((cellX + cellY) % 2 == 0) ? ColorA : ColorB;
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply();

            _cachedTexture = texture;
            _cached = Sprite.Create(
                texture,
                new Rect(0f, 0f, TextureSize, TextureSize),
                new Vector2(0.5f, 0.5f),
                PixelsPerUnit,
                0,
                SpriteMeshType.FullRect);

            return _cached;
        }

        internal static void ResetCacheForTests()
        {
            if (_cached != null)
            {
                if (Application.isPlaying)
                    Object.Destroy(_cached);
                else
                    Object.DestroyImmediate(_cached);
                _cached = null;
            }

            if (_cachedTexture != null)
            {
                if (Application.isPlaying)
                    Object.Destroy(_cachedTexture);
                else
                    Object.DestroyImmediate(_cachedTexture);
                _cachedTexture = null;
            }
        }
    }
}
