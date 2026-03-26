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
        private const float HomeDampingFactor = 8f;
        private const float WalkAnimationThreshold = 0.05f;

        // Shared low-friction material so characters slide past each other.
        private static PhysicsMaterial2D _frictionlessMaterial;

        private Transform _target;
        private Transform _homeAnchor;
        private Animator _animator;
        private bool _hasAnimator;
        private Rigidbody2D _rb;
        private WorldConveyor _conveyor;
        private bool _isMoving;
        private bool _isCharge;
        private Vector2 _homeOffset;
        private float _homeFacingX;

        /// <summary>Cached Animator from GetComponentInChildren (may be null).</summary>
        public Animator Animator => _animator;

        /// <summary>Whether an Animator was found on this character.</summary>
        public bool HasAnimator => _hasAnimator;

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
            _conveyor = GetComponentInParent<WorldConveyor>();

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

            // Store initial facing direction to restore on home arrival.
            _homeFacingX = transform.localScale.x < 0f ? 1f : -1f;
        }

        private void FixedUpdate()
        {
            if (_rb == null)
                return;

            if (_target == null)
            {
                bool scrolling = _conveyor != null && _conveyor.IsScrolling;

                // During scroll: let the kinematic parent carry the character.
                // Do NOT home toward the anchor — the conveyor drift IS the desired motion.
                if (scrolling)
                {
                    _rb.linearVelocity = Vector2.zero;
                    SetMoving(true, false);
                    return;
                }

                // No combat target — return to home anchor if available.
                if (_homeAnchor != null)
                {
                    Vector2 homePos = (Vector2)_homeAnchor.position + _homeOffset;
                    Vector2 currentPos = (Vector2)transform.position;
                    float distToHome = Vector2.Distance(homePos, currentPos);
                    if (distToHome > HomeArrivalThreshold)
                    {
                        Vector2 dir = (homePos - currentPos).normalized;
                        FlipToward(dir.x);
                        float correctionSpeed = Mathf.Min(distToHome * HomeDampingFactor, _moveSpeed);
                        _rb.linearVelocity = dir * correctionSpeed;
                        SetMoving(true, false);
                    }
                    else
                    {
                        _rb.linearVelocity = Vector2.zero;
                        SetMoving(false, false);
                        FlipToward(_homeFacingX);
                    }
                }
                else
                {
                    _rb.linearVelocity = Vector2.zero;
                    SetMoving(false, false);
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

            _rb.linearVelocity = direction * _moveSpeed;
            SetMoving(true, true);
        }

        /// <summary>Overrides the serialized move speed at runtime (set from stats on spawn).</summary>
        public void SetMoveSpeed(float speed)
        {
            _moveSpeed = speed;
        }

        /// <summary>Sets the formation offset from the home anchor for this character's home position.</summary>
        public void SetHomeOffset(Vector2 offset)
        {
            _homeOffset = offset;
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

        private void SetMoving(bool moving, bool isCharge)
        {
            if (_isMoving == moving && _isCharge == isCharge)
                return;

            _isMoving = moving;
            _isCharge = isCharge;

            if (!_hasAnimator)
                return;

            if (!_isMoving)
            {
                _animator.Play(AnimHashes.Idle);
                return;
            }

            _animator.Play(_isCharge ? AnimHashes.Run : AnimHashes.Walk);
        }
    }
}
