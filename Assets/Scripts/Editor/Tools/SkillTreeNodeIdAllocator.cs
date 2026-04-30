using System.Collections.Generic;
using RogueliteAutoBattler.Data;

namespace RogueliteAutoBattler.Editor.Tools
{
    internal static class SkillTreeNodeIdAllocator
    {
        public static int ComputeNextNodeId(IReadOnlyList<SkillTreeData.SkillNodeEntry> nodes)
        {
            if (nodes.Count == 0) return 0;
            int max = nodes[0].id;
            for (int i = 1; i < nodes.Count; i++)
            {
                if (nodes[i].id > max)
                    max = nodes[i].id;
            }
            return max + 1;
        }
    }
}
