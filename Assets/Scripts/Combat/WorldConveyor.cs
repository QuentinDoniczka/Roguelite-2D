using UnityEngine;

namespace RogueliteAutoBattler.Combat
{
    /// <summary>
    /// Moves its own transform along the X axis with symmetric acceleration/deceleration.
    /// Attach to the CombatWorld root. Driven externally via <see cref="ScrollBy"/>.
    /// </summary>
    public class WorldConveyor : MonoBehaviour
    {
        private float _targetX;
        private float _currentSpeed;
        private float _maxSpeed;
        private float _acceleration;
        private bool _isScrolling;
        private bool _decelerating;

        /// <summary>Current scroll speed in world units per second.</summary>
        public float CurrentSpeed => _currentSpeed;

        /// <summary>True while a scroll is in progress.</summary>
        public bool IsScrolling => _isScrolling;

        /// <summary>Fired when a scroll reaches its target position.</summary>
        public event System.Action OnScrollComplete;

        /// <summary>Fired once when the scroll enters the deceleration phase.</summary>
        public event System.Action OnDecelerationStarted;

        /// <summary>
        /// Starts a scroll. The world moves <paramref name="distance"/> units to the left (negative X).
        /// Accelerates from 0 to <paramref name="maxSpeed"/>, then decelerates symmetrically to stop at the target.
        /// </summary>
        public void ScrollBy(float distance, float maxSpeed, float acceleration)
        {
            if (acceleration <= 0f)
            {
                Debug.LogWarning($"[{nameof(WorldConveyor)}] acceleration must be > 0 (was {acceleration}). Ignoring scroll.");
                return;
            }

            if (distance <= 0f)
            {
                Debug.LogWarning($"[{nameof(WorldConveyor)}] distance must be > 0 (was {distance}). Ignoring scroll.");
                return;
            }

            if (maxSpeed <= 0f)
            {
                Debug.LogWarning($"[{nameof(WorldConveyor)}] maxSpeed must be > 0 (was {maxSpeed}). Ignoring scroll.");
                return;
            }

            _targetX = transform.position.x - distance;
            _maxSpeed = maxSpeed;
            _acceleration = acceleration;
            _currentSpeed = 0f;
            _isScrolling = true;
            _decelerating = false;
        }

        private void Update()
        {
            if (!_isScrolling)
                return;

            Vector3 pos = transform.position;
            float posX = pos.x;
            float remaining = Mathf.Abs(_targetX - posX);

            // Close enough — snap and finish.
            if (remaining < 0.01f)
            {
                Arrive(pos);
                return;
            }

            // Braking distance: v^2 / (2a)
            float brakingDist = (_currentSpeed * _currentSpeed) / (2f * _acceleration);

            if (remaining <= brakingDist + 0.1f)
            {
                // Decelerate
                if (!_decelerating)
                {
                    _decelerating = true;
                    OnDecelerationStarted?.Invoke();
                }

                _currentSpeed -= _acceleration * Time.deltaTime;
                if (_currentSpeed < 0.1f)
                    _currentSpeed = 0.1f;
            }
            else if (_currentSpeed < _maxSpeed)
            {
                // Accelerate
                _currentSpeed += _acceleration * Time.deltaTime;
                if (_currentSpeed > _maxSpeed)
                    _currentSpeed = _maxSpeed;
            }

            float direction = Mathf.Sign(_targetX - posX);
            float step = _currentSpeed * Time.deltaTime;

            if (step >= remaining)
            {
                Arrive(pos);
            }
            else
            {
                transform.position = new Vector3(posX + direction * step, pos.y, pos.z);
            }
        }

        private void Arrive(Vector3 pos)
        {
            transform.position = new Vector3(_targetX, pos.y, pos.z);
            _currentSpeed = 0f;
            _isScrolling = false;
            OnScrollComplete?.Invoke();
        }
    }
}
