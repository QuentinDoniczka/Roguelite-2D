using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RogueliteAutoBattler.UI.Core
{
    [RequireComponent(typeof(Button))]
    public class TabButton : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private int _tabIndex;

        [Header("References")]
        [SerializeField] private Image _icon;
        [SerializeField] private TextMeshProUGUI _label;

        [Header("Colors")]
        [SerializeField] private Color _normalColor = Color.gray;
        [SerializeField] private Color _selectedColor = Color.white;

        private Button _button;

        public int TabIndex => _tabIndex;
        public bool IsSelected { get; private set; }

        private void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(OnClick);
        }

        private void OnDestroy()
        {
            if (_button != null)
            {
                _button.onClick.RemoveListener(OnClick);
            }
        }

        private void OnClick()
        {
            if (NavigationManager.Instance == null)
            {
                return;
            }

            NavigationManager.Instance.SwitchTab(_tabIndex);
        }

        public void Select()
        {
            IsSelected = true;
            ApplyColor(_selectedColor);
        }

        public void Deselect()
        {
            IsSelected = false;
            ApplyColor(_normalColor);
        }

        private void ApplyColor(Color color)
        {
            if (_icon != null)
            {
                _icon.color = color;
            }

            if (_label != null)
            {
                _label.color = color;
            }
        }
    }
}
