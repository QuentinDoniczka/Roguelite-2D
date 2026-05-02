using UnityEditor;

namespace RogueliteAutoBattler.Tests.EditMode
{
    internal static class AssetFolderUtils
    {
        internal static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder)) return;
            int lastSlash = folder.LastIndexOf('/');
            string parent = folder.Substring(0, lastSlash);
            string leaf = folder.Substring(lastSlash + 1);
            if (!AssetDatabase.IsValidFolder(parent))
                EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, leaf);
        }
    }
}
