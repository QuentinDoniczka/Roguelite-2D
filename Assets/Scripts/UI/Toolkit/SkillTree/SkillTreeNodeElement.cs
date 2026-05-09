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
        private const string RaysClassName = "skill-tree-node__rays";
        private const float NodeHalfSize = 32f;
        private const long PulseIntervalMs = 800;
        private const long RaysRotationIntervalMs = 50;
        private const float RaysDegreesPerTick = 0.3f;

        private Color _currentColor = Color.white;
        private float _raysRotationDegrees;
        private readonly VisualElement _halo;
        private readonly VisualElement _rays;

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
            ApplyHaloSizeFromSettings();

            var orb = SkillTreeNodeOrbResolver.Get();
            if (orb != null)
            {
                var background = new StyleBackground(orb);
                style.backgroundImage = background;
                _halo.style.backgroundImage = background;
            }

            _rays = CreateLayer(RaysClassName, OrbLayerKind.Rays);
            Add(_rays);

            SetState(SkillTreeNodeVisualState.Locked);

            schedule.Execute(TogglePulseIfAvailable).StartingIn(PulseIntervalMs).Every(PulseIntervalMs);

            _rays.RegisterCallback<AttachToPanelEvent>(OnRaysAttached);
        }

        public void SetState(SkillTreeNodeVisualState newState)
        {
            var previousState = CurrentState;

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

            if (previousState == SkillTreeNodeVisualState.Max && newState != SkillTreeNodeVisualState.Max)
            {
                _raysRotationDegrees = 0f;
                _rays.transform.rotation = Quaternion.identity;
            }

            ApplyHaloOpacityForCurrentState();
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

        private static VisualElement CreateLayer(string className, OrbLayerKind kind)
        {
            var layer = new VisualElement
            {
                name = className,
                pickingMode = PickingMode.Ignore,
            };
            layer.AddToClassList(className);
            var texture = SkillTreeNodeOrbResolver.Get(kind);
            if (texture != null)
            {
                layer.style.backgroundImage = new StyleBackground(texture);
            }
            return layer;
        }

        private void TogglePulseIfAvailable()
        {
            if (ClassListContains(AvailableClassName))
                ToggleInClassList(PulseOnClassName);
            else if (ClassListContains(PulseOnClassName))
                RemoveFromClassList(PulseOnClassName);
        }

        private void OnRaysAttached(AttachToPanelEvent _)
        {
            _rays.UnregisterCallback<AttachToPanelEvent>(OnRaysAttached);
            _rays.schedule.Execute(RotateRaysIfMax)
                .StartingIn(RaysRotationIntervalMs)
                .Every(RaysRotationIntervalMs);
        }

        private void RotateRaysIfMax()
        {
            if (CurrentState != SkillTreeNodeVisualState.Max)
                return;
            _raysRotationDegrees = (_raysRotationDegrees + RaysDegreesPerTick) % 360f;
            _rays.transform.rotation = Quaternion.Euler(0f, 0f, _raysRotationDegrees);
        }

        private void ApplyHaloSizeFromSettings()
        {
            float size = SkillTreeVisualSettingsResolver.GetHaloSize();
            _halo.style.width = new StyleLength(new Length(size, LengthUnit.Pixel));
            _halo.style.height = new StyleLength(new Length(size, LengthUnit.Pixel));
            float offset = -(size - NodeHalfSize * 2f) * 0.5f;
            _halo.style.top = new StyleLength(new Length(offset, LengthUnit.Pixel));
            _halo.style.left = new StyleLength(new Length(offset, LengthUnit.Pixel));
        }

        private void ApplyHaloOpacityForCurrentState()
        {
            _halo.style.opacity = SkillTreeVisualSettingsResolver.GetOpacityForState(CurrentState);
        }

        private void OnClick(ClickEvent _) => Clicked?.Invoke(NodeIndex);
    }
}
