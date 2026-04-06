using RogueliteAutoBattler.UI.Core;
using UnityEngine;

namespace RogueliteAutoBattler.UI.Screens.SkillTree
{
    public class SkillTreeScreen : UIScreen
    {
        [SerializeField] private SkillTreeInputHandler _inputHandler;
        [SerializeField] private SkillTreeNodeManager _nodeManager;
        [SerializeField] private SkillTreeDarknessOverlay _darknessOverlay;

        protected override void Awake()
        {
            base.Awake();
            if (_inputHandler != null && _nodeManager != null)
            {
                _inputHandler.OnVoidClicked += _nodeManager.DeselectAll;
                _nodeManager.Initialize();
            }

            if (_darknessOverlay != null)
                _darknessOverlay.Initialize();
        }

        private void OnDestroy()
        {
            if (_inputHandler != null && _nodeManager != null)
                _inputHandler.OnVoidClicked -= _nodeManager.DeselectAll;
        }
    }
}
