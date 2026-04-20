using System.Collections.Generic;
using System.Linq;
using RogueliteAutoBattler.Combat.Core;
using RogueliteAutoBattler.Combat.Environment;
using RogueliteAutoBattler.Combat.Levels;
using RogueliteAutoBattler.Combat.Visuals;
using RogueliteAutoBattler.Common;
using RogueliteAutoBattler.Core;
using RogueliteAutoBattler.Data;
using UnityEditor;
using UnityEngine;

namespace RogueliteAutoBattler.Editor
{
    internal static class CombatWorldBuilder
    {
        private const float CameraOrthoSize = 5.4f;
        private const float CameraZPosition = -10f;
        private const float GroundWidth = 200f;

        // Editor-only initial Ground placement (runtime GroundFitter recomputes on Play).
        private const float EditorGroundX = 95.96f;
        private const float EditorGroundY = 2.16f;
        private const float EditorGroundHeight = 6.48f;

        private const float HomeAnchorWorldOffsetY = 0f;

        private const string WeaponSpritesFolder = "Assets/Sprites/Items/melee weapons";
        private const string HatSpritesFolder    = "Assets/Sprites/Items/Wardrobe/cloth";
        private const string ShieldSpritePath    = "Assets/Sprites/Items/melee weapons/shield.png";

        private static readonly string[] HeadSpriteFolders = new[]
        {
            "Assets/Sprites/Characters/human/head",
            "Assets/Sprites/Characters/elf/head",
            "Assets/Sprites/Characters/goblin/head",
            "Assets/Sprites/Characters/orc/head"
        };

        private const string GridSpritePath = "Assets/Sprites/Environment/grid_ground.png";

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

        internal static GameObject CreateCombatWorld()
        {
            var root = new GameObject(GameBootstrap.CombatWorldName);
            root.transform.position = Vector3.zero;
            Undo.RegisterCreatedObjectUndo(root, "Create CombatWorld");

            var groundGo = new GameObject("Ground");
            groundGo.transform.SetParent(root.transform, false);
            Undo.RegisterCreatedObjectUndo(groundGo, "Create CombatWorld");
            SpriteRenderer groundRenderer = groundGo.AddComponent<SpriteRenderer>();
            groundRenderer.sprite = CreateOrLoadGridSprite();
            groundRenderer.drawMode = SpriteDrawMode.Tiled;

            groundGo.transform.localPosition = new Vector3(EditorGroundX, EditorGroundY, 0f);
            groundRenderer.size = new Vector2(GroundWidth, EditorGroundHeight);
            groundRenderer.sortingLayerName = SortingLayers.Background;
            groundRenderer.sortingOrder = -10;
            groundRenderer.color = Color.white;

            var unlitShader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
            if (unlitShader != null)
                groundRenderer.material = new Material(unlitShader);
            else
                Debug.LogWarning($"[{nameof(CombatWorldBuilder)}] Shader 'Sprite-Unlit-Default' not found. Ground may render black.");
            groundGo.AddComponent<GroundFitter>();

            var teamGo = new GameObject(CombatSetupHelper.TeamContainerName);
            teamGo.transform.SetParent(root.transform, false);
            Undo.RegisterCreatedObjectUndo(teamGo, "Create CombatWorld");

            var enemiesGo = new GameObject(CombatSetupHelper.EnemiesContainerName);
            enemiesGo.transform.SetParent(root.transform, false);
            Undo.RegisterCreatedObjectUndo(enemiesGo, "Create CombatWorld");

            var fxGo = new GameObject("Effects");
            fxGo.transform.SetParent(root.transform, false);
            Undo.RegisterCreatedObjectUndo(fxGo, "Create CombatWorld");

            var rootRb = root.AddComponent<Rigidbody2D>();
            rootRb.bodyType = RigidbodyType2D.Kinematic;

            root.AddComponent<WorldConveyor>();
            root.AddComponent<CombatWorldVisibility>();
            root.AddComponent<TeamRoster>();

            var spawnManager = root.AddComponent<CombatSpawnManager>();

            var soSpawnManager = new SerializedObject(spawnManager);
            EditorUIFactory.SetObj(soSpawnManager, "_teamContainer", teamGo.transform);

            var teamDb = AssetDatabase.LoadAssetAtPath<TeamDatabase>(TeamBuilderTab.TeamDatabaseDefaultPath);
            if (teamDb == null)
            {
                teamDb = ScriptableObject.CreateInstance<TeamDatabase>();

                var soTeamDb = new SerializedObject(teamDb);
                var alliesProp = soTeamDb.FindProperty("allies");
                if (alliesProp != null)
                {
                    alliesProp.arraySize = 1;
                    var defaultAlly = alliesProp.GetArrayElementAtIndex(0);
                    defaultAlly.FindPropertyRelative("allyName").stringValue = TeamBuilderTab.TeamDefaultAllyName;
                    var defaultPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(TeamBuilderTab.TeamDefaultAllyPrefabPath);
                    if (defaultPrefab != null)
                        defaultAlly.FindPropertyRelative("prefab").objectReferenceValue = defaultPrefab;
                    else
                        Debug.LogWarning($"[{nameof(CombatWorldBuilder)}] sampleCharacterHuman.prefab not found — assign ally prefab manually.");
                    defaultAlly.FindPropertyRelative("maxHp").intValue             = TeamBuilderTab.TeamDefaultMaxHp;
                    defaultAlly.FindPropertyRelative("atk").intValue               = TeamBuilderTab.TeamDefaultAtk;
                    defaultAlly.FindPropertyRelative("attackSpeed").floatValue     = TeamBuilderTab.TeamDefaultAttackSpeed;
                    defaultAlly.FindPropertyRelative("moveSpeed").floatValue       = TeamBuilderTab.TeamDefaultMoveSpeed;
                    defaultAlly.FindPropertyRelative("regenHpPerSecond").floatValue = TeamBuilderTab.TeamDefaultRegenHpPerSec;
                    defaultAlly.FindPropertyRelative("colliderRadius").floatValue  = TeamBuilderTab.TeamDefaultColliderRadius;
                    soTeamDb.ApplyModifiedPropertiesWithoutUndo();
                }

                EditorUIFactory.EnsureDirectoryExists(TeamBuilderTab.TeamDatabaseDefaultPath);
                AssetDatabase.CreateAsset(teamDb, TeamBuilderTab.TeamDatabaseDefaultPath);
                AssetDatabase.SaveAssets();
            }
            EditorUIFactory.SetObj(soSpawnManager, "_teamDatabase", teamDb);

            var levelManager = root.AddComponent<LevelManager>();
            var soLevelManager = new SerializedObject(levelManager);
            EditorUIFactory.SetObj(soLevelManager, "_groundRenderer", groundRenderer);
            EditorUIFactory.SetObj(soLevelManager, "_enemiesContainer", enemiesGo.transform);
            EditorUIFactory.SetObj(soLevelManager, "_teamContainer", teamGo.transform);

            var levelDb = AssetDatabase.LoadAssetAtPath<LevelDatabase>(LevelDesignerTab.LevelDatabaseDefaultPath);
            if (levelDb != null)
                EditorUIFactory.SetObj(soLevelManager, "_levelDatabase", levelDb);

            var teamAnchor = FindOrCreateHomeAnchor(CombatSetupHelper.TeamHomeAnchorName, new Vector2(0.12f, 0.676f), new Vector2(0f, HomeAnchorWorldOffsetY));
            var enemiesAnchor = FindOrCreateHomeAnchor(CombatSetupHelper.EnemiesHomeAnchorName, new Vector2(0.88f, 0.676f), new Vector2(0f, HomeAnchorWorldOffsetY));
            var combatTrigger = FindOrCreateHomeAnchor(CombatSetupHelper.CombatTriggerZoneName, new Vector2(1f, 0.5f));

            EditorUIFactory.SetObj(soSpawnManager, "_teamHomeAnchor", teamAnchor);
            soSpawnManager.ApplyModifiedProperties();

            EditorUIFactory.SetObj(soLevelManager, "_teamHomeAnchor", teamAnchor);
            EditorUIFactory.SetObj(soLevelManager, "_enemiesHomeAnchor", enemiesAnchor);
            EditorUIFactory.SetObj(soLevelManager, "_combatTriggerZone", combatTrigger);
            soLevelManager.ApplyModifiedProperties();

            AddVisualEquipmentTestLoop(root);

            var damageNumberConfig = AssetDatabase.LoadAssetAtPath<DamageNumberConfig>(SettingsWindow.ConfigPath);
            if (damageNumberConfig == null)
            {
                damageNumberConfig = ScriptableObject.CreateInstance<DamageNumberConfig>();
                EditorUIFactory.EnsureDirectoryExists(SettingsWindow.ConfigPath);
                AssetDatabase.CreateAsset(damageNumberConfig, SettingsWindow.ConfigPath);
                AssetDatabase.SaveAssets();
            }

            var damageNumberBootstrap = root.AddComponent<DamageNumberBootstrap>();
            var soDamageNumberBootstrap = new SerializedObject(damageNumberBootstrap);
            EditorUIFactory.SetObj(soDamageNumberBootstrap, "_config", damageNumberConfig);
            EditorUIFactory.SetObj(soDamageNumberBootstrap, "_effectsContainer", fxGo.transform);
            soDamageNumberBootstrap.ApplyModifiedProperties();

            return root;
        }

        private static void AddVisualEquipmentTestLoop(GameObject combatWorld)
        {
            var equipmentLoop = combatWorld.AddComponent<VisualEquipmentTestLoop>();
            var so = new SerializedObject(equipmentLoop);

            var headSprites = new List<Sprite>();
            foreach (var folder in HeadSpriteFolders)
                headSprites.AddRange(LoadSpritesFromFolder(folder));
            SetSpriteArray(so, "_headSprites", headSprites.ToArray());

            var weaponSprites = LoadSpritesFromFolder(
                WeaponSpritesFolder,
                excludePathContaining: "shield");
            SetSpriteArray(so, "_weaponSprites", weaponSprites);

            var hatSprites = LoadSpritesFromFolder(HatSpritesFolder);
            SetSpriteArray(so, "_hatSprites", hatSprites);

            var shieldSprites = LoadSpritesFromFile(ShieldSpritePath);
            SetSpriteArray(so, "_shieldSprites", shieldSprites);

            so.ApplyModifiedProperties();

            Debug.Log($"[{nameof(CombatWorldBuilder)}] VisualEquipmentTestLoop wired — " +
                      $"heads:{headSprites.Count}, weapons:{weaponSprites.Length}, hats:{hatSprites.Length}, shields:{shieldSprites.Length}");
        }

        private static Sprite[] LoadSpritesFromFolder(string folder, string excludePathContaining = null)
        {
            var guids = AssetDatabase.FindAssets("t:Sprite", new[] { folder });
            var result = new List<Sprite>();
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (excludePathContaining != null &&
                    path.Replace('\\', '/').ToLowerInvariant().Contains(excludePathContaining.ToLowerInvariant()))
                    continue;

                result.AddRange(AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>());
            }
            return result.ToArray();
        }

        private static Sprite[] LoadSpritesFromFile(string assetPath)
        {
            return AssetDatabase.LoadAllAssetsAtPath(assetPath).OfType<Sprite>().ToArray();
        }

        private static void SetSpriteArray(SerializedObject so, string propertyName, Sprite[] sprites)
        {
            var prop = so.FindProperty(propertyName);
            if (prop == null)
            {
                Debug.LogError($"[{nameof(CombatWorldBuilder)}] SerializedProperty '{propertyName}' not found on {so.targetObject.GetType().Name}.");
                return;
            }

            prop.arraySize = sprites.Length;
            for (int i = 0; i < sprites.Length; i++)
                prop.GetArrayElementAtIndex(i).objectReferenceValue = sprites[i];
        }

        private static Transform FindOrCreateHomeAnchor(string anchorName, Vector2 viewportPosition, Vector2 worldOffset = default)
        {
            var existing = GameObject.Find(anchorName);
            if (existing != null)
            {
                var existingAnchor = existing.GetComponent<ScreenAnchor>();
                if (existingAnchor != null)
                {
                    Undo.RecordObject(existingAnchor, $"Update {anchorName}");
                    var so = new SerializedObject(existingAnchor);
                    so.FindProperty("_viewportPosition").vector2Value = viewportPosition;
                    so.FindProperty("_worldOffset").vector2Value = worldOffset;
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
            so2.FindProperty("_worldOffset").vector2Value = worldOffset;
            so2.ApplyModifiedProperties();

            return go.transform;
        }

        private static Sprite CreateOrLoadGridSprite()
        {
            return CreateOrLoadCheckerboardSprite(GridSpritePath);
        }

        private static Sprite CreateOrLoadCheckerboardSprite(string path)
        {
            Sprite existing = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (existing != null)
                return existing;

            EditorUIFactory.EnsureDirectoryExists(path);

            var tex = new Texture2D(ProceduralGroundSprite.TextureSize, ProceduralGroundSprite.TextureSize, TextureFormat.RGBA32, false);
            var pixels = new Color32[ProceduralGroundSprite.TextureSize * ProceduralGroundSprite.TextureSize];

            for (int y = 0; y < ProceduralGroundSprite.TextureSize; y++)
            {
                for (int x = 0; x < ProceduralGroundSprite.TextureSize; x++)
                {
                    int cellX = x / ProceduralGroundSprite.CellSize;
                    int cellY = y / ProceduralGroundSprite.CellSize;
                    pixels[y * ProceduralGroundSprite.TextureSize + x] = ((cellX + cellY) % 2 == 0) ? ProceduralGroundSprite.ColorA : ProceduralGroundSprite.ColorB;
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
                importer.spritePixelsPerUnit = ProceduralGroundSprite.PixelsPerUnit;
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
