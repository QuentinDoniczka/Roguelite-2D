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

        [Serializable]
        public struct SkillNodeEntry
        {
            public int id;
            public Vector2 position;
            public List<int> connectedNodeIds;
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

        [Header("Edge Visual")]
        [SerializeField] private Color edgeColor = DefaultEdgeColor;
        [SerializeField] private Color ringGuideColor = new Color(0.25f, 0.25f, 0.25f, 0.5f);
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
        public Color EdgeColor { get => edgeColor; internal set => edgeColor = value; }
        public Color RingGuideColor { get => ringGuideColor; internal set => ringGuideColor = value; }
        public float EdgeThickness { get => edgeThickness; internal set => edgeThickness = value; }
        public IReadOnlyList<SkillNodeEntry> Nodes => nodes;

        public void GenerateNodes()
        {
            nodes.Clear();
            _cachedEdges = null;
            BuildRingLayout(nodes, ringNodeCount, ringRadius);
        }

        internal static void BuildRingLayout(List<SkillNodeEntry> output, int nodeCount, float radius)
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
                    connectedNodeIds = new List<int> { (i + 1) % nodeCount }
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
    }
}
