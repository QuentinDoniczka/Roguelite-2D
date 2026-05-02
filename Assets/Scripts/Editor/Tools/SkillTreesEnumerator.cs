using System;
using System.Collections.Generic;
using RogueliteAutoBattler.Data;
using UnityEditor;
using UnityEngine;

namespace RogueliteAutoBattler.Editor.Tools
{
    internal static class SkillTreesEnumerator
    {
        internal readonly struct TreeEntry
        {
            public readonly string Guid;
            public readonly string AssetPath;
            public readonly string DisplayName;
            public readonly SkillTreeData Asset;

            public TreeEntry(string guid, string assetPath, string displayName, SkillTreeData asset)
            {
                Guid = guid;
                AssetPath = assetPath;
                DisplayName = displayName;
                Asset = asset;
            }
        }

        internal static IReadOnlyList<TreeEntry> Enumerate(string folder)
        {
            if (string.IsNullOrEmpty(folder) || !AssetDatabase.IsValidFolder(folder))
                return Array.Empty<TreeEntry>();

            var guids = AssetDatabase.FindAssets($"t:{nameof(SkillTreeData)}", new[] { folder });
            var entries = new List<TreeEntry>(guids.Length);
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<SkillTreeData>(path);
                if (asset == null) continue;
                var displayName = System.IO.Path.GetFileNameWithoutExtension(path);
                entries.Add(new TreeEntry(guid, path, displayName, asset));
            }
            entries.Sort((a, b) => string.CompareOrdinal(a.DisplayName, b.DisplayName));
            return entries;
        }

        internal static bool SetActivePointer(string pointerAssetPath, SkillTreeData target)
        {
            if (string.IsNullOrEmpty(pointerAssetPath) || target == null) return false;
            var pointer = AssetDatabase.LoadAssetAtPath<ActiveSkillTreePointer>(pointerAssetPath);
            if (pointer == null) return false;
            var so = new SerializedObject(pointer);
            so.FindProperty(ActiveSkillTreePointer.FieldNames.Target).objectReferenceValue = target;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(pointer);
            AssetDatabase.SaveAssets();
            return true;
        }
    }
}
