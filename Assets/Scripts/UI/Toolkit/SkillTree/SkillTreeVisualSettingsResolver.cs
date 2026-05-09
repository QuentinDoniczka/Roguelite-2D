using System;
using RogueliteAutoBattler.Data;
using UnityEngine;

namespace RogueliteAutoBattler.UI.Toolkit.SkillTree
{
    internal static class SkillTreeVisualSettingsResolver
    {
        internal const string ResourcesLoadPath = "UI/SkillTreeVisualSettings";

        private static SkillTreeVisualSettings _cache;

        internal static Func<SkillTreeVisualSettings> Provider =
            () => Resources.Load<SkillTreeVisualSettings>(ResourcesLoadPath);

        internal static SkillTreeVisualSettings Get() => _cache ??= Provider();

        internal static float GetOpacityForState(SkillTreeNodeVisualState state)
        {
            var settings = Get();
            if (settings == null)
            {
                return state switch
                {
                    SkillTreeNodeVisualState.Locked => 0f,
                    SkillTreeNodeVisualState.Available => 0.6f,
                    SkillTreeNodeVisualState.Purchased => 0.85f,
                    SkillTreeNodeVisualState.Max => 1f,
                    _ => 0f
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
            return settings != null ? settings.HaloSize : 120f;
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
