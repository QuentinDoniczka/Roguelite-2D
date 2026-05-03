using System.Collections.Generic;
using RogueliteAutoBattler.Data;
using UnityEngine;

namespace RogueliteAutoBattler.Editor.Tools
{
    internal static class MirrorPartnerFinder
    {
        public const float DefaultMatchToleranceUnits = 0.5f;

        public static int FindPartnerIndex(
            IReadOnlyList<SkillTreeData.SkillNodeEntry> nodes,
            int sourceIndex,
            float mirrorAxisDegrees,
            float toleranceUnits)
        {
            if (nodes == null || sourceIndex < 0 || sourceIndex >= nodes.Count || toleranceUnits <= 0f)
                return -1;
            Vector2 source = nodes[sourceIndex].position;
            // reflection across line through origin at angle t: R = (cos2t·x + sin2t·y, sin2t·x − cos2t·y)
            float t2 = mirrorAxisDegrees * 2f * Mathf.Deg2Rad;
            float c = Mathf.Cos(t2);
            float s = Mathf.Sin(t2);
            Vector2 reflected = new Vector2(source.x * c + source.y * s, source.x * s - source.y * c);
            int best = -1;
            float bestDist = toleranceUnits;
            for (int i = 0; i < nodes.Count; i++)
            {
                if (i == sourceIndex) continue;
                float d = Vector2.Distance(nodes[i].position, reflected);
                if (d < bestDist) { bestDist = d; best = i; }
            }
            return best;
        }
    }
}
