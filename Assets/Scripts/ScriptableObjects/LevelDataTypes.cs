using System;
using System.Collections.Generic;
using UnityEngine;
using RogueliteAutoBattler.Combat;

namespace RogueliteAutoBattler.Data
{
    [Serializable]
    public class StageData
    {
        [SerializeField] private string stageName = "New Stage";
        [SerializeField] private List<LevelData> levels = new List<LevelData>();

        public string StageName => stageName;
        public List<LevelData> Levels => levels;
    }

    [Serializable]
    public class LevelData
    {
        [SerializeField] private string levelName = "New Level";
        [SerializeField] private List<WaveData> waves = new List<WaveData>();

        public string LevelName => levelName;
        public List<WaveData> Waves => waves;
    }

    [Serializable]
    public class WaveData
    {
        [SerializeField] private string waveName = "Wave";
        [Tooltip("Terrain identifier for this wave (placeholder)")]
        [SerializeField] private string terrain = "";
        [SerializeField] private List<EnemySpawnData> enemies = new List<EnemySpawnData>();

        public string WaveName => waveName;
        public string Terrain => terrain;
        public List<EnemySpawnData> Enemies => enemies;
    }

    [Serializable]
    public class EnemySpawnData
    {
        [SerializeField] private string enemyName = "Enemy";
        [Tooltip("Enemy prefab — must have Rigidbody2D root + Animator child")]
        [SerializeField] private GameObject prefab;
        [SerializeField] private CharacterStats baseStats;
        [SerializeField] private StatOverrideSet statOverrides = new StatOverrideSet();
        [SerializeField] private Vector2 spawnOffset;

        public string EnemyName => enemyName;
        public GameObject Prefab => prefab;
        public CharacterStats BaseStats => baseStats;
        public StatOverrideSet StatOverrides => statOverrides;
        public Vector2 SpawnOffset => spawnOffset;
    }

    [Serializable]
    public class StatOverride
    {
        [SerializeField] private string fieldName;
        [SerializeField] private bool enabled;
        [SerializeField] private int intValue;
        [SerializeField] private float floatValue;

        public string FieldName => fieldName;
        public bool Enabled => enabled;
        public int IntValue => intValue;
        public float FloatValue => floatValue;
    }

    [Serializable]
    public class StatOverrideSet
    {
        [SerializeField] private List<StatOverride> overrides = new List<StatOverride>();

        public List<StatOverride> Overrides => overrides;
    }
}
