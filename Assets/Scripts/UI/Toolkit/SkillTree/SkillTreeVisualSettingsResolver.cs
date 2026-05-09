using System;
using RogueliteAutoBattler.Data;
using UnityEngine;

namespace RogueliteAutoBattler.UI.Toolkit.SkillTree
{
    internal static class SkillTreeVisualSettingsResolver
    {
        internal const string ResourcesLoadPath = "UI/SkillTreeVisualSettings";

        private const float DefaultHaloSize = 120f;
        private const float DefaultOpacityLocked = 0f;
        private const float DefaultOpacityAvailable = 0.6f;
        private const float DefaultOpacityPurchased = 0.85f;
        private const float DefaultOpacityMax = 1f;

        private static SkillTreeVisualSettings _cache;

        internal static Func<SkillTreeVisualSettings> Provider =
            () => Resources.Load<SkillTreeVisualSettings>(ResourcesLoadPath);

        internal static SkillTreeVisualSettings Get()
        {
            if (_cache == null)
                _cache = Provider();
            return _cache;
        }

        internal static float GetOpacityForState(SkillTreeNodeVisualState state)
        {
            var settings = Get();
            if (settings == null)
            {
                return state switch
                {
                    SkillTreeNodeVisualState.Locked => DefaultOpacityLocked,
                    SkillTreeNodeVisualState.Available => DefaultOpacityAvailable,
                    SkillTreeNodeVisualState.Purchased => DefaultOpacityPurchased,
                    SkillTreeNodeVisualState.Max => DefaultOpacityMax,
                    _ => DefaultOpacityLocked
                };
            }
            return state switch
            {
                SkillTreeNodeVisualState.Locked => settings.HaloOpacityLocked,
                SkillTreeNodeVisualState.Available => settings.HaloOpacityAvailable,
                SkillTreeNodeVisualState.Purchased => settings.HaloOpacityPurchased,
                SkillTreeNodeVisualState.Max => settings.HaloOpacityMax,
                _ => settings.HaloOpacityLocked
            };
        }

        internal static float GetHaloSize()
        {
            var settings = Get();
            return settings != null ? settings.HaloSize : DefaultHaloSize;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void InvalidateCacheOnRuntimeLoad()
        {
            _cache = null;
        }

        internal static void ResetCache()
        {
            _cache = null;
        }
    }
}
