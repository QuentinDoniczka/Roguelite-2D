using System;
using System.Collections.Generic;
using UnityEngine;

namespace RogueliteAutoBattler.Data
{
    [CreateAssetMenu(fileName = "SkillTreeData", menuName = "Roguelite/Skill Tree Data")]
    public class SkillTreeData : ScriptableObject
    {
        public const string DefaultAssetPath = "Assets/Data/SkillTreeData.asset";

        [Serializable]
        public struct SkillNodeEntry
        {
            public int id;
            public Vector2 position;
        }

        [Header("Generation")]
        [SerializeField] private float ringRadius = 5f;
        [SerializeField, Range(3, 24)] private int ringNodeCount = 8;

        [Header("Visual")]
        [SerializeField] private float unitSize = 200f;
        [SerializeField] private float nodeSize = 80f;
        [SerializeField] private Color nodeColor = new Color(0.3f, 0.3f, 0.3f, 1f);
        [SerializeField] private Color borderNormalColor = Color.gray;
        [SerializeField] private Color borderSelectedColor = Color.yellow;

        [Header("Generated Nodes")]
        [SerializeField] private List<SkillNodeEntry> nodes = new List<SkillNodeEntry>();

        public float RingRadius { get => ringRadius; internal set => ringRadius = value; }
        public int RingNodeCount { get => ringNodeCount; internal set => ringNodeCount = value; }
        public float UnitSize { get => unitSize; internal set => unitSize = value; }
        public float NodeSize { get => nodeSize; internal set => nodeSize = value; }
        public Color NodeColor { get => nodeColor; internal set => nodeColor = value; }
        public Color BorderNormalColor { get => borderNormalColor; internal set => borderNormalColor = value; }
        public Color BorderSelectedColor { get => borderSelectedColor; internal set => borderSelectedColor = value; }
        public IReadOnlyList<SkillNodeEntry> Nodes => nodes;

        public void GenerateNodes()
        {
            nodes.Clear();
            BuildRingLayout(nodes, ringNodeCount, ringRadius);
        }

        internal static void BuildRingLayout(List<SkillNodeEntry> output, int nodeCount, float radius)
        {
            Debug.Assert(nodeCount > 0, "Ring node count must be positive");

            output.Add(new SkillNodeEntry { id = 0, position = Vector2.zero });

            for (int i = 0; i < nodeCount; i++)
            {
                float angle = i * (2f * Mathf.PI / nodeCount);
                Vector2 pos = new Vector2(radius * Mathf.Cos(angle), radius * Mathf.Sin(angle));
                output.Add(new SkillNodeEntry { id = i + 1, position = pos });
            }
        }
    }
}
