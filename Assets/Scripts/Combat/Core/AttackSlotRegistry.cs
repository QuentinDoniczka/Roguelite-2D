using System.Collections.Generic;
using UnityEngine;

namespace RogueliteAutoBattler.Combat.Core
{
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

        public static Vector2 Acquire(Transform target, Transform attacker, bool attackerFacesRight)
        {
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

            ReleaseFromAll(attacker);

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

        public static void ReleaseAll(Transform target)
        {
            _slots.Remove(target);
        }

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

        public static void Clear()
        {
            _slots.Clear();
        }

        private static Vector2 ComputeOffset(int slotIndex, bool attackerFacesRight)
        {
            float xSign = attackerFacesRight ? -1f : 1f;

            if (slotIndex < MaxFrontSlots)
            {
                float x = xSign * FaceOffset;
                float y = SlotIndexToVertical(slotIndex);
                return new Vector2(x, y);
            }

            int overflowIndex = slotIndex - MaxFrontSlots;
            float xBehind = -xSign * FaceOffset;
            float yBehind = SlotIndexToVertical(overflowIndex);
            return new Vector2(xBehind, yBehind);
        }

        private static float SlotIndexToVertical(int localIndex)
        {
            if (localIndex == 0) return 0f;

            int tier = (localIndex + 1) / 2;
            bool isTop = localIndex % 2 == 1;
            return isTop ? tier * VerticalSpacing : -tier * VerticalSpacing;
        }

        private static int FindSmallestAvailableIndex(List<SlotEntry> entries)
        {
            for (int i = entries.Count - 1; i >= 0; i--)
            {
                if (entries[i].Attacker == null)
                {
                    entries.RemoveAt(i);
                }
            }

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

        private static void ReleaseFromAll(Transform attacker)
        {
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
