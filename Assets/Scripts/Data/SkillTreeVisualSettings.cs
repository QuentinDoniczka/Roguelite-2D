using System;
using System.Collections.Generic;
using UnityEngine;

namespace RogueliteAutoBattler.Data
{
    [CreateAssetMenu(fileName = "SkillTreeVisualSettings", menuName = "Roguelite/Skill Tree Visual Settings")]
    public sealed class SkillTreeVisualSettings : ScriptableObject
    {
        internal static class FieldNames
        {
            internal const string HaloSize = nameof(haloSize);
            internal const string HaloOpacityLocked = nameof(haloOpacityLocked);
            internal const string HaloOpacityAvailable = nameof(haloOpacityAvailable);
            internal const string HaloOpacityPurchased = nameof(haloOpacityPurchased);
            internal const string HaloOpacityMax = nameof(haloOpacityMax);
        }

        internal const float MinHaloSize = 32f;
        internal const float MaxHaloSize = 200f;
        internal const float MinOpacity = 0f;
        internal const float MaxOpacity = 1f;

        internal const float DefaultHaloSize = 120f;
        internal const float DefaultHaloOpacityLocked = 0f;
        internal const float DefaultHaloOpacityAvailable = 0.6f;
        internal const float DefaultHaloOpacityPurchased = 0.85f;
        internal const float DefaultHaloOpacityMax = 1f;

        internal sealed class TunableDescriptor
        {
            internal string FieldName { get; }
            internal string DisplayLabel { get; }
            internal float Min { get; }
            internal float Max { get; }
            internal Func<SkillTreeVisualSettings, float> Getter { get; }
            internal Action<SkillTreeVisualSettings, float> Setter { get; }

            internal TunableDescriptor(
                string fieldName,
                string displayLabel,
                float min,
                float max,
                Func<SkillTreeVisualSettings, float> getter,
                Action<SkillTreeVisualSettings, float> setter)
            {
                FieldName = fieldName;
                DisplayLabel = displayLabel;
                Min = min;
                Max = max;
                Getter = getter;
                Setter = setter;
            }
        }

        [SerializeField, Range(MinHaloSize, MaxHaloSize)] private float haloSize = DefaultHaloSize;
        [SerializeField, Range(MinOpacity, MaxOpacity)] private float haloOpacityLocked = DefaultHaloOpacityLocked;
        [SerializeField, Range(MinOpacity, MaxOpacity)] private float haloOpacityAvailable = DefaultHaloOpacityAvailable;
        [SerializeField, Range(MinOpacity, MaxOpacity)] private float haloOpacityPurchased = DefaultHaloOpacityPurchased;
        [SerializeField, Range(MinOpacity, MaxOpacity)] private float haloOpacityMax = DefaultHaloOpacityMax;

        public float HaloSize => haloSize;
        public float HaloOpacityLocked => haloOpacityLocked;
        public float HaloOpacityAvailable => haloOpacityAvailable;
        public float HaloOpacityPurchased => haloOpacityPurchased;
        public float HaloOpacityMax => haloOpacityMax;

        private static readonly TunableDescriptor[] _tunables =
        {
            new TunableDescriptor(
                FieldNames.HaloSize, "Halo Size",
                MinHaloSize, MaxHaloSize,
                s => s.haloSize, (s, v) => s.haloSize = v),
            new TunableDescriptor(
                FieldNames.HaloOpacityLocked, "Opacity Locked",
                MinOpacity, MaxOpacity,
                s => s.haloOpacityLocked, (s, v) => s.haloOpacityLocked = v),
            new TunableDescriptor(
                FieldNames.HaloOpacityAvailable, "Opacity Available",
                MinOpacity, MaxOpacity,
                s => s.haloOpacityAvailable, (s, v) => s.haloOpacityAvailable = v),
            new TunableDescriptor(
                FieldNames.HaloOpacityPurchased, "Opacity Purchased",
                MinOpacity, MaxOpacity,
                s => s.haloOpacityPurchased, (s, v) => s.haloOpacityPurchased = v),
            new TunableDescriptor(
                FieldNames.HaloOpacityMax, "Opacity Max",
                MinOpacity, MaxOpacity,
                s => s.haloOpacityMax, (s, v) => s.haloOpacityMax = v),
        };

        internal static IReadOnlyList<TunableDescriptor> Tunables => _tunables;

        internal static SkillTreeVisualSettings CreateDefaults()
        {
            return ScriptableObject.CreateInstance<SkillTreeVisualSettings>();
        }

        internal void SetForTesting(float size, float locked, float available, float purchased, float max)
        {
            haloSize = size;
            haloOpacityLocked = locked;
            haloOpacityAvailable = available;
            haloOpacityPurchased = purchased;
            haloOpacityMax = max;
        }

        internal void SetFieldValue(string fieldName, float value)
        {
            for (int i = 0; i < _tunables.Length; i++)
            {
                if (_tunables[i].FieldName == fieldName)
                {
                    _tunables[i].Setter(this, value);
                    return;
                }
            }
            throw new ArgumentException($"Unknown field: {fieldName}", nameof(fieldName));
        }
    }
}
