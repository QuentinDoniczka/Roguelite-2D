using UnityEngine;

namespace RogueliteAutoBattler.Data
{
    public static class ActiveSkillNodePaletteResolver
    {
        internal const string ResourceName = "ActiveSkillNodePalette";

        public static SkillNodePalette GetActive()
        {
            var pointer = Resources.Load<ActiveSkillNodePalettePointer>(ResourceName);
            return pointer != null ? pointer.Target : null;
        }
    }
}
