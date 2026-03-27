using UnityEngine;

namespace RogueliteAutoBattler.Combat
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(CircleCollider2D))]
    public class CharacterMover : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float _moveSpeed = 2f;

        private const float HomeArrivalThreshold = 0.15f;
        private const float HomeDampingFactor = 8f;
        private const float ScrollWalkRatio = 0.9f;

        private static PhysicsMaterial2D _frictionlessMaterial;

        private Transform _target;
        private Transform _homeAnchor;
        private Animator _animator;
        private bool _hasAnimator;
        private Rigidbody2D _rb;
        private CircleCollider2D _col;
        private WorldConveyor _conveyor;
        private bool _isMoving;
        private bool _isCharge;
        private Vector2 _homeOffset;
        private float _homeFacingX;

        public Animator Animator => _animator;
        public bool HasAnimator => _hasAnimator;

        public Transform Target
        {
            get => _target;
            set => _target = value;
        }

        public Transform HomeAnchor
        {
            get => _homeAnchor;
            set => _homeAnchor = value;
        }

        public Vector2 HomeOffset => _homeOffset;

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

            if (_frictionlessMaterial == null)
            {
                _frictionlessMaterial = new PhysicsMaterial2D("LowFriction")
                {
                    friction = 0.15f,
                    bounciness = 0f
                };
            }

            _col = GetComponent<CircleCollider2D>();
            if (_col != null)
                _col.sharedMaterial = _frictionlessMaterial;

            if (_hasAnimator)
                _animator.applyRootMotion = false;

            _homeFacingX = transform.localScale.x < 0f ? 1f : -1f;
        }

        private void FixedUpdate()
        {
            if (_rb == null)
                return;

            if (_target == null)
            {
                if (_col != null) _col.enabled = false;

                bool scrolling = _conveyor != null && _conveyor.IsScrolling;

                if (scrolling)
                {
                    Vector2 scroll = _conveyor.ScrollVelocity;
                    float walkSpeed = _conveyor.MaxSpeed * ScrollWalkRatio;

                    Vector2 walkVel = Vector2.zero;
                    if (_homeAnchor != null)
                    {
                        Vector2 toHome = (Vector2)_homeAnchor.position + _homeOffset
                                       - (Vector2)transform.position;
                        if (toHome.sqrMagnitude > 0.0001f)
                        {
                            walkVel = toHome.normalized * walkSpeed;
                            FlipToward(toHome.x);
                        }
                    }

                    _rb.linearVelocity = scroll + walkVel;
                    SetMoving(true, false);
                    return;
                }

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

            if (_col != null) _col.enabled = true;

            Vector2 destination = (Vector2)_target.position;
            Vector2 direction = (destination - (Vector2)transform.position).normalized;

            FlipToward(direction.x);

            _rb.linearVelocity = direction * _moveSpeed;
            SetMoving(true, true);
        }

        public void SetMoveSpeed(float speed)
        {
            _moveSpeed = speed;
        }

        public void SetHomeOffset(Vector2 offset)
        {
            _homeOffset = offset;
        }

        public void Stop()
        {
            if (_rb != null)
                _rb.linearVelocity = Vector2.zero;
        }

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

            _animator.SetBool(AnimHashes.IsMoving, _isMoving);

            if (!_isMoving)
            {
                _animator.Play(AnimHashes.Idle);
                return;
            }

            _animator.Play(_isCharge ? AnimHashes.Run : AnimHashes.Walk);
        }
    }
}
