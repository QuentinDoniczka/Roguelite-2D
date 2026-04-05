using System;
using System.Collections.Generic;
using UnityEngine;

namespace RogueliteAutoBattler.Data
{
    [CreateAssetMenu(fileName = "SkillTreeData", menuName = "Roguelite/Skill Tree Data")]
    public class SkillTreeData : ScriptableObject
    {
        public const string DefaultAssetPath = "Assets/Data/SkillTreeData.asset";

        public const int DefaultRingNodeCount = 6;
        public const float DefaultRingRadius = 5f;
        public const float DefaultUnitSize = 200f;
        public const float DefaultNodeSize = 80f;
        public const float DefaultEdgeThickness = 4f;

        public static readonly Color DefaultNodeColor = new Color(0.3f, 0.3f, 0.3f, 1f);
        public static readonly Color DefaultBorderNormalColor = Color.gray;
        public static readonly Color DefaultBorderSelectedColor = Color.yellow;
        public static readonly Color DefaultEdgeColor = new Color(0.6f, 0.6f, 0.6f, 1f);
        public static readonly Color DefaultRingGuideColor = new Color(0.25f, 0.25f, 0.25f, 0.5f);

        public enum CostType
        {
            Gold,
            SkillPoint
        }

        public enum StatModifierType
        {
            HP,
            RegenHP,
            Attack,
            Defense,
            Mana,
            Power
        }

        [Serializable]
        public struct SkillNodeEntry
        {
            public int id;
            public Vector2 position;
            public List<int> connectedNodeIds;
            public CostType costType;
            public int maxLevel;
            public int baseCost;
            public float costMultiplierOdd;
            public float costMultiplierEven;
            public int costAdditivePerLevel;
            public StatModifierType statModifierType;
            public float statModifierValuePerLevel;
        }

        [Header("Generation")]
        [SerializeField] private float ringRadius = DefaultRingRadius;
        [SerializeField, Range(3, 24)] private int ringNodeCount = DefaultRingNodeCount;

        [Header("Visual")]
        [SerializeField] private float unitSize = DefaultUnitSize;
        [SerializeField] private float nodeSize = DefaultNodeSize;
        [SerializeField] private Color nodeColor = DefaultNodeColor;
        [SerializeField] private Color borderNormalColor = DefaultBorderNormalColor;
        [SerializeField] private Color borderSelectedColor = DefaultBorderSelectedColor;

        [Header("Cost Formula")]
        [SerializeField] private int baseCost = 1;
        [SerializeField] private float costMultiplierOdd = 1f;
        [SerializeField] private float costMultiplierEven = 1f;
        [SerializeField] private int costAdditivePerLevel = 0;

        [Header("Edge Visual")]
        [SerializeField] private Color edgeColor = DefaultEdgeColor;
        [SerializeField] private Color ringGuideColor = DefaultRingGuideColor;
        [SerializeField] private float edgeThickness = DefaultEdgeThickness;

        [Header("Generated Nodes")]
        [SerializeField] private List<SkillNodeEntry> nodes = new List<SkillNodeEntry>();

        private (int fromId, int toId)[] _cachedEdges;

        private void OnValidate()
        {
            _cachedEdges = null;
        }

        public float RingRadius { get => ringRadius; internal set => ringRadius = value; }
        public int RingNodeCount { get => ringNodeCount; internal set => ringNodeCount = value; }
        public float UnitSize { get => unitSize; internal set => unitSize = value; }
        public float NodeSize { get => nodeSize; internal set => nodeSize = value; }
        public Color NodeColor { get => nodeColor; internal set => nodeColor = value; }
        public Color BorderNormalColor { get => borderNormalColor; internal set => borderNormalColor = value; }
        public Color BorderSelectedColor { get => borderSelectedColor; internal set => borderSelectedColor = value; }
        public int BaseCost { get => baseCost; internal set => baseCost = value; }
        public float CostMultiplierOdd { get => costMultiplierOdd; internal set => costMultiplierOdd = value; }
        public float CostMultiplierEven { get => costMultiplierEven; internal set => costMultiplierEven = value; }
        public int CostAdditivePerLevel { get => costAdditivePerLevel; internal set => costAdditivePerLevel = value; }
        public Color EdgeColor { get => edgeColor; internal set => edgeColor = value; }
        public Color RingGuideColor { get => ringGuideColor; internal set => ringGuideColor = value; }
        public float EdgeThickness { get => edgeThickness; internal set => edgeThickness = value; }
        public IReadOnlyList<SkillNodeEntry> Nodes => nodes;

        public void GenerateNodes()
        {
            nodes.Clear();
            _cachedEdges = null;
            BuildRingLayout(nodes, ringNodeCount, ringRadius, baseCost, costMultiplierOdd, costMultiplierEven, costAdditivePerLevel);
        }

        internal static void BuildRingLayout(List<SkillNodeEntry> output, int nodeCount, float radius,
            int defaultBaseCost = 1, float defaultMultOdd = 1f, float defaultMultEven = 1f, int defaultAdditive = 0)
        {
            Debug.Assert(nodeCount > 0, "Ring node count must be positive");

            for (int i = 0; i < nodeCount; i++)
            {
                float angle = i * (2f * Mathf.PI / nodeCount);
                Vector2 pos = new Vector2(radius * Mathf.Cos(angle), radius * Mathf.Sin(angle));
                output.Add(new SkillNodeEntry
                {
                    id = i,
                    position = pos,
                    connectedNodeIds = new List<int> { (i + 1) % nodeCount },
                    costType = CostType.Gold,
                    maxLevel = 0,
                    baseCost = defaultBaseCost,
                    costMultiplierOdd = defaultMultOdd,
                    costMultiplierEven = defaultMultEven,
                    costAdditivePerLevel = defaultAdditive,
                    statModifierType = StatModifierType.HP,
                    statModifierValuePerLevel = 0f
                });
            }
        }

        public (int fromId, int toId)[] GetEdges()
        {
            if (_cachedEdges != null) return _cachedEdges;

            var edges = new List<(int, int)>();
            foreach (var node in nodes)
            {
                if (node.connectedNodeIds == null) continue;
                foreach (int targetId in node.connectedNodeIds)
                    edges.Add((node.id, targetId));
            }
            _cachedEdges = edges.ToArray();
            return _cachedEdges;
        }

        public static int ComputeNodeCost(SkillNodeEntry node, int level)
        {
            int cost = node.baseCost;
            for (int i = 1; i <= level; i++)
            {
                float multiplier = (i % 2 == 1) ? node.costMultiplierOdd : node.costMultiplierEven;
                cost = Mathf.FloorToInt(cost * multiplier) + node.costAdditivePerLevel;
            }
            return cost;
        }

        internal void SetNode(int index, SkillNodeEntry entry)
        {
            Debug.Assert(index >= 0 && index < nodes.Count, $"SetNode index {index} out of range [0, {nodes.Count})");
            if (index < 0 || index >= nodes.Count) return;
            nodes[index] = entry;
            _cachedEdges = null;
        }
    }
}
