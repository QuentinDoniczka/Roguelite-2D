using UnityEngine;

namespace RogueliteAutoBattler.Data
{
    public static class ActiveSkillTreeResolver
    {
        internal const string ResourceName = "ActiveSkillTree";

        public static SkillTreeData GetActive()
        {
            var pointer = Resources.Load<ActiveSkillTreePointer>(ResourceName);
            return pointer != null ? pointer.Target : null;
        }
    }
}
