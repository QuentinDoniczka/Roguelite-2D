using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RogueliteAutoBattler.UI.Core
{
    /// <summary>
    /// A single tab button in the bottom navigation bar.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class TabButton : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField]
        [Tooltip("Index of the tab this button controls (0-4).")]
        private int _tabIndex;

        [Header("References")]
        [SerializeField]
        [Tooltip("Icon image for this tab.")]
        private Image _icon;

        [SerializeField]
        [Tooltip("Text label for this tab.")]
        private TextMeshProUGUI _label;

        [Header("Colors")]
        [SerializeField]
        [Tooltip("Color when the tab is not selected.")]
        private Color _normalColor = Color.gray;

        [SerializeField]
        [Tooltip("Color when the tab is selected.")]
        private Color _selectedColor = Color.white;

        private Button _button;

        /// <summary>
        /// Index of the tab this button controls.
        /// </summary>
        public int TabIndex => _tabIndex;

        /// <summary>
        /// Whether this tab is currently selected.
        /// </summary>
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

        /// <summary>
        /// Marks this tab as selected and applies the selected color.
        /// </summary>
        public void Select()
        {
            IsSelected = true;
            ApplyColor(_selectedColor);
        }

        /// <summary>
        /// Marks this tab as deselected and applies the normal color.
        /// </summary>
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
