using System.IO;
using RogueliteAutoBattler.Data;
using RogueliteAutoBattler.Services.Local;
using UnityEditor;
using UnityEngine;

namespace RogueliteAutoBattler.Editor.Tools
{
    internal static class ResetPlayerProgressMenu
    {
        private const string MenuPath = "Roguelite/Reset Player Progress";
        private const string LogTag = "[ResetPlayerProgress]";
        private const string DialogTitle = "Reset Player Progress";
        private const string DialogConfirmButton = "Reset";
        private const string DialogCancelButton = "Cancel";

        [MenuItem(MenuPath)]
        private static void ResetPlayerProgressMenuItem()
        {
            string jsonPath = Path.Combine(Application.persistentDataPath, LocalPlayerProgressionLoader.DefaultFileName);
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

            bool jsonFileDeleted = File.Exists(jsonPath);
            bool progressAssetReset = progressAsset != null;

            PerformReset(jsonPath, progressAsset);

            if (progressAssetReset)
                AssetDatabase.SaveAssets();

            Debug.Log($"{LogTag} Done. JSON deleted: {(jsonFileDeleted ? 1 : 0)}, asset reset: {(progressAssetReset ? 1 : 0)}.");
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
