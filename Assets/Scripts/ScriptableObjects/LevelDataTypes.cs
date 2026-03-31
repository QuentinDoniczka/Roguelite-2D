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

        public string StageName { get => stageName; internal set => stageName = value; }
        public Sprite Terrain { get => terrain; internal set => terrain = value; }
        public List<LevelData> Levels { get => levels; internal set => levels = value; }

        private StageData() { }

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
        [SerializeField] private List<StepData> steps = new List<StepData>();

        public string LevelName { get => levelName; internal set => levelName = value; }
        public List<StepData> Steps { get => steps; internal set => steps = value; }

        private LevelData() { }

        internal LevelData(string levelName, List<StepData> steps)
        {
            this.levelName = levelName;
            this.steps = steps;
        }
    }

    [Serializable]
    public class StepData
    {
        [SerializeField] private string stepName = "New Step";
        [SerializeField] private List<WaveData> waves = new List<WaveData>();

        public string StepName { get => stepName; internal set => stepName = value; }
        public List<WaveData> Waves { get => waves; internal set => waves = value; }

        private StepData() { }

        internal StepData(string stepName, List<WaveData> waves)
        {
            this.stepName = stepName;
            this.waves = waves;
        }
    }

    [Serializable]
    public class WaveData
    {
        [SerializeField] private string waveName = "Wave";
        [SerializeField] private float spawnDelay;
        [SerializeField] private List<EnemySpawnData> enemies = new List<EnemySpawnData>();

        public string WaveName { get => waveName; internal set => waveName = value; }
        public float SpawnDelay { get => spawnDelay; internal set => spawnDelay = value; }
        public List<EnemySpawnData> Enemies { get => enemies; internal set => enemies = value; }

        private WaveData() { }

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

        public string EnemyName { get => enemyName; internal set => enemyName = value; }
        public GameObject Prefab { get => prefab; internal set => prefab = value; }
        public int Hp { get => hp; internal set => hp = value; }
        public int Atk { get => atk; internal set => atk = value; }
        public float AttackSpeed { get => attackSpeed; internal set => attackSpeed = value; }
        public float MoveSpeed { get => moveSpeed; internal set => moveSpeed = value; }
        public float AttackRange { get => attackRange; internal set => attackRange = value; }
        public AttackType AttackType { get => attackType; internal set => attackType = value; }
        public float ColliderRadius { get => colliderRadius; internal set => colliderRadius = value; }
        public int GoldDrop { get => goldDrop; internal set => goldDrop = value; }
        public AppearanceData Appearance => appearance;

        private EnemySpawnData() { }

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

        public string AllyName { get => allyName; internal set => allyName = value; }
        public GameObject Prefab { get => prefab; internal set => prefab = value; }
        public int MaxHp { get => maxHp; internal set => maxHp = value; }
        public int Atk { get => atk; internal set => atk = value; }
        public float AttackSpeed { get => attackSpeed; internal set => attackSpeed = value; }
        public float MoveSpeed { get => moveSpeed; internal set => moveSpeed = value; }
        public float RegenHpPerSecond { get => regenHpPerSecond; internal set => regenHpPerSecond = value; }
        public float ColliderRadius { get => colliderRadius; internal set => colliderRadius = value; }
        public AppearanceData Appearance => appearance;
    }
}
