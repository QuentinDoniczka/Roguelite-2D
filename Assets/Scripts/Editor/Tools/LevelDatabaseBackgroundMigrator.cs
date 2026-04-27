using System.IO;
using System.Linq;
using RogueliteAutoBattler.Data;
using UnityEditor;
using UnityEngine;

namespace RogueliteAutoBattler.Editor.Tools
{
    internal static class LevelDatabaseBackgroundMigrator
    {
        private const string LevelDatabaseAssetPath = "Assets/Data/LevelDatabase.asset";
        private const string DefaultBackgroundPath = "Assets/Sprites/Environment/backgroundtest.png";
        private const string Level1BackgroundPath = "Assets/Sprites/Environment/grid_ground.png";
        private const string LogPrefix = "[LevelDatabaseBackgroundMigrator]";

        [MenuItem("Tools/Roguelite/Migrate LevelDatabase Backgrounds")]
        internal static void MigrateLevelDatabaseBackgrounds()
        {
            var db = AssetDatabase.LoadAssetAtPath<LevelDatabase>(LevelDatabaseAssetPath);
            if (db == null)
            {
                Debug.LogError($"{LogPrefix} LevelDatabase not found at {LevelDatabaseAssetPath}");
                return;
            }

            var defaultSprite = LoadCanonicalSprite(DefaultBackgroundPath);
            if (defaultSprite == null)
            {
                Debug.LogError($"{LogPrefix} Default background sprite not found at {DefaultBackgroundPath}");
                return;
            }

            var gridSprite = LoadCanonicalSprite(Level1BackgroundPath);
            if (gridSprite == null)
            {
                Debug.LogError($"{LogPrefix} Level 1 background sprite not found at {Level1BackgroundPath}");
                return;
            }

            var so = new SerializedObject(db);

            so.FindProperty("defaultBackground").objectReferenceValue = defaultSprite;

            var stagesProp = so.FindProperty("stages");
            for (int stageIndex = 0; stageIndex < stagesProp.arraySize; stageIndex++)
            {
                var stageProp = stagesProp.GetArrayElementAtIndex(stageIndex);
                var levelsProp = stageProp.FindPropertyRelative("levels");
                for (int levelIndex = 0; levelIndex < levelsProp.arraySize; levelIndex++)
                {
                    var levelProp = levelsProp.GetArrayElementAtIndex(levelIndex);
                    var fitProp = levelProp.FindPropertyRelative("fit");
                    fitProp.enumValueIndex = 0;

                    if (stageIndex == 0 && levelIndex == 0)
                    {
                        levelProp.FindPropertyRelative("background").objectReferenceValue = gridSprite;
                    }
                }
            }

            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(db);
            AssetDatabase.SaveAssets();

            Debug.Log($"{LogPrefix} LevelDatabase backgrounds migrated. Default=backgroundtest, Level1=grid_ground.");
        }

        private static Sprite LoadCanonicalSprite(string path)
        {
            var fileName = Path.GetFileNameWithoutExtension(path);
            var sprites = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToArray();
            return sprites.FirstOrDefault(s => s.name == fileName) ?? sprites.FirstOrDefault();
        }
    }
}
