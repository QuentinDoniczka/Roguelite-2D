using RogueliteAutoBattler.UI.Core;
using UnityEngine;

namespace RogueliteAutoBattler.UI.Screens.SkillTree
{
    public class SkillTreeScreen : UIScreen
    {
        [SerializeField] private SkillTreeInputHandler _inputHandler;
        [SerializeField] private SkillTreeNodeManager _nodeManager;

        protected override void Awake()
        {
            base.Awake();
            _inputHandler.OnVoidClicked += _nodeManager.DeselectAll;
            _nodeManager.Initialize();
        }

        private void OnDestroy()
        {
            if (_inputHandler != null && _nodeManager != null)
                _inputHandler.OnVoidClicked -= _nodeManager.DeselectAll;
        }
    }
}
