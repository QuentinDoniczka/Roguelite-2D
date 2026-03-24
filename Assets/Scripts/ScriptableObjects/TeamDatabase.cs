using System.Collections.Generic;
using UnityEngine;

namespace RogueliteAutoBattler.Data
{
    [CreateAssetMenu(fileName = "TeamDatabase", menuName = "Roguelite/Team Database")]
    public class TeamDatabase : ScriptableObject
    {
        [SerializeField] private List<AllySpawnData> allies = new List<AllySpawnData>();

        public List<AllySpawnData> Allies => allies;
    }
}
