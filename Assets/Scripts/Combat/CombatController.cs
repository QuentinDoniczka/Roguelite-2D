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
        [Tooltip("Attack reach in world units (distance check).")]
        [SerializeField] private float _attackRange = 0.5f;

        private const float ChopAttackDuration = 0.5f;
        private const float FadeOutDuration = 0.25f;

        private CharacterMover _mover;
        private Animator _animator;
        private bool _hasAnimator;
        private CombatStats _stats;
        private bool _hasStats;
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
            _animator = _mover.Animator;
            _hasAnimator = _mover.HasAnimator;
            _stats = GetComponent<CombatStats>();
            _hasStats = _stats != null;

            if (_hasStats)
                _stats.OnDied += HandleSelfDied;

            _mover.OnBlocked += HandleBlocked;
        }

        private void OnDestroy()
        {
            // Use Unity's null check (not cached _hasStats) — the object may be destroyed at teardown.
            if (_stats != null)
                _stats.OnDied -= HandleSelfDied;

            if (_mover != null)
                _mover.OnBlocked -= HandleBlocked;

            UnsubscribeFromTarget();
        }

        private void FixedUpdate()
        {
            if (_state == CombatState.Dead || (_hasStats && _stats.IsDead))
                return;

            if (_mover == null || _mover.Target == null)
                return;

            float dist = Vector2.Distance(transform.position, _mover.Target.position);
            bool inRange = dist <= _attackRange;

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
                    FaceTarget();

                    UnsubscribeFromTarget();
                    // Runs only on state transition, not per-frame.
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

            // Try to find a new target.
            Transform newTarget = FindNewTarget?.Invoke();
            _mover.Target = newTarget; // null if none — CharacterMover returns to home anchor.
            SetState(CombatState.Moving);
        }

        private void HandleBlocked()
        {
            if (_state != CombatState.Moving)
                return;
            if (FindNewTarget == null)
                return;

            Transform current = _mover.Target;
            Transform alternative = FindNewTarget();

            if (alternative != null && alternative != current)
            {
                _mover.Target = alternative;
            }
        }

        private void FaceTarget()
        {
            if (_mover.Target == null) return;

            float dirX = _mover.Target.position.x - transform.position.x;
            _mover.FlipToward(dirX);
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
            FaceTarget();

            if (!_hasStats || _stats.AttackSpeed <= 0f)
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

            if (_hasStats && _stats.IsDead)
                return;

            _waitingForHit = false;

            if (_hasStats && _targetStats != null && !_targetStats.IsDead)
            {
                int damage = _stats.Atk;
#if UNITY_EDITOR
                // Capture name before TakeDamage — it may kill the target, firing
                // HandleTargetDied synchronously which nulls _targetStats.
                string targetName = _targetStats.gameObject.name;
#endif
                _targetStats.TakeDamage(damage);
#if UNITY_EDITOR
                Debug.Log($"[Attack] {name} hits {targetName} for {damage} dmg.");
#endif
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
