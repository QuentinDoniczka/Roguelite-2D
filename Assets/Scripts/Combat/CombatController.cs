using UnityEngine;

namespace RogueliteAutoBattler.Combat
{
    public enum CombatState
    {
        None,
        Moving,
        Attacking
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
        }

        private void FixedUpdate()
        {
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

            _state = newState;

            switch (_state)
            {
                case CombatState.Moving:
                    _mover.enabled = true;
                    _waitingForHit = false;
                    if (_hasAnimator)
                        _animator.speed = 1f;
                    break;

                case CombatState.Attacking:
                    _mover.Stop();
                    _mover.enabled = false;
                    _targetStats = _mover.Target != null
                        ? _mover.Target.GetComponent<CombatStats>()
                        : null;
                    _attackTimer = 0f; // attack immediately on first transition
                    break;
            }
        }

        private void StartAttackSwing()
        {
            if (_stats == null || _stats.BaseStats == null || _stats.BaseStats.attackSpeed <= 0f)
                return;

            _waitingForHit = true;

            float attackInterval = 1f / _stats.BaseStats.attackSpeed;
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

            _waitingForHit = false;

            if (_stats != null && _targetStats != null && !_targetStats.IsDead)
            {
                _targetStats.TakeDamage(_stats.BaseStats.atk);
                Debug.Log($"[Attack] {name} hits {_targetStats.gameObject.name} for {_stats.BaseStats.atk} dmg. HP: {_targetStats.CurrentHp}/{_targetStats.MaxHp}");
            }

            if (_hasAnimator)
            {
                _animator.speed = 1f;
                _animator.Play(AnimIdle);
            }

            if (_stats != null && _stats.BaseStats != null)
                _attackTimer = 1f / _stats.BaseStats.attackSpeed;
        }
    }
}
