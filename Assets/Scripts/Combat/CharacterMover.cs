using UnityEngine;

namespace RogueliteAutoBattler.Combat
{
    /// <summary>
    /// Moves this character along the X axis toward a target Transform using Rigidbody2D velocity.
    /// Drives the Animator via Play("Walk"/"Idle"). Stop decisions (e.g. attack range) are
    /// the responsibility of the caller — use <see cref="Stop"/> or disable this component.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
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
            _animator = GetComponent<Animator>();
            _rb = GetComponent<Rigidbody2D>();

            if (_animator != null)
                _animator.applyRootMotion = false;

            // Configure Rigidbody2D for movement via velocity.
            _rb.bodyType = RigidbodyType2D.Dynamic;
            _rb.gravityScale = 0f;
            _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            _rb.interpolation = RigidbodyInterpolation2D.Interpolate;

            Debug.Log($"[CharacterMover] Awake on {gameObject.name} — rb={_rb != null}, animator={_animator != null}");
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
            float dirX = Mathf.Sign(deltaX);
            _rb.linearVelocity = new Vector2(dirX * _moveSpeed, 0f);
            SetMoving(true);

            Debug.Log($"[CharacterMover] {gameObject.name} deltaX={deltaX:F2} vel={_rb.linearVelocity} pos={transform.position}");
        }

        /// <summary>
        /// Immediately zeroes velocity. Does not play any animation — the caller is
        /// responsible for driving the Animator after this point (e.g. CombatController).
        /// </summary>
        public void Stop()
        {
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
