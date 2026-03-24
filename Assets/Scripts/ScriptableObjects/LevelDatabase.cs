using System.Collections.Generic;
using UnityEngine;

namespace RogueliteAutoBattler.Data
{
    [CreateAssetMenu(fileName = "LevelDatabase", menuName = "Roguelite/Level Database")]
    public class LevelDatabase : ScriptableObject
    {
        [SerializeField] private List<StageData> stages = new List<StageData>();

        public List<StageData> Stages => stages;
    }
}
