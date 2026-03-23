using UnityEngine;

namespace RogueliteAutoBattler.Combat
{
    /// <summary>
    /// Spawns the ally character at the start of combat. Enemy spawning is handled
    /// by <see cref="LevelManager"/> via wave data.
    /// The prefab must have the Root/Visual hierarchy (Rigidbody2D on root, Animator on Visual child).
    /// </summary>
    public class CombatSpawnManager : MonoBehaviour
    {
        [Header("Prefab")]
        [Tooltip("Character prefab (Root with Rigidbody2D → Visual child with Animator).")]
        [SerializeField] private GameObject _characterPrefab;

        [Header("Spawn Positions")]
        [Tooltip("World Y position for ally spawn.")]
        [SerializeField] private float _spawnY = 0f;

        private const float AllyScreenRatio = 0.7f;

        [Header("Stats")]
        [Tooltip("CharacterStats asset for the ally.")]
        [SerializeField] private CharacterStats _allyStats;

        [Header("Containers")]
        [SerializeField] private Transform _teamContainer;
        [SerializeField] private Transform _enemiesContainer;

        public const string TeamContainerName = "Team";
        public const string EnemiesContainerName = "Enemies";

        private const string AllyName = "Warrior";
        public const string EnemyName = "Enemy";

        // The default sprite faces left. Flip X to face right.
        private static readonly Vector3 FacingRightScale = new Vector3(-1f, 1f, 1f);

        /// <summary>The spawned ally instance. Used by LevelManager to wire targets.</summary>
        public GameObject AllyInstance { get; private set; }

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

            // Compute ally spawn in screen-space, convert to CombatWorld local coords.
            var cam = Camera.main;
            float worldX = cam != null ? -(cam.orthographicSize * cam.aspect * AllyScreenRatio) : -2f;
            float localX = worldX - transform.position.x;

            AllyInstance = Instantiate(_characterPrefab, Vector3.zero, Quaternion.identity, _teamContainer);
            AllyInstance.transform.localPosition = new Vector3(localX, _spawnY, 0f);
            AllyInstance.name = AllyName;
            AllyInstance.transform.localScale = FacingRightScale;

            // Add components in dependency order: Stats first (CombatController reads it in Awake).
            InitializeStats(AllyInstance, _allyStats, AllyName);

            // HealthBar reads CombatStats in its Awake — must be added after InitializeStats.
            AllyInstance.AddComponent<HealthBar>();

            var allyMover = AllyInstance.AddComponent<CharacterMover>();
            if (_allyStats != null) allyMover.SetMoveSpeed(_allyStats.moveSpeed);

            var allyController = AllyInstance.AddComponent<CombatController>();
            WireAnimationRelay(AllyInstance, allyController);

            // Ally starts with no target — LevelManager will assign one when enemies spawn.
        }

        private void InitializeStats(GameObject character, CharacterStats stats, string label)
        {
            if (stats == null)
            {
                Debug.LogWarning($"[{nameof(CombatSpawnManager)}] No stats assigned for {label}!", this);
                return;
            }

            var combatStats = character.AddComponent<CombatStats>();
            combatStats.Initialize(stats);
        }

        private void WireAnimationRelay(GameObject character, CombatController controller)
        {
            var animator = character.GetComponentInChildren<Animator>();
            if (animator == null)
            {
                Debug.LogWarning($"[{nameof(CombatSpawnManager)}] No Animator found on {character.name} — AnimationEventRelay not added.");
                return;
            }
            var relay = animator.gameObject.AddComponent<AnimationEventRelay>();
            relay.Initialize(controller);
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
