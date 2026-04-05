using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SysRandom = System.Random;

namespace RogueliteAutoBattler.UI.Screens.SkillTree
{
    public class SkillTreeNodeManager : MonoBehaviour
    {
        private const int PlaceholderNodeCount = 10;
        private const int DeterministicSeed = 42;
        private const float PlacementRadius = 5f;

        [Header("Container")]
        [SerializeField] private RectTransform _content;

        [Header("Node Sizing")]
        [SerializeField] private float _nodeSize = 80f;
        [SerializeField] private float _unitSize = 200f;

        [Header("Node Colors")]
        [SerializeField] private Color _nodeColor = new Color(0.3f, 0.3f, 0.3f, 1f);
        [SerializeField] private Color _borderNormalColor = Color.gray;
        [SerializeField] private Color _borderSelectedColor = Color.yellow;

        private readonly List<SkillTreeNode> _nodes = new List<SkillTreeNode>();
        private SkillTreeNode _selectedNode;

        public event Action<SkillTreeNode> OnNodeSelected;
        public event Action OnNodeDeselected;

        public void Initialize()
        {
            var rng = new SysRandom(DeterministicSeed);

            for (int i = 0; i < PlaceholderNodeCount; i++)
            {
                float angle = (float)(rng.NextDouble() * 2.0 * Mathf.PI);
                float radius = Mathf.Sqrt((float)rng.NextDouble()) * PlacementRadius;
                Vector2 treePosition = new Vector2(radius * Mathf.Cos(angle), radius * Mathf.Sin(angle));
                CreateNode(i, treePosition);
            }
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
            iconImage.color = _nodeColor;

            var borderGo = new GameObject("Border");
            borderGo.transform.SetParent(nodeGo.transform, false);

            var borderRect = borderGo.AddComponent<RectTransform>();
            borderRect.anchorMin = Vector2.zero;
            borderRect.anchorMax = Vector2.one;
            borderRect.sizeDelta = Vector2.zero;
            borderRect.localScale = Vector3.one;

            var borderImage = borderGo.AddComponent<Image>();
            borderImage.color = _borderNormalColor;
            borderImage.raycastTarget = false;

            var node = nodeGo.AddComponent<SkillTreeNode>();
            node.Setup(borderImage, _borderNormalColor, _borderSelectedColor);
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
    }
}
