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

        [SerializeField, Range(32f, 200f)] private float haloSize = 120f;
        [SerializeField, Range(0f, 1f)] private float haloOpacityLocked = 0f;
        [SerializeField, Range(0f, 1f)] private float haloOpacityAvailable = 0.6f;
        [SerializeField, Range(0f, 1f)] private float haloOpacityPurchased = 0.85f;
        [SerializeField, Range(0f, 1f)] private float haloOpacityMax = 1f;

        public float HaloSize => haloSize;
        public float HaloOpacityLocked => haloOpacityLocked;
        public float HaloOpacityAvailable => haloOpacityAvailable;
        public float HaloOpacityPurchased => haloOpacityPurchased;
        public float HaloOpacityMax => haloOpacityMax;

        internal void SetForTesting(float size, float locked, float available, float purchased, float max)
        {
            haloSize = size;
            haloOpacityLocked = locked;
            haloOpacityAvailable = available;
            haloOpacityPurchased = purchased;
            haloOpacityMax = max;
        }
    }
}
