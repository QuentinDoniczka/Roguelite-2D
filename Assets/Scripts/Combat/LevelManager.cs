using System.Collections;
using RogueliteAutoBattler.Data;
using UnityEngine;

namespace RogueliteAutoBattler.Combat
{
    /// <summary>
    /// Reads the LevelDatabase at runtime, applies the current stage's terrain sprite,
    /// and spawns enemy waves for the current level. Attach to the CombatWorld root.
    /// </summary>
    [RequireComponent(typeof(WorldConveyor))]
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

        [Header("Scroll Transition")]
        [Tooltip("Distance in world units the world scrolls left between levels.")]
        [SerializeField] private float _scrollDistance = 2f;

        [Header("Enemy Spawn")]
        [Tooltip("Extra X offset to spawn enemies off-screen to the right of EnemiesHomeAnchor.")]
        [SerializeField] private float _enemySpawnOffscreenX = 1f;

        [Header("Anchors")]
        [SerializeField] private Transform _enemiesHomeAnchor;
        [SerializeField] private Transform _combatTriggerZone;

        private const float FallbackEnemySpawnX = 1f;

        private int _aliveEnemyCount;
        private int _pendingWaveCount;
        private bool _levelInProgress;
        private WorldConveyor _conveyor;

        private float CombatZoneX => _combatTriggerZone != null ? _combatTriggerZone.position.x : float.MaxValue;

        private void FixedUpdate()
        {
            if (!_levelInProgress) return;
            AssignAllyTargetsInZone();
        }

        private IEnumerator Start()
        {
            CombatSetupHelper.FindContainersIfNeeded(transform, ref _teamContainer, ref _enemiesContainer, nameof(LevelManager));
            _conveyor = GetComponent<WorldConveyor>();
            ApplyStage(_currentStageIndex);
            // Wait until an ally actually exists in the team container.
            yield return new WaitUntil(() => TargetFinder.Closest(_teamContainer, Vector3.zero) != null);
            StartLevel(_currentLevelIndex);
        }

        /// <summary>
        /// Applies the terrain sprite for the given stage index.
        /// </summary>
        public void ApplyStage(int stageIndex)
        {
            if (_levelDatabase == null)
            {
#if UNITY_EDITOR
                Debug.LogWarning($"[{nameof(LevelManager)}] No LevelDatabase assigned.");
#endif
                return;
            }

            if (_levelDatabase.Stages == null || stageIndex < 0 || stageIndex >= _levelDatabase.Stages.Count)
            {
#if UNITY_EDITOR
                Debug.LogWarning($"[{nameof(LevelManager)}] Stage index {stageIndex} out of range.");
#endif
                return;
            }

            var stage = _levelDatabase.Stages[stageIndex];
            _currentStageIndex = stageIndex;

            if (stage.Terrain != null && _groundRenderer != null)
            {
                _groundRenderer.sprite = stage.Terrain;
#if UNITY_EDITOR
                Debug.Log($"[{nameof(LevelManager)}] Applied terrain '{stage.Terrain.name}' for stage '{stage.StageName}'");
#endif
            }
        }

        /// <summary>
        /// Starts spawning enemy waves for the given level index within the current stage.
        /// </summary>
        public void StartLevel(int levelIndex)
        {
            if (_levelDatabase == null)
            {
#if UNITY_EDITOR
                Debug.LogWarning($"[{nameof(LevelManager)}] No LevelDatabase assigned.");
#endif
                return;
            }

            var stages = _levelDatabase.Stages;
            if (stages == null || _currentStageIndex < 0 || _currentStageIndex >= stages.Count)
            {
#if UNITY_EDITOR
                Debug.LogWarning($"[{nameof(LevelManager)}] Current stage index {_currentStageIndex} out of range.");
#endif
                return;
            }

            var stage = stages[_currentStageIndex];
            if (stage.Levels == null || levelIndex < 0 || levelIndex >= stage.Levels.Count)
            {
#if UNITY_EDITOR
                Debug.LogWarning($"[{nameof(LevelManager)}] Level index {levelIndex} out of range for stage '{stage.StageName}'.");
#endif
                return;
            }

            _currentLevelIndex = levelIndex;
            _aliveEnemyCount = 0;
            _levelInProgress = true;

            var level = stage.Levels[levelIndex];
            _pendingWaveCount = level.Waves.Count;
#if UNITY_EDITOR
            Debug.Log($"[{nameof(LevelManager)}] Starting level '{level.LevelName}' with {level.Waves.Count} wave(s).");
#endif

            for (int i = 0; i < level.Waves.Count; i++)
            {
                StartCoroutine(SpawnWaveCoroutine(level.Waves[i], i));
            }
        }

        private IEnumerator SpawnWaveCoroutine(WaveData wave, int waveIndex)
        {
            if (wave.SpawnDelay > 0f)
            {
#if UNITY_EDITOR
                Debug.Log($"[{nameof(LevelManager)}] Wave {waveIndex} '{wave.WaveName}' waiting {wave.SpawnDelay}s...");
#endif
                yield return new WaitForSeconds(wave.SpawnDelay);
            }

#if UNITY_EDITOR
            Debug.Log($"[{nameof(LevelManager)}] Spawning wave {waveIndex} '{wave.WaveName}' ({wave.Enemies.Count} enemies).");
#endif

            // Calculate formation positions for this wave's enemies.
            Vector2 anchorPos = _enemiesHomeAnchor != null
                ? (Vector2)_enemiesHomeAnchor.position
                : new Vector2(FallbackEnemySpawnX, 0f);
            Vector2 spawnAnchor = new Vector2(anchorPos.x + _enemySpawnOffscreenX, anchorPos.y);
            Vector2[] positions = FormationLayout.GetPositions(spawnAnchor, wave.Enemies.Count, facingRight: false);

            for (int i = 0; i < wave.Enemies.Count; i++)
            {
                Vector2 offset = positions[i] - anchorPos;
                SpawnEnemy(wave.Enemies[i], positions[i], offset);
            }

            _pendingWaveCount--;
            CheckLevelComplete();
        }

        private void SpawnEnemy(EnemySpawnData data, Vector2 spawnPos, Vector2 homeOffset)
        {
            if (data.Prefab == null)
            {
#if UNITY_EDITOR
                Debug.LogWarning($"[{nameof(LevelManager)}] EnemySpawnData '{data.EnemyName}' has no prefab assigned.");
#endif
                return;
            }

            Vector3 spawnPosition = new Vector3(spawnPos.x, spawnPos.y, 0f);

            GameObject enemy = Instantiate(data.Prefab, spawnPosition, Quaternion.identity, _enemiesContainer);
            enemy.name = data.EnemyName;

            var components = CombatSetupHelper.AssembleCharacter(
                enemy,
                data.Hp,
                data.Atk,
                data.AttackSpeed,
                0f,
                data.MoveSpeed,
                _enemiesHomeAnchor,
                homeOffset,
                data.ColliderRadius,
                data.Appearance,
                nameof(LevelManager));

            // Track enemy death for level progression.
            _aliveEnemyCount++;
            components.Stats.OnDied += OnEnemyDied;

            var enemyTransform = enemy.transform;

            // CombatController — set attack range, wire retarget delegate with closure on position.
            components.Controller.SetAttackRange(data.AttackRange);
            components.Controller.FindNewTarget = () => TargetFinder.Closest(_teamContainer, enemyTransform.position);

            // Set target through CombatController.Target so OnDied subscription is wired.
            Transform allyTarget = TargetFinder.Closest(_teamContainer, enemyTransform.position);
            if (allyTarget != null)
                components.Controller.Target = allyTarget;
#if UNITY_EDITOR
            else
                Debug.LogWarning($"[{nameof(LevelManager)}] No alive ally found for enemy '{data.EnemyName}' to target.");
#endif

            // Wire ally to target this enemy if it has no target yet.
            SetAllyTarget(enemy.transform);

#if UNITY_EDITOR
            Debug.Log($"[{nameof(LevelManager)}] Spawned enemy '{data.EnemyName}' at {spawnPosition}");
#endif
        }

        /// <summary>
        /// Sets the ally's target to the given enemy if the ally currently has no target
        /// or its current target is dead. Also wires the retarget delegate once.
        /// </summary>
        private void SetAllyTarget(Transform firstEnemy)
        {
            if (_teamContainer == null)
                return;

            for (int i = 0; i < _teamContainer.childCount; i++)
            {
                var allyTransform = _teamContainer.GetChild(i);
                if (!allyTransform.TryGetComponent<CombatStats>(out var allyStats) || allyStats.IsDead)
                    continue;

                if (!allyTransform.TryGetComponent<CharacterMover>(out var allyMover))
                    continue;

                // Assign target if ally has none or current target is dead
                bool needsTarget = allyMover.Target == null;
                if (!needsTarget && allyMover.Target.TryGetComponent<CombatStats>(out var targetStats))
                    needsTarget = targetStats.IsDead;

                if (needsTarget)
                {
                    // Only target this enemy if it's inside the combat zone.
                    // Use CombatController.Target so OnDied subscription is wired.
                    bool inZone = firstEnemy.position.x <= CombatZoneX;
                    if (inZone && allyTransform.TryGetComponent<CombatController>(out var allyController))
                        allyController.Target = firstEnemy;
                }

                WireAllyRetarget(allyTransform);
            }
        }

        private void OnEnemyDied()
        {
            _aliveEnemyCount--;
            CheckLevelComplete();
        }

        private void CheckLevelComplete()
        {
            if (_levelInProgress && _pendingWaveCount <= 0 && _aliveEnemyCount <= 0)
            {
                _levelInProgress = false;
                OnLevelComplete();
            }
        }

        private void OnLevelComplete()
        {
#if UNITY_EDITOR
            Debug.Log($"[{nameof(LevelManager)}] Level complete! Starting transition...");
#endif

            ClearAllyTargets();

            var stages = _levelDatabase.Stages;
            if (stages == null || _currentStageIndex < 0 || _currentStageIndex >= stages.Count)
            {
#if UNITY_EDITOR
                Debug.LogWarning($"[{nameof(LevelManager)}] Stage index {_currentStageIndex} out of range on level complete.");
#endif
                return;
            }

            var stage = stages[_currentStageIndex];
            _currentLevelIndex++;

            if (_currentLevelIndex < stage.Levels.Count)
            {
                StartCoroutine(LevelTransitionCoroutine());
            }
            else
            {
#if UNITY_EDITOR
                Debug.Log($"[{nameof(LevelManager)}] Stage '{stage.StageName}' complete!");
#endif
            }
        }

        private IEnumerator LevelTransitionCoroutine()
        {
            // Scroll starts immediately — allies walk back to HomeAnchor
            // while the world scrolls; the slow conveyor lets them catch up naturally.

            // Start scroll and spawn enemies when deceleration begins
            if (_conveyor != null)
            {
                bool enemiesSpawned = false;
                void OnDecel()
                {
                    if (enemiesSpawned) return;
                    enemiesSpawned = true;
#if UNITY_EDITOR
                    Debug.Log($"[{nameof(LevelManager)}] Deceleration started. Spawning level {_currentLevelIndex}.");
#endif
                    StartLevel(_currentLevelIndex);
                }

                _conveyor.OnDecelerationStarted += OnDecel;
                _conveyor.ScrollBy(_scrollDistance);

                // Wait for scroll to finish
                yield return new WaitUntil(() => !_conveyor.IsScrolling);
                _conveyor.OnDecelerationStarted -= OnDecel;

                // If scroll was too short for deceleration phase, spawn now
                if (!enemiesSpawned)
                {
#if UNITY_EDITOR
                    Debug.Log($"[{nameof(LevelManager)}] Scroll ended without decel phase. Spawning level {_currentLevelIndex}.");
#endif
                    StartLevel(_currentLevelIndex);
                }
            }
            else
            {
                StartLevel(_currentLevelIndex);
            }
        }

        private void AssignAllyTargetsInZone()
        {
            if (_teamContainer == null || _enemiesContainer == null) return;

            for (int i = 0; i < _teamContainer.childCount; i++)
            {
                var ally = _teamContainer.GetChild(i);
                if (!ally.TryGetComponent<CombatController>(out var controller)) continue;
                if (controller.Target != null) continue;
                if (!ally.TryGetComponent<CombatStats>(out var stats) || stats.IsDead) continue;

                Transform target = TargetFinder.Closest(_enemiesContainer, ally.position, float.MaxValue, CombatZoneX);
                if (target != null)
                {
                    controller.Target = target;
                    WireAllyRetarget(ally);
                }
            }
        }

        private void WireAllyRetarget(Transform ally)
        {
            if (!ally.TryGetComponent<CombatController>(out var controller)) return;
            if (controller.FindNewTarget != null) return;

            var allyRef = ally;
            controller.FindNewTarget = () => TargetFinder.Closest(_enemiesContainer, allyRef.position, float.MaxValue, CombatZoneX);
        }

        private void ClearAllyTargets()
        {
            if (_teamContainer == null) return;

            for (int i = 0; i < _teamContainer.childCount; i++)
            {
                var ally = _teamContainer.GetChild(i);
                if (ally.TryGetComponent<CombatController>(out var controller))
                {
                    controller.Disengage();
                }
            }
        }

    }
}
