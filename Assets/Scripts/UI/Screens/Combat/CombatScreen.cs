using RogueliteAutoBattler.UI.Core;
using UnityEngine;
using UnityEngine.UI;

namespace RogueliteAutoBattler.UI.Screens.Combat
{
    public class CombatScreen : UIScreen
    {
        [SerializeField] private DamageNumberSettingsPanel _settingsPanel;
        [SerializeField] private Button _settingsButton;

        protected override void Awake()
        {
            base.Awake();
            if (_settingsButton != null)
                _settingsButton.onClick.AddListener(OnSettingsButtonClicked);
        }

        private void OnSettingsButtonClicked()
        {
            if (_settingsPanel != null)
                _settingsPanel.Toggle();
        }
    }
}
