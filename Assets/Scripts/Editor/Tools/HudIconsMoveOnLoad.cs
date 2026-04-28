using UnityEditor;
using UnityEngine;

namespace RogueliteAutoBattler.Editor.Tools
{
    [InitializeOnLoad]
    internal static class HudIconsMoveOnLoad
    {
        private const string ExecutedKey = "HudIconsMoveOnLoad_Executed";
        private const string SelfPath = "Assets/Scripts/Editor/Tools/HudIconsMoveOnLoad.cs";

        static HudIconsMoveOnLoad()
        {
            if (SessionState.GetBool(ExecutedKey, false))
                return;

            SessionState.SetBool(ExecutedKey, true);
            EditorApplication.delayCall += RunOnce;
        }

        private static void RunOnce()
        {
            HudIconsImporter.EnsureHudIconsFolderExists();

            foreach (string slug in HudIconsImporter.HudIconSlugs)
            {
                string src = $"Assets/Sprites/{slug}.png";
                string dst = HudIconsImporter.GetHudIconAssetPath(slug);

                if (AssetDatabase.LoadAssetAtPath<UnityEngine.Texture2D>(src) == null)
                {
                    Debug.Log($"[HudIconsMoveOnLoad] {slug}: not found at {src}, skipping");
                    continue;
                }

                if (AssetDatabase.LoadAssetAtPath<UnityEngine.Texture2D>(dst) != null)
                {
                    Debug.Log($"[HudIconsMoveOnLoad] {slug}: already at {dst}, skipping move");
                    continue;
                }

                string error = AssetDatabase.MoveAsset(src, dst);
                if (!string.IsNullOrEmpty(error))
                {
                    Debug.LogError($"[HudIconsMoveOnLoad] move {slug}: {error}");
                    continue;
                }

                HudIconsImporter.ApplyHudIconSettings(dst);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[HudIconsMoveOnLoad] move complete — self-deleting bootstrap");

            AssetDatabase.DeleteAsset(SelfPath);
            AssetDatabase.SaveAssets();
        }
    }
}
