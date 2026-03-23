using UnityEngine;

namespace RogueliteAutoBattler.Combat
{
    /// <summary>
    /// Spawns an ally and an enemy from a shared character prefab at the start of combat,
    /// then wires combat controllers so they walk toward and attack each other.
    /// </summary>
    public class CombatSpawnManager : MonoBehaviour
    {
        [Header("Prefab")]
        [Tooltip("Character prefab used for both ally and enemy.")]
        [SerializeField] private GameObject _characterPrefab;

        [Header("Spawn Positions")]
        [Tooltip("World X position where the ally spawns.")]
        [SerializeField] private float _allySpawnX = -1f;

        [Tooltip("World X position where the enemy spawns.")]
        [SerializeField] private float _enemySpawnX = 1f;

        [Tooltip("World Y position for both spawns.")]
        [SerializeField] private float _spawnY = 0f;

        [Header("Containers")]
        [SerializeField] private Transform _teamContainer;
        [SerializeField] private Transform _enemiesContainer;

        public const string TeamContainerName = "Team";
        public const string EnemiesContainerName = "Enemies";

        private const string AllyName = "Warrior";
        private const string EnemyName = "Enemy";
        private static readonly Vector3 FacingRightScale = new Vector3(-1f, 1f, 1f);

        public GameObject AllyInstance { get; private set; }
        public GameObject EnemyInstance { get; private set; }

        private void Start()
        {
            if (_characterPrefab == null)
            {
                Debug.LogError($"[{nameof(CombatSpawnManager)}] No character prefab assigned!", this);
                return;
            }

            FindContainersIfNeeded();

            // Disable auto-scroll during combat.
            var scrollManager = GetComponent<CombatScrollManager>();
            if (scrollManager != null)
                scrollManager.enabled = false;

            // Spawn ally
            AllyInstance = Instantiate(_characterPrefab, new Vector3(_allySpawnX, _spawnY, 0f), Quaternion.identity, _teamContainer);
            AllyInstance.name = AllyName;
            AllyInstance.transform.localScale = FacingRightScale;

            // Spawn enemy
            EnemyInstance = Instantiate(_characterPrefab, new Vector3(_enemySpawnX, _spawnY, 0f), Quaternion.identity, _enemiesContainer);
            EnemyInstance.name = EnemyName;

            // Add components manually — no RequireComponent chain
            SetupCombatCharacter(AllyInstance, EnemyInstance.transform);
            SetupCombatCharacter(EnemyInstance, AllyInstance.transform);
        }

        private void SetupCombatCharacter(GameObject character, Transform target)
        {
            // Add Rigidbody2D first
            var rb = character.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Discrete;

            // Add CharacterMover
            var mover = character.AddComponent<CharacterMover>();
            mover.Target = target;

            // Add CombatController
            character.AddComponent<CombatController>();

            Debug.Log($"[CombatSpawnManager] Setup {character.name}: rb.simulated={rb.simulated}, rb.bodyType={rb.bodyType}, target={target.name}");
        }

        private void FindContainersIfNeeded()
        {
            if (_teamContainer == null)
                _teamContainer = transform.Find(TeamContainerName);
            if (_enemiesContainer == null)
                _enemiesContainer = transform.Find(EnemiesContainerName);

            if (_teamContainer == null)
                Debug.LogWarning($"[{nameof(CombatSpawnManager)}] '{TeamContainerName}' container not found!");
            if (_enemiesContainer == null)
                Debug.LogWarning($"[{nameof(CombatSpawnManager)}] '{EnemiesContainerName}' container not found!");
        }
    }
}
