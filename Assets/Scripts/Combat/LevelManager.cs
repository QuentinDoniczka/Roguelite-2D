using System.Collections;
using RogueliteAutoBattler.Data;
using UnityEngine;

namespace RogueliteAutoBattler.Combat
{
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
        [SerializeField] private Transform _enemiesContainer;
        [SerializeField] private Transform _teamContainer;

        [Header("Scroll Transition")]
        [SerializeField] private float _scrollDistance = 2f;

        [Header("Enemy Spawn")]
        [SerializeField] private float _enemySpawnOffscreenX = 1f;

        [Header("Anchors")]
        [SerializeField] private Transform _teamHomeAnchor;
        [SerializeField] private Transform _enemiesHomeAnchor;
        [SerializeField] private Transform _combatTriggerZone;

        private const float FallbackEnemySpawnX = 1f;

        private int _aliveEnemyCount;
        private int _aliveAllyCount;
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
            if (_teamHomeAnchor == null)
                _teamHomeAnchor = GameObject.Find(CombatSetupHelper.TeamHomeAnchorName)?.transform;
            if (_enemiesHomeAnchor == null)
                _enemiesHomeAnchor = GameObject.Find(CombatSetupHelper.EnemiesHomeAnchorName)?.transform;
            _conveyor = GetComponent<WorldConveyor>();
            ApplyStage(_currentStageIndex);
            yield return new WaitUntil(() => TargetFinder.Closest(_teamContainer, Vector3.zero) != null);
            WireAllyDeathTracking();
            StartLevel(_currentLevelIndex);
        }

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

            Vector2 anchorPos = _enemiesHomeAnchor != null
                ? (Vector2)_enemiesHomeAnchor.position
                : new Vector2(FallbackEnemySpawnX, 0f);
            Vector2 spawnAnchor = new Vector2(anchorPos.x + _enemySpawnOffscreenX, anchorPos.y);
            Vector2[] spawnPositions = FormationLayout.GetPositions(spawnAnchor, wave.Enemies.Count, facingRight: false);
            Vector2[] homePositions = FormationLayout.GetPositions(anchorPos, wave.Enemies.Count, facingRight: false);

            for (int i = 0; i < wave.Enemies.Count; i++)
            {
                Vector2 offset = homePositions[i] - anchorPos;
                SpawnEnemy(wave.Enemies[i], spawnPositions[i], offset);
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

            IgnoreCollisionWithOppositeTeam(enemy, _teamContainer);

            _aliveEnemyCount++;
            components.Stats.OnDied += OnEnemyDied;

            var enemyTransform = enemy.transform;

            components.Controller.SetAttackRange(data.AttackRange);
            components.Controller.SetAttackerFacing(false);
            components.Controller.FindNewTarget = () => TargetFinder.LeastContested(_teamContainer, enemyTransform.position);

            Transform allyTarget = TargetFinder.LeastContested(_teamContainer, enemyTransform.position);
            if (allyTarget != null)
                components.Controller.Target = allyTarget;
#if UNITY_EDITOR
            else
                Debug.LogWarning($"[{nameof(LevelManager)}] No alive ally found for enemy '{data.EnemyName}' to target.");
#endif

            SetAllyTarget(enemy.transform);

#if UNITY_EDITOR
            Debug.Log($"[{nameof(LevelManager)}] Spawned enemy '{data.EnemyName}' at {spawnPosition}");
#endif
        }

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

                bool needsTarget = allyMover.Target == null;
                if (!needsTarget && allyMover.Target.TryGetComponent<CombatStats>(out var targetStats))
                    needsTarget = targetStats.IsDead;

                if (needsTarget)
                {
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
            AttackSlotRegistry.Clear();
            CombatSetupHelper.RecalculateFormation(_teamContainer, _teamHomeAnchor, facingRight: true);

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

                yield return new WaitUntil(() => !_conveyor.IsScrolling);
                _conveyor.OnDecelerationStarted -= OnDecel;

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

                Transform target = TargetFinder.LeastContested(_enemiesContainer, ally.position, float.MaxValue, CombatZoneX);
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
            controller.FindNewTarget = () => TargetFinder.LeastContested(_enemiesContainer, allyRef.position, float.MaxValue, CombatZoneX);
        }

        private void ClearAllyTargets() => DisengageAll(_teamContainer);

        private void ClearEnemyTargets() => DisengageAll(_enemiesContainer);

        private static void DisengageAll(Transform container)
        {
            if (container == null) return;

            for (int i = 0; i < container.childCount; i++)
            {
                var child = container.GetChild(i);
                if (child.TryGetComponent<CombatController>(out var controller))
                {
                    controller.Disengage();
                }
            }
        }

        private void OnAllyDied()
        {
            _aliveAllyCount--;
            CheckLevelLost();
        }

        private void CheckLevelLost()
        {
            if (_levelInProgress && _aliveAllyCount <= 0)
            {
                _levelInProgress = false;
                OnLevelLost();
            }
        }

        private void OnLevelLost()
        {
#if UNITY_EDITOR
            Debug.Log($"[{nameof(LevelManager)}] Level lost! All allies defeated.");
#endif
            ClearEnemyTargets();
            AttackSlotRegistry.Clear();
            CombatSetupHelper.RecalculateFormation(_enemiesContainer, _enemiesHomeAnchor, facingRight: false);
        }

        private void WireAllyDeathTracking()
        {
            _aliveAllyCount = 0;

            if (_teamContainer == null) return;

            for (int i = 0; i < _teamContainer.childCount; i++)
            {
                var ally = _teamContainer.GetChild(i);
                if (!ally.TryGetComponent<CombatStats>(out var stats) || stats.IsDead)
                    continue;

                stats.OnDied -= OnAllyDied;
                stats.OnDied += OnAllyDied;
                _aliveAllyCount++;
            }
        }

        internal int AliveAllyCount => _aliveAllyCount;
        internal bool LevelInProgress => _levelInProgress;

        internal void InitializeForTest(Transform teamContainer, Transform enemiesContainer, Transform teamHomeAnchor = null, Transform enemiesHomeAnchor = null)
        {
            _teamContainer = teamContainer;
            _enemiesContainer = enemiesContainer;
            if (teamHomeAnchor != null) _teamHomeAnchor = teamHomeAnchor;
            if (enemiesHomeAnchor != null) _enemiesHomeAnchor = enemiesHomeAnchor;
            _levelInProgress = true;
        }

        internal void WireAllyDeathTrackingForTest() => WireAllyDeathTracking();
        internal void ClearAllyTargetsForTest() => ClearAllyTargets();
        internal void ClearEnemyTargetsForTest() => ClearEnemyTargets();

        internal void RecalculateAllyFormationForTest() =>
            CombatSetupHelper.RecalculateFormation(_teamContainer, _teamHomeAnchor, facingRight: true);

        internal void RecalculateEnemyFormationForTest() =>
            CombatSetupHelper.RecalculateFormation(_enemiesContainer, _enemiesHomeAnchor, facingRight: false);

        private static void IgnoreCollisionWithOppositeTeam(GameObject character, Transform oppositeContainer)
        {
            if (oppositeContainer == null) return;

            var col = character.GetComponent<Collider2D>();
            if (col == null) return;

            for (int i = 0; i < oppositeContainer.childCount; i++)
            {
                var otherCol = oppositeContainer.GetChild(i).GetComponent<Collider2D>();
                if (otherCol != null)
                    Physics2D.IgnoreCollision(col, otherCol, true);
            }
        }
    }
}
