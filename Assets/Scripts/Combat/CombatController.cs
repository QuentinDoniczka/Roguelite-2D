using System.Collections;
using UnityEngine;

namespace RogueliteAutoBattler.Combat
{
    public enum CombatState
    {
        None,
        Moving,
        Attacking,
        Dead
    }

    /// <summary>
    /// Manages auto-battle state for a single character. Reads distance to the target
    /// each FixedUpdate and transitions between Moving and Attacking states.
    /// Delegates movement to <see cref="CharacterMover"/> and animation to the Animator.
    /// Damage is dealt via animation event callback (<see cref="OnAnimationHit"/>),
    /// ensuring visual and logical synchronization.
    /// </summary>
    [RequireComponent(typeof(CharacterMover))]
    [RequireComponent(typeof(CombatStats))]
    public class CombatController : MonoBehaviour
    {
        [Header("Combat")]
        [Tooltip("Horizontal reach in world units (rectangle width).")]
        [SerializeField] private float _attackRange = 0.5f;

        // Vertical reach = half of horizontal. Forces face-to-face combat in a side-scroller.
        private const float VerticalRangeRatio = 0.25f;

        private const float ChopAttackDuration = 0.5f;
        private const float FadeOutDuration = 0.25f;

        private CharacterMover _mover;
        private Animator _animator;
        private bool _hasAnimator;
        private CombatStats _stats;
        private CombatStats _targetStats;
        private CombatState _state;
        private float _nextAttackTime;
        private bool _waitingForHit;

        /// <summary>Current combat state of this character.</summary>
        public CombatState State => _state;

        /// <summary>
        /// Callback to find a new target when the current one dies.
        /// Set by the spawning system (LevelManager or CombatSpawnManager).
        /// Returns null if no target is available.
        /// </summary>
        public System.Func<Transform> FindNewTarget { get; set; }

        /// <summary>The Transform this character moves toward and attacks.</summary>
        public Transform Target
        {
            get => _mover.Target;
            set => _mover.Target = value;
        }

        private void Awake()
        {
            _mover = GetComponent<CharacterMover>();
            _animator = GetComponentInChildren<Animator>();
            _hasAnimator = _animator != null;
            _stats = GetComponent<CombatStats>();

            if (_stats != null)
                _stats.OnDied += HandleSelfDied;
        }

        private void OnDestroy()
        {
            if (_stats != null)
                _stats.OnDied -= HandleSelfDied;

            UnsubscribeFromTarget();
        }

        private void FixedUpdate()
        {
            if (_state == CombatState.Dead || (_stats != null && _stats.IsDead))
                return;

            if (_mover == null || _mover.Target == null)
                return;

            float signedDeltaX = _mover.Target.position.x - transform.position.x;
            float facingSign = -Mathf.Sign(transform.localScale.x);
            bool targetInFront = signedDeltaX * facingSign >= 0f;

            float deltaX = Mathf.Abs(signedDeltaX);
            float deltaY = Mathf.Abs(_mover.Target.position.y - transform.position.y);
            bool inRange = targetInFront && deltaX <= _attackRange && deltaY <= _attackRange * VerticalRangeRatio;

            if (inRange)
                SetState(CombatState.Attacking);
            else
                SetState(CombatState.Moving);

            if (_state == CombatState.Attacking && !_waitingForHit && Time.time >= _nextAttackTime)
            {
                StartAttackSwing();
            }
        }

        private void SetState(CombatState newState)
        {
            if (_state == newState)
                return;

            // Cannot transition out of Dead
            if (_state == CombatState.Dead)
                return;

            _state = newState;

            switch (_state)
            {
                case CombatState.Moving:
                    UnsubscribeFromTarget();
                    _targetStats = null;
                    _mover.enabled = true;
                    _waitingForHit = false;
                    if (_hasAnimator)
                        _animator.speed = 1f;
                    break;

                case CombatState.Attacking:
                    _mover.Stop();
                    _mover.enabled = false;

                    UnsubscribeFromTarget();
                    _targetStats = _mover.Target != null
                        ? _mover.Target.GetComponent<CombatStats>()
                        : null;
                    if (_targetStats != null)
                        _targetStats.OnDied += HandleTargetDied;
                    break;

                case CombatState.Dead:
                    UnsubscribeFromTarget();
                    _mover.Stop();
                    _mover.enabled = false;
                    _waitingForHit = false;
                    if (_hasAnimator)
                    {
                        _animator.speed = 1f;
                        _animator.Play(AnimHashes.Idle);
                    }
                    StartCoroutine(FadeOutAndDestroy());
                    break;

                case CombatState.None:
                    UnsubscribeFromTarget();
                    _targetStats = null;
                    _mover.Stop();
                    _mover.enabled = false;
                    _waitingForHit = false;
                    if (_hasAnimator)
                    {
                        _animator.speed = 1f;
                        _animator.Play(AnimHashes.Idle);
                    }
                    break;
            }
        }

        private void HandleSelfDied()
        {
            SetState(CombatState.Dead);
        }

        private void HandleTargetDied()
        {
            UnsubscribeFromTarget();
            _targetStats = null;

            // Try to find a new target
            if (FindNewTarget != null)
            {
                Transform newTarget = FindNewTarget();
                if (newTarget != null)
                {
                    _mover.Target = newTarget;
                    SetState(CombatState.Moving);
                    return;
                }
            }

            // No target available — return to home anchor (CharacterMover handles it).
            _mover.Target = null;
            _waitingForHit = false;
            _mover.enabled = true;
            _state = CombatState.Moving;
            if (_hasAnimator)
                _animator.speed = 1f;
        }

        private void UnsubscribeFromTarget()
        {
            if (_targetStats != null)
                _targetStats.OnDied -= HandleTargetDied;
        }

        private IEnumerator FadeOutAndDestroy()
        {
            var renderers = GetComponentsInChildren<SpriteRenderer>();
            var startAlphas = new float[renderers.Length];
            for (int i = 0; i < renderers.Length; i++)
                startAlphas[i] = renderers[i].color.a;

            float elapsed = 0f;
            while (elapsed < FadeOutDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / FadeOutDuration);

                for (int i = 0; i < renderers.Length; i++)
                {
                    var c = renderers[i].color;
                    c.a = Mathf.Lerp(startAlphas[i], 0f, t);
                    renderers[i].color = c;
                }

                yield return null;
            }

            Destroy(gameObject);
        }

        /// <summary>Overrides the serialized attack range at runtime (set from EnemySpawnData on spawn).</summary>
        public void SetAttackRange(float range)
        {
            _attackRange = range;
        }

        private void StartAttackSwing()
        {
            if (_stats == null || _stats.AttackSpeed <= 0f)
                return;

            _waitingForHit = true;

            // Set cooldown NOW, not in OnAnimationHit — survives retarget/state changes.
            float attackInterval = 1f / _stats.AttackSpeed;
            _nextAttackTime = Time.time + attackInterval;
            float animSpeed = attackInterval < ChopAttackDuration
                ? ChopAttackDuration / attackInterval
                : 1f;

            if (_hasAnimator)
            {
                _animator.speed = animSpeed;
                _animator.Play(AnimHashes.ChopAttack, -1, 0f);
            }
        }

        /// <summary>
        /// Called by <see cref="AnimationEventRelay"/> when the attack animation
        /// reaches the hit frame. Deals damage and returns to idle.
        /// </summary>
        public void OnAnimationHit()
        {
            if (!_waitingForHit)
                return;

            if (_stats != null && _stats.IsDead)
                return;

            _waitingForHit = false;

            if (_stats != null && _targetStats != null && !_targetStats.IsDead)
            {
                // Capture before TakeDamage — it may kill the target, firing HandleTargetDied
                // synchronously which nulls _targetStats.
                string targetName = _targetStats.gameObject.name;
                int damage = _stats.Atk;
                _targetStats.TakeDamage(damage);
                Debug.Log($"[Attack] {name} hits {targetName} for {damage} dmg.");
            }

            if (_hasAnimator)
            {
                _animator.speed = 1f;
                _animator.Play(AnimHashes.Idle);
            }

            // Cooldown already set in StartAttackSwing.
        }
    }
}
