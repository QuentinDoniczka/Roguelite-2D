using System.Collections.Generic;
using RogueliteAutoBattler.Data;

namespace RogueliteAutoBattler.UI.Toolkit.SkillTree
{
    public sealed class SkillTreeStateEvaluator
    {
        private const int NotFoundIndex = -1;

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
            return HasUnlockedPrerequisite(nodeIndex)
                ? SkillTreeNodeVisualState.Available
                : SkillTreeNodeVisualState.Locked;
        }

        private bool HasUnlockedPrerequisite(int nodeIndex)
        {
            var node = _data.Nodes[nodeIndex];
            for (var i = 0; i < node.connectedNodeIds.Count; i++)
            {
                var connectedId = node.connectedNodeIds[i];
                var connectedIndex = FindIndexById(connectedId);
                if (connectedIndex >= 0 && _progress.GetLevel(connectedIndex) > 0)
                {
                    return true;
                }
            }
            return node.connectedNodeIds.Count == 0;
        }

        private int FindIndexById(int nodeId)
        {
            return _idToIndexMap.TryGetValue(nodeId, out var index) ? index : NotFoundIndex;
        }
    }
}
