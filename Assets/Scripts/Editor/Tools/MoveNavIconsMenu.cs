using UnityEditor;
using UnityEngine;

namespace RogueliteAutoBattler.Editor.Tools
{
    internal static class MoveNavIconsMenu
    {
        private const string NavIconsFolder = "Assets/UI/Icons/Nav";
        private const string SourceFolder = "Assets/Scripts/Test";

        private static readonly (string src, string dst)[] Moves =
        {
            ("Assets/Scripts/Test/clan.png",    "Assets/UI/Icons/Nav/guilde.png"),
            ("Assets/Scripts/Test/compass.png", "Assets/UI/Icons/Nav/map.png"),
            ("Assets/Scripts/Test/forest.png",  "Assets/UI/Icons/Nav/skilltree.png"),
            ("Assets/Scripts/Test/houses.png",  "Assets/UI/Icons/Nav/village.png"),
            ("Assets/Scripts/Test/store.png",   "Assets/UI/Icons/Nav/shop.png"),
        };

        [MenuItem("Tools/Roguelite/Move Nav Icons From Test")]
        internal static void Execute()
        {
            EnsureNavIconsFolder();

            int moved = 0;
            foreach ((string src, string dst) in Moves)
            {
                string error = AssetDatabase.MoveAsset(src, dst);
                if (!string.IsNullOrEmpty(error))
                {
                    Debug.LogError($"[MoveNavIconsMenu] Move failed: {src} -> {dst} | {error}");
                    continue;
                }

                NavIconsImporter.ApplyNavIconSettings(dst);
                moved++;
                Debug.Log($"[MoveNavIconsMenu] Moved + configured: {src} -> {dst}");
            }

            DeleteSourceFolderIfEmpty();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[MoveNavIconsMenu] Done. {moved}/{Moves.Length} icons moved to {NavIconsFolder}.");
        }

        private static void EnsureNavIconsFolder()
        {
            if (AssetDatabase.IsValidFolder("Assets/UI/Icons"))
                return;
            AssetDatabase.CreateFolder("Assets/UI", "Icons");
            Debug.Log("[MoveNavIconsMenu] Created Assets/UI/Icons");

            if (!AssetDatabase.IsValidFolder(NavIconsFolder))
            {
                AssetDatabase.CreateFolder("Assets/UI/Icons", "Nav");
                Debug.Log($"[MoveNavIconsMenu] Created {NavIconsFolder}");
            }
        }

        private static void DeleteSourceFolderIfEmpty()
        {
            string[] remaining = AssetDatabase.FindAssets("", new[] { SourceFolder });
            if (remaining.Length == 0)
            {
                bool deleted = AssetDatabase.DeleteAsset(SourceFolder);
                Debug.Log($"[MoveNavIconsMenu] Deleted empty folder {SourceFolder}: {deleted}");
            }
            else
            {
                Debug.LogWarning($"[MoveNavIconsMenu] {SourceFolder} is not empty ({remaining.Length} assets remain). Skipping folder deletion.");
            }
        }
    }
}
