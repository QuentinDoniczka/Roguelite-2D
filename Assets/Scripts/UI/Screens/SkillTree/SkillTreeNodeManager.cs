using System;
using System.Collections.Generic;
using RogueliteAutoBattler.Data;
using UnityEngine;
using UnityEngine.UI;

namespace RogueliteAutoBattler.UI.Screens.SkillTree
{
    public class SkillTreeNodeManager : MonoBehaviour
    {
        private const float BorderPadding = 4f;

        [Header("Data")]
        [SerializeField] private SkillTreeData _data;

        [Header("Container")]
        [SerializeField] private RectTransform _content;

        [Header("Node Sizing")]
        [SerializeField] private float _nodeSize = SkillTreeData.DefaultNodeSize;
        [SerializeField] private float _unitSize = SkillTreeData.DefaultUnitSize;

        [Header("Node Colors")]
        [SerializeField] private Color _nodeColor = new Color(0.3f, 0.3f, 0.3f, 1f);
        [SerializeField] private Color _borderNormalColor = Color.gray;
        [SerializeField] private Color _borderSelectedColor = Color.yellow;

        [Header("Visuals")]
        [SerializeField] private Sprite _circleSprite;

        private readonly List<SkillTreeNode> _nodes = new List<SkillTreeNode>();
        private SkillTreeNode _selectedNode;

        public event Action<SkillTreeNode> OnNodeSelected;
        public event Action OnNodeDeselected;

        public void Initialize()
        {
            if (_data != null)
            {
                _nodeSize = _data.NodeSize;
                _unitSize = _data.UnitSize;
                _nodeColor = _data.NodeColor;
                _borderNormalColor = _data.BorderNormalColor;
                _borderSelectedColor = _data.BorderSelectedColor;

                if (_data.Nodes.Count > 0)
                {
                    foreach (var entry in _data.Nodes)
                        CreateNode(entry.id, entry.position);
                }
                else
                {
                    Debug.LogWarning("[SkillTreeNodeManager] SkillTreeData has no generated nodes. Use the Skill Tree Designer to generate nodes first.");
                }

                return;
            }

            var fallbackNodes = new List<SkillTreeData.SkillNodeEntry>();
            SkillTreeData.BuildRingLayout(fallbackNodes, SkillTreeData.DefaultRingNodeCount, SkillTreeData.DefaultRingRadius);
            foreach (var entry in fallbackNodes)
                CreateNode(entry.id, entry.position);
        }

        private void CreateNode(int index, Vector2 treePosition)
        {
            var nodeGo = new GameObject($"Node_{index}");
            nodeGo.transform.SetParent(_content, false);

            var nodeRect = nodeGo.AddComponent<RectTransform>();
            nodeRect.anchorMin = Vector2.one * 0.5f;
            nodeRect.anchorMax = Vector2.one * 0.5f;
            nodeRect.pivot = Vector2.one * 0.5f;
            nodeRect.anchoredPosition = treePosition * _unitSize;
            nodeRect.sizeDelta = Vector2.one * _nodeSize;
            nodeRect.localScale = Vector3.one;

            var iconImage = nodeGo.AddComponent<Image>();
            iconImage.color = _borderNormalColor;
            iconImage.sprite = _circleSprite;
            iconImage.type = Image.Type.Simple;
            iconImage.preserveAspect = true;

            var fillGo = new GameObject("Fill");
            fillGo.transform.SetParent(nodeGo.transform, false);

            var fillRect = fillGo.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.sizeDelta = Vector2.one * -BorderPadding * 2;
            fillRect.localScale = Vector3.one;

            var fillImage = fillGo.AddComponent<Image>();
            fillImage.color = _nodeColor;
            fillImage.sprite = _circleSprite;
            fillImage.raycastTarget = false;
            fillImage.preserveAspect = true;

            var node = nodeGo.AddComponent<SkillTreeNode>();
            node.Setup(iconImage, _borderNormalColor, _borderSelectedColor);
            node.Initialize(index);
            node.OnNodeClicked += HandleNodeClicked;

            _nodes.Add(node);
        }

        private void HandleNodeClicked(SkillTreeNode node)
        {
            if (_selectedNode == node)
            {
                _selectedNode.SetSelected(false);
                _selectedNode = null;
                OnNodeDeselected?.Invoke();
                return;
            }

            if (_selectedNode != null)
            {
                _selectedNode.SetSelected(false);
            }

            _selectedNode = node;
            _selectedNode.SetSelected(true);
            OnNodeSelected?.Invoke(_selectedNode);
        }

        public void DeselectAll()
        {
            if (_selectedNode == null) return;

            _selectedNode.SetSelected(false);
            _selectedNode = null;
            OnNodeDeselected?.Invoke();
        }

        public void ClearNodes()
        {
            foreach (var node in _nodes)
            {
                if (node != null)
                {
                    node.OnNodeClicked -= HandleNodeClicked;
                    SafeDestroy(node.gameObject);
                }
            }
            _nodes.Clear();
            _selectedNode = null;

            if (_content != null)
            {
                for (int i = _content.childCount - 1; i >= 0; i--)
                    SafeDestroy(_content.GetChild(i).gameObject);
            }
        }

        private void OnDestroy()
        {
            foreach (var node in _nodes)
            {
                if (node != null)
                {
                    node.OnNodeClicked -= HandleNodeClicked;
                }
            }
        }

        private static void SafeDestroy(GameObject go)
        {
            if (Application.isPlaying)
                Destroy(go);
            else
                DestroyImmediate(go);
        }
    }
}
