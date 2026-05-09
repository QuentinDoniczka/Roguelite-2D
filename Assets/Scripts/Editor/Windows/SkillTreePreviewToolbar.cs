using System;
using RogueliteAutoBattler.Data;
using UnityEditor;
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
            toolbar.style.backgroundColor = new UnityEngine.UIElements.StyleColor(new UnityEngine.Color(0.2f, 0.2f, 0.2f, 1f));

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
            container.style.marginRight = 8f;

            var label = new Label(labelText);
            label.style.width = LabelWidth;
            label.style.flexShrink = 0;
            container.Add(label);

            var slider = new Slider(min, max);
            slider.style.width = SliderWidth;
            slider.value = initialValue;

            slider.RegisterValueChangedCallback(evt =>
            {
                if (_settings == null) return;

                Undo.RegisterCompleteObjectUndo(_settings, "Edit Skill Tree Visual Settings");
                _settings.SetFieldValue(fieldName, evt.newValue);
                EditorUtility.SetDirty(_settings);
                _onChanged?.Invoke();
            });

            container.Add(slider);
            return container;
        }
    }
}
