using UnityEngine;

namespace RogueliteAutoBattler.Combat
{
    /// <summary>
    /// Moves this character along the X axis toward a target Transform.
    /// Uses Rigidbody2D.linearVelocity for physics-based movement.
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
            _animator = GetComponentInChildren<Animator>();
            _rb = GetComponent<Rigidbody2D>();

            if (_rb == null)
                Debug.LogError($"[{nameof(CharacterMover)}] Rigidbody2D not found on {name}. Movement will not work.", this);

            if (_animator != null)
                _animator.applyRootMotion = false;
        }

        private void FixedUpdate()
        {
            if (_rb == null)
                return;

            if (_target == null)
            {
                _rb.linearVelocity = Vector2.zero;
                SetMoving(false);
                return;
            }

            float deltaX = _target.position.x - transform.position.x;
            _rb.linearVelocity = new Vector2(Mathf.Sign(deltaX) * _moveSpeed, 0f);
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
