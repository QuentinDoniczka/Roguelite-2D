using UnityEngine;

namespace RogueliteAutoBattler.Combat
{
    /// <summary>
    /// Drives <see cref="WorldConveyor"/> in a loop: wait, scroll, wait, scroll...
    /// All parameters are exposed in the Inspector.
    /// </summary>
    [RequireComponent(typeof(WorldConveyor))]
    public class CombatScrollManager : MonoBehaviour
    {
        [Header("Scroll Settings")]
        [Tooltip("Distance in world units for each scroll segment.")]
        [SerializeField] private float _scrollDistance = 10f;

        [Tooltip("Maximum scroll speed in units per second.")]
        [SerializeField] private float _maxSpeed = 5f;

        [Tooltip("Acceleration and deceleration in units per second squared.")]
        [SerializeField] private float _acceleration = 3f;

        [Header("Loop")]
        [Tooltip("Pause duration in seconds between scroll segments.")]
        [SerializeField] private float _pauseBetweenScrolls = 5f;

        [Header("References")]
        [SerializeField] private WorldConveyor _conveyor;

        private float _timer;
        private bool _waitingForScroll;

        private void Start()
        {
            if (_conveyor == null)
                TryGetComponent(out _conveyor);

            if (_conveyor == null)
            {
                Debug.LogWarning($"[{nameof(CombatScrollManager)}] No WorldConveyor found. Disabling.");
                enabled = false;
                return;
            }

            _conveyor.OnScrollComplete += OnScrollDone;
            _timer = _pauseBetweenScrolls;
            _waitingForScroll = false;
        }

        private void OnDestroy()
        {
            if (_conveyor != null)
                _conveyor.OnScrollComplete -= OnScrollDone;
        }

        private void Update()
        {
            if (_waitingForScroll)
                return;

            _timer -= Time.deltaTime;
            if (_timer <= 0f)
            {
                _conveyor.ScrollBy(_scrollDistance, _maxSpeed, _acceleration);
                _waitingForScroll = true;
            }
        }

        private void OnScrollDone()
        {
            _waitingForScroll = false;
            _timer = _pauseBetweenScrolls;
        }
    }
}
