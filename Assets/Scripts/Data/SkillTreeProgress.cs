using System.Collections.Generic;
using UnityEngine;

namespace RogueliteAutoBattler.Data
{
    [CreateAssetMenu(fileName = "SkillTreeProgress", menuName = "Roguelite/Skill Tree Progress")]
    public class SkillTreeProgress : ScriptableObject
    {
        [SerializeField] private List<int> levels = new List<int>();

        public int GetLevel(int nodeIndex)
        {
            if (nodeIndex < 0 || nodeIndex >= levels.Count) return 0;
            return levels[nodeIndex];
        }

        public void SetLevel(int nodeIndex, int level)
        {
            while (levels.Count <= nodeIndex)
                levels.Add(0);
            levels[nodeIndex] = level;
        }

        public void ResetAll()
        {
            for (int i = 0; i < levels.Count; i++)
                levels[i] = 0;
        }
    }
}
