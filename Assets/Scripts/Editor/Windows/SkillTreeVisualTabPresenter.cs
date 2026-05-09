using System;
using RogueliteAutoBattler.Data;
using RogueliteAutoBattler.UI.Toolkit.SkillTree;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RogueliteAutoBattler.Editor.Windows
{
    internal sealed class SkillTreeVisualTabPresenter
    {
        private const float CanvasFlexGrow = 0.6f;
        private const float InspectorFlexGrow = 0.4f;
        private const float InspectorMinWidthPixels = 280f;
        private const float InspectorPaddingPixels = 8f;
        private const float InspectorContentFlexGrow = 1f;

        private readonly VisualElement _parent;
        private readonly float _topOffset;
        private readonly Action _onSettingsChanged;

        private SkillTreeVisualSettings _visualSettings;
        private SerializedObject _visualSettingsSerializedObject;
        private VisualElement _root;
        private VisualElement _canvasHost;
        private VisualElement _inspectorHost;
        private SkillTreePreviewPanel _previewPanel;

        private SkillTreeData _data;
        private SkillNodePalette _palette;

        internal SkillTreeVisualTabPresenter(VisualElement parent, float topOffset, Action onSettingsChanged = null)
        {
            _parent = parent;
            _topOffset = topOffset;
            _onSettingsChanged = onSettingsChanged;
        }

        internal void Bind(SkillTreeData data, SkillNodePalette palette)
        {
            _data = data;
            _palette = palette;

            if (_root == null)
                Initialize();
            else
                RebuildPreviewPanel();
        }

        internal void Show()
        {
            if (_root != null)
                _root.style.display = DisplayStyle.Flex;
        }

        internal void Hide()
        {
            if (_root != null)
                _root.style.display = DisplayStyle.None;
        }

        internal void OnUndoRedo()
        {
            _visualSettingsSerializedObject?.Update();
            _previewPanel?.Rebuild();
        }

        internal void Dispose()
        {
            if (_visualSettings != null)
                AssetDatabase.SaveAssets();

            if (_root != null && _root.parent != null)
                _root.RemoveFromHierarchy();
            _root = null;
            _canvasHost = null;
            _inspectorHost = null;
            _previewPanel = null;
            _visualSettingsSerializedObject?.Dispose();
            _visualSettingsSerializedObject = null;
            _visualSettings = null;
        }

        private void Initialize()
        {
            _visualSettingsSerializedObject?.Dispose();
            _visualSettingsSerializedObject = null;

            _visualSettings = SkillTreeVisualSettingsResolver.Get();

            _root = new VisualElement();
            _root.style.position = Position.Absolute;
            _root.style.top = _topOffset;
            _root.style.left = 0f;
            _root.style.right = 0f;
            _root.style.bottom = 0f;
            _root.style.flexDirection = FlexDirection.Row;
            _root.style.display = DisplayStyle.None;
            _parent.Add(_root);

            _canvasHost = new VisualElement();
            _canvasHost.style.flexGrow = CanvasFlexGrow;
            _canvasHost.style.minWidth = 0;
            _root.Add(_canvasHost);

            _inspectorHost = new VisualElement();
            _inspectorHost.style.flexGrow = InspectorFlexGrow;
            _inspectorHost.style.minWidth = InspectorMinWidthPixels;
            _inspectorHost.style.paddingLeft = InspectorPaddingPixels;
            _inspectorHost.style.paddingRight = InspectorPaddingPixels;
            _inspectorHost.style.paddingTop = InspectorPaddingPixels;
            _inspectorHost.style.paddingBottom = InspectorPaddingPixels;
            _root.Add(_inspectorHost);

            if (_visualSettings == null)
            {
                string missingAssetMessage =
                    $"SkillTreeVisualSettings asset not found at Resources/{SkillTreeVisualSettingsResolver.ResourcesLoadPath}.asset.";
                var error = new IMGUIContainer(() =>
                    EditorGUILayout.HelpBox(missingAssetMessage, MessageType.Error));
                _inspectorHost.Add(error);
                return;
            }

            _visualSettingsSerializedObject = new SerializedObject(_visualSettings);

            _previewPanel = new SkillTreePreviewPanel(_data, _palette);
            _canvasHost.Add(_previewPanel.BuildRoot());

            var inspector = new IMGUIContainer(DrawInspector);
            inspector.style.flexGrow = InspectorContentFlexGrow;
            _inspectorHost.Add(inspector);
        }

        private void RebuildPreviewPanel()
        {
            if (_canvasHost == null) return;
            _canvasHost.Clear();
            _previewPanel = new SkillTreePreviewPanel(_data, _palette);
            _canvasHost.Add(_previewPanel.BuildRoot());
        }

        private void DrawInspector()
        {
            if (_visualSettingsSerializedObject == null) return;

            _visualSettingsSerializedObject.Update();

            EditorGUI.BeginChangeCheck();

            foreach (var descriptor in SkillTreeVisualSettings.Tunables)
            {
                var prop = _visualSettingsSerializedObject.FindProperty(descriptor.FieldName);
                if (prop != null)
                    EditorGUILayout.PropertyField(prop, new GUIContent(descriptor.DisplayLabel));
            }

            bool changed = EditorGUI.EndChangeCheck();

            _visualSettingsSerializedObject.ApplyModifiedProperties();

            if (changed)
                OnSettingsChanged();
        }

        private void OnSettingsChanged()
        {
            if (_visualSettings == null) return;

            EditorUtility.SetDirty(_visualSettings);
            SkillTreeVisualSettingsResolver.ResetCache();
            _previewPanel?.Rebuild();
            _onSettingsChanged?.Invoke();
        }

        internal SerializedObject SerializedObjectForTests => _visualSettingsSerializedObject;
        internal SkillTreeVisualSettings SettingsForTests => _visualSettings;
        internal SkillTreePreviewPanel PreviewPanelForTests => _previewPanel;
        internal void InvokeInitializeForTests() => Initialize();
        internal void InvokeOnSettingsChangedForTests() => OnSettingsChanged();
    }
}
