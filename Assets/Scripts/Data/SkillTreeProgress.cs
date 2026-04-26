using System;
using System.Collections.Generic;
using UnityEngine;

namespace RogueliteAutoBattler.Data
{
    [CreateAssetMenu(fileName = "SkillTreeProgress", menuName = "Roguelite/Skill Tree Progress")]
    public class SkillTreeProgress : ScriptableObject
    {
        public const int BulkResetNodeIndex = -1;

        [SerializeField] private List<int> levels = new List<int>();

        internal IReadOnlyList<int> Levels => levels;

        public event Action<int, int> OnLevelChanged;

        public int GetLevel(int nodeIndex)
        {
            if (nodeIndex < 0 || nodeIndex >= levels.Count) return 0;
            return levels[nodeIndex];
        }

        public void SetLevel(int nodeIndex, int level)
        {
            while (levels.Count <= nodeIndex)
                levels.Add(0);
            if (levels[nodeIndex] == level) return;
            levels[nodeIndex] = level;
            OnLevelChanged?.Invoke(nodeIndex, level);
        }

        public void ResetAll()
        {
            for (int i = 0; i < levels.Count; i++)
                levels[i] = 0;
            OnLevelChanged?.Invoke(BulkResetNodeIndex, 0);
        }
    }
}
