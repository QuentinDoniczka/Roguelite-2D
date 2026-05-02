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
                if (!string.IsNullOrEmpty(targetPath) && targetPath.StartsWith(treesFolder))
                    return;
            }

            EnsureFolderExists(treesFolder);
            EnsureFolderExists(System.IO.Path.GetDirectoryName(pointerPath).Replace('\\', '/'));

            SkillTreeData targetAsset;
            if (AssetDatabase.LoadAssetAtPath<SkillTreeData>(migratedPath) != null)
            {
                targetAsset = AssetDatabase.LoadAssetAtPath<SkillTreeData>(migratedPath);
            }
            else if (AssetDatabase.LoadAssetAtPath<SkillTreeData>(legacyPath) != null)
            {
                var moveError = AssetDatabase.MoveAsset(legacyPath, migratedPath);
                if (!string.IsNullOrEmpty(moveError))
                {
                    Debug.LogError($"[{nameof(SkillTreeAssetMigration)}] Failed to move {legacyPath} to {migratedPath}: {moveError}");
                    return;
                }
                Debug.Log($"[{nameof(SkillTreeAssetMigration)}] Moved {legacyPath} to {migratedPath}.");
                targetAsset = AssetDatabase.LoadAssetAtPath<SkillTreeData>(migratedPath);
            }
            else
            {
                Debug.LogWarning($"[{nameof(SkillTreeAssetMigration)}] No SkillTreeData found at legacy or migrated path. Creating empty asset at {migratedPath}.");
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

        private static void EnsureFolderExists(string folderPath)
        {
            if (string.IsNullOrEmpty(folderPath)) return;
            if (AssetDatabase.IsValidFolder(folderPath)) return;

            var parts = folderPath.Replace('\\', '/').Split('/');
            var current = parts[0];
            for (var i = 1; i < parts.Length; i++)
            {
                var next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }
    }
}
