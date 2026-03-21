using UnityEngine;

namespace RogueliteAutoBattler.Combat
{
    /// <summary>
    /// Moves this character along the X axis toward a target Transform.
    /// Stops when within <see cref="_stoppingDistance"/>. Drives the Animator
    /// <c>IsMoving</c> bool parameter when an Animator is present.
    /// </summary>
    public class CharacterMover : MonoBehaviour
    {
        [Header("Movement")]
        [Tooltip("Movement speed in world units per second.")]
        [SerializeField] private float _moveSpeed = 2f;

        [Tooltip("Distance at which the character stops approaching the target.")]
        [SerializeField] private float _stoppingDistance = 0.5f;

        private Transform _target;
        private Animator _animator;
        private bool _isMoving;
        private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");

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
        }

        private void Update()
        {
            if (_target == null)
            {
                SetMoving(false);
                return;
            }

            float deltaX = _target.position.x - transform.position.x;

            if (Mathf.Abs(deltaX) <= _stoppingDistance)
            {
                SetMoving(false);
                return;
            }

            // Move only in X — Y and Z are never touched.
            Vector3 pos = transform.position;
            pos.x = Mathf.MoveTowards(pos.x, _target.position.x, _moveSpeed * Time.deltaTime);
            transform.position = pos;

            SetMoving(true);
        }

        private void SetMoving(bool moving)
        {
            if (_isMoving == moving)
                return;

            _isMoving = moving;

            if (_animator != null)
                _animator.SetBool(IsMovingHash, _isMoving);
        }
    }
}
