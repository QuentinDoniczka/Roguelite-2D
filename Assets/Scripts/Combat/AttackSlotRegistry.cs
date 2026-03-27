using System.Collections.Generic;
using UnityEngine;

namespace RogueliteAutoBattler.Combat
{
    /// <summary>
    /// Static registry that manages attack slots around each target.
    /// Ensures multiple attackers spread out instead of stacking on the same point.
    /// </summary>
    public static class AttackSlotRegistry
    {
        private const float FaceOffset = 0.25f;
        private const float VerticalSpacing = 0.3f;
        private const int MaxFrontSlots = 5;

        private struct SlotEntry
        {
            public Transform Attacker;
            public int SlotIndex;
        }

        private static readonly Dictionary<Transform, List<SlotEntry>> _slots =
            new Dictionary<Transform, List<SlotEntry>>();

        /// <summary>
        /// Registers <paramref name="attacker"/> on <paramref name="target"/> and returns the slot offset.
        /// Idempotent: if attacker already registered on this target, returns existing slot.
        /// If attacker was registered on a different target, releases from old target first.
        /// </summary>
        public static Vector2 Acquire(Transform target, Transform attacker, bool attackerFacesRight)
        {
            // If already registered on this target, return existing offset
            if (_slots.TryGetValue(target, out var entries))
            {
                for (int i = 0; i < entries.Count; i++)
                {
                    if (entries[i].Attacker == attacker)
                    {
                        return ComputeOffset(entries[i].SlotIndex, attackerFacesRight);
                    }
                }
            }

            // Release from any other target the attacker may be registered on
            ReleaseFromAll(attacker);

            // Find the smallest available slot index
            if (!_slots.TryGetValue(target, out entries))
            {
                entries = new List<SlotEntry>();
                _slots[target] = entries;
            }

            int slotIndex = FindSmallestAvailableIndex(entries);

            entries.Add(new SlotEntry
            {
                Attacker = attacker,
                SlotIndex = slotIndex
            });

            return ComputeOffset(slotIndex, attackerFacesRight);
        }

        /// <summary>
        /// Frees the slot held by <paramref name="attacker"/> on <paramref name="target"/>.
        /// No-op if not registered.
        /// </summary>
        public static void Release(Transform target, Transform attacker)
        {
            if (!_slots.TryGetValue(target, out var entries))
                return;

            for (int i = entries.Count - 1; i >= 0; i--)
            {
                if (entries[i].Attacker == attacker)
                {
                    entries.RemoveAt(i);
                    break;
                }
            }

            if (entries.Count == 0)
            {
                _slots.Remove(target);
            }
        }

        /// <summary>
        /// Frees all slots for <paramref name="target"/>.
        /// Safety net on target death.
        /// </summary>
        public static void ReleaseAll(Transform target)
        {
            _slots.Remove(target);
        }

        /// <summary>
        /// Returns the count of attackers currently registered on <paramref name="target"/>.
        /// Skips entries where the attacker has been destroyed.
        /// </summary>
        public static int AttackerCount(Transform target)
        {
            if (!_slots.TryGetValue(target, out var entries))
                return 0;

            int count = 0;
            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i].Attacker != null)
                {
                    count++;
                }
            }

            return count;
        }

        /// <summary>
        /// Clears the entire registry. Used between levels.
        /// </summary>
        public static void Clear()
        {
            _slots.Clear();
        }

        /// <summary>
        /// Computes the world-space offset for a given slot index.
        /// Front slots (0..MaxFrontSlots-1) are on the facing side.
        /// Overflow slots (MaxFrontSlots+) are behind the target.
        /// </summary>
        private static Vector2 ComputeOffset(int slotIndex, bool attackerFacesRight)
        {
            float xSign = attackerFacesRight ? -1f : 1f;

            if (slotIndex < MaxFrontSlots)
            {
                // Front side: slot 0 is center, then alternate top/bottom
                float x = xSign * FaceOffset;
                float y = SlotIndexToVertical(slotIndex);
                return new Vector2(x, y);
            }

            // Overflow: behind the target, same vertical pattern
            int overflowIndex = slotIndex - MaxFrontSlots;
            float xBehind = -xSign * FaceOffset;
            float yBehind = SlotIndexToVertical(overflowIndex);
            return new Vector2(xBehind, yBehind);
        }

        /// <summary>
        /// Maps a local index (0,1,2,3,4,...) to a vertical offset.
        /// 0 -> 0, 1 -> +Spacing, 2 -> -Spacing, 3 -> +2*Spacing, 4 -> -2*Spacing, ...
        /// </summary>
        private static float SlotIndexToVertical(int localIndex)
        {
            if (localIndex == 0) return 0f;

            int tier = (localIndex + 1) / 2;
            bool isTop = localIndex % 2 == 1;
            return isTop ? tier * VerticalSpacing : -tier * VerticalSpacing;
        }

        /// <summary>
        /// Finds the smallest slot index not currently occupied in the entry list.
        /// Accounts for gaps left by released slots.
        /// </summary>
        private static int FindSmallestAvailableIndex(List<SlotEntry> entries)
        {
            // Purge destroyed attackers while we scan
            for (int i = entries.Count - 1; i >= 0; i--)
            {
                if (entries[i].Attacker == null)
                {
                    entries.RemoveAt(i);
                }
            }

            // Find smallest unused index
            for (int candidate = 0; ; candidate++)
            {
                bool taken = false;
                for (int i = 0; i < entries.Count; i++)
                {
                    if (entries[i].SlotIndex == candidate)
                    {
                        taken = true;
                        break;
                    }
                }

                if (!taken) return candidate;
            }
        }

        /// <summary>
        /// Releases <paramref name="attacker"/> from any target it may be registered on.
        /// </summary>
        private static void ReleaseFromAll(Transform attacker)
        {
            // An attacker can only be registered on one target at a time
            // (Acquire releases from any previous target before registering on the new one).
            Transform targetToClean = null;

            foreach (var kvp in _slots)
            {
                var entries = kvp.Value;
                for (int i = 0; i < entries.Count; i++)
                {
                    if (entries[i].Attacker == attacker)
                    {
                        targetToClean = kvp.Key;
                        break;
                    }
                }

                if (targetToClean != null) break;
            }

            if (targetToClean != null)
            {
                Release(targetToClean, attacker);
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnDomainReload()
        {
            _slots.Clear();
        }
    }
}
