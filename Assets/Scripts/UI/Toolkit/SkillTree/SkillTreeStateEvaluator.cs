using System.Collections.Generic;
using RogueliteAutoBattler.Data;

namespace RogueliteAutoBattler.UI.Toolkit.SkillTree
{
    public sealed class SkillTreeStateEvaluator
    {
        private readonly SkillTreeData _data;
        private readonly SkillTreeProgress _progress;
        private readonly Dictionary<int, int> _idToIndexMap;

        public SkillTreeStateEvaluator(SkillTreeData data, SkillTreeProgress progress)
        {
            _data = data;
            _progress = progress;
            _idToIndexMap = new Dictionary<int, int>(data.Nodes.Count);
            for (var i = 0; i < data.Nodes.Count; i++)
            {
                _idToIndexMap[data.Nodes[i].id] = i;
            }
        }

        public IReadOnlyDictionary<int, int> IdToIndexMap => _idToIndexMap;

        public SkillTreeNodeVisualState GetState(int nodeIndex)
        {
            var currentLevel = _progress.GetLevel(nodeIndex);
            var node = _data.Nodes[nodeIndex];
            if (SkillTreeData.IsMaxLevel(node, currentLevel))
            {
                return SkillTreeNodeVisualState.Max;
            }
            if (currentLevel > 0)
            {
                return SkillTreeNodeVisualState.Purchased;
            }
            return SkillTreeNodeVisualState.Available;
        }
    }
}
