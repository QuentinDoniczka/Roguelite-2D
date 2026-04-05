using System;
using System.Collections.Generic;
using UnityEngine;
using SysRandom = System.Random;

namespace RogueliteAutoBattler.Data
{
    [CreateAssetMenu(fileName = "SkillTreeData", menuName = "Roguelite/Skill Tree Data")]
    public class SkillTreeData : ScriptableObject
    {
        [Serializable]
        public struct SkillNodeEntry
        {
            public int id;
            public Vector2 position;
        }

        [Header("Generation")]
        [SerializeField] private int nodeCount = 10;
        [SerializeField] private int seed = 42;
        [SerializeField] private float placementRadius = 5f;

        [Header("Visual")]
        [SerializeField] private float unitSize = 200f;
        [SerializeField] private float nodeSize = 80f;
        [SerializeField] private Color nodeColor = new Color(0.3f, 0.3f, 0.3f, 1f);
        [SerializeField] private Color borderNormalColor = Color.gray;
        [SerializeField] private Color borderSelectedColor = Color.yellow;

        [Header("Generated Nodes")]
        [SerializeField] private List<SkillNodeEntry> nodes = new List<SkillNodeEntry>();

        public int NodeCount { get => nodeCount; internal set => nodeCount = value; }
        public int Seed { get => seed; internal set => seed = value; }
        public float PlacementRadius { get => placementRadius; internal set => placementRadius = value; }
        public float UnitSize { get => unitSize; internal set => unitSize = value; }
        public float NodeSize { get => nodeSize; internal set => nodeSize = value; }
        public Color NodeColor { get => nodeColor; internal set => nodeColor = value; }
        public Color BorderNormalColor { get => borderNormalColor; internal set => borderNormalColor = value; }
        public Color BorderSelectedColor { get => borderSelectedColor; internal set => borderSelectedColor = value; }
        public List<SkillNodeEntry> Nodes { get => nodes; internal set => nodes = value; }

        public void GenerateNodes()
        {
            nodes.Clear();
            var rng = new SysRandom(seed);

            for (int i = 0; i < nodeCount; i++)
            {
                float angle = (float)(rng.NextDouble() * 2.0 * Mathf.PI);
                float radius = Mathf.Sqrt((float)rng.NextDouble()) * placementRadius;
                var entry = new SkillNodeEntry
                {
                    id = i,
                    position = new Vector2(radius * Mathf.Cos(angle), radius * Mathf.Sin(angle))
                };
                nodes.Add(entry);
            }
        }
    }
}
