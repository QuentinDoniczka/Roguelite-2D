using UnityEngine;

namespace RogueliteAutoBattler.Combat
{
    /// <summary>
    /// Spawns an ally and an enemy from a shared character prefab at the start of combat,
    /// then wires a <see cref="CharacterMover"/> on the ally so it walks toward the enemy.
    /// </summary>
    public class CombatSpawnManager : MonoBehaviour
    {
        [Header("Prefab")]
        [Tooltip("Character prefab used for both ally and enemy.")]
        [SerializeField] private GameObject _characterPrefab;

        [Header("Spawn Positions")]
        // Spawn X values are relative to world origin (camera centered at x=0).
        // Defaults assume portrait 9:16 layout: orthographic size 5.4, visible half-width ~3.0.
        // At -3 and +3 the characters are near the screen edges but fully visible.
        [Tooltip("World X position where the ally spawns. Default -3 places it near the left edge of a portrait 9:16 viewport (ortho size 5.4, half-width ~3.0).")]
        [SerializeField] private float _allySpawnX = -3f;

        [Tooltip("World X position where the enemy spawns. Default +3 places it near the right edge of a portrait 9:16 viewport (ortho size 5.4, half-width ~3.0).")]
        [SerializeField] private float _enemySpawnX = 3f;

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

        // Enemies face left (toward the ally). Negating X on the root flips the
        // entire multi-sprite rig. Collider2D and Physics2D handle negative scale correctly.
        private static readonly Vector3 FacingLeftScale = new Vector3(-1f, 1f, 1f);

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

            AllyInstance = Instantiate(
                _characterPrefab,
                new Vector3(_allySpawnX, _spawnY, 0f),
                Quaternion.identity,
                _teamContainer
            );
            AllyInstance.name = AllyName;

            EnemyInstance = Instantiate(
                _characterPrefab,
                new Vector3(_enemySpawnX, _spawnY, 0f),
                Quaternion.identity,
                _enemiesContainer
            );
            EnemyInstance.name = EnemyName;
            EnemyInstance.transform.localScale = FacingLeftScale;

            var mover = AllyInstance.AddComponent<CharacterMover>();
            mover.Target = EnemyInstance.transform;
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
