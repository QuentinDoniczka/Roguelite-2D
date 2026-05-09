using System;
using RogueliteAutoBattler.Data;
using UnityEngine;

namespace RogueliteAutoBattler.UI.Toolkit.SkillTree
{
    internal static class SkillTreeVisualSettingsResolver
    {
        internal const string ResourcesLoadPath = "Data/SkillTreeVisualSettings";

        private static SkillTreeVisualSettings _cache;
        private static SkillTreeVisualSettings _defaultsCache;

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
            if (settings == null) settings = GetDefaults();
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
            if (settings == null) settings = GetDefaults();
            return settings.HaloSize;
        }

        private static SkillTreeVisualSettings GetDefaults()
        {
            if (_defaultsCache == null)
                _defaultsCache = SkillTreeVisualSettings.CreateDefaults();
            return _defaultsCache;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void InvalidateCacheOnRuntimeLoad()
        {
            _cache = null;
            _defaultsCache = null;
        }

        internal static void ResetCache()
        {
            _cache = null;
            _defaultsCache = null;
        }
    }
}
