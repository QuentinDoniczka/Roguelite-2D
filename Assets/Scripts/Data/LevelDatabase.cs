using System.Collections.Generic;
using UnityEngine;

namespace RogueliteAutoBattler.Data
{
    [CreateAssetMenu(fileName = "LevelDatabase", menuName = "Roguelite/Level Database")]
    public class LevelDatabase : ScriptableObject
    {
        [SerializeField] private List<StageData> stages = new List<StageData>();
        [SerializeField] private Sprite defaultBackground;

        public List<StageData> Stages { get => stages; internal set => stages = value; }
        public Sprite DefaultBackground { get => defaultBackground; internal set => defaultBackground = value; }
    }
}
