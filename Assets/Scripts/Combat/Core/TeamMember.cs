using RogueliteAutoBattler.Data;
using UnityEngine;

namespace RogueliteAutoBattler.Combat.Core
{
    public sealed class TeamMember
    {
        public int Index { get; }
        public AllySpawnData SpawnData { get; }
        public GameObject GameObject { get; internal set; }
        public CombatStats Stats { get; internal set; }

        public bool IsDead => Stats != null && Stats.IsDead;

        internal TeamMember(int index, AllySpawnData spawnData)
        {
            Index = index;
            SpawnData = spawnData;
        }
    }
}
