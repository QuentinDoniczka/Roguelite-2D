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

        [Header("Edge Visual")]
        [SerializeField] private Color _edgeColor = new Color(0.6f, 0.6f, 0.6f, 1f);
        [SerializeField] private float _edgeThickness = 4f;

        [Header("Visuals")]
        [SerializeField] private Sprite _circleSprite;

        private readonly List<SkillTreeNode> _nodes = new List<SkillTreeNode>();
        private readonly List<RectTransform> _edgeLines = new List<RectTransform>();
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
                _edgeColor = _data.EdgeColor;
                _edgeThickness = _data.EdgeThickness;

                if (_data.Nodes.Count > 0)
                {
                    foreach (var entry in _data.Nodes)
                        CreateNode(entry.id, entry.position);

                    var edges = _data.GetEdges();
                    foreach (var (fromId, toId) in edges)
                    {
                        if (fromId < _data.Nodes.Count && toId < _data.Nodes.Count)
                            CreateEdge(_data.Nodes[fromId].position, _data.Nodes[toId].position, fromId, toId);
                    }
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

        private void CreateEdge(Vector2 fromPosition, Vector2 toPosition, int fromId, int toId)
        {
            var edgeGo = new GameObject($"Edge_{fromId}_{toId}");
            edgeGo.transform.SetParent(_content, false);

            var edgeRect = edgeGo.AddComponent<RectTransform>();
            edgeRect.anchorMin = Vector2.one * 0.5f;
            edgeRect.anchorMax = Vector2.one * 0.5f;
            edgeRect.pivot = Vector2.one * 0.5f;

            Vector2 fromPos = fromPosition * _unitSize;
            Vector2 toPos = toPosition * _unitSize;
            Vector2 midpoint = (fromPos + toPos) * 0.5f;
            float distance = Vector2.Distance(fromPos, toPos);
            float angle = Mathf.Atan2(toPos.y - fromPos.y, toPos.x - fromPos.x) * Mathf.Rad2Deg;

            edgeRect.anchoredPosition = midpoint;
            edgeRect.sizeDelta = new Vector2(distance, _edgeThickness);
            edgeRect.localRotation = Quaternion.Euler(0f, 0f, angle);
            edgeRect.localScale = Vector3.one;

            var edgeImage = edgeGo.AddComponent<Image>();
            edgeImage.color = _edgeColor;
            edgeImage.raycastTarget = false;

            edgeGo.transform.SetAsFirstSibling();
            _edgeLines.Add(edgeRect);
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
            foreach (var edge in _edgeLines)
            {
                if (edge != null)
                    SafeDestroy(edge.gameObject);
            }
            _edgeLines.Clear();

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
