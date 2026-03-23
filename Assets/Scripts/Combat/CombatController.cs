using UnityEngine;

namespace RogueliteAutoBattler.Combat
{
    /// <summary>Combat states a character can be in during auto-battle.</summary>
    public enum CombatState
    {
        Moving,
        Attacking
    }

    /// <summary>
    /// Manages auto-battle state for a single character. Reads distance to the target
    /// each FixedUpdate and transitions between Moving and Attacking states.
    /// Delegates movement to <see cref="CharacterMover"/> and animation to the Animator.
    /// </summary>
    [RequireComponent(typeof(CharacterMover))]
    public class CombatController : MonoBehaviour
    {
        [Header("Combat")]
        [Tooltip("Distance in world units at which the character stops and attacks.")]
        [SerializeField] private float _attackRange = 0.5f;

        private CharacterMover _mover;
        private Animator _animator;
        private CombatState _state;

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
            _animator = GetComponent<Animator>();
        }

        private void FixedUpdate()
        {
            if (_mover.Target == null)
                return;

            float distance = Mathf.Abs(_mover.Target.position.x - transform.position.x);

            if (distance <= _attackRange)
                SetState(CombatState.Attacking);
            else
                SetState(CombatState.Moving);
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
                    break;

                case CombatState.Attacking:
                    _mover.Stop();
                    _mover.enabled = false;
                    if (_animator != null)
                        _animator.Play("ChopAttack");
                    break;
            }
        }
    }
}
