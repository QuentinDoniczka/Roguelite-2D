using System.Collections.Generic;

namespace RogueliteAutoBattler.Data
{
    public static class SkillTreeRefundCalculator
    {
        public readonly struct Refund
        {
            public readonly int Gold;
            public readonly int SkillPoint;

            public Refund(int gold, int skillPoint)
            {
                Gold = gold;
                SkillPoint = skillPoint;
            }
        }

        public static Refund Compute(SkillTreeData currentTree, IReadOnlyList<int> savedLevels)
        {
            if (currentTree == null || savedLevels == null) return new Refund(0, 0);

            int gold = 0;
            int skillPoint = 0;
            var nodes = currentTree.Nodes;
            int boundedCount = System.Math.Min(savedLevels.Count, nodes.Count);

            for (int i = 0; i < boundedCount; i++)
            {
                int spentLevel = savedLevels[i];
                if (spentLevel <= 0) continue;
                var node = nodes[i];
                int effectiveLevel = (node.maxLevel > 0 && spentLevel > node.maxLevel) ? node.maxLevel : spentLevel;

                int sumForNode = 0;
                for (int lvl = 0; lvl < effectiveLevel; lvl++)
                    sumForNode += SkillTreeData.ComputeNodeCost(node, lvl);

                if (node.costType == SkillTreeData.CostType.Gold)
                    gold += sumForNode;
                else
                    skillPoint += sumForNode;
            }

            return new Refund(gold, skillPoint);
        }
    }
}
