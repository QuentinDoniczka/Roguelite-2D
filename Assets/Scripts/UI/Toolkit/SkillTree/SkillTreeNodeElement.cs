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
        private const string PulseOnClassName = "skill-tree-node--pulse-on";
        private const string HaloClassName = "skill-tree-node__halo";
        private const float NodeHalfSize = 32f;
        private const long PulseIntervalMs = 800;

        private Color _currentColor = Color.white;
        private readonly VisualElement _halo;

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

            _halo = new VisualElement { name = "node-halo", pickingMode = PickingMode.Ignore };
            _halo.AddToClassList(HaloClassName);
            Add(_halo);

            var orb = SkillTreeNodeOrbResolver.Get();
            if (orb != null)
            {
                var background = new StyleBackground(orb);
                style.backgroundImage = background;
                _halo.style.backgroundImage = background;
            }

            SetState(SkillTreeNodeVisualState.Locked);

            schedule.Execute(TogglePulseIfAvailable).StartingIn(PulseIntervalMs).Every(PulseIntervalMs);
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

            if (!ClassListContains(AvailableClassName))
                RemoveFromClassList(PulseOnClassName);
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
            _halo.style.unityBackgroundImageTintColor = new StyleColor(color);
        }

        public void SetDataPosition(Vector2 dataPosition, float unitToPixelScale)
        {
            style.left = new StyleLength(new Length(dataPosition.x * unitToPixelScale - NodeHalfSize, LengthUnit.Pixel));
            style.top = new StyleLength(new Length(dataPosition.y * unitToPixelScale - NodeHalfSize, LengthUnit.Pixel));
        }

        private void TogglePulseIfAvailable()
        {
            if (ClassListContains(AvailableClassName))
                ToggleInClassList(PulseOnClassName);
            else if (ClassListContains(PulseOnClassName))
                RemoveFromClassList(PulseOnClassName);
        }

        private void OnClick(ClickEvent _) => Clicked?.Invoke(NodeIndex);
    }
}
