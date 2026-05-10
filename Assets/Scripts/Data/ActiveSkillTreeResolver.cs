using System;
using UnityEngine;

namespace RogueliteAutoBattler.Data
{
    public static class ActiveSkillTreeResolver
    {
        internal const string ResourceName = "ActiveSkillTree";

        internal static Func<SkillTreeData> Provider { get; set; } = LoadFromResources;

        public static SkillTreeData GetActive()
        {
            return Provider != null ? Provider() : null;
        }

        internal static void ResetProviderToDefault()
        {
            Provider = LoadFromResources;
        }

        private static SkillTreeData LoadFromResources()
        {
            var pointer = Resources.Load<ActiveSkillTreePointer>(ResourceName);
            return pointer != null ? pointer.Target : null;
        }
    }
}
