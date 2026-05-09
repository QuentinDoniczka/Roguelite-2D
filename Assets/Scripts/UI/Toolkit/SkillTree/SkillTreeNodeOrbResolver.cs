using System;
using System.Collections.Generic;
using UnityEngine;

namespace RogueliteAutoBattler.UI.Toolkit.SkillTree
{
    // Mirror of OrbLayerKind in SkillTreeNodeOrbSpriteGenerator (Editor asmdef cannot expose enums to Runtime).
    internal enum OrbLayerKind { Core, Halo, Frame, Rim, InnerGlow, Sparkle }

    internal static class SkillTreeNodeOrbResolver
    {
        internal const string ResourcesLoadPath = "UI/SkillTreeNodeOrb";

        private static readonly Dictionary<OrbLayerKind, Texture2D> _cacheByKind = new();

        internal static Func<OrbLayerKind, Texture2D> Provider =
            kind => Resources.Load<Texture2D>(ResourcesLoadPathFor(kind));

        internal static string ResourcesLoadPathFor(OrbLayerKind kind) =>
            kind == OrbLayerKind.Core
                ? "UI/SkillTreeNodeOrb"
                : $"UI/SkillTreeNodeOrb_{kind}";

        internal static Texture2D Get(OrbLayerKind kind)
        {
            if (_cacheByKind.TryGetValue(kind, out Texture2D cached) && cached != null)
                return cached;

            Texture2D loaded = Provider(kind);
            _cacheByKind[kind] = loaded;
            return loaded;
        }

        internal static Texture2D Get()
        {
            return Get(OrbLayerKind.Core);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void InvalidateCacheOnRuntimeLoad()
        {
            _cacheByKind.Clear();
        }

        internal static void ResetCache()
        {
            _cacheByKind.Clear();
        }
    }
}
