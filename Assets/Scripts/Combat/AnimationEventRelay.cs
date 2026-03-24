using UnityEngine;

namespace RogueliteAutoBattler.Combat
{
    /// <summary>
    /// Relays animation events from the Visual child (where the Animator lives)
    /// to the CombatController on the parent root GameObject.
    /// </summary>
    public class AnimationEventRelay : MonoBehaviour
    {
        private CombatController _controller;

        /// <summary>Binds this relay to the given controller.</summary>
        public void Initialize(CombatController controller)
        {
            _controller = controller;
        }

        // Called by ChopAttack animation event at the hit frame.
        public void OnAttackHit()
        {
            if (_controller != null)
                _controller.OnAnimationHit();
        }
    }
}
