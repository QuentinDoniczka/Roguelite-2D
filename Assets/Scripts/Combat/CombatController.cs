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
    public class CombatController : MonoBehaviour
    {
        [Header("Combat")]
        [Tooltip("Distance in world units at which the character stops and attacks.")]
        [SerializeField] private float _attackRange = 0.5f;

        private const float ChopAttackDuration = 0.5f;
        private const float FadeOutDuration = 0.25f;
        private const string AnimChopAttack = "ChopAttack";
        private const string AnimIdle = "Idle";

        private CharacterMover _mover;
        private Animator _animator;
        private bool _hasAnimator;
        private CombatStats _stats;
        private CombatStats _targetStats;
        private CombatState _state;
        private float _attackTimer;
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

            float distance = Mathf.Abs(_mover.Target.position.x - transform.position.x);

            if (distance <= _attackRange)
                SetState(CombatState.Attacking);
            else
                SetState(CombatState.Moving);

            if (_state == CombatState.Attacking && !_waitingForHit)
            {
                _attackTimer -= Time.fixedDeltaTime;
                if (_attackTimer <= 0f)
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

                    _attackTimer = 0f; // attack immediately on first transition
                    break;

                case CombatState.Dead:
                    UnsubscribeFromTarget();
                    _mover.Stop();
                    _mover.enabled = false;
                    _waitingForHit = false;
                    if (_hasAnimator)
                    {
                        _animator.speed = 1f;
                        _animator.Play(AnimIdle);
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
                        _animator.Play(AnimIdle);
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

            SetState(CombatState.None);
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

            float attackInterval = 1f / _stats.AttackSpeed;
            float animSpeed = attackInterval < ChopAttackDuration
                ? ChopAttackDuration / attackInterval
                : 1f;

            if (_hasAnimator)
            {
                _animator.speed = animSpeed;
                _animator.Play(AnimChopAttack, -1, 0f);
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
                _targetStats.TakeDamage(_stats.Atk);
                Debug.Log($"[Attack] {name} hits {_targetStats.gameObject.name} for {_stats.Atk} dmg. HP: {_targetStats.CurrentHp}/{_targetStats.MaxHp}");
            }

            if (_hasAnimator)
            {
                _animator.speed = 1f;
                _animator.Play(AnimIdle);
            }

            if (_stats != null && _stats.AttackSpeed > 0f)
                _attackTimer = 1f / _stats.AttackSpeed;
        }
    }
}
