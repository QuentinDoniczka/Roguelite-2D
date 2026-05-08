using System.Collections.Generic;
using RogueliteAutoBattler.Data;
using UnityEditor;
using UnityEngine;

namespace RogueliteAutoBattler.Editor.Tools
{
    internal static class DefaultNodePaletteCreator
    {
        private const string MenuPath = "Roguelite/Skill Tree/Create Default Node Palette";
        private const string LogTag = "[DefaultNodePaletteCreator]";
        private const string PaletteFolder = "Assets/Data/SkillTrees";
        private const string PalettePath = PaletteFolder + "/DefaultNodePalette.asset";
        private const string ResourcesFolder = "Assets/Resources";
        private const string PointerPath = ResourcesFolder + "/ActiveSkillNodePalette.asset";

        [MenuItem(MenuPath)]
        public static void CreateDefaultPalette()
        {
            EditorAssetFolders.EnsureFolder(PaletteFolder);
            EditorAssetFolders.EnsureFolder(ResourcesFolder);

            var existingPalette = AssetDatabase.LoadAssetAtPath<SkillNodePalette>(PalettePath);
            SkillNodePalette palette;
            if (existingPalette != null)
            {
                Debug.LogWarning($"{LogTag} Palette already exists at {PalettePath}; selecting existing asset.");
                palette = existingPalette;
                Selection.activeObject = palette;
            }
            else
            {
                palette = ScriptableObject.CreateInstance<SkillNodePalette>();
                PopulateDefaultEntries(palette);
                AssetDatabase.CreateAsset(palette, PalettePath);
                Debug.Log($"{LogTag} Created palette at {PalettePath}.");
            }

            var existingPointer = AssetDatabase.LoadAssetAtPath<ActiveSkillNodePalettePointer>(PointerPath);
            if (existingPointer == null)
            {
                var pointer = ScriptableObject.CreateInstance<ActiveSkillNodePalettePointer>();
                AssetDatabase.CreateAsset(pointer, PointerPath);
                AssignPointerTarget(pointer, palette);
                Debug.Log($"{LogTag} Created pointer at {PointerPath}.");
            }
            else
            {
                AssignPointerTarget(existingPointer, palette);
                Debug.Log($"{LogTag} Updated existing pointer at {PointerPath}.");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeObject = palette;
        }

        private static void PopulateDefaultEntries(SkillNodePalette palette)
        {
            var entries = new List<SkillNodePalette.PaletteEntry>
            {
                new SkillNodePalette.PaletteEntry { tag = NodeColorTag.Default, color = ColorFromBytes(255, 255, 255) },
                new SkillNodePalette.PaletteEntry { tag = NodeColorTag.Red,     color = ColorFromBytes(220, 70, 70) },
                new SkillNodePalette.PaletteEntry { tag = NodeColorTag.Orange,  color = ColorFromBytes(240, 140, 50) },
                new SkillNodePalette.PaletteEntry { tag = NodeColorTag.Yellow,  color = ColorFromBytes(240, 210, 80) },
                new SkillNodePalette.PaletteEntry { tag = NodeColorTag.Green,   color = ColorFromBytes(90, 200, 110) },
                new SkillNodePalette.PaletteEntry { tag = NodeColorTag.Cyan,    color = ColorFromBytes(80, 210, 220) },
                new SkillNodePalette.PaletteEntry { tag = NodeColorTag.Blue,    color = ColorFromBytes(90, 140, 235) },
                new SkillNodePalette.PaletteEntry { tag = NodeColorTag.Purple,  color = ColorFromBytes(170, 100, 220) },
                new SkillNodePalette.PaletteEntry { tag = NodeColorTag.Pink,    color = ColorFromBytes(235, 120, 180) }
            };

            var serialized = new SerializedObject(palette);
            var entriesProperty = serialized.FindProperty(SkillNodePalette.FieldNames.Entries);
            entriesProperty.arraySize = entries.Count;
            for (int i = 0; i < entries.Count; i++)
            {
                var element = entriesProperty.GetArrayElementAtIndex(i);
                element.FindPropertyRelative("tag").enumValueIndex = (int)entries[i].tag;
                element.FindPropertyRelative("color").colorValue = entries[i].color;
            }
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void AssignPointerTarget(ActiveSkillNodePalettePointer pointer, SkillNodePalette palette)
        {
            var serialized = new SerializedObject(pointer);
            var targetProperty = serialized.FindProperty(ActiveSkillNodePalettePointer.FieldNames.Target);
            targetProperty.objectReferenceValue = palette;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Color ColorFromBytes(byte r, byte g, byte b)
        {
            return new Color(r / 255f, g / 255f, b / 255f, 1f);
        }
    }
}
