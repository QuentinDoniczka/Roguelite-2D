using UnityEngine;

namespace RogueliteAutoBattler.Combat
{
    /// <summary>
    /// Moves this character along the X axis toward a target Transform using Rigidbody2D velocity.
    /// Stops when within <see cref="_stoppingDistance"/>. Drives the Animator via Play("Walk"/"Idle").
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class CharacterMover : MonoBehaviour
    {
        [Header("Movement")]
        [Tooltip("Movement speed in world units per second.")]
        [SerializeField] private float _moveSpeed = 2f;

        [Tooltip("Distance at which the character stops approaching the target.")]
        [SerializeField] private float _stoppingDistance = 0.5f;

        private Transform _target;
        private Animator _animator;
        private Rigidbody2D _rb;
        private bool _isMoving;

        /// <summary>The Transform this character moves toward. Set to null to stop.</summary>
        public Transform Target
        {
            get => _target;
            set => _target = value;
        }

        /// <summary>True while the character is actively moving toward its target.</summary>
        public bool IsMoving => _isMoving;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _rb = GetComponent<Rigidbody2D>();

            if (_animator != null)
                _animator.applyRootMotion = false;

            // Configure Rigidbody2D for kinematic-like movement via velocity.
            _rb.gravityScale = 0f;
            _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        private void FixedUpdate()
        {
            if (_target == null)
            {
                _rb.linearVelocity = Vector2.zero;
                SetMoving(false);
                return;
            }

            float deltaX = _target.position.x - transform.position.x;

            if (Mathf.Abs(deltaX) <= _stoppingDistance)
            {
                _rb.linearVelocity = Vector2.zero;
                SetMoving(false);
                return;
            }

            float dirX = Mathf.Sign(deltaX);
            _rb.linearVelocity = new Vector2(dirX * _moveSpeed, 0f);
            SetMoving(true);
        }

        private void SetMoving(bool moving)
        {
            if (_isMoving == moving)
                return;

            _isMoving = moving;

            if (_animator != null)
                _animator.Play(_isMoving ? "Walk" : "Idle");
        }
    }
}
