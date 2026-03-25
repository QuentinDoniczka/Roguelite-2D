using UnityEngine;

namespace RogueliteAutoBattler.Combat
{
    /// <summary>
    /// Pre-hashed Animator state names shared across combat scripts.
    /// Using int hashes avoids per-call string hashing and prevents silent typo failures.
    /// </summary>
    public static class AnimHashes
    {
        public static readonly int Idle = Animator.StringToHash("Idle");
        public static readonly int Walk = Animator.StringToHash("Walk");
        public static readonly int Run = Animator.StringToHash("Run");
        public static readonly int ChopAttack = Animator.StringToHash("ChopAttack");
    }
}
