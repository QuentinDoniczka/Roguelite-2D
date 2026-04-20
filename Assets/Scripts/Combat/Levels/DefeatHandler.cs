using System;
using System.Collections;
using RogueliteAutoBattler.Combat.Core;
using RogueliteAutoBattler.Combat.Environment;
using UnityEngine;

namespace RogueliteAutoBattler.Combat.Levels
{
    internal class DefeatHandler
    {
        private readonly TeamRoster _teamRoster;
        private readonly Transform _enemiesContainer;
        private readonly Transform _enemiesHomeAnchor;
        private readonly WaitForSeconds _waitDefeatReset;
        private readonly WorldConveyor _conveyor;
        private readonly Func<float> _characterScaleProvider;
        private readonly AllyTargetManager _allyTargetManager;

        internal event Action OnAllAlliesDead;

        internal int AliveAllyCount { get; private set; }

        internal DefeatHandler(
            TeamRoster teamRoster,
            Transform enemiesContainer,
            Transform enemiesHomeAnchor,
            WaitForSeconds waitDefeatReset,
            WorldConveyor conveyor,
            Func<float> characterScaleProvider,
            AllyTargetManager allyTargetManager)
        {
            _teamRoster = teamRoster;
            _enemiesContainer = enemiesContainer;
            _enemiesHomeAnchor = enemiesHomeAnchor;
            _waitDefeatReset = waitDefeatReset;
            _conveyor = conveyor;
            _characterScaleProvider = characterScaleProvider;
            _allyTargetManager = allyTargetManager;
        }

        internal void WireAllyDeathTracking()
        {
            AliveAllyCount = 0;

            if (_teamRoster == null) return;

            _teamRoster.OnMemberDied -= OnAllyDied;
            _teamRoster.OnMemberDied += OnAllyDied;

            var members = _teamRoster.Members;
            for (int i = 0; i < members.Count; i++)
            {
                if (!members[i].IsDead)
                    AliveAllyCount++;
            }
        }

        internal void HandleLevelLost()
        {
#if UNITY_EDITOR
            Debug.Log($"[{nameof(DefeatHandler)}] Level lost! All allies defeated.");
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

            _teamRoster?.ReviveAll();

            yield return null;

            wireAllyDeathTracking();
            startLevel(0);
        }

        private void OnAllyDied(TeamMember member)
        {
            AliveAllyCount--;
            if (AliveAllyCount <= 0)
                OnAllAlliesDead?.Invoke();
        }
    }
}
