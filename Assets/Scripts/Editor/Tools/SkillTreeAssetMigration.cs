using RogueliteAutoBattler.Data;
using UnityEditor;
using UnityEngine;

namespace RogueliteAutoBattler.Editor.Tools
{
    [InitializeOnLoad]
    internal static class SkillTreeAssetMigration
    {
        internal const string LegacyAssetPath = "Assets/Data/SkillTreeData.asset";
        internal const string MigratedAssetPath = "Assets/Data/SkillTrees/Default.asset";

        private const string LogTag = "[SkillTreeAssetMigration]";

        static SkillTreeAssetMigration()
        {
            EditorApplication.delayCall += RunDefault;
        }

        private static void RunDefault()
        {
            EditorApplication.delayCall -= RunDefault;
            RunForTest(LegacyAssetPath, MigratedAssetPath, EditorPaths.ActiveSkillTreePointerAsset, EditorPaths.SkillTreesFolder);
        }

        internal static void RunForTest(string legacyPath, string migratedPath, string pointerPath, string treesFolder)
        {
            var existingPointer = AssetDatabase.LoadAssetAtPath<ActiveSkillTreePointer>(pointerPath);
            if (existingPointer != null && existingPointer.Target != null)
            {
                var targetPath = AssetDatabase.GetAssetPath(existingPointer.Target);
                if (!string.IsNullOrEmpty(targetPath)
                    && targetPath.StartsWith(treesFolder)
                    && AssetDatabase.LoadAssetAtPath<SkillTreeData>(targetPath) == existingPointer.Target)
                    return;
            }

            EditorAssetFolders.EnsureFolder(treesFolder);
            EditorAssetFolders.EnsureFolder(System.IO.Path.GetDirectoryName(pointerPath).Replace('\\', '/'));

            var migratedAsset = AssetDatabase.LoadAssetAtPath<SkillTreeData>(migratedPath);
            SkillTreeData targetAsset;
            if (migratedAsset != null)
            {
                targetAsset = migratedAsset;
            }
            else if (AssetDatabase.LoadAssetAtPath<SkillTreeData>(legacyPath) != null)
            {
                var moveError = AssetDatabase.MoveAsset(legacyPath, migratedPath);
                if (!string.IsNullOrEmpty(moveError))
                {
                    Debug.LogError($"{LogTag} Failed to move {legacyPath} to {migratedPath}: {moveError}");
                    return;
                }
                Debug.Log($"{LogTag} Moved {legacyPath} to {migratedPath}.");
                targetAsset = AssetDatabase.LoadAssetAtPath<SkillTreeData>(migratedPath);
            }
            else
            {
                Debug.LogWarning($"{LogTag} No SkillTreeData found at legacy or migrated path. Creating empty asset at {migratedPath}.");
                targetAsset = ScriptableObject.CreateInstance<SkillTreeData>();
                AssetDatabase.CreateAsset(targetAsset, migratedPath);
            }

            ActiveSkillTreePointer pointer;
            if (existingPointer != null)
            {
                pointer = existingPointer;
            }
            else
            {
                pointer = ScriptableObject.CreateInstance<ActiveSkillTreePointer>();
                AssetDatabase.CreateAsset(pointer, pointerPath);
            }

            var so = new SerializedObject(pointer);
            so.FindProperty(ActiveSkillTreePointer.FieldNames.Target).objectReferenceValue = targetAsset;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(pointer);
            AssetDatabase.SaveAssets();
        }
    }
}
