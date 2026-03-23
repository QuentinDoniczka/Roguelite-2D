using System.Collections;
using RogueliteAutoBattler.Data;
using UnityEngine;

namespace RogueliteAutoBattler.Combat
{
    /// <summary>
    /// Reads the LevelDatabase at runtime, applies the current stage's terrain sprite,
    /// and spawns enemy waves for the current level. Attach to the CombatWorld root.
    /// </summary>
    public class LevelManager : MonoBehaviour
    {
        [Header("Database")]
        [SerializeField] private LevelDatabase _levelDatabase;

        [Header("Terrain")]
        [SerializeField] private SpriteRenderer _groundRenderer;

        [Header("Stage / Level")]
        [SerializeField] private int _currentStageIndex;
        [SerializeField] private int _currentLevelIndex;

        [Header("Containers")]
        [Tooltip("Parent transform for spawned enemies.")]
        [SerializeField] private Transform _enemiesContainer;

        [Tooltip("Parent transform for the ally team (used to find targets for enemies).")]
        [SerializeField] private Transform _teamContainer;

        private const float BaseEnemySpawnX = 1f;

        private void Start()
        {
            FindContainersIfNeeded();
            ApplyStage(_currentStageIndex);
            StartLevel(_currentLevelIndex);
        }

        /// <summary>
        /// Applies the terrain sprite for the given stage index.
        /// </summary>
        public void ApplyStage(int stageIndex)
        {
            if (_levelDatabase == null)
            {
                Debug.LogWarning($"[{nameof(LevelManager)}] No LevelDatabase assigned.");
                return;
            }

            if (_levelDatabase.Stages == null || stageIndex < 0 || stageIndex >= _levelDatabase.Stages.Count)
            {
                Debug.LogWarning($"[{nameof(LevelManager)}] Stage index {stageIndex} out of range.");
                return;
            }

            var stage = _levelDatabase.Stages[stageIndex];
            _currentStageIndex = stageIndex;

            if (stage.Terrain != null && _groundRenderer != null)
            {
                _groundRenderer.sprite = stage.Terrain;
                Debug.Log($"[{nameof(LevelManager)}] Applied terrain '{stage.Terrain.name}' for stage '{stage.StageName}'");
            }
        }

        /// <summary>
        /// Starts spawning enemy waves for the given level index within the current stage.
        /// </summary>
        public void StartLevel(int levelIndex)
        {
            if (_levelDatabase == null)
            {
                Debug.LogWarning($"[{nameof(LevelManager)}] No LevelDatabase assigned.");
                return;
            }

            var stages = _levelDatabase.Stages;
            if (stages == null || _currentStageIndex < 0 || _currentStageIndex >= stages.Count)
            {
                Debug.LogWarning($"[{nameof(LevelManager)}] Current stage index {_currentStageIndex} out of range.");
                return;
            }

            var stage = stages[_currentStageIndex];
            if (stage.Levels == null || levelIndex < 0 || levelIndex >= stage.Levels.Count)
            {
                Debug.LogWarning($"[{nameof(LevelManager)}] Level index {levelIndex} out of range for stage '{stage.StageName}'.");
                return;
            }

            _currentLevelIndex = levelIndex;
            var level = stage.Levels[levelIndex];
            Debug.Log($"[{nameof(LevelManager)}] Starting level '{level.LevelName}' with {level.Waves.Count} wave(s).");

            for (int i = 0; i < level.Waves.Count; i++)
            {
                StartCoroutine(SpawnWaveCoroutine(level.Waves[i], i));
            }
        }

        private IEnumerator SpawnWaveCoroutine(WaveData wave, int waveIndex)
        {
            if (wave.SpawnDelay > 0f)
            {
                Debug.Log($"[{nameof(LevelManager)}] Wave {waveIndex} '{wave.WaveName}' waiting {wave.SpawnDelay}s...");
                yield return new WaitForSeconds(wave.SpawnDelay);
            }

            Debug.Log($"[{nameof(LevelManager)}] Spawning wave {waveIndex} '{wave.WaveName}' ({wave.Enemies.Count} enemies).");

            foreach (var enemyData in wave.Enemies)
            {
                SpawnEnemy(enemyData);
            }
        }

        private void SpawnEnemy(EnemySpawnData data)
        {
            if (data.Prefab == null)
            {
                Debug.LogWarning($"[{nameof(LevelManager)}] EnemySpawnData '{data.EnemyName}' has no prefab assigned.");
                return;
            }

            Vector3 spawnPosition = new Vector3(
                BaseEnemySpawnX + data.SpawnOffset.x,
                data.SpawnOffset.y,
                0f
            );

            GameObject enemy = Instantiate(data.Prefab, spawnPosition, Quaternion.identity, _enemiesContainer);
            enemy.name = data.EnemyName;

            // CombatStats — direct initialization from EnemySpawnData values (no SO needed).
            var combatStats = enemy.AddComponent<CombatStats>();
            combatStats.InitializeDirect(data.Hp, data.Atk, data.AttackSpeed);

            // HealthBar — must be added after CombatStats (reads it in Awake).
            enemy.AddComponent<HealthBar>();

            // CharacterMover — set speed and target to the first alive ally.
            var mover = enemy.AddComponent<CharacterMover>();
            mover.SetMoveSpeed(data.MoveSpeed);

            Transform allyTarget = FindFirstAliveAlly();
            if (allyTarget != null)
                mover.Target = allyTarget;
            else
                Debug.LogWarning($"[{nameof(LevelManager)}] No alive ally found for enemy '{data.EnemyName}' to target.");

            // CombatController — set attack range from EnemySpawnData.
            var controller = enemy.AddComponent<CombatController>();
            controller.SetAttackRange(data.AttackRange);

            // AnimationEventRelay — wire animation events to the controller.
            WireAnimationRelay(enemy, controller);

            Debug.Log($"[{nameof(LevelManager)}] Spawned enemy '{data.EnemyName}' at {spawnPosition}");
        }

        /// <summary>
        /// Finds the first alive ally in the team container (has CombatStats and is not dead).
        /// </summary>
        private Transform FindFirstAliveAlly()
        {
            if (_teamContainer == null)
                return null;

            for (int i = 0; i < _teamContainer.childCount; i++)
            {
                var child = _teamContainer.GetChild(i);
                if (child.TryGetComponent<CombatStats>(out var stats) && !stats.IsDead)
                    return child;
            }

            return null;
        }

        private void WireAnimationRelay(GameObject character, CombatController controller)
        {
            var animator = character.GetComponentInChildren<Animator>();
            if (animator == null)
            {
                Debug.LogWarning($"[{nameof(LevelManager)}] No Animator found on {character.name} — AnimationEventRelay not added.");
                return;
            }

            var relay = animator.gameObject.AddComponent<AnimationEventRelay>();
            relay.Initialize(controller);
        }

        private void FindContainersIfNeeded()
        {
            if (_teamContainer == null)
                _teamContainer = transform.Find(CombatSpawnManager.TeamContainerName);
            if (_enemiesContainer == null)
                _enemiesContainer = transform.Find(CombatSpawnManager.EnemiesContainerName);

            if (_teamContainer == null)
                Debug.LogWarning($"[{nameof(LevelManager)}] '{CombatSpawnManager.TeamContainerName}' container not found!");
            if (_enemiesContainer == null)
                Debug.LogWarning($"[{nameof(LevelManager)}] '{CombatSpawnManager.EnemiesContainerName}' container not found!");
        }
    }
}
