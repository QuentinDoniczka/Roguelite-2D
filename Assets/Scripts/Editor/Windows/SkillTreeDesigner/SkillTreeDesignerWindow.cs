using RogueliteAutoBattler.Data;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RogueliteAutoBattler.Editor.Windows.SkillTreeDesigner
{
    internal sealed class SkillTreeDesignerWindow : EditorWindow
    {
        private const string UxmlPath = "Assets/Scripts/Editor/Windows/SkillTreeDesigner/SkillTreeDesignerWindow.uxml";
        private const string UssPath = "Assets/Scripts/Editor/Windows/SkillTreeDesigner/SkillTreeDesignerWindow.uss";

        private SkillTreeData _data;
        private SerializedObject _serialized;
        private int? _selectedNodeId;
        private VisualElement _activeTabContent;
        private Button _activeTabButton;
        private SkillTreeCanvasElement _canvas;

        [MenuItem("Roguelite/Skill Tree Designer")]
        private static void OpenWindow()
        {
            GetWindow<SkillTreeDesignerWindow>("Skill Tree Designer");
        }

        private void CreateGUI() => BuildUI(rootVisualElement);

        internal void BuildUI(VisualElement root)
        {
            VisualTreeAsset uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UxmlPath);
            if (uxml == null)
            {
                Debug.LogWarning($"[SkillTreeDesignerWindow] UXML not found at {UxmlPath}");
                return;
            }

            TemplateContainer cloned = uxml.CloneTree();
            cloned.style.flexGrow = 1;
            root.Add(cloned);

            StyleSheet uss = AssetDatabase.LoadAssetAtPath<StyleSheet>(UssPath);
            if (uss != null)
                root.styleSheets.Add(uss);

            WireCanvas(root);

            string[] guids = AssetDatabase.FindAssets("t:SkillTreeData");
            if (guids.Length == 0)
            {
                Debug.LogWarning("[SkillTreeDesignerWindow] No SkillTreeData asset found in project.");
                return;
            }

            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            _data = AssetDatabase.LoadAssetAtPath<SkillTreeData>(path);
            if (_data == null)
            {
                Debug.LogWarning($"[SkillTreeDesignerWindow] Failed to load SkillTreeData at {path}");
                return;
            }

            _serialized = new SerializedObject(_data);

            if (_canvas != null)
                _canvas.SetData(_data, _selectedNodeId);

            WireTabButtons(root);
        }

        private void WireCanvas(VisualElement root)
        {
            VisualElement placeholder = root.Q<VisualElement>("canvas-root");
            if (placeholder == null) return;

            _canvas = new SkillTreeCanvasElement { name = "canvas-root" };
            _canvas.style.flexGrow = 1;

            var parent = placeholder.parent;
            int index = parent.IndexOf(placeholder);
            placeholder.RemoveFromHierarchy();
            parent.Insert(index, _canvas);

            _canvas.NodeClicked += OnNodeClickedId;
        }

        private void OnNodeClickedId(int nodeId)
        {
            _selectedNodeId = nodeId;
            if (_canvas != null)
                _canvas.SetData(_data, _selectedNodeId);
        }

        private void WireTabButtons(VisualElement root)
        {
            Button btnTree = root.Q<Button>("tab-btn-tree");
            Button btnNode = root.Q<Button>("tab-btn-node");
            Button btnBranch = root.Q<Button>("tab-btn-branch");

            VisualElement tabTree = root.Q<VisualElement>("tab-tree");
            VisualElement tabNode = root.Q<VisualElement>("tab-node");
            VisualElement tabBranch = root.Q<VisualElement>("tab-branch");

            btnTree.clicked += () => ActivateTab(btnTree, tabTree);
            btnNode.clicked += () => ActivateTab(btnNode, tabNode);
            btnBranch.clicked += () => ActivateTab(btnBranch, tabBranch);

            ActivateTab(btnTree, tabTree);
        }

        private void ActivateTab(Button button, VisualElement content)
        {
            if (_activeTabButton != null)
                _activeTabButton.RemoveFromClassList("tab-btn--active");
            if (_activeTabContent != null)
                _activeTabContent.RemoveFromClassList("tab-content--active");

            _activeTabButton = button;
            _activeTabContent = content;

            _activeTabButton.AddToClassList("tab-btn--active");
            _activeTabContent.AddToClassList("tab-content--active");
        }

        internal static string GetMenuItemPath() => "Roguelite/Skill Tree Designer";

        internal SkillTreeData Data => _data;
        internal SerializedObject SerializedData => _serialized;
        internal int? SelectedNodeId { get => _selectedNodeId; set => _selectedNodeId = value; }
        internal VisualElement Root => rootVisualElement;
        internal string ActiveTabName => _activeTabContent?.name;
        internal SkillTreeCanvasElement Canvas => _canvas;
    }
}
