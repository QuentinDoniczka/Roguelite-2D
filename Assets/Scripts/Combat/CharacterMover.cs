using UnityEngine;

namespace RogueliteAutoBattler.Combat
{
    /// <summary>
    /// Moves this character along the X axis toward a target Transform.
    /// Uses Rigidbody2D velocity for physics-based movement when available,
    /// falls back to transform.position if needed.
    /// </summary>
    public class CharacterMover : MonoBehaviour
    {
        [Header("Movement")]
        [Tooltip("Movement speed in world units per second.")]
        [SerializeField] private float _moveSpeed = 2f;

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

        private void Awake()
        {
            // Animator lives on the Visual child, not on this root GameObject.
            _animator = GetComponentInChildren<Animator>();
            _rb = GetComponent<Rigidbody2D>();

            if (_animator != null)
                _animator.applyRootMotion = false;

            if (_rb != null)
            {
                _rb.bodyType = RigidbodyType2D.Dynamic;
                _rb.gravityScale = 0f;
                _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
                _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            }
        }

        private void FixedUpdate()
        {
            if (_target == null)
            {
                if (_rb != null) _rb.linearVelocity = Vector2.zero;
                SetMoving(false);
                return;
            }

            float deltaX = _target.position.x - transform.position.x;

            if (_rb != null)
            {
                // Physics-based movement
                float dirX = Mathf.Sign(deltaX);
                _rb.linearVelocity = new Vector2(dirX * _moveSpeed, 0f);
            }
            else
            {
                // Fallback: direct transform movement
                Vector3 pos = transform.position;
                pos.x = Mathf.MoveTowards(pos.x, _target.position.x, _moveSpeed * Time.fixedDeltaTime);
                transform.position = pos;
            }

            SetMoving(true);
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
                _animator.Play(_isMoving ? "Walk" : "Idle");
        }
    }
}
