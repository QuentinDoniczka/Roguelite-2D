using System.Collections.Generic;
using RogueliteAutoBattler.Data;

namespace RogueliteAutoBattler.UI.Toolkit.SkillTree
{
    public sealed class SkillTreeStateEvaluator
    {
        private readonly SkillTreeData _data;
        private readonly SkillTreeProgress _progress;
        private readonly Dictionary<int, int> _idToIndexMap;
        private readonly Dictionary<int, List<int>> _parentsByIndex;

        public SkillTreeStateEvaluator(SkillTreeData data, SkillTreeProgress progress)
        {
            _data = data;
            _progress = progress;
            _idToIndexMap = new Dictionary<int, int>(data.Nodes.Count);
            for (var i = 0; i < data.Nodes.Count; i++)
            {
                _idToIndexMap[data.Nodes[i].id] = i;
            }

            _parentsByIndex = new Dictionary<int, List<int>>(data.Nodes.Count);
            for (var parentIndex = 0; parentIndex < data.Nodes.Count; parentIndex++)
            {
                var connectedIds = data.Nodes[parentIndex].connectedNodeIds;
                if (connectedIds == null) continue;
                foreach (var childId in connectedIds)
                {
                    if (!_idToIndexMap.TryGetValue(childId, out var childIndex)) continue;
                    if (!_parentsByIndex.TryGetValue(childIndex, out var parentList))
                    {
                        parentList = new List<int>();
                        _parentsByIndex[childIndex] = parentList;
                    }
                    parentList.Add(parentIndex);
                }
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
            if (IsLockedByParents(nodeIndex))
            {
                return SkillTreeNodeVisualState.Locked;
            }
            return SkillTreeNodeVisualState.Available;
        }

        private bool IsLockedByParents(int nodeIndex)
        {
            if (_data.Nodes[nodeIndex].id == SkillTreeData.CentralNodeId) return false;
            if (!_parentsByIndex.TryGetValue(nodeIndex, out var parents)) return true;
            if (parents.Count == 0) return true;
            foreach (var parentIndex in parents)
            {
                if (_progress.GetLevel(parentIndex) > 0)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
