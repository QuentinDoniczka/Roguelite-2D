using System.Collections;
using UnityEngine;

namespace RogueliteAutoBattler.Combat.Core
{
    public enum CombatState
    {
        None,
        Moving,
        Attacking,
        Dead
    }

    [RequireComponent(typeof(CharacterMover))]
    [RequireComponent(typeof(CombatStats))]
    public class CombatController : MonoBehaviour
    {
        [Header("Combat")]
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
        private bool _attackerFacesRight;

        public CombatState State => _state;

        public System.Func<Transform> FindNewTarget { get; set; }

        public void SetAttackerFacing(bool facesRight)
        {
            _attackerFacesRight = facesRight;
        }

        public Transform Target
        {
            get => _mover.Target;
            set
            {
                if (_mover != null && _mover.Target != null)
                    AttackSlotRegistry.Release(_mover.Target, transform);

                UnsubscribeFromTarget();
                _targetStats = null;
                _mover.Target = value;

                if (value != null)
                {
                    AttackSlotRegistry.Acquire(value, transform, _attackerFacesRight);

                    _targetStats = value.GetComponent<CombatStats>();
                    if (_targetStats != null)
                        _targetStats.OnDied += HandleTargetDied;
                }
            }
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
        }

        private void OnDestroy()
        {
            if (_mover != null && _mover.Target != null)
                AttackSlotRegistry.Release(_mover.Target, transform);

            if (_stats != null)
                _stats.OnDied -= HandleSelfDied;

            UnsubscribeFromTarget();
        }

        private void FixedUpdate()
        {
            if (_state == CombatState.Dead || (_hasStats && _stats.IsDead))
                return;

            if (_mover == null || _mover.Target == null)
                return;

            if (_targetStats != null && _targetStats.IsDead)
            {
                HandleTargetDied();
                return;
            }

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

            if (_state == CombatState.Dead)
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
                    FaceTarget();
                    if (_hasAnimator)
                    {
                        _animator.speed = 1f;
                        _animator.SetBool(AnimHashes.IsMoving, false);
                        _animator.Play(AnimHashes.Idle);
                    }
                    break;

                case CombatState.Dead:
                    EnterInactiveState();
                    StartCoroutine(FadeOutAndDestroy());
                    break;

                case CombatState.None:
                    EnterInactiveState();
                    break;
            }
        }

        private void EnterInactiveState()
        {
            _mover.Stop();
            _mover.enabled = false;
            _waitingForHit = false;
            if (_hasAnimator)
            {
                _animator.speed = 1f;
                _animator.Play(AnimHashes.Idle);
            }
        }

        private void HandleSelfDied()
        {
            SetState(CombatState.Dead);
        }

        private void HandleTargetDied()
        {
            if (_state == CombatState.Dead)
                return;

            Transform newTarget = FindNewTarget?.Invoke();
            Target = newTarget;
            SetState(CombatState.Moving);
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

        public void Disengage()
        {
            Target = null;
            FindNewTarget = null;

            if (_state == CombatState.Dead)
                return;

            _state = CombatState.Moving;
            _mover.enabled = true;
            _waitingForHit = false;
            if (_hasAnimator)
            {
                _animator.speed = 1f;
            }
        }

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
        }
    }
}
