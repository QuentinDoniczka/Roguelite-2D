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
    public class AppearanceData
    {
        [SerializeField] private Sprite headSprite;
        [SerializeField] private Sprite hatSprite;
        [SerializeField] private Sprite weaponSprite;
        [SerializeField] private Sprite shieldSprite;

        public Sprite HeadSprite => headSprite;
        public Sprite HatSprite => hatSprite;
        public Sprite WeaponSprite => weaponSprite;
        public Sprite ShieldSprite => shieldSprite;
    }

    [Serializable]
    public class StageData
    {
        [SerializeField] private string stageName = "New Stage";
        [SerializeField] private Sprite terrain;
        [SerializeField] private List<LevelData> levels = new List<LevelData>();

        public string StageName => stageName;
        public Sprite Terrain => terrain;
        public List<LevelData> Levels => levels;

        public StageData() { }

        internal StageData(string stageName, Sprite terrain, List<LevelData> levels)
        {
            this.stageName = stageName;
            this.terrain = terrain;
            this.levels = levels;
        }
    }

    [Serializable]
    public class LevelData
    {
        [SerializeField] private string levelName = "New Level";
        [SerializeField] private List<WaveData> waves = new List<WaveData>();

        public string LevelName => levelName;
        public List<WaveData> Waves => waves;

        public LevelData() { }

        internal LevelData(string levelName, List<WaveData> waves)
        {
            this.levelName = levelName;
            this.waves = waves;
        }
    }

    [Serializable]
    public class WaveData
    {
        [SerializeField] private string waveName = "Wave";
        [SerializeField] private float spawnDelay;
        [SerializeField] private List<EnemySpawnData> enemies = new List<EnemySpawnData>();

        public string WaveName => waveName;
        public float SpawnDelay => spawnDelay;
        public List<EnemySpawnData> Enemies => enemies;

        public WaveData() { }

        internal WaveData(string waveName, float spawnDelay, List<EnemySpawnData> enemies)
        {
            this.waveName = waveName;
            this.spawnDelay = spawnDelay;
            this.enemies = enemies;
        }
    }

    [Serializable]
    public class EnemySpawnData
    {
        [SerializeField] private string enemyName = "Enemy";
        [SerializeField] private GameObject prefab;
        [SerializeField] private int hp = 50;
        [SerializeField] private int atk = 10;
        [SerializeField] private float attackSpeed = 1f;
        [SerializeField] private float moveSpeed = 2f;
        [SerializeField] private float attackRange = 0.5f;
        [SerializeField] private AttackType attackType = AttackType.Melee;
        [SerializeField] private float colliderRadius = 0.10f;
        [SerializeField] private int goldDrop;

        [Header("Appearance")]
        [SerializeField] private AppearanceData appearance = new AppearanceData();

        public string EnemyName => enemyName;
        public GameObject Prefab => prefab;
        public int Hp => hp;
        public int Atk => atk;
        public float AttackSpeed => attackSpeed;
        public float MoveSpeed => moveSpeed;
        public float AttackRange => attackRange;
        public AttackType AttackType => attackType;
        public float ColliderRadius => colliderRadius;
        public int GoldDrop => goldDrop;
        public AppearanceData Appearance => appearance;

        public EnemySpawnData() { }

        internal EnemySpawnData(string enemyName, int hp, int atk)
        {
            this.enemyName = enemyName;
            this.hp = hp;
            this.atk = atk;
        }
    }

    [Serializable]
    public class AllySpawnData
    {
        [SerializeField] private string allyName = "Warrior";
        [SerializeField] private GameObject prefab;
        [SerializeField] private int maxHp = 100;
        [SerializeField] private int atk = 10;
        [SerializeField] private float attackSpeed = 1f;
        [SerializeField] private float moveSpeed = 2f;
        [SerializeField] private float regenHpPerSecond = 0f;
        [SerializeField] private float colliderRadius = 0.10f;

        [Header("Appearance")]
        [SerializeField] private AppearanceData appearance = new AppearanceData();

        public string AllyName => allyName;
        public GameObject Prefab => prefab;
        public int MaxHp => maxHp;
        public int Atk => atk;
        public float AttackSpeed => attackSpeed;
        public float MoveSpeed => moveSpeed;
        public float RegenHpPerSecond => regenHpPerSecond;
        public float ColliderRadius => colliderRadius;
        public AppearanceData Appearance => appearance;
    }
}
