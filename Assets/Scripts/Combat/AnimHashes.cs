using UnityEngine;

namespace RogueliteAutoBattler.Combat
{
    public static class AnimHashes
    {
        public static readonly int Idle = Animator.StringToHash("Idle");
        public static readonly int Walk = Animator.StringToHash("Walk");
        public static readonly int Run = Animator.StringToHash("Run");
        public static readonly int ChopAttack = Animator.StringToHash("ChopAttack");
        public static readonly int IsMoving = Animator.StringToHash("IsMoving");
    }
}
