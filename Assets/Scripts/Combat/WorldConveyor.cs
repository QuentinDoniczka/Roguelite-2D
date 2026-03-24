using UnityEngine;

namespace RogueliteAutoBattler.Combat
{
    /// <summary>
    /// Moves its own transform along the X axis with symmetric acceleration/deceleration.
    /// Attach to the CombatWorld root. Driven externally via <see cref="ScrollBy"/>.
    /// Uses a kinematic Rigidbody2D so child dynamic bodies keep their correct velocity.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class WorldConveyor : MonoBehaviour
    {
        private float _targetX;
        private float _currentSpeed;
        private float _maxSpeed;
        private float _acceleration;
        private bool _isScrolling;
        private bool _decelerating;
        private Rigidbody2D _rb;

        [Header("Defaults")]
        [Tooltip("Default maximum scroll speed in units per second.")]
        [SerializeField] private float _defaultMaxSpeed = 1f;

        [Tooltip("Default acceleration/deceleration in units per second squared.")]
        [SerializeField] private float _defaultAcceleration = 0.5f;

        /// <summary>Current scroll speed in world units per second.</summary>
        public float CurrentSpeed => _currentSpeed;

        /// <summary>True while a scroll is in progress.</summary>
        public bool IsScrolling => _isScrolling;

        /// <summary>Fired when a scroll reaches its target position.</summary>
        public event System.Action OnScrollComplete;

        /// <summary>Fired once when the scroll enters the deceleration phase.</summary>
        public event System.Action OnDecelerationStarted;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.bodyType = RigidbodyType2D.Kinematic;
        }

        /// <summary>
        /// Starts a scroll using the default max speed and acceleration configured in the Inspector.
        /// </summary>
        public void ScrollBy(float distance)
        {
            ScrollBy(distance, _defaultMaxSpeed, _defaultAcceleration);
        }

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

            _targetX = _rb.position.x - distance;
            _maxSpeed = maxSpeed;
            _acceleration = acceleration;
            _currentSpeed = 0f;
            _isScrolling = true;
            _decelerating = false;
        }

        private void FixedUpdate()
        {
            if (!_isScrolling)
                return;

            float posX = _rb.position.x;
            float remaining = Mathf.Abs(_targetX - posX);

            // Close enough — snap and finish.
            if (remaining < 0.01f)
            {
                Arrive();
                return;
            }

            float dt = Time.fixedDeltaTime;

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

                _currentSpeed -= _acceleration * dt;
                if (_currentSpeed < 0.1f)
                    _currentSpeed = 0.1f;
            }
            else if (_currentSpeed < _maxSpeed)
            {
                // Accelerate
                _currentSpeed += _acceleration * dt;
                if (_currentSpeed > _maxSpeed)
                    _currentSpeed = _maxSpeed;
            }

            float direction = Mathf.Sign(_targetX - posX);
            float step = _currentSpeed * dt;

            if (step >= remaining)
            {
                Arrive();
            }
            else
            {
                _rb.MovePosition(new Vector2(posX + direction * step, _rb.position.y));
            }
        }

        private void Arrive()
        {
            _rb.MovePosition(new Vector2(_targetX, _rb.position.y));
            _currentSpeed = 0f;
            _isScrolling = false;
            OnScrollComplete?.Invoke();
        }
    }
}
