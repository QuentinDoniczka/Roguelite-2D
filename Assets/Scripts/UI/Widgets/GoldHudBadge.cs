using RogueliteAutoBattler.Combat;
using TMPro;
using UnityEngine;

namespace RogueliteAutoBattler.UI.Widgets
{
    public class GoldHudBadge : MonoBehaviour
    {
        private TMP_Text _label;
        private GoldWallet _wallet;

        private void Awake()
        {
            _label = GetComponentInChildren<TMP_Text>();
        }

        private void Start()
        {
            var wallets = FindObjectsByType<GoldWallet>(FindObjectsSortMode.None);
            if (wallets.Length > 0)
            {
                _wallet = wallets[0];
                _wallet.OnGoldChanged += UpdateDisplay;
                UpdateDisplay(_wallet.Gold);
            }
        }

        private void OnDestroy()
        {
            if (_wallet != null)
                _wallet.OnGoldChanged -= UpdateDisplay;
        }

        private void UpdateDisplay(int total)
        {
            if (_label != null)
                _label.text = GoldFormatter.Format(total);
        }
    }
}
