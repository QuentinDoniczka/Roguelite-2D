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
        private float _faceOffset = 0.25f;

        private const float HomeArrivalThreshold = 0.15f;

        // Shared zero-friction material so characters slide past each other.
        private static PhysicsMaterial2D _frictionlessMaterial;

        private Transform _target;
        private Transform _homeAnchor;
        private Animator _animator;
        private Rigidbody2D _rb;
        private bool _isMoving;

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
            _rb = GetComponent<Rigidbody2D>();

            if (_rb == null)
                Debug.LogError($"[{nameof(CharacterMover)}] Rigidbody2D not found on {name}. Movement will not work.", this);
            else
                _rb.freezeRotation = true;

            // Zero-friction material so characters push each other smoothly without gripping.
            if (_frictionlessMaterial == null)
            {
                _frictionlessMaterial = new PhysicsMaterial2D("Frictionless")
                {
                    friction = 0f,
                    bounciness = 0f
                };
            }

            var col = GetComponent<CircleCollider2D>();
            if (col != null)
                col.sharedMaterial = _frictionlessMaterial;

            if (_animator != null)
                _animator.applyRootMotion = false;
        }

        private void FixedUpdate()
        {
            if (_rb == null)
                return;

            if (_target == null)
            {
                // No combat target — return to home anchor if available.
                if (_homeAnchor != null)
                {
                    float distToHome = Vector2.Distance(transform.position, _homeAnchor.position);
                    if (distToHome > HomeArrivalThreshold)
                    {
                        Vector2 dir = ((Vector2)_homeAnchor.position - (Vector2)transform.position).normalized;
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
            // Always approach from the side matching the character's facing direction.
            // Facing right (scale.x < 0) → stand to the target's left (-offset).
            // Facing left  (scale.x > 0) → stand to the target's right (+offset).
            float side = (transform.localScale.x < 0f) ? -_faceOffset : _faceOffset;
            Vector2 destination = new Vector2(_target.position.x + side, _target.position.y);
            Vector2 direction = (destination - (Vector2)transform.position).normalized;
            _rb.linearVelocity = direction * _moveSpeed;
            SetMoving(true);
        }

        /// <summary>Overrides the serialized move speed at runtime (set from stats on spawn).</summary>
        public void SetMoveSpeed(float speed)
        {
            _moveSpeed = speed;
        }

        /// <summary>Sets how far in front of the target this character aims to stand.</summary>
        public void SetFaceOffset(float offset)
        {
            _faceOffset = offset;
        }

        /// <summary>Immediately zeroes velocity.</summary>
        public void Stop()
        {
            if (_rb != null)
                _rb.linearVelocity = Vector2.zero;
        }

        private void SetMoving(bool moving)
        {
            if (_isMoving == moving)
                return;

            _isMoving = moving;

            if (_animator != null)
                _animator.Play(_isMoving ? AnimHashes.Walk : AnimHashes.Idle);
        }
    }
}
