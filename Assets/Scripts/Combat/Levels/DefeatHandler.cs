using System;
using System.Collections;
using RogueliteAutoBattler.Combat.Core;
using RogueliteAutoBattler.Combat.Environment;
using UnityEngine;

namespace RogueliteAutoBattler.Combat.Levels
{
    internal class DefeatHandler
    {
        private readonly Transform _teamContainer;
        private readonly Transform _enemiesContainer;
        private readonly Transform _enemiesHomeAnchor;
        private readonly WaitForSeconds _waitDefeatReset;
        private readonly WorldConveyor _conveyor;
        private readonly CombatSpawnManager _spawnManager;
        private readonly Func<float> _characterScaleProvider;
        private readonly AllyTargetManager _allyTargetManager;

        internal event Action OnAllAlliesDead;

        internal int AliveAllyCount { get; private set; }

        internal DefeatHandler(
            Transform teamContainer,
            Transform enemiesContainer,
            Transform enemiesHomeAnchor,
            WaitForSeconds waitDefeatReset,
            WorldConveyor conveyor,
            CombatSpawnManager spawnManager,
            Func<float> characterScaleProvider,
            AllyTargetManager allyTargetManager)
        {
            _teamContainer = teamContainer;
            _enemiesContainer = enemiesContainer;
            _enemiesHomeAnchor = enemiesHomeAnchor;
            _waitDefeatReset = waitDefeatReset;
            _conveyor = conveyor;
            _spawnManager = spawnManager;
            _characterScaleProvider = characterScaleProvider;
            _allyTargetManager = allyTargetManager;
        }

        internal void WireAllyDeathTracking()
        {
            AliveAllyCount = 0;

            if (_teamContainer == null) return;

            for (int i = 0; i < _teamContainer.childCount; i++)
            {
                var ally = _teamContainer.GetChild(i);
                if (!ally.TryGetComponent<CombatStats>(out var stats) || stats.IsDead)
                    continue;

                stats.OnDied -= OnAllyDied;
                stats.OnDied += OnAllyDied;
                AliveAllyCount++;
            }
        }

        internal void HandleLevelLost()
        {
#if UNITY_EDITOR
            Debug.Log($"[{nameof(LevelManager)}] Level lost! All allies defeated.");
#endif
            _allyTargetManager.ClearEnemyTargets();
            AttackSlotRegistry.Clear();
            CombatSetupHelper.RecalculateFormation(_enemiesContainer, _enemiesHomeAnchor, facingRight: false, characterScale: _characterScaleProvider());
        }

        internal IEnumerator DefeatResetCoroutine(Action<int, int, int> onResetIndices, Action<int> applyStage, Action<int> startLevel, Action wireAllyDeathTracking, Action<int> resetEnemySpawnerCount, Action<int> resetPendingWaveCount)
        {
            yield return _waitDefeatReset;

            CombatSetupHelper.DestroyAllChildren(_enemiesContainer);

            if (_conveyor != null)
                _conveyor.ResetPosition();

            onResetIndices(0, 0, 0);

            applyStage(0);

            resetEnemySpawnerCount(0);
            resetPendingWaveCount(0);

            _spawnManager.RespawnAllies();

            yield return null;

            wireAllyDeathTracking();
            startLevel(0);
        }

        private void OnAllyDied()
        {
            AliveAllyCount--;
            OnAllAlliesDead?.Invoke();
        }
    }
}
