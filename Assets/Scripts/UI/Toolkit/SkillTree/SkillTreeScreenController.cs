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
        private readonly List<Vector2> _positionsCache = new();
        private readonly List<SkillTreeNodeVisualState> _statesCache = new();
        private SkillTreeEdgeLayer _edgeLayer;
        private SkillTreeDetailPanelController _detailController;
        private SkillTreePanZoomManipulator _panZoomManipulator;
        private SkillTreeStateEvaluator _stateEvaluator;
        private VisualElement _viewport;
        private VisualElement _content;
        private bool _hasCenteredContentOnViewport;
        private int _selectedNodeIndex = NoSelectedNodeIndex;

        public IReadOnlyList<SkillTreeNodeElement> NodeElements => _nodeElements;
        public int SelectedNodeIndex => _selectedNodeIndex;

        internal Vector3 ContentTargetPosition => _content != null ? _content.transform.position : Vector3.zero;
        internal bool HasCenteredContent => _hasCenteredContentOnViewport;
        internal VisualElement Viewport => _viewport;

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

            _stateEvaluator = new SkillTreeStateEvaluator(_data, _progress);

            SpawnNodes(nodesLayer);
            ReplaceEdgeLayerElement(content, edgeLayerElement);
            RefreshAllNodeStates();
            RefreshEdgeLayer();

            _detailController = new SkillTreeDetailPanelController(
                panelRoot, nameLabel, levelLabel, costLabel, bonusLabel,
                upgradeButton, closeButton, _data, _progress, _goldWallet, _skillPointWallet);
            _detailController.NodeUpgraded += OnNodeUpgraded;
            _detailController.Closed += OnDetailClosed;

            _viewport = viewport;
            _content = content;

            _panZoomManipulator = new SkillTreePanZoomManipulator(viewport, content);
            viewport.AddManipulator(_panZoomManipulator);

            viewport.RegisterCallback<GeometryChangedEvent>(HandleViewportGeometryChangedToCenterContent);
        }

        private void HandleViewportGeometryChangedToCenterContent(GeometryChangedEvent evt)
        {
            if (_hasCenteredContentOnViewport) return;
            if (_viewport.contentRect.width <= 0f || _viewport.contentRect.height <= 0f) return;
            _content.transform.position = new Vector3(_viewport.contentRect.width * 0.5f, _viewport.contentRect.height * 0.5f, 0f);
            _hasCenteredContentOnViewport = true;
        }

        private void SpawnNodes(VisualElement nodesLayer)
        {
            nodesLayer.Clear();
            _nodeElements.Clear();
            for (var i = 0; i < _data.Nodes.Count; i++)
            {
                var nodeElement = new SkillTreeNodeElement(i);
                nodeElement.SetDataPosition(_data.Nodes[i].position, UnitToPixelScale);
                nodeElement.Clicked += HandleNodeClicked;
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
                _nodeElements[i].SetState(_stateEvaluator.GetState(i));
            }
        }

        private void RefreshEdgeLayer()
        {
            if (_edgeLayer == null) return;
            var edges = _data.GetEdges();
            _positionsCache.Clear();
            _statesCache.Clear();
            for (var i = 0; i < _data.Nodes.Count; i++)
            {
                _positionsCache.Add(_data.Nodes[i].position);
                _statesCache.Add(_stateEvaluator.GetState(i));
            }
            _edgeLayer.SetEdges(edges, _positionsCache, _stateEvaluator.IdToIndexMap, _statesCache, UnitToPixelScale);
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

        private void OnNodeUpgraded(int _)
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
            if (_viewport != null)
            {
                _viewport.UnregisterCallback<GeometryChangedEvent>(HandleViewportGeometryChangedToCenterContent);
            }
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
