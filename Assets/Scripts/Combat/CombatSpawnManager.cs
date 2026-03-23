using UnityEngine;

namespace RogueliteAutoBattler.Combat
{
    /// <summary>
    /// Spawns an ally and an enemy from a shared character prefab at the start of combat,
    /// then wires a <see cref="CombatController"/> on each so they walk toward and attack each other.
    /// </summary>
    public class CombatSpawnManager : MonoBehaviour
    {
        [Header("Prefab")]
        [Tooltip("Character prefab used for both ally and enemy.")]
        [SerializeField] private GameObject _characterPrefab;

        [Header("Spawn Positions")]
        // Spawn X values are relative to world origin (camera centered at x=0).
        // Defaults assume portrait 9:16 layout: orthographic size 5.4, visible half-width ~3.0.
        [Tooltip("World X position where the ally spawns.")]
        [SerializeField] private float _allySpawnX = -1f;

        [Tooltip("World X position where the enemy spawns.")]
        [SerializeField] private float _enemySpawnX = 1f;

        [Tooltip("World Y position for both spawns.")]
        [SerializeField] private float _spawnY = 0f;

        [Header("Containers")]
        [Tooltip("Parent transform for ally instances. Auto-resolved to a child named 'Team' if null.")]
        [SerializeField] private Transform _teamContainer;

        [Tooltip("Parent transform for enemy instances. Auto-resolved to a child named 'Enemies' if null.")]
        [SerializeField] private Transform _enemiesContainer;

        // Container names are shared with CombatWorldBuilder, which creates the matching
        // child GameObjects. Keep these in sync with the hierarchy structure.
        public const string TeamContainerName = "Team";
        public const string EnemiesContainerName = "Enemies";

        private const string AllyName = "Warrior";
        private const string EnemyName = "Enemy";

        // The default sprite faces left. The ally needs to be flipped to face right
        // (toward the enemy). Negating X on the root flips the entire multi-sprite rig.
        private static readonly Vector3 FacingRightScale = new Vector3(-1f, 1f, 1f);

        /// <summary>The spawned ally GameObject.</summary>
        public GameObject AllyInstance { get; private set; }

        /// <summary>The spawned enemy GameObject.</summary>
        public GameObject EnemyInstance { get; private set; }

        private void Start()
        {
            if (_characterPrefab == null)
            {
                Debug.LogError($"[{nameof(CombatSpawnManager)}] No character prefab assigned!", this);
                return;
            }

            FindContainersIfNeeded();

            // Disable auto-scroll during combat — characters use their own movement.
            var scrollManager = GetComponent<CombatScrollManager>();
            if (scrollManager != null)
                scrollManager.enabled = false;

            AllyInstance = Instantiate(
                _characterPrefab,
                new Vector3(_allySpawnX, _spawnY, 0f),
                Quaternion.identity,
                _teamContainer
            );
            AllyInstance.name = AllyName;
            AllyInstance.transform.localScale = FacingRightScale;

            EnemyInstance = Instantiate(
                _characterPrefab,
                new Vector3(_enemySpawnX, _spawnY, 0f),
                Quaternion.identity,
                _enemiesContainer
            );
            EnemyInstance.name = EnemyName;

            var allyController = AllyInstance.AddComponent<CombatController>();
            allyController.Target = EnemyInstance.transform;

            var enemyController = EnemyInstance.AddComponent<CombatController>();
            enemyController.Target = AllyInstance.transform;
        }

        private void FindContainersIfNeeded()
        {
            if (_teamContainer == null)
                _teamContainer = transform.Find(TeamContainerName);

            if (_enemiesContainer == null)
                _enemiesContainer = transform.Find(EnemiesContainerName);

            if (_teamContainer == null)
                Debug.LogWarning($"[{nameof(CombatSpawnManager)}] '{TeamContainerName}' container not found as child!", this);

            if (_enemiesContainer == null)
                Debug.LogWarning($"[{nameof(CombatSpawnManager)}] '{EnemiesContainerName}' container not found as child!", this);
        }
    }
}
