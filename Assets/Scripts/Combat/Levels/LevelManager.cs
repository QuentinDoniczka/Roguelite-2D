using System;
using System.Collections;
using RogueliteAutoBattler.Combat.Core;
using RogueliteAutoBattler.Combat.Environment;
using RogueliteAutoBattler.Combat.Visuals;
using RogueliteAutoBattler.Data;
using RogueliteAutoBattler.Economy;
using UnityEngine;

namespace RogueliteAutoBattler.Combat.Levels
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
        private int _currentStepIndex;

        [Header("Containers")]
        [SerializeField] private Transform _enemiesContainer;
        [SerializeField] private Transform _teamContainer;

        [Header("Scroll Transition")]
        [SerializeField] private float _scrollDistance = 3f;
        [SerializeField] private float _stepScrollDistance = 1.5f;

        [Header("Enemy Spawn")]
        [SerializeField] private float _enemySpawnOffscreenX = 1f;

        [Header("Anchors")]
        [SerializeField] private Transform _teamHomeAnchor;
        [SerializeField] private Transform _enemiesHomeAnchor;
        [SerializeField] private Transform _combatTriggerZone;

        public event Action<int, int> OnStageStarted;
        public event Action<int, int> OnLevelStarted;
        public event Action<int> OnStepStarted;
        public event Action<int, int, int> OnWaveSpawned;

        public int CurrentStageIndex => _currentStageIndex;
        public int CurrentLevelIndex => _currentLevelIndex;
        public int CurrentStepIndex => _currentStepIndex;

        public int TotalStepsInCurrentLevel =>
            TryGetCurrentLevel(out var level) ? level.Steps.Count : 0;

        public int TotalLevelsInCurrentStage =>
            TryGetCurrentStage(out var stage) ? stage.Levels.Count : 0;

        private const float FallbackEnemySpawnX = 1f;
        private const float SpawnSpeedThreshold = 0.3f;

        [Header("Defeat Reset")]
        [SerializeField] private float _defeatResetDelay = 1.5f;

        private int _aliveEnemyCount;
        private int _aliveAllyCount;
        private int _pendingWaveCount;
        private bool _levelInProgress;
        private WorldConveyor _conveyor;
        private GoldWallet _goldWallet;
        private CombatSpawnManager _spawnManager;

        internal float DefeatResetDelay => _defeatResetDelay;
        internal float StepScrollDistance => _stepScrollDistance;

        private float CombatZoneX => _combatTriggerZone != null ? _combatTriggerZone.position.x : float.MaxValue;

        private bool TryGetCurrentStage(out StageData stage)
        {
            stage = null;
            if (_levelDatabase == null) return false;
            var stages = _levelDatabase.Stages;
            if (stages == null || _currentStageIndex < 0 || _currentStageIndex >= stages.Count) return false;
            stage = stages[_currentStageIndex];
            return true;
        }

        private bool TryGetCurrentLevel(out LevelData level)
        {
            level = null;
            if (!TryGetCurrentStage(out var stage)) return false;
            var levels = stage.Levels;
            if (levels == null || _currentLevelIndex < 0 || _currentLevelIndex >= levels.Count) return false;
            level = levels[_currentLevelIndex];
            return true;
        }

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
            _spawnManager = GetComponent<CombatSpawnManager>();
            var wallets = FindObjectsByType<GoldWallet>(FindObjectsSortMode.None);
            if (wallets.Length > 0) _goldWallet = wallets[0];
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

            OnStageStarted?.Invoke(_currentStageIndex, _currentLevelIndex);
        }

        public void StartLevel(int levelIndex)
        {
            if (!TryGetCurrentStage(out var stage))
            {
#if UNITY_EDITOR
                Debug.LogWarning($"[{nameof(LevelManager)}] Current stage index {_currentStageIndex} out of range.");
#endif
                return;
            }

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

            OnLevelStarted?.Invoke(_currentStageIndex, _currentLevelIndex);

            var level = stage.Levels[levelIndex];
            _currentStepIndex = 0;

            if (level.Steps.Count == 0) return;

            var step = level.Steps[_currentStepIndex];
            _pendingWaveCount = step.Waves.Count;

            OnStepStarted?.Invoke(_currentStepIndex);

#if UNITY_EDITOR
            Debug.Log($"[{nameof(LevelManager)}] Starting level '{level.LevelName}' step {_currentStepIndex} with {step.Waves.Count} wave(s).");
#endif

            for (int i = 0; i < step.Waves.Count; i++)
            {
                StartCoroutine(SpawnWaveCoroutine(step.Waves[i], i));
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

            if (!_levelInProgress) yield break;

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

            OnWaveSpawned?.Invoke(_currentStageIndex, _currentLevelIndex, waveIndex);

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
                nameof(LevelManager),
                healthBarFillColor: HealthBar.EnemyFillColor,
                isAlly: false);

            IgnoreCollisionWithOppositeTeam(enemy, _teamContainer);

            _aliveEnemyCount++;
            components.Stats.OnDied += OnEnemyDied;

            var enemyTransform = enemy.transform;

            int goldAmount = data.GoldDrop;
            if (goldAmount > 0)
            {
                components.Stats.OnDied += () =>
                {
                    if (enemyTransform == null) return;
                    CoinFlyService.Show(enemyTransform.position, () =>
                    {
                        if (_goldWallet != null) _goldWallet.Add(goldAmount);
                    });
                };
            }

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
            if (!_levelInProgress || _pendingWaveCount > 0 || _aliveEnemyCount > 0) return;

            if (TryGetCurrentLevel(out var level) && _currentStepIndex + 1 < level.Steps.Count)
            {
                StartCoroutine(ScrollAndSpawnCoroutine(_stepScrollDistance, StartNextStep));
                return;
            }

            _levelInProgress = false;
            OnLevelComplete();
        }

        private void OnLevelComplete()
        {
#if UNITY_EDITOR
            Debug.Log($"[{nameof(LevelManager)}] Level complete! Starting transition...");
#endif

            if (!TryGetCurrentStage(out var stage))
            {
#if UNITY_EDITOR
                Debug.LogWarning($"[{nameof(LevelManager)}] Stage index {_currentStageIndex} out of range on level complete.");
#endif
                return;
            }

            _currentLevelIndex++;

            if (_currentLevelIndex < stage.Levels.Count)
            {
                StartCoroutine(ScrollAndSpawnCoroutine(_scrollDistance, () => StartLevel(_currentLevelIndex)));
            }
            else
            {
#if UNITY_EDITOR
                Debug.Log($"[{nameof(LevelManager)}] Stage '{stage.StageName}' complete!");
#endif
            }
        }

        private void StartNextStep()
        {
            if (!TryGetCurrentLevel(out var level)) return;

            _currentStepIndex++;
            var step = level.Steps[_currentStepIndex];
            _pendingWaveCount = step.Waves.Count;

            OnStepStarted?.Invoke(_currentStepIndex);

#if UNITY_EDITOR
            Debug.Log($"[{nameof(LevelManager)}] Starting step {_currentStepIndex} with {step.Waves.Count} wave(s).");
#endif

            for (int i = 0; i < step.Waves.Count; i++)
            {
                StartCoroutine(SpawnWaveCoroutine(step.Waves[i], i));
            }
        }

        private IEnumerator ScrollAndSpawnCoroutine(float scrollDistance, Action onReadyToSpawn)
        {
            _levelInProgress = false;

            ClearAllyTargets();
            AttackSlotRegistry.Clear();
            CombatSetupHelper.RecalculateFormation(_teamContainer, _teamHomeAnchor, facingRight: true);

            if (_conveyor != null && scrollDistance > 0f)
            {
                bool decelStarted = false;
                void OnDecel() => decelStarted = true;

                _conveyor.OnDecelerationStarted += OnDecel;
                _conveyor.ScrollBy(scrollDistance);

                yield return new WaitUntil(() =>
                    !_conveyor.IsScrolling ||
                    (decelStarted && _conveyor.CurrentSpeed <= SpawnSpeedThreshold));

                _conveyor.OnDecelerationStarted -= OnDecel;

#if UNITY_EDITOR
                Debug.Log($"[{nameof(LevelManager)}] Spawning after scroll (speed: {_conveyor.CurrentSpeed:F2}).");
#endif
                onReadyToSpawn?.Invoke();

                if (_conveyor.IsScrolling)
                    yield return new WaitUntil(() => !_conveyor.IsScrolling);
            }
            else
            {
                onReadyToSpawn?.Invoke();
            }

            _levelInProgress = true;
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
            StartCoroutine(DefeatResetCoroutine());
        }

        private IEnumerator DefeatResetCoroutine()
        {
            yield return new WaitForSeconds(_defeatResetDelay);

            CombatSetupHelper.DestroyAllChildren(_enemiesContainer);

            if (_conveyor != null)
                _conveyor.ResetPosition();

            _currentStageIndex = 0;
            _currentLevelIndex = 0;
            _currentStepIndex = 0;

            ApplyStage(0);

            _aliveEnemyCount = 0;
            _pendingWaveCount = 0;

            _spawnManager.RespawnAllies();

            yield return null;

            WireAllyDeathTracking();
            StartLevel(0);
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

        internal void InitializeForTest(Transform teamContainer, Transform enemiesContainer, Transform teamHomeAnchor = null, Transform enemiesHomeAnchor = null, LevelDatabase levelDatabase = null)
        {
            _teamContainer = teamContainer;
            _enemiesContainer = enemiesContainer;
            if (teamHomeAnchor != null) _teamHomeAnchor = teamHomeAnchor;
            if (enemiesHomeAnchor != null) _enemiesHomeAnchor = enemiesHomeAnchor;
            if (levelDatabase != null) _levelDatabase = levelDatabase;
            _levelInProgress = true;
        }

        internal void SetSpawnManagerForTest(CombatSpawnManager spawnManager)
        {
            _spawnManager = spawnManager;
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
