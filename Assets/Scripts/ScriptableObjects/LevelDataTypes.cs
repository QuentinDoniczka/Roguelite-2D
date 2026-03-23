using System;
using System.Collections.Generic;
using UnityEngine;

namespace RogueliteAutoBattler.Data
{
    public enum AttackType
    {
        Melee,
        Range
    }

    [Serializable]
    public class StageData
    {
        [SerializeField] private string stageName = "New Stage";
        [SerializeField] [Tooltip("Ground sprite for this stage's terrain.")]
        private Sprite terrain;
        [SerializeField] private List<LevelData> levels = new List<LevelData>();

        public string StageName => stageName;
        public Sprite Terrain => terrain;
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
        [SerializeField] [Tooltip("Delay in seconds before this wave spawns (Wave 1 is always 0).")]
        private float spawnDelay;
        [SerializeField] private List<EnemySpawnData> enemies = new List<EnemySpawnData>();

        public string WaveName => waveName;
        public float SpawnDelay => spawnDelay;
        public List<EnemySpawnData> Enemies => enemies;
    }

    [Serializable]
    public class EnemySpawnData
    {
        [SerializeField] private string enemyName = "Enemy";
        [Tooltip("Enemy prefab — must have Rigidbody2D root + Animator child")]
        [SerializeField] private GameObject prefab;
        [SerializeField] private int hp = 50;
        [SerializeField] private int atk = 10;
        [SerializeField] private float attackSpeed = 1f;
        [SerializeField] private float moveSpeed = 2f;
        [SerializeField] private float attackRange = 0.5f;
        [SerializeField] private AttackType attackType = AttackType.Melee;
        [SerializeField] private Vector2 spawnOffset;

        public string EnemyName => enemyName;
        public GameObject Prefab => prefab;
        public int Hp => hp;
        public int Atk => atk;
        public float AttackSpeed => attackSpeed;
        public float MoveSpeed => moveSpeed;
        public float AttackRange => attackRange;
        public AttackType AttackType => attackType;
        public Vector2 SpawnOffset => spawnOffset;
    }
}
