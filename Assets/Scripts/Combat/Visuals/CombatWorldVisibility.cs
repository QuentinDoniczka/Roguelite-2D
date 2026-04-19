using System.Collections.Generic;
using RogueliteAutoBattler.Combat.Core;
using RogueliteAutoBattler.UI.Toolkit;
using UnityEngine;

namespace RogueliteAutoBattler.Combat.Visuals
{
    public class CombatWorldVisibility : MonoBehaviour
    {
        private Transform _teamContainer;
        private Transform _enemiesContainer;
        private bool _visible = true;
        private int _lastTeamChildCount;
        private int _lastEnemyChildCount;
        private readonly List<Renderer> _rendererBuffer = new List<Renderer>();
        private NavigationManager _cachedNavigationManager;

        public bool Visible => _visible;

        private void Start()
        {
            _teamContainer = transform.Find(CombatSetupHelper.TeamContainerName);
            _enemiesContainer = transform.Find(CombatSetupHelper.EnemiesContainerName);

            if (NavigationHost.Instance != null
                && NavigationHost.Instance.Navigation != null)
            {
                _cachedNavigationManager = NavigationHost.Instance.Navigation;
                _cachedNavigationManager.OnTabChanged += OnTabChanged;
            }
        }

        private void OnDestroy()
        {
            if (_cachedNavigationManager != null)
                _cachedNavigationManager.OnTabChanged -= OnTabChanged;
        }

        private void OnTabChanged(int tabIndex)
        {
            SetVisible(tabIndex < 0);
        }

        public void SetVisible(bool visible)
        {
            if (_visible == visible) return;
            _visible = visible;

            ToggleRenderers(_teamContainer, visible);
            ToggleRenderers(_enemiesContainer, visible);

            DamageNumberService.Suppressed = !visible;
            CoinFlyService.Suppressed = !visible;

            if (!visible)
            {
                _lastTeamChildCount = _teamContainer != null ? _teamContainer.childCount : 0;
                _lastEnemyChildCount = _enemiesContainer != null ? _enemiesContainer.childCount : 0;
            }
        }

        private void LateUpdate()
        {
            if (_visible) return;

            CheckForNewChildren(_teamContainer, ref _lastTeamChildCount);
            CheckForNewChildren(_enemiesContainer, ref _lastEnemyChildCount);
        }

        private void CheckForNewChildren(Transform container, ref int lastCount)
        {
            if (container == null) return;
            int currentCount = container.childCount;
            if (currentCount == lastCount) return;

            ToggleRenderers(container, false);
            lastCount = currentCount;
        }

        private void ToggleRenderers(Transform container, bool enabled)
        {
            if (container == null) return;
            container.GetComponentsInChildren(true, _rendererBuffer);
            for (int i = 0; i < _rendererBuffer.Count; i++)
                _rendererBuffer[i].enabled = enabled;
            _rendererBuffer.Clear();
        }

        internal void InitializeForTest(Transform teamContainer, Transform enemiesContainer)
        {
            _teamContainer = teamContainer;
            _enemiesContainer = enemiesContainer;
        }

        internal void WireNavigationForTest(NavigationManager navigationManager)
        {
            if (_cachedNavigationManager != null)
                _cachedNavigationManager.OnTabChanged -= OnTabChanged;

            _cachedNavigationManager = navigationManager;
            if (_cachedNavigationManager != null)
                _cachedNavigationManager.OnTabChanged += OnTabChanged;
        }
    }
}
