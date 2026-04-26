using UnityEditor;
using UnityEngine;

namespace RogueliteAutoBattler.Editor.Tools
{
    internal static class MoveNavIconsMenu
    {
        private const string LogPrefix = "[MoveNavIconsMenu]";
        private const string SourceFolder = "Assets/Scripts/Test";

        private static readonly (string SourceFileName, string DestinationSlug)[] Moves =
        {
            ("clan.png",    "guilde"),
            ("compass.png", "map"),
            ("forest.png",  "skilltree"),
            ("houses.png",  "village"),
            ("store.png",   "shop"),
        };

        [MenuItem("Tools/Roguelite/Move Nav Icons From Test")]
        internal static void Execute()
        {
            NavIconsImporter.EnsureNavIconsFolderExists();

            int moved = 0;
            try
            {
                AssetDatabase.StartAssetEditing();
                foreach ((string sourceFileName, string destinationSlug) in Moves)
                {
                    string sourcePath = $"{SourceFolder}/{sourceFileName}";
                    string destinationPath = NavIconsImporter.GetNavIconAssetPath(destinationSlug);

                    string moveError = AssetDatabase.MoveAsset(sourcePath, destinationPath);
                    if (!string.IsNullOrEmpty(moveError))
                    {
                        Debug.LogError($"{LogPrefix} Move failed: {sourcePath} -> {destinationPath} | {moveError}");
                        continue;
                    }

                    NavIconsImporter.ApplyNavIconSettings(destinationPath);
                    moved++;
                    Debug.Log($"{LogPrefix} Moved + configured: {sourcePath} -> {destinationPath}");
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }

            DeleteSourceFolderIfEmpty();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"{LogPrefix} Done. {moved}/{Moves.Length} icons moved to {NavIconsImporter.NavIconsFolder}.");
        }

        private static void DeleteSourceFolderIfEmpty()
        {
            if (!AssetDatabase.IsValidFolder(SourceFolder))
                return;

            string[] remaining = AssetDatabase.FindAssets(string.Empty, new[] { SourceFolder });
            if (remaining.Length == 0)
            {
                bool deleted = AssetDatabase.DeleteAsset(SourceFolder);
                Debug.Log($"{LogPrefix} Deleted empty folder {SourceFolder}: {deleted}");
            }
            else
            {
                Debug.LogWarning($"{LogPrefix} {SourceFolder} is not empty ({remaining.Length} assets remain). Skipping folder deletion.");
            }
        }
    }
}
