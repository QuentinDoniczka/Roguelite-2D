using RogueliteAutoBattler.Combat;
using RogueliteAutoBattler.Data;
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

        // Must match GroundFitter._gameAreaBottomRatio default (0.40f).
        private const float GameAreaBottomRatio = 0.40f;

        private const string GridSpritePath = "Assets/Sprites/Environment/grid_ground.png";
        private const int GridTextureSize = 64;   // pixels per tile
        private const int GridCellSize = 8;        // pixels per checkerboard cell
        private const int GridPixelsPerUnit = 64;  // 1 tile = 1 world unit

        // Checkerboard colours — dark green / light green so movement is obvious.
        private static readonly Color32 GridColorA = new Color32(45, 90, 39, 255);   // #2d5a27
        private static readonly Color32 GridColorB = new Color32(61, 122, 55, 255);  // #3d7a37

        // Blue variant for alternate terrain.
        private const string GridBlueSpritePath = "Assets/Sprites/Environment/grid_ground_blue.png";
        private static readonly Color32 GridBlueColorA = new Color32(30, 60, 120, 255);  // #1e3c78
        private static readonly Color32 GridBlueColorB = new Color32(45, 85, 160, 255);  // #2d55a0

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
        /// Attaches WorldConveyor (side-scroll motion) and CombatSpawnManager (spawn orchestrator) to the root.
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

            // Kinematic Rigidbody2D on CombatWorld root so WorldConveyor can use
            // MovePosition — this keeps child dynamic Rigidbody2Ds in sync with physics.
            var rootRb = root.AddComponent<Rigidbody2D>();
            rootRb.bodyType = RigidbodyType2D.Kinematic;

            // WorldConveyor drives the physical side-scroll motion.
            root.AddComponent<WorldConveyor>();

            // CombatSpawnManager handles spawning adventurers and enemies into their containers.
            var spawnManager = root.AddComponent<CombatSpawnManager>();

            var soSpawnManager = new SerializedObject(spawnManager);
            EditorUIFactory.SetObj(soSpawnManager, "_teamContainer", teamGo.transform);
            EditorUIFactory.SetObj(soSpawnManager, "_enemiesContainer", enemiesGo.transform);

            // Assign TeamDatabase asset — create one with a default warrior if it doesn't exist.
            const string teamDbPath = "Assets/Data/TeamDatabase.asset";
            var teamDb = AssetDatabase.LoadAssetAtPath<TeamDatabase>(teamDbPath);
            if (teamDb == null)
            {
                teamDb = ScriptableObject.CreateInstance<TeamDatabase>();

                var soTeamDb = new SerializedObject(teamDb);
                var alliesProp = soTeamDb.FindProperty("allies");
                if (alliesProp != null)
                {
                    alliesProp.arraySize = 1;
                    var defaultAlly = alliesProp.GetArrayElementAtIndex(0);
                    defaultAlly.FindPropertyRelative("allyName").stringValue = "Warrior";
                    var defaultPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Characters/sampleCharacterHuman.prefab");
                    if (defaultPrefab != null)
                        defaultAlly.FindPropertyRelative("prefab").objectReferenceValue = defaultPrefab;
                    else
                        Debug.LogWarning($"[{nameof(CombatWorldBuilder)}] sampleCharacterHuman.prefab not found — assign ally prefab manually.");
                    defaultAlly.FindPropertyRelative("maxHp").intValue             = 100;
                    defaultAlly.FindPropertyRelative("atk").intValue               = 10;
                    defaultAlly.FindPropertyRelative("attackSpeed").floatValue     = 1f;
                    defaultAlly.FindPropertyRelative("moveSpeed").floatValue       = 2f;
                    defaultAlly.FindPropertyRelative("regenHpPerSecond").floatValue = 0f;
                    defaultAlly.FindPropertyRelative("colliderRadius").floatValue  = 0.05f;
                    soTeamDb.ApplyModifiedPropertiesWithoutUndo();
                }

                EditorUIFactory.EnsureDirectoryExists(teamDbPath);
                AssetDatabase.CreateAsset(teamDb, teamDbPath);
                AssetDatabase.SaveAssets();
            }
            EditorUIFactory.SetObj(soSpawnManager, "_teamDatabase", teamDb);

            // LevelManager reads the LevelDatabase at runtime and swaps the ground sprite.
            var levelManager = root.AddComponent<LevelManager>();
            var soLevelManager = new SerializedObject(levelManager);
            EditorUIFactory.SetObj(soLevelManager, "_groundRenderer", groundRenderer);
            EditorUIFactory.SetObj(soLevelManager, "_enemiesContainer", enemiesGo.transform);
            EditorUIFactory.SetObj(soLevelManager, "_teamContainer", teamGo.transform);

            var levelDb = AssetDatabase.LoadAssetAtPath<LevelDatabase>("Assets/Data/LevelDatabase.asset");
            if (levelDb != null)
                EditorUIFactory.SetObj(soLevelManager, "_levelDatabase", levelDb);

            // Screen-absolute anchors at scene root (outside CombatWorld so they
            // don't scroll). Home positions for characters + combat zone boundary.
            var teamAnchor = FindOrCreateHomeAnchor(CombatSpawnManager.TeamHomeAnchorName, new Vector2(0.12f, 0.70f));
            var enemiesAnchor = FindOrCreateHomeAnchor(CombatSpawnManager.EnemiesHomeAnchorName, new Vector2(0.88f, 0.70f));
            var combatTrigger = FindOrCreateHomeAnchor(CombatSpawnManager.CombatTriggerZoneName, new Vector2(1f, 0.5f));

            // Wire anchor references to managers.
            EditorUIFactory.SetObj(soSpawnManager, "_teamHomeAnchor", teamAnchor);
            soSpawnManager.ApplyModifiedProperties();

            EditorUIFactory.SetObj(soLevelManager, "_enemiesHomeAnchor", enemiesAnchor);
            EditorUIFactory.SetObj(soLevelManager, "_combatTriggerZone", combatTrigger);
            soLevelManager.ApplyModifiedProperties();

            return root;
        }

        private static Transform FindOrCreateHomeAnchor(string anchorName, Vector2 viewportPosition)
        {
            // Reuse existing anchor if already in the scene to avoid duplicates on rebuild.
            var existing = GameObject.Find(anchorName);
            if (existing != null)
            {
                var existingAnchor = existing.GetComponent<ScreenAnchor>();
                if (existingAnchor != null)
                {
                    Undo.RecordObject(existingAnchor, $"Update {anchorName}");
                    var so = new SerializedObject(existingAnchor);
                    so.FindProperty("_viewportPosition").vector2Value = viewportPosition;
                    so.ApplyModifiedProperties();
                }
                return existing.transform;
            }

            var go = new GameObject(anchorName);
            go.transform.position = Vector3.zero;
            Undo.RegisterCreatedObjectUndo(go, $"Create {anchorName}");

            var anchor = go.AddComponent<ScreenAnchor>();
            var so2 = new SerializedObject(anchor);
            so2.FindProperty("_viewportPosition").vector2Value = viewportPosition;
            so2.ApplyModifiedProperties();

            return go.transform;
        }

        // ------------------------------------------------------------------
        // Grid sprite
        // ------------------------------------------------------------------

        /// <summary>
        /// Creates a 64x64 checkerboard PNG in Assets/Sprites/Environment/ and imports it
        /// as a Sprite with Repeat wrap mode and FullRect mesh so SpriteDrawMode.Tiled works.
        /// Returns the imported Sprite asset.
        /// </summary>
        /// <summary>
        /// Creates or loads the blue checkerboard grid sprite (alternate terrain).
        /// </summary>
        internal static Sprite CreateOrLoadBlueGridSprite()
        {
            return CreateOrLoadCheckerboardSprite(GridBlueSpritePath, GridBlueColorA, GridBlueColorB);
        }

        internal static Sprite CreateOrLoadGridSprite()
        {
            return CreateOrLoadCheckerboardSprite(GridSpritePath, GridColorA, GridColorB);
        }

        private static Sprite CreateOrLoadCheckerboardSprite(string path, Color32 colorA, Color32 colorB)
        {
            Sprite existing = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (existing != null)
                return existing;

            EditorUIFactory.EnsureDirectoryExists(path);

            var tex = new Texture2D(GridTextureSize, GridTextureSize, TextureFormat.RGBA32, false);
            var pixels = new Color32[GridTextureSize * GridTextureSize];

            for (int y = 0; y < GridTextureSize; y++)
            {
                for (int x = 0; x < GridTextureSize; x++)
                {
                    int cellX = x / GridCellSize;
                    int cellY = y / GridCellSize;
                    pixels[y * GridTextureSize + x] = ((cellX + cellY) % 2 == 0) ? colorA : colorB;
                }
            }

            tex.SetPixels32(pixels);
            tex.Apply();

            System.IO.File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);

            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = GridPixelsPerUnit;
                importer.filterMode = FilterMode.Point;
                importer.wrapMode = TextureWrapMode.Repeat;
                importer.textureCompression = TextureImporterCompression.Uncompressed;

                var spriteSettings = new TextureImporterSettings();
                importer.ReadTextureSettings(spriteSettings);
                spriteSettings.spriteMeshType = SpriteMeshType.FullRect;
                importer.SetTextureSettings(spriteSettings);

                importer.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
    }
}
