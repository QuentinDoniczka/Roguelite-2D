using RogueliteAutoBattler.Data;
using RogueliteAutoBattler.Economy;
using RogueliteAutoBattler.UI.Core;
using UnityEngine;

namespace RogueliteAutoBattler.UI.Screens.SkillTree
{
    public class SkillTreeScreen : UIScreen
    {
        [SerializeField] private SkillTreeInputHandler _inputHandler;
        [SerializeField] private SkillTreeNodeManager _nodeManager;
        [SerializeField] private SkillTreeDetailPanel _detailPanel;
        [SerializeField] private SkillTreeData _data;
        [SerializeField] private SkillTreeProgress _progress;
        [SerializeField] private GoldWallet _goldWallet;
        [SerializeField] private SkillPointWallet _skillPointWallet;
        [SerializeField] private RectTransform _gameArea;
        [SerializeField] private RectTransform _infoArea;

        private float _originalGameAreaYMin;
        private float _originalInfoAreaYMax;

        protected override void Awake()
        {
            base.Awake();

            if (_gameArea != null)
                _originalGameAreaYMin = _gameArea.anchorMin.y;
            if (_infoArea != null)
                _originalInfoAreaYMax = _infoArea.anchorMax.y;

            if (_inputHandler != null && _nodeManager != null)
            {
                _inputHandler.OnVoidClicked += _nodeManager.DeselectAll;
                _nodeManager.Initialize();
            }

            if (_detailPanel != null)
            {
                _detailPanel.Initialize(_data, _progress, _goldWallet, _skillPointWallet);

                if (_nodeManager != null)
                {
                    _nodeManager.OnNodeSelected += HandleNodeSelected;
                    _nodeManager.OnNodeDeselected += HandleNodeDeselected;
                }
            }
        }

        public override void OnShow()
        {
            base.OnShow();

            if (_gameArea != null)
            {
                var min = _gameArea.anchorMin;
                min.y = _infoArea != null ? _infoArea.anchorMin.y : _originalGameAreaYMin;
                _gameArea.anchorMin = min;
            }

            if (_infoArea != null)
            {
                var max = _infoArea.anchorMax;
                max.y = _infoArea.anchorMin.y;
                _infoArea.anchorMax = max;
            }
        }

        public override void OnHide()
        {
            if (_detailPanel != null && _detailPanel.IsVisible)
                _detailPanel.Hide();

            if (_gameArea != null)
            {
                var min = _gameArea.anchorMin;
                min.y = _originalGameAreaYMin;
                _gameArea.anchorMin = min;
            }

            if (_infoArea != null)
            {
                var max = _infoArea.anchorMax;
                max.y = _originalInfoAreaYMax;
                _infoArea.anchorMax = max;
            }

            base.OnHide();
        }

        private void HandleNodeSelected(SkillTreeNode node)
        {
            if (_detailPanel != null)
                _detailPanel.ShowNode(node);
        }

        private void HandleNodeDeselected()
        {
            if (_detailPanel != null)
                _detailPanel.Hide();
        }

        private void OnDestroy()
        {
            if (_inputHandler != null && _nodeManager != null)
                _inputHandler.OnVoidClicked -= _nodeManager.DeselectAll;

            if (_nodeManager != null && _detailPanel != null)
            {
                _nodeManager.OnNodeSelected -= HandleNodeSelected;
                _nodeManager.OnNodeDeselected -= HandleNodeDeselected;
            }
        }
    }
}
