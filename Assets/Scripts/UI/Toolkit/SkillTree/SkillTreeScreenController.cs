using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using RogueliteAutoBattler.Data;
using RogueliteAutoBattler.Economy;

namespace RogueliteAutoBattler.UI.Toolkit.SkillTree
{
    [RequireComponent(typeof(UIDocument))]
    public sealed class SkillTreeScreenController : MonoBehaviour
    {
        private const string ScreenElementName = "screen-skilltree";
        private const string ViewportElementName = "skilltree-viewport";
        private const string ContentElementName = "skilltree-content";
        private const string EdgeLayerElementName = "skilltree-edge-layer";
        private const string NodesLayerElementName = "skilltree-nodes-layer";
        private const string DetailPanelElementName = "skilltree-detail-panel";
        private const string DetailNameName = "skilltree-detail-name";
        private const string DetailLevelName = "skilltree-detail-level";
        private const string DetailCostName = "skilltree-detail-cost";
        private const string DetailBonusName = "skilltree-detail-bonus";
        private const string DetailUpgradeButtonName = "skilltree-detail-upgrade-btn";
        private const string DetailCloseButtonName = "skilltree-detail-close-btn";

        private const float UnitToPixelScale = 40f;
        private const int NoSelectedNodeIndex = -1;

        [SerializeField] private UIDocument _uiDocument;
        [SerializeField] private SkillTreeData _data;
        [SerializeField] private SkillTreeProgress _progress;
        [SerializeField] private GoldWallet _goldWallet;
        [SerializeField] private SkillPointWallet _skillPointWallet;

        private readonly List<SkillTreeNodeElement> _nodeElements = new();
        private SkillTreeEdgeLayer _edgeLayer;
        private SkillTreeDetailPanelController _detailController;
        private SkillTreePanZoomManipulator _panZoomManipulator;
        private int _selectedNodeIndex = NoSelectedNodeIndex;

        public IReadOnlyList<SkillTreeNodeElement> NodeElements => _nodeElements;
        public int SelectedNodeIndex => _selectedNodeIndex;

        private void Awake()
        {
            if (_uiDocument == null) _uiDocument = GetComponent<UIDocument>();
            if (_data == null || _progress == null)
            {
                Debug.LogError($"{nameof(SkillTreeScreenController)} requires SkillTreeData and SkillTreeProgress references.");
                return;
            }
            if (_goldWallet == null) _goldWallet = Object.FindFirstObjectByType<GoldWallet>();
            if (_skillPointWallet == null) _skillPointWallet = Object.FindFirstObjectByType<SkillPointWallet>();
            if (_goldWallet == null || _skillPointWallet == null)
            {
                Debug.LogError($"{nameof(SkillTreeScreenController)} could not locate GoldWallet or SkillPointWallet.");
                return;
            }

            var root = _uiDocument.rootVisualElement;
            if (root == null)
            {
                Debug.LogError($"{nameof(SkillTreeScreenController)} UIDocument has no rootVisualElement.");
                return;
            }

            Build(root);
        }

        private void Build(VisualElement root)
        {
            var viewport = root.Q<VisualElement>(ViewportElementName);
            var content = root.Q<VisualElement>(ContentElementName);
            var nodesLayer = root.Q<VisualElement>(NodesLayerElementName);
            var edgeLayerElement = root.Q<VisualElement>(EdgeLayerElementName);
            var panelRoot = root.Q<VisualElement>(DetailPanelElementName);
            var nameLabel = root.Q<Label>(DetailNameName);
            var levelLabel = root.Q<Label>(DetailLevelName);
            var costLabel = root.Q<Label>(DetailCostName);
            var bonusLabel = root.Q<Label>(DetailBonusName);
            var upgradeButton = root.Q<Button>(DetailUpgradeButtonName);
            var closeButton = root.Q<Button>(DetailCloseButtonName);

            if (viewport == null || content == null || nodesLayer == null || edgeLayerElement == null
                || panelRoot == null || nameLabel == null || levelLabel == null || costLabel == null
                || bonusLabel == null || upgradeButton == null || closeButton == null)
            {
                Debug.LogError($"{nameof(SkillTreeScreenController)} could not resolve all skill-tree elements in UXML.");
                return;
            }

            SpawnNodes(nodesLayer);
            ReplaceEdgeLayerElement(content, edgeLayerElement);
            RefreshAllNodeStates();
            RefreshEdgeLayer();

            _detailController = new SkillTreeDetailPanelController(
                panelRoot, nameLabel, levelLabel, costLabel, bonusLabel,
                upgradeButton, closeButton, _data, _progress, _goldWallet, _skillPointWallet);
            _detailController.NodeUpgraded += OnNodeUpgraded;
            _detailController.Closed += OnDetailClosed;

            _panZoomManipulator = new SkillTreePanZoomManipulator(viewport, content);
            viewport.AddManipulator(_panZoomManipulator);
        }

        private void SpawnNodes(VisualElement nodesLayer)
        {
            nodesLayer.Clear();
            _nodeElements.Clear();
            for (var i = 0; i < _data.Nodes.Count; i++)
            {
                var nodeElement = new SkillTreeNodeElement(i);
                nodeElement.SetDataPosition(_data.Nodes[i].position, UnitToPixelScale);
                var capturedIndex = i;
                nodeElement.Clicked += index => HandleNodeClicked(capturedIndex);
                nodesLayer.Add(nodeElement);
                _nodeElements.Add(nodeElement);
            }
        }

        private void ReplaceEdgeLayerElement(VisualElement content, VisualElement existingPlaceholder)
        {
            var index = content.IndexOf(existingPlaceholder);
            content.Remove(existingPlaceholder);
            _edgeLayer = new SkillTreeEdgeLayer();
            content.Insert(index, _edgeLayer);
        }

        private void RefreshAllNodeStates()
        {
            for (var i = 0; i < _nodeElements.Count; i++)
            {
                _nodeElements[i].SetState(ComputeNodeState(i));
            }
        }

        private SkillTreeNodeVisualState ComputeNodeState(int nodeIndex)
        {
            var currentLevel = _progress.GetLevel(nodeIndex);
            var node = _data.Nodes[nodeIndex];
            if (SkillTreeData.IsMaxLevel(node, currentLevel))
            {
                return SkillTreeNodeVisualState.Max;
            }
            if (currentLevel > 0)
            {
                return SkillTreeNodeVisualState.Purchased;
            }
            return HasUnlockedPrerequisite(nodeIndex)
                ? SkillTreeNodeVisualState.Available
                : SkillTreeNodeVisualState.Locked;
        }

        private bool HasUnlockedPrerequisite(int nodeIndex)
        {
            var node = _data.Nodes[nodeIndex];
            for (var i = 0; i < node.connectedNodeIds.Count; i++)
            {
                var connectedId = node.connectedNodeIds[i];
                var connectedIndex = FindIndexById(connectedId);
                if (connectedIndex >= 0 && _progress.GetLevel(connectedIndex) > 0)
                {
                    return true;
                }
            }
            return node.connectedNodeIds.Count == 0;
        }

        private int FindIndexById(int nodeId)
        {
            for (var i = 0; i < _data.Nodes.Count; i++)
            {
                if (_data.Nodes[i].id == nodeId)
                {
                    return i;
                }
            }
            return -1;
        }

        private void RefreshEdgeLayer()
        {
            if (_edgeLayer == null) return;
            var edges = _data.GetEdges();
            var positions = new List<Vector2>(_data.Nodes.Count);
            var ids = new List<int>(_data.Nodes.Count);
            var states = new List<SkillTreeNodeVisualState>(_data.Nodes.Count);
            for (var i = 0; i < _data.Nodes.Count; i++)
            {
                positions.Add(_data.Nodes[i].position);
                ids.Add(_data.Nodes[i].id);
                states.Add(ComputeNodeState(i));
            }
            _edgeLayer.SetEdges(edges, positions, ids, states, UnitToPixelScale);
        }

        private void HandleNodeClicked(int nodeIndex)
        {
            if (_panZoomManipulator != null && _panZoomManipulator.ExceededClickVersusDragThreshold)
            {
                return;
            }
            SetSelectedNode(nodeIndex);
            _detailController.Show(nodeIndex);
        }

        private void SetSelectedNode(int nodeIndex)
        {
            if (_selectedNodeIndex >= 0 && _selectedNodeIndex < _nodeElements.Count)
            {
                _nodeElements[_selectedNodeIndex].SetSelected(false);
            }
            _selectedNodeIndex = nodeIndex;
            if (_selectedNodeIndex >= 0 && _selectedNodeIndex < _nodeElements.Count)
            {
                _nodeElements[_selectedNodeIndex].SetSelected(true);
            }
        }

        private void OnNodeUpgraded(int nodeIndex)
        {
            RefreshAllNodeStates();
            RefreshEdgeLayer();
        }

        private void OnDetailClosed()
        {
            if (_selectedNodeIndex >= 0 && _selectedNodeIndex < _nodeElements.Count)
            {
                _nodeElements[_selectedNodeIndex].SetSelected(false);
            }
            _selectedNodeIndex = NoSelectedNodeIndex;
        }

        private void OnDestroy()
        {
            if (_detailController != null)
            {
                _detailController.NodeUpgraded -= OnNodeUpgraded;
                _detailController.Closed -= OnDetailClosed;
                _detailController.Dispose();
                _detailController = null;
            }
        }
    }
}
