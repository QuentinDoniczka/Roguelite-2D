using System;
using System.Collections.Generic;
using System.Globalization;
using RogueliteAutoBattler.Combat.Core;
using UnityEngine;
using UnityEngine.Serialization;

namespace RogueliteAutoBattler.Data
{
    [CreateAssetMenu(fileName = "SkillTreeData", menuName = "Roguelite/Skill Tree Data")]
    public class SkillTreeData : ScriptableObject
    {
        public const int DefaultRingNodeCount = 6;
        public const float DefaultRingRadius = 5f;
        public const float DefaultUnitSize = 40f;
        public const float DefaultNodeSize = 48f;
        public const float DefaultEdgeThickness = 4f;
        public const int DefaultMaxLevel = 5;

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

        public enum StatModifierMode
        {
            Flat,
            Percent
        }

        internal static readonly (StatType stat, StatModifierMode mode, float valuePerLevel)[] DefaultNodeStatRotation =
        {
            (StatType.Hp, StatModifierMode.Percent, 5f),
            (StatType.Atk, StatModifierMode.Percent, 5f),
            (StatType.Def, StatModifierMode.Percent, 5f),
            (StatType.AttackSpeed, StatModifierMode.Percent, 5f),
            (StatType.CritRate, StatModifierMode.Percent, 5f),
            (StatType.RegenHp, StatModifierMode.Percent, 5f)
        };

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
            public StatType statModifierType;
            public StatModifierMode statModifierMode;
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
        [FormerlySerializedAs("defaultMaxLevel")]
        [SerializeField] private int defaultGeneratedMaxLevel = DefaultMaxLevel;

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
        public int DefaultGeneratedMaxLevel { get => defaultGeneratedMaxLevel; internal set => defaultGeneratedMaxLevel = value; }
        public Color EdgeColor { get => edgeColor; internal set => edgeColor = value; }
        public Color RingGuideColor { get => ringGuideColor; internal set => ringGuideColor = value; }
        public float EdgeThickness { get => edgeThickness; internal set => edgeThickness = value; }
        public IReadOnlyList<SkillNodeEntry> Nodes => nodes;

        public void GenerateNodes()
        {
            nodes.Clear();
            _cachedEdges = null;
            BuildRingLayout(nodes, ringNodeCount, ringRadius, baseCost, costMultiplierOdd, costMultiplierEven, costAdditivePerLevel, defaultGeneratedMaxLevel);
        }

        internal static void BuildRingLayout(List<SkillNodeEntry> output, int nodeCount, float radius,
            int defaultBaseCost = 1, float defaultMultOdd = 1f, float defaultMultEven = 1f, int defaultAdditive = 0, int defaultGeneratedMaxLevel = DefaultMaxLevel)
        {
            Debug.Assert(nodeCount > 0, "Ring node count must be positive");

            int rotationLength = DefaultNodeStatRotation.Length;
            for (int i = 0; i < nodeCount; i++)
            {
                float angle = i * (2f * Mathf.PI / nodeCount);
                Vector2 pos = new Vector2(radius * Mathf.Cos(angle), radius * Mathf.Sin(angle));
                var rotationEntry = DefaultNodeStatRotation[i % rotationLength];
                output.Add(new SkillNodeEntry
                {
                    id = i,
                    position = pos,
                    connectedNodeIds = new List<int>(),
                    costType = CostType.Gold,
                    maxLevel = defaultGeneratedMaxLevel,
                    baseCost = defaultBaseCost,
                    costMultiplierOdd = defaultMultOdd,
                    costMultiplierEven = defaultMultEven,
                    costAdditivePerLevel = defaultAdditive,
                    statModifierType = rotationEntry.stat,
                    statModifierMode = rotationEntry.mode,
                    statModifierValuePerLevel = rotationEntry.valuePerLevel
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

        public static bool IsMaxLevel(SkillNodeEntry node, int currentLevel)
        {
            return node.maxLevel > 0 && currentLevel >= node.maxLevel;
        }

        public static string GetStatDisplayName(StatType type)
        {
            return type switch
            {
                StatType.Hp => "HP",
                StatType.RegenHp => "Regen HP",
                StatType.Atk => "Attack",
                StatType.Def => "Defense",
                StatType.Mana => "Mana",
                StatType.Power => "Power",
                StatType.AttackSpeed => "Attack Speed",
                StatType.CritRate => "Crit Rate",
                _ => type.ToString()
            };
        }

        public static string FormatBonus(float value, StatModifierMode mode)
        {
            if (mode == StatModifierMode.Percent)
                return $"+{value.ToString("0.##", CultureInfo.InvariantCulture)}%";
            return $"+{value.ToString("0.##", CultureInfo.InvariantCulture)}";
        }

        internal void SetNode(int index, SkillNodeEntry entry)
        {
            Debug.Assert(index >= 0 && index < nodes.Count, $"SetNode index {index} out of range [0, {nodes.Count})");
            if (index < 0 || index >= nodes.Count) return;
            nodes[index] = entry;
            _cachedEdges = null;
        }

        internal void InitializeForTest(List<SkillNodeEntry> testNodes)
        {
            Debug.Assert(testNodes != null, "InitializeForTest requires non-null node list — pass empty list explicitly.");
            nodes = testNodes ?? new List<SkillNodeEntry>();
            _cachedEdges = null;
        }
    }
}
