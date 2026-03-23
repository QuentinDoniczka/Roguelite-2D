using UnityEngine;

namespace RogueliteAutoBattler.Combat
{
    /// <summary>Combat states a character can be in during auto-battle.</summary>
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
    /// Requires <see cref="CombatStats"/> on the same GameObject (added at spawn).
    /// </summary>
    public class CombatController : MonoBehaviour
    {
        [Header("Combat")]
        [Tooltip("Distance in world units at which the character stops and attacks.")]
        [SerializeField] private float _attackRange = 0.5f;

        private CharacterMover _mover;
        private Animator _animator;
        private CombatStats _stats;
        private CombatStats _targetStats;
        private CombatState _state;
        private float _attackTimer;

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
            // Animator lives on the Visual child, not on this root GameObject.
            _animator = GetComponentInChildren<Animator>();
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

            if (_state == CombatState.Attacking)
            {
                _attackTimer -= Time.fixedDeltaTime;
                if (_attackTimer <= 0f)
                {
                    Attack();
                    _attackTimer = 1f / _stats.BaseStats.attackSpeed;
                }
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
                    if (_animator != null)
                        _animator.speed = 1f;
                    break;

                case CombatState.Attacking:
                    _mover.Stop();
                    _mover.enabled = false;
                    _targetStats = _mover.Target != null
                        ? _mover.Target.GetComponent<CombatStats>()
                        : null;
                    _attackTimer = 0f; // attack immediately on first hit
                    if (_animator != null && _stats != null)
                    {
                        _animator.speed = _stats.BaseStats.attackSpeed;
                        _animator.Play("ChopAttack");
                    }
                    break;
            }
        }

        private void Attack()
        {
            if (_targetStats != null && !_targetStats.IsDead)
            {
                _targetStats.TakeDamage(_stats.BaseStats.atk);
                if (_animator != null)
                    _animator.Play("ChopAttack", -1, 0f); // restart from beginning
            }
        }
    }
}
