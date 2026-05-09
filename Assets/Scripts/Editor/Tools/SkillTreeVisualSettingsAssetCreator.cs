using RogueliteAutoBattler.Data;
using UnityEditor;
using UnityEngine;

namespace RogueliteAutoBattler.Editor.Tools
{
    internal static class SkillTreeVisualSettingsAssetCreator
    {
        internal const string AssetPath = "Assets/Resources/Data/SkillTreeVisualSettings.asset";
        private const string FolderResourcesData = "Assets/Resources/Data";

        [MenuItem("Tools/Skill Tree/Create Visual Settings")]
        private static void CreateMenu()
        {
            EnsureExists();
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<SkillTreeVisualSettings>(AssetPath);
        }

        [InitializeOnLoadMethod]
        private static void EnsureOnEditorLoad()
        {
            EnsureExists();
        }

        internal static SkillTreeVisualSettings EnsureExists()
        {
            EditorAssetFolders.EnsureFolder(FolderResourcesData);
            var existing = AssetDatabase.LoadAssetAtPath<SkillTreeVisualSettings>(AssetPath);
            if (existing != null) return existing;
            var instance = ScriptableObject.CreateInstance<SkillTreeVisualSettings>();
            AssetDatabase.CreateAsset(instance, AssetPath);
            AssetDatabase.SaveAssets();
            return instance;
        }
    }
}
