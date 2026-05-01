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
        public const int CentralNodeId = 0;
        public const float DefaultUnitSize = 40f;
        public const float DefaultNodeSize = 48f;
        public const float DefaultEdgeThickness = 4f;
        public const int DefaultMaxLevel = 5;
        public const int DefaultCentralUnlockCost = 100;
        private const int CentralMaxLevel = 1;

        public static readonly Color DefaultNodeColor = new Color(0.3f, 0.3f, 0.3f, 1f);
        public static readonly Color DefaultBorderNormalColor = Color.gray;
        public static readonly Color DefaultBorderSelectedColor = Color.yellow;
        public static readonly Color DefaultEdgeColor = new Color(0.6f, 0.6f, 0.6f, 1f);

        internal static class FieldNames
        {
            public const string UnitSize = nameof(SkillTreeData.unitSize);
            public const string NodeSize = nameof(SkillTreeData.nodeSize);
            public const string NodeColor = nameof(SkillTreeData.nodeColor);
            public const string BorderNormalColor = nameof(SkillTreeData.borderNormalColor);
            public const string BorderSelectedColor = nameof(SkillTreeData.borderSelectedColor);
            public const string BaseCost = nameof(SkillTreeData.baseCost);
            public const string CostMultiplierOdd = nameof(SkillTreeData.costMultiplierOdd);
            public const string CostMultiplierEven = nameof(SkillTreeData.costMultiplierEven);
            public const string CostAdditivePerLevel = nameof(SkillTreeData.costAdditivePerLevel);
            public const string DefaultGeneratedMaxLevel = nameof(SkillTreeData.defaultGeneratedMaxLevel);
            public const string EdgeColor = nameof(SkillTreeData.edgeColor);
            public const string EdgeThickness = nameof(SkillTreeData.edgeThickness);
        }

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
        [SerializeField] private int costAdditivePerLevel;
        [FormerlySerializedAs("defaultMaxLevel")]
        [SerializeField] private int defaultGeneratedMaxLevel = DefaultMaxLevel;

        [Header("Edge Visual")]
        [SerializeField] private Color edgeColor = DefaultEdgeColor;
        [SerializeField] private float edgeThickness = DefaultEdgeThickness;

        [Header("Central Node")]
        [SerializeField] private int centralUnlockCost = DefaultCentralUnlockCost;

        [Header("Nodes")]
        [SerializeField] private List<SkillNodeEntry> nodes = new List<SkillNodeEntry>();

        private (int fromId, int toId)[] _cachedEdges;

        private void OnEnable()
        {
            EnsureCentralNode();
        }

        private void OnValidate()
        {
            EnsureCentralNode();
        }

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
        public float EdgeThickness { get => edgeThickness; internal set => edgeThickness = value; }
        public int CentralUnlockCost { get => centralUnlockCost; internal set => centralUnlockCost = value; }
        public IReadOnlyList<SkillNodeEntry> Nodes => nodes;

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
                StatType.None => "(none)",
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

        internal void AddNode(SkillNodeEntry entry)
        {
            foreach (var existing in nodes)
            {
                if (existing.id == entry.id)
                    throw new ArgumentException($"A node with id {entry.id} already exists.");
            }
            nodes.Add(entry);
            _cachedEdges = null;
        }

        internal void AddEdge(int parentId, int childId)
        {
            if (parentId == childId)
                throw new ArgumentException($"Self-loop not allowed: parentId == childId == {parentId}.");
            int parentIndex = IndexOfId(parentId);
            if (parentIndex < 0)
                throw new ArgumentException($"No node found with parentId {parentId}.");

            var parent = nodes[parentIndex];
            if (parent.connectedNodeIds == null)
                parent.connectedNodeIds = new List<int>();
            parent.connectedNodeIds.Add(childId);
            nodes[parentIndex] = parent;
            _cachedEdges = null;
        }

        private int IndexOfId(int id)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i].id == id) return i;
            }
            return -1;
        }

        public bool RemoveNode(int id)
        {
            if (id == CentralNodeId)
            {
                Debug.LogWarning("Cannot remove central node (id 0).");
                return false;
            }

            int targetIndex = IndexOfId(id);
            if (targetIndex < 0) return false;

            nodes.RemoveAt(targetIndex);

            for (int i = 0; i < nodes.Count; i++)
            {
                var current = nodes[i];
                if (current.connectedNodeIds == null) continue;
                if (current.connectedNodeIds.RemoveAll(connectedId => connectedId == id) > 0)
                {
                    nodes[i] = current;
                }
            }

            _cachedEdges = null;
            return true;
        }

        internal void EnsureCentralNode()
        {
            int existingIndex = IndexOfId(CentralNodeId);

            List<int> preservedChildren = existingIndex >= 0 && nodes[existingIndex].connectedNodeIds != null
                ? new List<int>(nodes[existingIndex].connectedNodeIds)
                : new List<int>();

            var centralEntry = new SkillNodeEntry
            {
                id = CentralNodeId,
                position = Vector2.zero,
                connectedNodeIds = preservedChildren,
                costType = CostType.Gold,
                maxLevel = CentralMaxLevel,
                baseCost = centralUnlockCost,
                costMultiplierOdd = 1f,
                costMultiplierEven = 1f,
                costAdditivePerLevel = 0,
                statModifierType = StatType.None,
                statModifierMode = StatModifierMode.Flat,
                statModifierValuePerLevel = 0f
            };

            if (existingIndex >= 0)
                nodes[existingIndex] = centralEntry;
            else
                nodes.Add(centralEntry);

            _cachedEdges = null;
        }

        internal void AddBranchNode(SkillNodeEntry entry, int parentId)
        {
            if (parentId == entry.id)
                throw new ArgumentException($"Self-loop not allowed: parentId == entry.id == {parentId}.");

            int parentIndex = IndexOfId(parentId);
            if (parentIndex < 0)
                throw new ArgumentException($"No node found with parentId {parentId}.");

            if (IndexOfId(entry.id) >= 0)
                throw new ArgumentException($"A node with id {entry.id} already exists.");

            nodes.Add(entry);

            var parent = nodes[parentIndex];
            if (parent.connectedNodeIds == null)
                parent.connectedNodeIds = new List<int>();
            parent.connectedNodeIds.Add(entry.id);
            nodes[parentIndex] = parent;

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
