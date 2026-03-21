using RogueliteAutoBattler.Combat;
using UnityEditor;
using UnityEngine;

namespace RogueliteAutoBattler.Editor
{
    /// <summary>
    /// Builds the CombatWorld 2D scene hierarchy and configures the main camera.
    /// </summary>
    internal static class CombatWorldBuilder
    {
        private const float CameraOrthoSize = 5.4f;
        private const float CameraZPosition = -10f;

        // Ground tile — 200 units wide so the camera never sees an edge during a scroll session.
        // Height and centerY are calculated at runtime by GroundFitter.
        private const float GroundWidth = 200f;

        private const string GridSpritePath = "Assets/Sprites/Environment/grid_ground.png";
        private const int GridTextureSize = 64;   // pixels per tile
        private const int GridCellSize = 8;        // pixels per checkerboard cell
        private const int GridPixelsPerUnit = 64;  // 1 tile = 1 world unit

        // Checkerboard colours — dark green / light green so movement is obvious.
        private static readonly Color32 GridColorA = new Color32(45, 90, 39, 255);   // #2d5a27
        private static readonly Color32 GridColorB = new Color32(61, 122, 55, 255);  // #3d7a37

        // ------------------------------------------------------------------
        // Camera
        // ------------------------------------------------------------------

        /// <summary>
        /// Configures the main camera as orthographic with the correct settings for 2D mobile.
        /// Creates one if none exists.
        /// </summary>
        internal static Camera ConfigureMainCamera()
        {
            Camera cam = Camera.main;
            if (cam == null)
                cam = Object.FindFirstObjectByType<Camera>(FindObjectsInactive.Include);

            if (cam == null)
            {
                var camGo = new GameObject("Main Camera");
                cam = camGo.AddComponent<Camera>();
                camGo.tag = "MainCamera";
                Undo.RegisterCreatedObjectUndo(camGo, "Main Camera");
            }
            else
            {
                Undo.RecordObject(cam, "Configure Camera");
                Undo.RecordObject(cam.transform, "Configure Camera");
            }

            cam.orthographic = true;
            cam.orthographicSize = CameraOrthoSize;
            cam.transform.position = new Vector3(0, 0, CameraZPosition);
            cam.backgroundColor = Color.black;
            cam.clearFlags = CameraClearFlags.SolidColor;

            return cam;
        }

        // ------------------------------------------------------------------
        // CombatWorld hierarchy
        // ------------------------------------------------------------------

        /// <summary>
        /// Creates a CombatWorld container with a tiled ground and containers for characters/effects.
        /// Also attaches WorldConveyorDebug for play-mode scroll testing.
        /// </summary>
        internal static GameObject CreateCombatWorld()
        {
            var root = new GameObject("CombatWorld");
            root.transform.position = Vector3.zero;

            // Ground — tiled checkerboard, sized to match GameArea (top 60% of screen).
            // Positioned behind everything on Background sorting layer.
            // GroundFitter recalculates size and position at runtime from the camera.
            var groundGo = new GameObject("Ground");
            groundGo.transform.SetParent(root.transform, false);
            SpriteRenderer groundRenderer = groundGo.AddComponent<SpriteRenderer>();
            groundRenderer.sprite = CreateOrLoadGridSprite();
            groundRenderer.drawMode = SpriteDrawMode.Tiled;
            groundRenderer.size = new Vector2(GroundWidth, 6.48f); // initial value — overridden by GroundFitter
            groundRenderer.sortingLayerName = "Background";
            groundRenderer.sortingOrder = -10;
            groundRenderer.color = Color.white;
            // URP 2D uses Lit sprites by default — without a Light2D in the scene,
            // sprites appear black. The ground is a flat background that doesn't
            // need lighting, so we use the Unlit material.
            groundRenderer.material = new Material(Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default"));
            groundGo.AddComponent<GroundFitter>();

            // Characters container — place all adventurer and enemy prefabs here.
            // Every SpriteRenderer in this subtree must use sorting layer "Characters" (order 3).
            // Use "Roguelite > Set Character Sorting Layer" or "Roguelite > Fix All Sorting Layers"
            // to bulk-assign the layer after dropping prefabs in.
            var charsGo = new GameObject("Characters");
            charsGo.transform.SetParent(root.transform, false);

            // Effects container — VFX, projectiles, hit-sparks, etc.
            // SpriteRenderers here should use sorting layer "Effects" (order 4) so they
            // render on top of characters. Particle Systems default to this layer automatically
            // when placed under this container and "Fix All Sorting Layers" is run.
            var fxGo = new GameObject("Effects");
            fxGo.transform.SetParent(root.transform, false);

            // Debug scroll helper — active only in play mode via OnGUI.
            root.AddComponent<WorldConveyorDebug>();

            return root;
        }

        // ------------------------------------------------------------------
        // Grid sprite
        // ------------------------------------------------------------------

        /// <summary>
        /// Creates a 64x64 checkerboard PNG in Assets/Sprites/Environment/ and imports it
        /// as a Sprite with Repeat wrap mode and FullRect mesh so SpriteDrawMode.Tiled works.
        /// Returns the imported Sprite asset.
        /// </summary>
        internal static Sprite CreateOrLoadGridSprite()
        {
            // Re-use existing asset if already imported correctly.
            Sprite existing = AssetDatabase.LoadAssetAtPath<Sprite>(GridSpritePath);
            if (existing != null)
                return existing;

            // Ensure the directory exists.
            string directory = System.IO.Path.GetDirectoryName(GridSpritePath);
            if (!System.IO.Directory.Exists(directory))
                System.IO.Directory.CreateDirectory(directory);

            // Build the checkerboard texture in memory.
            var tex = new Texture2D(GridTextureSize, GridTextureSize, TextureFormat.RGBA32, false);
            var pixels = new Color32[GridTextureSize * GridTextureSize];

            for (int y = 0; y < GridTextureSize; y++)
            {
                for (int x = 0; x < GridTextureSize; x++)
                {
                    int cellX = x / GridCellSize;
                    int cellY = y / GridCellSize;
                    pixels[y * GridTextureSize + x] = ((cellX + cellY) % 2 == 0) ? GridColorA : GridColorB;
                }
            }

            tex.SetPixels32(pixels);
            tex.Apply();

            System.IO.File.WriteAllBytes(GridSpritePath, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);

            // First import — sets the raw asset type.
            AssetDatabase.ImportAsset(GridSpritePath, ImportAssetOptions.ForceUpdate);

            // Configure the importer for tiled sprite use.
            var importer = (TextureImporter)AssetImporter.GetAtPath(GridSpritePath);
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = GridPixelsPerUnit;
                importer.filterMode = FilterMode.Point;
                importer.wrapMode = TextureWrapMode.Repeat;
                importer.textureCompression = TextureImporterCompression.Uncompressed;

                // FullRect mesh is required for SpriteDrawMode.Tiled to function.
                var spriteSettings = new TextureImporterSettings();
                importer.ReadTextureSettings(spriteSettings);
                spriteSettings.spriteMeshType = SpriteMeshType.FullRect;
                importer.SetTextureSettings(spriteSettings);

                importer.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Sprite>(GridSpritePath);
        }

        // ------------------------------------------------------------------
        // Kept for callers that reference the placeholder (canvas background etc.)
        // ------------------------------------------------------------------

        /// <summary>
        /// Creates a 4x4 white texture saved as asset, or loads it if it already exists.
        /// </summary>
        internal static Sprite CreateOrLoadPlaceholderSprite()
        {
            const string path = "Assets/Sprites/Environment/placeholder_white.png";

            Sprite existing = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (existing != null)
                return existing;

            string directory = System.IO.Path.GetDirectoryName(path);
            if (!System.IO.Directory.Exists(directory))
                System.IO.Directory.CreateDirectory(directory);

            var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
            var pixels = new Color[16];
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = Color.white;
            tex.SetPixels(pixels);
            tex.Apply();

            System.IO.File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);

            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spritePixelsPerUnit = 4;
                importer.filterMode = FilterMode.Point;
                importer.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
    }
}
