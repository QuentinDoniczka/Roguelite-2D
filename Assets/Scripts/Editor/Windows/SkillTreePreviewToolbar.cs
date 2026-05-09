using System;
using RogueliteAutoBattler.Data;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RogueliteAutoBattler.Editor.Windows
{
    internal sealed class SkillTreePreviewToolbar
    {
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

            foreach (var descriptor in SkillTreeVisualSettings.Tunables)
            {
                toolbar.Add(BuildSlider(descriptor));
            }

            return toolbar;
        }

        private VisualElement BuildSlider(SkillTreeVisualSettings.TunableDescriptor descriptor)
        {
            var container = new VisualElement();
            container.style.flexDirection = FlexDirection.Row;
            container.style.alignItems = Align.Center;
            container.style.marginRight = SliderContainerMarginRight;

            var label = new Label(descriptor.DisplayLabel);
            label.style.width = LabelWidth;
            label.style.flexShrink = 0;
            container.Add(label);

            float initialValue = descriptor.Getter(_settings);
            var slider = new ChangeNotifyingSlider(descriptor.Min, descriptor.Max, initialValue);
            slider.style.width = SliderWidth;
            string fieldName = descriptor.FieldName;
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

        // LSP intentionally bent: BaseField<T>.value setter does NOT dispatch ChangeEvent when panel == null
        // (headless test mode), so RegisterValueChangedCallback never fires on a detached element. Overriding
        // SetValueWithoutNotify lets the toolbar tests assign `slider.value = X` and observe the callback.
        // See SkillTreePreviewToolbarTests.SliderChange_WritesToSettings_AndCallsOnChanged.
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
