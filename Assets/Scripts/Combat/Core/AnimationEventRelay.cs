using UnityEngine;

namespace RogueliteAutoBattler.Combat.Core
{
    public class AnimationEventRelay : MonoBehaviour
    {
        private CombatController _controller;

        public void Initialize(CombatController controller)
        {
            _controller = controller;
        }

        public void OnAttackHit()
        {
            if (_controller != null)
                _controller.OnAnimationHit();
        }
    }
}
