using System;
using System.Collections.Generic;
using UnityEngine;

namespace RogueliteAutoBattler.UI.Toolkit.SkillTree
{
    internal enum OrbLayerKind { Core, Halo, Rays }

    internal static class SkillTreeNodeOrbResolver
    {
        internal const string ResourcesLoadPath = "UI/SkillTreeNodeOrb";
        private const string ResourcesLoadPathFormat = "UI/SkillTreeNodeOrb_{0}";

        private static readonly Dictionary<OrbLayerKind, Texture2D> _cacheByKind = new Dictionary<OrbLayerKind, Texture2D>();

        internal static Func<OrbLayerKind, Texture2D> Provider =
            kind => Resources.Load<Texture2D>(ResourcesLoadPathFor(kind));

        internal static string ResourcesLoadPathFor(OrbLayerKind kind) =>
            kind == OrbLayerKind.Core
                ? ResourcesLoadPath
                : string.Format(ResourcesLoadPathFormat, kind);

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
