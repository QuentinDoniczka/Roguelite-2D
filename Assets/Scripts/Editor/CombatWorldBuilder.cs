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
        internal const string CombatWorldPrefabPath = "Assets/Prefabs/CombatWorld.prefab";

        private const float CameraOrthoSize = 5.4f;
        private const float CameraZPosition = -10f;

        // Ground tile — 200 units wide so the camera never sees an edge during a scroll session.
        // Height and centerY are calculated at runtime by GroundFitter.
        private const float GroundWidth = 200f;

        // Must match GroundFitter._gameAreaBottomRatio default (0.40f).
        private const float GameAreaBottomRatio = 0.40f;

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
        /// Attaches WorldConveyor (side-scroll motion), CombatScrollManager (phase controller),
        /// and CombatSpawnManager (spawn orchestrator) to the root.
        /// GroundFitter is attached to the Ground child.
        /// </summary>
        internal static GameObject CreateCombatWorld()
        {
            var root = new GameObject("CombatWorld");
            root.transform.position = Vector3.zero;
            Undo.RegisterCreatedObjectUndo(root, "Create CombatWorld");

            // Ground — tiled checkerboard, sized to match GameArea (top 60% of screen).
            // Positioned behind everything on Background sorting layer.
            // GroundFitter recalculates size and position at runtime from the camera.
            var groundGo = new GameObject("Ground");
            groundGo.transform.SetParent(root.transform, false);
            Undo.RegisterCreatedObjectUndo(groundGo, "Create CombatWorld");
            SpriteRenderer groundRenderer = groundGo.AddComponent<SpriteRenderer>();
            groundRenderer.sprite = CreateOrLoadGridSprite();
            groundRenderer.drawMode = SpriteDrawMode.Tiled;
            // Position and size for edit-mode preview (GroundFitter overrides at runtime).
            // GameArea = top 60%: bottom at y = -orthoSize + 0.4*visibleHeight, top at y = orthoSize
            float visibleHeight = CameraOrthoSize * 2f;
            float gameAreaBottom = -CameraOrthoSize + GameAreaBottomRatio * visibleHeight;
            float groundHeight = CameraOrthoSize - gameAreaBottom;
            float groundCenterY = (gameAreaBottom + CameraOrthoSize) * 0.5f;
            // Anchor left edge to the left of the screen (assume ~16:9 aspect for edit preview).
            float previewAspect = 9f / 16f; // portrait mobile
            float previewHalfWidth = CameraOrthoSize * previewAspect;
            float anchorX = -previewHalfWidth + GroundWidth * 0.5f;
            groundGo.transform.localPosition = new Vector3(anchorX, groundCenterY, 0f);
            groundRenderer.size = new Vector2(GroundWidth, groundHeight);
            groundRenderer.sortingLayerName = "Background";
            groundRenderer.sortingOrder = -10;
            groundRenderer.color = Color.white;
            // URP 2D uses Lit sprites by default — without a Light2D in the scene,
            // sprites appear black. The ground is a flat background that doesn't
            // need lighting, so we use the Unlit material.
            var unlitShader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
            if (unlitShader != null)
                groundRenderer.material = new Material(unlitShader);
            else
                Debug.LogWarning($"[{nameof(CombatWorldBuilder)}] Shader 'Sprite-Unlit-Default' not found. Ground may render black.");
            groundGo.AddComponent<GroundFitter>();

            // Team container — place adventurer prefabs here.
            // Every SpriteRenderer in this subtree must use sorting layer "Characters".
            var teamGo = new GameObject(CombatSpawnManager.TeamContainerName);
            teamGo.transform.SetParent(root.transform, false);
            Undo.RegisterCreatedObjectUndo(teamGo, "Create CombatWorld");

            // Enemies container — place enemy prefabs here.
            // Every SpriteRenderer in this subtree must use sorting layer "Characters".
            var enemiesGo = new GameObject(CombatSpawnManager.EnemiesContainerName);
            enemiesGo.transform.SetParent(root.transform, false);
            Undo.RegisterCreatedObjectUndo(enemiesGo, "Create CombatWorld");

            // Effects container — VFX, projectiles, hit-sparks, etc.
            // SpriteRenderers here should use sorting layer "Effects" (order 4) so they
            // render on top of characters.
            var fxGo = new GameObject("Effects");
            fxGo.transform.SetParent(root.transform, false);
            Undo.RegisterCreatedObjectUndo(fxGo, "Create CombatWorld");

            // Runtime scroll components on the CombatWorld root.
            // WorldConveyor drives the physical side-scroll motion.
            // CombatScrollManager orchestrates scroll phases and references the conveyor.
            root.AddComponent<WorldConveyor>();
            var scrollManager = root.AddComponent<CombatScrollManager>();

            var soScrollManager = new SerializedObject(scrollManager);
            EditorUIFactory.SetObj(soScrollManager, "_conveyor", root.GetComponent<WorldConveyor>());
            soScrollManager.ApplyModifiedProperties();

            // CombatSpawnManager handles spawning adventurers and enemies into their containers.
            var spawnManager = root.AddComponent<CombatSpawnManager>();

            var soSpawnManager = new SerializedObject(spawnManager);
            EditorUIFactory.SetObj(soSpawnManager, "_teamContainer", teamGo.transform);
            EditorUIFactory.SetObj(soSpawnManager, "_enemiesContainer", enemiesGo.transform);

            // Auto-assign the default character prefab so the scene is ready to play immediately.
            var characterPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Characters/sampleCharacterHuman.prefab");
            if (characterPrefab != null)
                EditorUIFactory.SetObj(soSpawnManager, "_characterPrefab", characterPrefab);
            else
                Debug.LogWarning($"[{nameof(CombatWorldBuilder)}] sampleCharacterHuman.prefab not found — assign _characterPrefab manually.");

            // Auto-assign stats assets.
            var allyStats = AssetDatabase.LoadAssetAtPath<CharacterStats>("Assets/Data/Adventurers/WarriorStats.asset");
            if (allyStats != null)
                EditorUIFactory.SetObj(soSpawnManager, "_allyStats", allyStats);

            var enemyStats = AssetDatabase.LoadAssetAtPath<CharacterStats>("Assets/Data/Enemies/EnemyStats.asset");
            if (enemyStats != null)
                EditorUIFactory.SetObj(soSpawnManager, "_enemyStats", enemyStats);

            soSpawnManager.ApplyModifiedProperties();

            return root;
        }

        // ------------------------------------------------------------------
        // CombatWorld prefab
        // ------------------------------------------------------------------

        /// <summary>
        /// Returns the CombatWorld prefab asset, creating and saving it if it does not exist.
        /// When the prefab is freshly created the instance is left in the scene connected to it.
        /// </summary>
        internal static GameObject EnsureCombatWorldPrefab()
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(CombatWorldPrefabPath);
            if (existing != null)
                return existing;

            // Build the hierarchy in the scene temporarily, then save as a prefab.
            var instance = CreateCombatWorld();

            EditorUIFactory.EnsureDirectoryExists(CombatWorldPrefabPath);
            var prefab = PrefabUtility.SaveAsPrefabAssetAndConnect(
                instance, CombatWorldPrefabPath, InteractionMode.AutomatedAction);

            Debug.Log($"[CombatWorldBuilder] Created CombatWorld prefab at {CombatWorldPrefabPath}");
            return prefab;
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

            EditorUIFactory.EnsureDirectoryExists(GridSpritePath);

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
    }
}
