using UnityEngine;

namespace RogueliteAutoBattler.Combat.Environment
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class WorldConveyor : MonoBehaviour
    {
        private const float ArrivalThreshold = 0.01f;
        private const float BrakingDistanceBuffer = 0.1f;
        private const float MinimumScrollSpeed = 0.1f;

        private float _targetX;
        private float _currentSpeed;
        private float _maxSpeed;
        private float _acceleration;
        private bool _isScrolling;
        private bool _decelerating;
        private Rigidbody2D _rb;
        private Vector2 _initialPosition;

        [Header("Defaults")]
        [SerializeField] private float _defaultMaxSpeed = 1f;
        [SerializeField] private float _defaultAcceleration = 0.3f;

        public float CurrentSpeed => _currentSpeed;
        public float MaxSpeed => _maxSpeed;

        public Vector2 ScrollVelocity => _isScrolling
            ? new Vector2(Mathf.Sign(_targetX - _rb.position.x) * _currentSpeed, 0f)
            : Vector2.zero;

        public bool IsScrolling => _isScrolling;

        public event System.Action OnScrollComplete;
        public event System.Action OnDecelerationStarted;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.bodyType = RigidbodyType2D.Kinematic;
            _initialPosition = _rb.position;
        }

        public void ScrollBy(float distance)
        {
            ScrollBy(distance, _defaultMaxSpeed, _defaultAcceleration);
        }

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

            if (remaining < ArrivalThreshold)
            {
                Arrive();
                return;
            }

            float dt = Time.fixedDeltaTime;
            float brakingDist = (_currentSpeed * _currentSpeed) / (2f * _acceleration);

            if (remaining <= brakingDist + BrakingDistanceBuffer)
            {
                if (!_decelerating)
                {
                    _decelerating = true;
                    OnDecelerationStarted?.Invoke();
                }

                _currentSpeed -= _acceleration * dt;
                if (_currentSpeed < MinimumScrollSpeed)
                    _currentSpeed = MinimumScrollSpeed;
            }
            else if (_currentSpeed < _maxSpeed)
            {
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

        public void ResetPosition()
        {
            _isScrolling = false;
            _decelerating = false;
            _currentSpeed = 0f;
            _rb.position = _initialPosition;
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
