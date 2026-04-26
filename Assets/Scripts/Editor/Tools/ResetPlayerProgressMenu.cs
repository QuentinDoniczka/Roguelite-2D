using System.IO;
using RogueliteAutoBattler.Data;
using UnityEditor;
using UnityEngine;

namespace RogueliteAutoBattler.Editor.Tools
{
    internal static class ResetPlayerProgressMenu
    {
        private const string MenuPath = "Roguelite/Reset Player Progress";
        private const string PersistedFileName = "player_progression.json";
        private const string LogTag = "[ResetPlayerProgress]";
        private const string DialogTitle = "Reset Player Progress";
        private const string DialogConfirmButton = "Reset";
        private const string DialogCancelButton = "Cancel";

        [MenuItem(MenuPath)]
        private static void ResetPlayerProgressMenuItem()
        {
            string jsonPath = Path.Combine(Application.persistentDataPath, PersistedFileName);
            string message =
                "Reset all player progression?\n\n" +
                "- Skill tree levels -> 0\n" +
                "- Gold -> 0 (on next Play session)\n\n" +
                $"JSON file: {jsonPath}\n\n" +
                "This cannot be undone.";

            if (!EditorUtility.DisplayDialog(DialogTitle, message, DialogConfirmButton, DialogCancelButton))
                return;

            SkillTreeProgress progressAsset =
                AssetDatabase.LoadAssetAtPath<SkillTreeProgress>(EditorPaths.SkillTreeProgressAsset);

            bool fileExisted = File.Exists(jsonPath);
            bool assetReset = progressAsset != null;

            PerformReset(jsonPath, progressAsset);

            if (assetReset)
                AssetDatabase.SaveAssets();

            int deletedFiles = fileExisted ? 1 : 0;
            int resetAssets = assetReset ? 1 : 0;
            Debug.Log($"{LogTag} Done. JSON deleted: {deletedFiles}, asset reset: {resetAssets}.");
        }

        internal static void PerformReset(string jsonPath, SkillTreeProgress progressAsset)
        {
            if (File.Exists(jsonPath))
                File.Delete(jsonPath);

            if (progressAsset != null)
            {
                progressAsset.ResetAll();
                EditorUtility.SetDirty(progressAsset);
            }
        }
    }
}
