using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace RogueliteAutoBattler.UI.Toolkit.SkillTree
{
    public enum SkillTreeNodeVisualState
    {
        Locked,
        Available,
        Purchased,
        Max
    }

    public sealed class SkillTreeNodeElement : VisualElement
    {
        private const string BaseClassName = "skill-tree-node";
        private const string LockedClassName = "skill-tree-node--locked";
        private const string AvailableClassName = "skill-tree-node--available";
        private const string PurchasedClassName = "skill-tree-node--purchased";
        private const string MaxClassName = "skill-tree-node--max";
        private const string SelectedClassName = "skill-tree-node--selected";
        private const float NodeHalfSize = 32f;

        private Color _currentColor = Color.white;

        public int NodeIndex { get; }
        public SkillTreeNodeVisualState CurrentState { get; private set; }
        public bool IsSelected { get; private set; }
        public Color CurrentColor => _currentColor;

        public event Action<int> Clicked;

        public SkillTreeNodeElement(int nodeIndex)
        {
            NodeIndex = nodeIndex;
            AddToClassList(BaseClassName);
            RegisterCallback<ClickEvent>(OnClick);

            var orb = SkillTreeNodeOrbResolver.Get();
            if (orb != null)
            {
                style.backgroundImage = new StyleBackground(orb);
            }

            SetState(SkillTreeNodeVisualState.Locked);
        }

        public void SetState(SkillTreeNodeVisualState newState)
        {
            RemoveFromClassList(LockedClassName);
            RemoveFromClassList(AvailableClassName);
            RemoveFromClassList(PurchasedClassName);
            RemoveFromClassList(MaxClassName);

            CurrentState = newState;

            string stateClassName = newState switch
            {
                SkillTreeNodeVisualState.Locked => LockedClassName,
                SkillTreeNodeVisualState.Available => AvailableClassName,
                SkillTreeNodeVisualState.Purchased => PurchasedClassName,
                SkillTreeNodeVisualState.Max => MaxClassName,
                _ => LockedClassName
            };

            AddToClassList(stateClassName);
        }

        public void SetSelected(bool selected)
        {
            IsSelected = selected;

            if (selected)
                AddToClassList(SelectedClassName);
            else
                RemoveFromClassList(SelectedClassName);
        }

        public void SetColorTag(Color color)
        {
            _currentColor = color;
            style.backgroundColor = new StyleColor(color);
            style.borderLeftColor = new StyleColor(color);
            style.borderRightColor = new StyleColor(color);
            style.borderTopColor = new StyleColor(color);
            style.borderBottomColor = new StyleColor(color);
            style.unityBackgroundImageTintColor = new StyleColor(color);
        }

        public void SetDataPosition(Vector2 dataPosition, float unitToPixelScale)
        {
            style.left = new StyleLength(new Length(dataPosition.x * unitToPixelScale - NodeHalfSize, LengthUnit.Pixel));
            style.top = new StyleLength(new Length(dataPosition.y * unitToPixelScale - NodeHalfSize, LengthUnit.Pixel));
        }

        private void OnClick(ClickEvent _) => Clicked?.Invoke(NodeIndex);
    }
}
