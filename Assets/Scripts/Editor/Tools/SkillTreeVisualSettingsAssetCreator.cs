using RogueliteAutoBattler.Data;
using UnityEditor;
using UnityEngine;

namespace RogueliteAutoBattler.Editor.Tools
{
    internal static class SkillTreeVisualSettingsAssetCreator
    {
        internal const string AssetPath = "Assets/Resources/UI/SkillTreeVisualSettings.asset";
        private const string FolderResourcesUI = "Assets/Resources/UI";

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
            EditorAssetFolders.EnsureFolder(FolderResourcesUI);
            var existing = AssetDatabase.LoadAssetAtPath<SkillTreeVisualSettings>(AssetPath);
            if (existing != null) return existing;
            var instance = ScriptableObject.CreateInstance<SkillTreeVisualSettings>();
            AssetDatabase.CreateAsset(instance, AssetPath);
            AssetDatabase.SaveAssets();
            return instance;
        }
    }
}
