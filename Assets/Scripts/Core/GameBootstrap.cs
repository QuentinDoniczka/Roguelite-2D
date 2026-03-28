using RogueliteAutoBattler.UI.Core;
using UnityEngine;

namespace RogueliteAutoBattler.Core
{
    public class GameBootstrap : MonoBehaviour
    {
        [SerializeField] private Canvas _canvas;
        [SerializeField] private Transform _combatWorld;
        [SerializeField] private NavigationManager _navigationManager;
        [SerializeField] private Camera _mainCamera;

        internal Canvas Canvas => _canvas;
        internal Transform CombatWorld => _combatWorld;
        internal NavigationManager NavigationManager => _navigationManager;
        internal Camera MainCamera => _mainCamera;

        internal void SetRefs(Canvas canvas, Transform combatWorld, NavigationManager navigationManager, Camera mainCamera)
        {
            _canvas = canvas;
            _combatWorld = combatWorld;
            _navigationManager = navigationManager;
            _mainCamera = mainCamera;
        }

        private void Awake()
        {
            if (_canvas == null)
                Debug.LogError("[GameBootstrap] _canvas is not assigned.", this);

            if (_combatWorld == null)
                Debug.LogError("[GameBootstrap] _combatWorld is not assigned.", this);

            if (_navigationManager == null)
                Debug.LogError("[GameBootstrap] _navigationManager is not assigned.", this);

            if (_mainCamera == null)
                Debug.LogError("[GameBootstrap] _mainCamera is not assigned.", this);
        }
    }
}
