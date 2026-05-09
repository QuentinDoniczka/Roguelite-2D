using System;
using RogueliteAutoBattler.Data;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RogueliteAutoBattler.Editor.Windows
{
    internal sealed class SkillTreePreviewToolbar
    {
        private const float MinHaloSize = 32f;
        private const float MaxHaloSize = 200f;
        private const float MinOpacity = 0f;
        private const float MaxOpacity = 1f;
        private const float LabelWidth = 130f;
        private const float SliderWidth = 140f;
        private const float ToolbarPadding = 6f;
        private const float SliderContainerMarginRight = 8f;
        private const string UndoLabel = "Edit Skill Tree Visual Settings";
        private static readonly Color ToolbarBackgroundColor = new Color(0.2f, 0.2f, 0.2f, 1f);

        private readonly SkillTreeVisualSettings _settings;
        private readonly Action _onChanged;

        internal SkillTreePreviewToolbar(SkillTreeVisualSettings settings, Action onChanged)
        {
            _settings = settings;
            _onChanged = onChanged;
        }

        internal VisualElement BuildToolbar()
        {
            var toolbar = new VisualElement();
            toolbar.style.flexDirection = FlexDirection.Row;
            toolbar.style.flexWrap = Wrap.Wrap;
            toolbar.style.alignItems = Align.Center;
            toolbar.style.paddingTop = ToolbarPadding;
            toolbar.style.paddingBottom = ToolbarPadding;
            toolbar.style.paddingLeft = ToolbarPadding;
            toolbar.style.paddingRight = ToolbarPadding;
            toolbar.style.backgroundColor = new StyleColor(ToolbarBackgroundColor);

            if (_settings == null) return toolbar;

            toolbar.Add(BuildSlider(
                "Halo Size",
                SkillTreeVisualSettings.FieldNames.HaloSize,
                _settings.HaloSize,
                MinHaloSize,
                MaxHaloSize));

            toolbar.Add(BuildSlider(
                "Opacity Locked",
                SkillTreeVisualSettings.FieldNames.HaloOpacityLocked,
                _settings.HaloOpacityLocked,
                MinOpacity,
                MaxOpacity));

            toolbar.Add(BuildSlider(
                "Opacity Available",
                SkillTreeVisualSettings.FieldNames.HaloOpacityAvailable,
                _settings.HaloOpacityAvailable,
                MinOpacity,
                MaxOpacity));

            toolbar.Add(BuildSlider(
                "Opacity Purchased",
                SkillTreeVisualSettings.FieldNames.HaloOpacityPurchased,
                _settings.HaloOpacityPurchased,
                MinOpacity,
                MaxOpacity));

            toolbar.Add(BuildSlider(
                "Opacity Max",
                SkillTreeVisualSettings.FieldNames.HaloOpacityMax,
                _settings.HaloOpacityMax,
                MinOpacity,
                MaxOpacity));

            return toolbar;
        }

        private VisualElement BuildSlider(string labelText, string fieldName, float initialValue, float min, float max)
        {
            var container = new VisualElement();
            container.style.flexDirection = FlexDirection.Row;
            container.style.alignItems = Align.Center;
            container.style.marginRight = SliderContainerMarginRight;

            var label = new Label(labelText);
            label.style.width = LabelWidth;
            label.style.flexShrink = 0;
            container.Add(label);

            var slider = new ChangeNotifyingSlider(min, max, initialValue);
            slider.style.width = SliderWidth;
            slider.OnValueChanged = newValue => OnSliderChanged(fieldName, newValue);

            container.Add(slider);
            return container;
        }

        private void OnSliderChanged(string fieldName, float newValue)
        {
            if (_settings == null) return;
            Undo.RegisterCompleteObjectUndo(_settings, UndoLabel);
            _settings.SetFieldValue(fieldName, newValue);
            EditorUtility.SetDirty(_settings);
            _onChanged?.Invoke();
        }

        private sealed class ChangeNotifyingSlider : Slider
        {
            internal Action<float> OnValueChanged;
            private readonly bool _initialized;

            public ChangeNotifyingSlider(float min, float max, float initialValue) : base(min, max)
            {
                SetValueWithoutNotify(initialValue);
                _initialized = true;
            }

            public override void SetValueWithoutNotify(float newValue)
            {
                base.SetValueWithoutNotify(newValue);
                if (_initialized)
                    OnValueChanged?.Invoke(newValue);
            }
        }
    }
}
