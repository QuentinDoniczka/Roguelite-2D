using System;
using System.Collections.Generic;
using RogueliteAutoBattler.Data;
using UnityEditor;
using UnityEngine;

namespace RogueliteAutoBattler.Editor.Tools
{
    internal static class SkillTreesEnumerator
    {
        internal const string DuplicateSuffix = "_Copy";
        private const int FirstAdditionalDuplicateIndex = 2;

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

        internal static bool IsPathUnderSkillTreesFolder(string fullPath, string skillTreesFolder)
        {
            if (string.IsNullOrEmpty(fullPath) || string.IsNullOrEmpty(skillTreesFolder)) return false;
            var normalized = fullPath.Replace('\\', '/');
            var assetsRelative = ConvertAbsoluteToAssetRelative(normalized);
            if (string.IsNullOrEmpty(assetsRelative)) return false;
            return assetsRelative.StartsWith(skillTreesFolder + "/", StringComparison.Ordinal)
                || assetsRelative == skillTreesFolder;
        }

        internal static string ConvertAbsoluteToAssetRelative(string fullPath)
        {
            if (string.IsNullOrEmpty(fullPath)) return null;
            var normalized = fullPath.Replace('\\', '/');
            var dataPath = Application.dataPath.Replace('\\', '/');
            if (normalized.StartsWith(dataPath, StringComparison.Ordinal))
                return "Assets" + normalized.Substring(dataPath.Length);
            if (normalized.StartsWith("Assets/", StringComparison.Ordinal) || normalized == "Assets")
                return normalized;
            return null;
        }

        internal static string MakeUniqueDuplicatePath(string sourcePath)
        {
            if (string.IsNullOrEmpty(sourcePath)) return null;
            var folder = System.IO.Path.GetDirectoryName(sourcePath)?.Replace('\\', '/');
            var nameNoExt = System.IO.Path.GetFileNameWithoutExtension(sourcePath);
            var ext = System.IO.Path.GetExtension(sourcePath);
            if (string.IsNullOrEmpty(folder) || string.IsNullOrEmpty(nameNoExt) || string.IsNullOrEmpty(ext))
                return null;

            var candidate = $"{folder}/{nameNoExt}{DuplicateSuffix}{ext}";
            int n = FirstAdditionalDuplicateIndex;
            while (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(candidate) != null)
            {
                candidate = $"{folder}/{nameNoExt}{DuplicateSuffix}_{n}{ext}";
                n++;
            }
            return candidate;
        }
    }
}
