using System;
using UnityEngine;

namespace RogueliteAutoBattler.UI.Toolkit.SkillTree
{
    internal static class SkillTreeNodeOrbResolver
    {
        private const string ResourcesPath = "UI/SkillTreeNodeOrb";

        private static Texture2D _cached;

        internal static Func<Texture2D> Provider = () => Resources.Load<Texture2D>(ResourcesPath);

        internal static Texture2D Get()
        {
            if (_cached != null)
                return _cached;

            _cached = Provider();
            return _cached;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void InvalidateCacheOnRuntimeLoad()
        {
            _cached = null;
        }

        internal static void ResetCache()
        {
            _cached = null;
        }
    }
}
