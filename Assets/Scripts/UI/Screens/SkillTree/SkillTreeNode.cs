using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RogueliteAutoBattler.UI.Screens.SkillTree
{
    public class SkillTreeNode : MonoBehaviour, IPointerClickHandler
    {
        [Header("Visuals")]
        [SerializeField] private Image _icon;
        [SerializeField] private Image _border;

        [Header("Colors")]
        [SerializeField] private Color _normalColor = Color.gray;
        [SerializeField] private Color _selectedColor = Color.yellow;

        private bool _isSelected;

        public int NodeIndex { get; private set; }

        public bool IsSelected => _isSelected;

        public event Action<SkillTreeNode> OnNodeClicked;

        public void Setup(Image icon, Image border, Color normalColor, Color selectedColor)
        {
            _icon = icon;
            _border = border;
            _normalColor = normalColor;
            _selectedColor = selectedColor;
            _border.color = _normalColor;
        }

        public void Initialize(int index)
        {
            NodeIndex = index;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            OnNodeClicked?.Invoke(this);
            eventData.Use();
        }

        public void SetSelected(bool selected)
        {
            _isSelected = selected;
            _border.color = _isSelected ? _selectedColor : _normalColor;
        }
    }
}
