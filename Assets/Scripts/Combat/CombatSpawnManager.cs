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

            // Spawn ally — root faces right (flipped scale), Visual child is unscaled.
            AllyInstance = SpawnCharacter(AllyName, _allySpawnX, _teamContainer, facingRight: true);

            // Spawn enemy — faces left by default (no flip needed).
            EnemyInstance = SpawnCharacter(EnemyName, _enemySpawnX, _enemiesContainer, facingRight: false);

            // Wire combat targets after both roots exist.
            SetupCombatCharacter(AllyInstance, EnemyInstance.transform);
            SetupCombatCharacter(EnemyInstance, AllyInstance.transform);
        }

        /// <summary>
        /// Instantiates the prefab and wraps it under a new root so that the Animator
        /// (on the Visual child) cannot conflict with the Rigidbody2D (on the root).
        ///
        /// Resulting hierarchy:
        ///   Root [Rigidbody2D, CharacterMover, CombatController]
        ///   └── Visual  (= original prefab instance, keeps SpriteRenderer + Animator)
        /// </summary>
        private GameObject SpawnCharacter(string characterName, float spawnX, Transform container, bool facingRight)
        {
            // Create the physics root at the desired world position.
            var root = new GameObject(characterName);
            root.transform.SetParent(container, worldPositionStays: false);
            root.transform.position = new Vector3(spawnX, _spawnY, 0f);

            if (facingRight)
                root.transform.localScale = FacingRightScale;

            // Instantiate the prefab as the Visual child.
            var visual = Instantiate(_characterPrefab, root.transform);
            visual.name = "Visual";
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.identity;
            visual.transform.localScale = Vector3.one;

            return root;
        }

        private void SetupCombatCharacter(GameObject root, Transform target)
        {
            // Rigidbody2D lives on the root — no Animator conflict.
            var rb = root.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Discrete;

            // CharacterMover and CombatController also on the root.
            var mover = root.AddComponent<CharacterMover>();
            mover.Target = target;

            root.AddComponent<CombatController>();

            Debug.Log($"[CombatSpawnManager] Setup {root.name}: rb.simulated={rb.simulated}, rb.bodyType={rb.bodyType}, target={target.name}");
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
