using System;
using RogueliteAutoBattler.Data;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RogueliteAutoBattler.Editor.Windows.SkillTreeDesigner
{
    internal sealed class NodeTabController
    {
        // Matches SkillTreeData.CentralNodeId — kept local to avoid a public coupling on the constant.
        private const int CentralNodeId = SkillTreeData.CentralNodeId;

        private readonly SkillTreeData _data;
        private readonly SerializedObject _serialized;
        private readonly SkillTreeCanvasElement _canvas;
        private readonly Func<int?> _selectedIdProvider;
        private readonly Action<int?> _setSelectedId;

        private readonly Label _infoLabel;
        private readonly Button _deleteButton;

        internal event Action NodeDeleted;

        internal NodeTabController(
            VisualElement tabContent,
            SkillTreeData data,
            SerializedObject serialized,
            SkillTreeCanvasElement canvas,
            Func<int?> selectedIdProvider,
            Action<int?> setSelectedId = null)
        {
            _data = data;
            _serialized = serialized;
            _canvas = canvas;
            _selectedIdProvider = selectedIdProvider;
            _setSelectedId = setSelectedId;

            _infoLabel = new Label("No node selected") { name = "node-info" };

            _deleteButton = new Button(Delete) { name = "node-delete", text = "Delete Node" };
            _deleteButton.SetEnabled(false);

            tabContent.Add(_infoLabel);
            tabContent.Add(_deleteButton);
        }

        internal void OnSelectionChanged(int? selectedId)
        {
            RefreshSelectedInfo(selectedId);
            RefreshDeleteButtonEnabled(selectedId);
        }

        internal void RefreshSelectedInfo(int? selectedId)
        {
            if (!selectedId.HasValue)
            {
                _infoLabel.text = "No node selected";
                return;
            }

            int id = selectedId.Value;
            Vector2 position = Vector2.zero;
            if (_data != null)
            {
                foreach (var node in _data.Nodes)
                {
                    if (node.id == id)
                    {
                        position = node.position;
                        break;
                    }
                }
            }

            _infoLabel.text = $"Node id: {id}  pos: ({position.x:0.##}, {position.y:0.##})";
        }

        internal void RefreshDeleteButtonEnabled(int? selectedId)
        {
            _deleteButton.SetEnabled(selectedId.HasValue && selectedId.Value != CentralNodeId);
        }

        internal void Delete()
        {
            int? selectedId = _selectedIdProvider();
            if (!selectedId.HasValue) return;
            if (selectedId.Value == CentralNodeId) return;

            if (_data == null) return;

            _data.RemoveNode(selectedId.Value);
            EditorUtility.SetDirty(_data);

            _setSelectedId?.Invoke(null);

            if (_canvas != null)
                _canvas.SetData(_data, null);

            NodeDeleted?.Invoke();
        }

        internal Button DeleteButton => _deleteButton;
        internal bool IsDeleteEnabled => _deleteButton.enabledSelf;
        internal Label InfoLabel => _infoLabel;
    }
}
