using UnityEngine;

namespace RogueliteAutoBattler.Data
{
    [CreateAssetMenu(fileName = "ActiveSkillNodePalettePointer", menuName = "Roguelite/Active Skill Node Palette Pointer")]
    public sealed class ActiveSkillNodePalettePointer : ScriptableObject
    {
        internal static class FieldNames
        {
            internal const string Target = nameof(target);
        }

        [SerializeField] private SkillNodePalette target;

        public SkillNodePalette Target
        {
            get => target;
            internal set => target = value;
        }
    }
}
