using System;
using System.Collections.Generic;
using RogueliteAutoBattler.Data;
using UnityEngine;

namespace RogueliteAutoBattler.Editor.Tools
{
    internal static class NodeConnectionsInspector
    {
        public readonly struct ConnectionRow
        {
            public readonly int OtherNodeId;
            public readonly float DistanceUnits;
            public readonly bool IsOutgoing;

            public ConnectionRow(int otherNodeId, float distance, bool isOutgoing)
            {
                OtherNodeId = otherNodeId;
                DistanceUnits = distance;
                IsOutgoing = isOutgoing;
            }
        }

        private static readonly IReadOnlyList<ConnectionRow> Empty = Array.Empty<ConnectionRow>();

        public static IReadOnlyList<ConnectionRow> CollectConnections(
            SkillTreeData data,
            int selectedNodeIndex)
        {
            if (data == null || selectedNodeIndex < 0 || selectedNodeIndex >= data.Nodes.Count) return Empty;
            var selected = data.Nodes[selectedNodeIndex];
            var rows = new List<ConnectionRow>();
            foreach (var edge in data.GetEdges())
            {
                int otherId;
                bool outgoing;
                if (edge.fromId == selected.id)       { otherId = edge.toId; outgoing = true; }
                else if (edge.toId == selected.id)    { otherId = edge.fromId; outgoing = false; }
                else                                   continue;

                int otherIdx = -1;
                for (int i = 0; i < data.Nodes.Count; i++)
                {
                    if (data.Nodes[i].id == otherId) { otherIdx = i; break; }
                }
                if (otherIdx < 0) continue;
                float dist = Vector2.Distance(selected.position, data.Nodes[otherIdx].position);
                rows.Add(new ConnectionRow(otherId, dist, outgoing));
            }
            rows.Sort((a, b) => a.OtherNodeId.CompareTo(b.OtherNodeId));
            return rows;
        }
    }
}
