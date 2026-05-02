using UnityEngine;

namespace RogueliteAutoBattler.Data
{
    [CreateAssetMenu(fileName = "ActiveSkillTreePointer", menuName = "Roguelite/Active Skill Tree Pointer")]
    public sealed class ActiveSkillTreePointer : ScriptableObject
    {
        internal static class FieldNames
        {
            internal const string Target = nameof(target);
        }

        [SerializeField] private SkillTreeData target;

        public SkillTreeData Target
        {
            get => target;
            internal set => target = value;
        }
    }
}
