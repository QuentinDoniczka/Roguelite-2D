using UnityEngine;

namespace RogueliteAutoBattler.UI.Toolkit.SkillTree
{
    internal static class SkillTreeNodeOrbResolver
    {
        private const string ResourcesPath = "UI/SkillTreeNodeOrb";

        private static Texture2D _cached;

        internal static Texture2D Get()
        {
            if (_cached != null)
                return _cached;

            _cached = Resources.Load<Texture2D>(ResourcesPath);
            return _cached;
        }

        internal static void ResetCache()
        {
            _cached = null;
        }
    }
}
