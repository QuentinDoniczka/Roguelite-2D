using System;
using System.Collections;
using RogueliteAutoBattler.Combat.Core;
using RogueliteAutoBattler.Combat.Environment;
using RogueliteAutoBattler.Data;
using RogueliteAutoBattler.Economy;
using UnityEngine;

namespace RogueliteAutoBattler.Combat.Levels
{
    [RequireComponent(typeof(WorldConveyor))]
    public class LevelManager : MonoBehaviour
    {
        private const float SpawnSpeedThreshold = 0.3f;

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
        [SerializeField] private float _levelScrollDistance = 4f;
        [SerializeField] private float _stepScrollDistance = 2f;

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

        public StepType GetStepType(int stepIndex)
        {
            if (!TryGetCurrentLevel(out var level)) return StepType.Normal;
            if (stepIndex < 0 || stepIndex >= level.Steps.Count) return StepType.Normal;
            return level.Steps[stepIndex].StepType;
        }

        [Header("Defeat Reset")]
        [SerializeField] private float _defeatResetDelay = 1.5f;

        private int _pendingWaveCount;
        private bool _levelInProgress;
        private WorldConveyor _conveyor;
        private GoldWallet _goldWallet;
        private CombatSpawnManager _spawnManager;
        private TeamRoster _teamRoster;
        private WaitForSeconds _waitDefeatReset;
        private GroundFitter _groundFitter;
        private bool _groundFitterCached;

        private EnemySpawner _enemySpawner;
        private AllyTargetManager _allyTargetManager;
        private DefeatHandler _defeatHandler;

        internal float DefeatResetDelay => _defeatResetDelay;
        internal float StepScrollDistance => _stepScrollDistance;

        private float CombatZoneX => _combatTriggerZone != null ? _combatTriggerZone.position.x : float.MaxValue;

        private float CharacterScale => _spawnManager != null ? _spawnManager.CharacterScale : 1f;

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
            _allyTargetManager?.AssignAllyTargetsInZone();
        }

        private IEnumerator Start()
        {
            CombatSetupHelper.FindContainersIfNeeded(transform, ref _teamContainer, ref _enemiesContainer, nameof(LevelManager));
            if (_teamHomeAnchor == null)
            {
                Debug.LogError($"[{nameof(LevelManager)}] TeamHomeAnchor not assigned!", this);
                yield break;
            }
            if (_enemiesHomeAnchor == null)
            {
                Debug.LogError($"[{nameof(LevelManager)}] EnemiesHomeAnchor not assigned!", this);
                yield break;
            }
            _conveyor = GetComponent<WorldConveyor>();
            _spawnManager = GetComponent<CombatSpawnManager>();
            _teamRoster = GetComponent<TeamRoster>();
            _waitDefeatReset = new WaitForSeconds(_defeatResetDelay);
            var wallets = FindObjectsByType<GoldWallet>(FindObjectsSortMode.None);
            if (wallets.Length > 0) _goldWallet = wallets[0];

            CreateHelpers();

            ApplyStage(_currentStageIndex);
            yield return new WaitUntil(() => TargetFinder.Closest(_teamContainer, Vector3.zero) != null);
            WireAllyDeathTracking();
            StartLevel(_currentLevelIndex);
        }

        private void CreateHelpers()
        {
            _allyTargetManager = new AllyTargetManager(
                _teamContainer,
                _enemiesContainer,
                () => CombatZoneX);

            _enemySpawner = new EnemySpawner(
                _enemiesContainer,
                _teamContainer,
                _enemiesHomeAnchor,
                () => CharacterScale,
                () => _goldWallet,
                _enemySpawnOffscreenX);
            _enemySpawner.OnEnemyDied += OnEnemyDied;

            _defeatHandler = new DefeatHandler(
                _teamRoster,
                _enemiesContainer,
                _enemiesHomeAnchor,
                _waitDefeatReset,
                _conveyor,
                () => CharacterScale,
                _allyTargetManager);
            _defeatHandler.OnAllAlliesDead += CheckLevelLost;
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

            _currentStageIndex = stageIndex;

            OnStageStarted?.Invoke(_currentStageIndex, _currentLevelIndex);
        }

        internal void ApplyLevel(LevelData level)
        {
            if (_groundRenderer == null) return;

            EnsureGroundFitterCached();

            Sprite sprite = level?.Background != null
                ? level.Background
                : _levelDatabase != null ? _levelDatabase.DefaultBackground : null;

            if (sprite != null)
            {
                _groundRenderer.sprite = sprite;
#if UNITY_EDITOR
                Debug.Log($"[{nameof(LevelManager)}] Applied background '{sprite.name}' for level '{level?.LevelName}'");
#endif
            }
            else
            {
                Debug.LogWarning($"[{nameof(LevelManager)}] No background available for level '{level?.LevelName}' (DefaultBackground also null) — keeping current sprite.");
            }

            if (_groundFitter != null && level != null)
            {
                _groundFitter.SetFitMode(level.Fit);
            }
        }

        private void EnsureGroundFitterCached()
        {
            if (_groundFitterCached) return;
            _groundFitterCached = true;
            if (_groundRenderer == null) return;
            _groundRenderer.TryGetComponent(out _groundFitter);
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

            _teamRoster?.ReviveAll();

            _currentLevelIndex = levelIndex;
            _enemySpawner.ResetAliveEnemyCount();
            _levelInProgress = true;

            var level = stage.Levels[levelIndex];
            ApplyLevel(level);

            OnLevelStarted?.Invoke(_currentStageIndex, _currentLevelIndex);

            _currentStepIndex = 0;

            SpawnStep(level);
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

            Vector2[] homePositions;
            Vector2[] spawnPositions = _enemySpawner.CalculateSpawnPositions(wave.Enemies.Count, out homePositions);
            Vector2 anchorPos = _enemySpawner.GetAnchorPosition();

            for (int i = 0; i < wave.Enemies.Count; i++)
            {
                Vector2 offset = homePositions[i] - anchorPos;
                _enemySpawner.SpawnEnemy(wave.Enemies[i], spawnPositions[i], offset, OnEnemySpawned);
            }

            OnWaveSpawned?.Invoke(_currentStageIndex, _currentLevelIndex, waveIndex);

            _pendingWaveCount--;
            CheckLevelComplete();
        }

        private void OnEnemySpawned(Transform enemyTransform)
        {
            _allyTargetManager.SetAllyTarget(enemyTransform);
        }

        private void OnEnemyDied()
        {
            CheckLevelComplete();
        }

        private void CheckLevelComplete()
        {
            if (!_levelInProgress || _pendingWaveCount > 0 || _enemySpawner.AliveEnemyCount > 0) return;

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
                StartCoroutine(ScrollAndSpawnCoroutine(_levelScrollDistance, () => StartLevel(_currentLevelIndex)));
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

            SpawnStep(level);
        }

        private void SpawnStep(LevelData level)
        {
            if (_currentStepIndex < 0 || _currentStepIndex >= level.Steps.Count)
            {
#if UNITY_EDITOR
                Debug.LogWarning($"[{nameof(LevelManager)}] Step index {_currentStepIndex} out of range for level '{level.LevelName}' ({level.Steps.Count} steps).");
#endif
                return;
            }

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

            _allyTargetManager.ClearAllyTargets();
            AttackSlotRegistry.Clear();
            CombatSetupHelper.RecalculateFormation(_teamContainer, _teamHomeAnchor, facingRight: true, characterScale: CharacterScale);

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
                _levelInProgress = true;
                onReadyToSpawn?.Invoke();

                if (_conveyor.IsScrolling)
                    yield return new WaitUntil(() => !_conveyor.IsScrolling);
            }
            else
            {
                _levelInProgress = true;
                onReadyToSpawn?.Invoke();
            }
        }

        private void CheckLevelLost()
        {
            if (_levelInProgress && _defeatHandler.AliveAllyCount <= 0)
            {
                _levelInProgress = false;
                UnitSelectionManager.Instance?.ForceDeselect();
                _defeatHandler.HandleLevelLost();
                StartCoroutine(_defeatHandler.DefeatResetCoroutine(
                    (stage, level, step) =>
                    {
                        _currentStageIndex = stage;
                        _currentLevelIndex = level;
                        _currentStepIndex = step;
                    },
                    ApplyStage,
                    StartLevel,
                    WireAllyDeathTracking,
                    _ => _enemySpawner.ResetAliveEnemyCount(),
                    count => _pendingWaveCount = count));
            }
        }

        private void WireAllyDeathTracking()
        {
            _defeatHandler.WireAllyDeathTracking();
        }

        internal int AliveAllyCount => _defeatHandler != null ? _defeatHandler.AliveAllyCount : 0;
        internal bool LevelInProgress => _levelInProgress;

        internal void InitializeForTest(Transform teamContainer, Transform enemiesContainer, Transform teamHomeAnchor = null, Transform enemiesHomeAnchor = null, LevelDatabase levelDatabase = null, TeamRoster teamRoster = null)
        {
            _teamContainer = teamContainer;
            _enemiesContainer = enemiesContainer;
            if (teamHomeAnchor != null) _teamHomeAnchor = teamHomeAnchor;
            if (enemiesHomeAnchor != null) _enemiesHomeAnchor = enemiesHomeAnchor;
            if (levelDatabase != null) _levelDatabase = levelDatabase;
            _levelInProgress = true;
            _waitDefeatReset = new WaitForSeconds(_defeatResetDelay);
            _teamRoster = teamRoster != null ? teamRoster : GetComponent<TeamRoster>();

            CreateHelpers();
        }

        internal void SetSpawnManagerForTest(CombatSpawnManager spawnManager)
        {
            _spawnManager = spawnManager;

            if (_defeatHandler != null)
            {
                _defeatHandler.OnAllAlliesDead -= CheckLevelLost;
                _defeatHandler.UnwireAllyDeathTracking();

                _defeatHandler = new DefeatHandler(
                    _teamRoster,
                    _enemiesContainer,
                    _enemiesHomeAnchor,
                    _waitDefeatReset,
                    _conveyor,
                    () => CharacterScale,
                    _allyTargetManager);
                _defeatHandler.OnAllAlliesDead += CheckLevelLost;
            }
        }

        internal void WireAllyDeathTrackingForTest() => WireAllyDeathTracking();
        internal void ClearAllyTargetsForTest() => _allyTargetManager.ClearAllyTargets();
        internal void ClearEnemyTargetsForTest() => _allyTargetManager.ClearEnemyTargets();

        internal void RecalculateAllyFormationForTest() =>
            CombatSetupHelper.RecalculateFormation(_teamContainer, _teamHomeAnchor, facingRight: true, characterScale: CharacterScale);

        internal void RecalculateEnemyFormationForTest() =>
            CombatSetupHelper.RecalculateFormation(_enemiesContainer, _enemiesHomeAnchor, facingRight: false, characterScale: CharacterScale);
    }
}
