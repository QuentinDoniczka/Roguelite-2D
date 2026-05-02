using UnityEngine;

namespace RogueliteAutoBattler.Data
{
    [CreateAssetMenu(fileName = "ActiveSkillTreePointer", menuName = "Roguelite/Active Skill Tree Pointer")]
    public sealed class ActiveSkillTreePointer : ScriptableObject
    {
        [SerializeField] private SkillTreeData target;

        public SkillTreeData Target
        {
            get => target;
            internal set => target = value;
        }
    }
}
