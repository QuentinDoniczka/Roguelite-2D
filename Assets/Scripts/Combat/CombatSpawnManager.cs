using UnityEngine;

namespace RogueliteAutoBattler.Combat
{
    /// <summary>
    /// Spawns an ally and an enemy from a shared character prefab at the start of combat,
    /// then wires combat controllers so they walk toward and attack each other.
    /// The prefab must have the Root/Visual hierarchy (Rigidbody2D on root, Animator on Visual child).
    /// </summary>
    public class CombatSpawnManager : MonoBehaviour
    {
        [Header("Prefab")]
        [Tooltip("Character prefab (Root with Rigidbody2D → Visual child with Animator).")]
        [SerializeField] private GameObject _characterPrefab;

        [Header("Spawn Positions")]
        [Tooltip("World X position where the ally spawns.")]
        [SerializeField] private float _allySpawnX = -1f;

        [Tooltip("World X position where the enemy spawns.")]
        [SerializeField] private float _enemySpawnX = 1f;

        [Tooltip("World Y position for both spawns.")]
        [SerializeField] private float _spawnY = 0f;

        [Header("Stats")]
        [Tooltip("Default CharacterStats asset applied to both ally and enemy at spawn.")]
        [SerializeField] private CharacterStats _defaultStats;

        [Header("Containers")]
        [SerializeField] private Transform _teamContainer;
        [SerializeField] private Transform _enemiesContainer;

        public const string TeamContainerName = "Team";
        public const string EnemiesContainerName = "Enemies";

        private const string AllyName = "Warrior";
        private const string EnemyName = "Enemy";

        // The default sprite faces left. Flip X to face right.
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

            // Spawn — prefab already has Root (Rigidbody2D) → Visual (Animator) hierarchy.
            AllyInstance = Instantiate(_characterPrefab, new Vector3(_allySpawnX, _spawnY, 0f), Quaternion.identity, _teamContainer);
            AllyInstance.name = AllyName;
            AllyInstance.transform.localScale = FacingRightScale;

            EnemyInstance = Instantiate(_characterPrefab, new Vector3(_enemySpawnX, _spawnY, 0f), Quaternion.identity, _enemiesContainer);
            EnemyInstance.name = EnemyName;

            // Add combat components to root (which already has Rigidbody2D from the prefab).
            var allyMover = AllyInstance.AddComponent<CharacterMover>();
            var enemyMover = EnemyInstance.AddComponent<CharacterMover>();

            AllyInstance.AddComponent<CombatController>();
            EnemyInstance.AddComponent<CombatController>();

            // Initialize stats before wiring targets so CombatController can read them immediately.
            if (_defaultStats != null)
            {
                var allyStats = AllyInstance.AddComponent<CombatStats>();
                allyStats.Initialize(_defaultStats);
                allyMover.SetMoveSpeed(_defaultStats.moveSpeed);

                var enemyStats = EnemyInstance.AddComponent<CombatStats>();
                enemyStats.Initialize(_defaultStats);
                enemyMover.SetMoveSpeed(_defaultStats.moveSpeed);
            }
            else
            {
                Debug.LogWarning($"[{nameof(CombatSpawnManager)}] No CharacterStats assigned — characters will have no stats!", this);
            }

            // Wire targets after both exist.
            allyMover.Target = EnemyInstance.transform;
            enemyMover.Target = AllyInstance.transform;
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
