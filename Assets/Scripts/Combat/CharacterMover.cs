using UnityEngine;

namespace RogueliteAutoBattler.Combat
{
    /// <summary>
    /// Moves this character along the X axis toward a target Transform.
    /// Uses Rigidbody2D.linearVelocity for physics-based movement.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(CircleCollider2D))]
    public class CharacterMover : MonoBehaviour
    {
        [Header("Movement")]
        [Tooltip("Movement speed in world units per second.")]
        [SerializeField] private float _moveSpeed = 2f;

        // Characters aim for a point in front of their target (face-to-face), not on top.
        private const float FaceOffset = 0.25f;

        private const float HomeArrivalThreshold = 0.15f;
        private const float BlockCastRadius = 0.15f;
        private const float BlockCastDistance = 0.15f;

        private static readonly RaycastHit2D[] BlockCastBuffer = new RaycastHit2D[8];

        // Shared low-friction material so characters slide past each other.
        private static PhysicsMaterial2D _frictionlessMaterial;

        private Transform _target;
        private Transform _homeAnchor;
        private Animator _animator;
        private bool _hasAnimator;
        private Rigidbody2D _rb;
        private bool _isMoving;
        private bool _isBlocked;

        /// <summary>Cached Animator from GetComponentInChildren (may be null).</summary>
        public Animator Animator => _animator;

        /// <summary>Whether an Animator was found on this character.</summary>
        public bool HasAnimator => _hasAnimator;

        /// <summary>Whether the character is blocked by another combat unit ahead.</summary>
        public bool IsBlocked => _isBlocked;

        /// <summary>Fired once when the character becomes blocked by a friendly unit.</summary>
        public event System.Action OnBlocked;

        /// <summary>The Transform this character moves toward. Set to null to stop.</summary>
        public Transform Target
        {
            get => _target;
            set => _target = value;
        }

        /// <summary>
        /// Screen-absolute anchor to return to when Target is null.
        /// Should be a Transform outside CombatWorld (e.g. with ScreenAnchor component).
        /// </summary>
        public Transform HomeAnchor
        {
            get => _homeAnchor;
            set => _homeAnchor = value;
        }

        private void Awake()
        {
            _animator = GetComponentInChildren<Animator>();
            _hasAnimator = _animator != null;
            _rb = GetComponent<Rigidbody2D>();

            if (_rb == null)
                Debug.LogError($"[{nameof(CharacterMover)}] Rigidbody2D not found on {name}. Movement will not work.", this);
            else
                _rb.freezeRotation = true;

            // Low-friction material so characters push each other smoothly without gripping.
            if (_frictionlessMaterial == null)
            {
                _frictionlessMaterial = new PhysicsMaterial2D("Frictionless")
                {
                    friction = 0.15f,
                    bounciness = 0f
                };
            }

            var col = GetComponent<CircleCollider2D>();
            if (col != null)
                col.sharedMaterial = _frictionlessMaterial;

            if (_hasAnimator)
                _animator.applyRootMotion = false;
        }

        private void FixedUpdate()
        {
            if (_rb == null)
                return;

            if (_target == null)
            {
                _isBlocked = false;

                // No combat target — return to home anchor if available.
                if (_homeAnchor != null)
                {
                    float sqrDistToHome = ((Vector2)_homeAnchor.position - (Vector2)transform.position).sqrMagnitude;
                    if (sqrDistToHome > HomeArrivalThreshold * HomeArrivalThreshold)
                    {
                        Vector2 dir = ((Vector2)_homeAnchor.position - (Vector2)transform.position).normalized;
                        FlipToward(dir.x);
                        _rb.linearVelocity = dir * _moveSpeed;
                        SetMoving(true);
                    }
                    else
                    {
                        _rb.linearVelocity = Vector2.zero;
                        SetMoving(false);
                    }
                }
                else
                {
                    _rb.linearVelocity = Vector2.zero;
                    SetMoving(false);
                }
                return;
            }

            // Aim for a point in front of the target, not on top.
            // Use raw direction to target so offset is immune to flip changes.
            float rawDirX = _target.position.x - transform.position.x;
            float side = rawDirX > 0f ? -FaceOffset : FaceOffset;
            Vector2 destination = new Vector2(_target.position.x + side, _target.position.y);
            Vector2 direction = (destination - (Vector2)transform.position).normalized;

            FlipToward(direction.x);

            // CircleCastNonAlloc to detect blocking by another combat unit ahead.
            // Uses all hits so non-CombatStats colliders (ground, triggers) are skipped.
            int hitCount = Physics2D.CircleCastNonAlloc(
                (Vector2)transform.position,
                BlockCastRadius,
                direction,
                BlockCastBuffer,
                BlockCastDistance
            );

            bool blocked = false;
            for (int i = 0; i < hitCount; i++)
            {
                var h = BlockCastBuffer[i];
                if (h.transform != transform
                    && h.transform != _target
                    && h.transform.TryGetComponent<CombatStats>(out _))
                {
                    blocked = true;
                    break;
                }
            }

            if (blocked)
            {
                _rb.linearVelocity = Vector2.zero;
                SetMoving(false);
                if (!_isBlocked)
                {
                    _isBlocked = true;
                    OnBlocked?.Invoke();
                }
            }
            else
            {
                _rb.linearVelocity = direction * _moveSpeed;
                _isBlocked = false;
                SetMoving(true);
            }
        }

        /// <summary>Overrides the serialized move speed at runtime (set from stats on spawn).</summary>
        public void SetMoveSpeed(float speed)
        {
            _moveSpeed = speed;
        }

        /// <summary>Immediately zeroes velocity.</summary>
        public void Stop()
        {
            if (_rb != null)
                _rb.linearVelocity = Vector2.zero;
        }

        /// <summary>
        /// Flips the character sprite to face the given X direction.
        /// Sprites face LEFT natively: directionX > 0 flips to face right, directionX &lt; 0 keeps native.
        /// </summary>
        public void FlipToward(float directionX)
        {
            if (Mathf.Approximately(directionX, 0f))
                return;

            var s = transform.localScale;
            s.x = directionX > 0f ? -1f : 1f;
            transform.localScale = s;
        }

        private void SetMoving(bool moving)
        {
            if (_isMoving == moving)
                return;

            _isMoving = moving;

            if (_hasAnimator)
                _animator.Play(_isMoving ? AnimHashes.Walk : AnimHashes.Idle);
        }
    }
}
