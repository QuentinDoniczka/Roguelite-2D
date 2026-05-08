using System;
using System.Collections.Generic;
using UnityEngine;

namespace RogueliteAutoBattler.Data
{
    [CreateAssetMenu(fileName = "SkillNodePalette", menuName = "Roguelite/Skill Node Palette")]
    public sealed class SkillNodePalette : ScriptableObject
    {
        internal static class FieldNames
        {
            internal const string Entries = nameof(entries);
        }

        [Serializable]
        public struct PaletteEntry
        {
            public NodeColorTag tag;
            public Color color;
        }

        [SerializeField] private List<PaletteEntry> entries = new();

        public Color GetColor(NodeColorTag tag)
        {
            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i].tag == tag)
                    return entries[i].color;
            }
            return Color.white;
        }
    }
}
