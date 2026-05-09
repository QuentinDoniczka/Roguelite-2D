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
        private const string HaloBreatheOnClassName = "skill-tree-node--halo-breathe-on";
        private const string HaloClassName = "skill-tree-node__halo";
        private const string FrameClassName = "skill-tree-node__frame";
        private const string RimClassName = "skill-tree-node__rim";
        private const string InnerGlowClassName = "skill-tree-node__inner-glow";
        private const string SparkleClassName = "skill-tree-node__sparkle";
        private const float NodeHalfSize = 32f;
        private const long PulseIntervalMs = 800;
        private const long HaloBreatheIntervalMs = 1600;
        private const long SparkleRotationIntervalMs = 50;
        private const float SparkleDegreesPerTick = 1.5f;

        private Color _currentColor = Color.white;
        private float _sparkleRotationDegrees;
        private readonly VisualElement _halo;
        private readonly VisualElement _frame;
        private readonly VisualElement _rim;
        private readonly VisualElement _innerGlow;
        private readonly VisualElement _sparkle;

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

            _frame = CreateLayer(FrameClassName, OrbLayerKind.Frame);
            Add(_frame);
            _rim = CreateLayer(RimClassName, OrbLayerKind.Rim);
            Add(_rim);
            _innerGlow = CreateLayer(InnerGlowClassName, OrbLayerKind.InnerGlow);
            Add(_innerGlow);
            _sparkle = CreateLayer(SparkleClassName, OrbLayerKind.Sparkle);
            Add(_sparkle);

            SetState(SkillTreeNodeVisualState.Locked);

            schedule.Execute(TogglePulseIfAvailable).StartingIn(PulseIntervalMs).Every(PulseIntervalMs);

            schedule.Execute(ToggleHaloBreatheIfActive)
                .StartingIn(HaloBreatheIntervalMs)
                .Every(HaloBreatheIntervalMs);

            schedule.Execute(RotateSparkleIfMax)
                .StartingIn(SparkleRotationIntervalMs)
                .Every(SparkleRotationIntervalMs);
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

            if (newState == SkillTreeNodeVisualState.Locked)
            {
                if (ClassListContains(HaloBreatheOnClassName))
                    RemoveFromClassList(HaloBreatheOnClassName);
            }

            if (previousState == SkillTreeNodeVisualState.Max && newState != SkillTreeNodeVisualState.Max)
            {
                _sparkleRotationDegrees = 0f;
                _sparkle.style.rotate = new StyleRotate(new Rotate(new Angle(0f, AngleUnit.Degree)));
            }
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
            _innerGlow.style.unityBackgroundImageTintColor = new StyleColor(color);
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

        private void ToggleHaloBreatheIfActive()
        {
            if (CurrentState == SkillTreeNodeVisualState.Locked)
            {
                if (ClassListContains(HaloBreatheOnClassName))
                    RemoveFromClassList(HaloBreatheOnClassName);
                return;
            }
            ToggleInClassList(HaloBreatheOnClassName);
        }

        private void RotateSparkleIfMax()
        {
            if (CurrentState != SkillTreeNodeVisualState.Max)
                return;
            _sparkleRotationDegrees = (_sparkleRotationDegrees + SparkleDegreesPerTick) % 360f;
            _sparkle.style.rotate = new StyleRotate(new Rotate(new Angle(_sparkleRotationDegrees, AngleUnit.Degree)));
        }

        private void OnClick(ClickEvent _) => Clicked?.Invoke(NodeIndex);
    }
}
