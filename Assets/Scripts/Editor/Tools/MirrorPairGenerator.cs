using System.Collections.Generic;
using System.Globalization;
using RogueliteAutoBattler.Data;
using UnityEngine;

namespace RogueliteAutoBattler.Editor.Tools
{
    internal static class MirrorPairGenerator
    {
        internal const string UndoLabelSingleNode = "Create Branch Node";
        internal const string UndoLabelMirroredPair = "Create Branch Node Pair (Mirrored)";
        internal const string MirrorSkippedWarningFormat = "Mirror skipped: a node already exists within {0} units of the reflected position.";

        private const string ErrorDataNull = "Skill tree data is null.";
        private const string ErrorInvalidParentIndex = "Invalid parent index.";

        internal readonly struct MirrorPairResult
        {
            public readonly bool OriginalCreated;
            public readonly bool MirrorCreated;
            public readonly string WarningMessage;
            public readonly int OriginalNewId;
            public readonly int MirrorNewId;

            private MirrorPairResult(bool original, bool mirror, string warning, int originalId, int mirrorId)
            {
                OriginalCreated = original;
                MirrorCreated = mirror;
                WarningMessage = warning;
                OriginalNewId = originalId;
                MirrorNewId = mirrorId;
            }

            internal static MirrorPairResult Invalid(string errorReason) => new MirrorPairResult(false, false, errorReason, -1, -1);
            internal static MirrorPairResult OriginalOnly(int originalId) => new MirrorPairResult(true, false, null, originalId, -1);
            internal static MirrorPairResult OriginalOnlyWithWarning(int originalId, string warning) => new MirrorPairResult(true, false, warning, originalId, -1);
            internal static MirrorPairResult Pair(int originalNodeId, int mirrorNodeId) => new MirrorPairResult(true, true, null, originalNodeId, mirrorNodeId);
        }

        public static MirrorPairResult TryGenerate(
            SkillTreeData data,
            int parentIndex,
            float distance,
            float resolvedAngleDegrees,
            bool mirrorEnabled,
            float mirrorBranchAngleDegrees)
        {
            if (data == null)
                return MirrorPairResult.Invalid(ErrorDataNull);

            if (parentIndex < 0 || parentIndex >= data.Nodes.Count)
                return MirrorPairResult.Invalid(ErrorInvalidParentIndex);

            var parentEntry = data.Nodes[parentIndex];
            Vector2 parentPosition = parentEntry.position;
            int parentId = parentEntry.id;

            Vector2 originalPos = BranchPlacement.ComputeBranchPosition(parentPosition, distance, resolvedAngleDegrees);
            int originalNewId = SkillTreeNodeIdAllocator.ComputeNextNodeId(data.Nodes);
            var originalEntry = SkillTreeNodeFactory.CreateBranchNode(originalNewId, originalPos);
            data.AddBranchNode(originalEntry, parentId);

            if (!mirrorEnabled)
                return MirrorPairResult.OriginalOnly(originalNewId);

            Vector2 mirrorPos = BranchPlacement.ComputeBranchPosition(parentPosition, distance, mirrorBranchAngleDegrees);

            if (HasCollisionAt(data.Nodes, mirrorPos, BranchPlacement.PositionTolerance))
            {
                string warning = string.Format(CultureInfo.InvariantCulture, MirrorSkippedWarningFormat, BranchPlacement.PositionTolerance);
                return MirrorPairResult.OriginalOnlyWithWarning(originalNewId, warning);
            }

            int mirrorNewId = SkillTreeNodeIdAllocator.ComputeNextNodeId(data.Nodes);
            var mirrorEntry = SkillTreeNodeFactory.CreateBranchNode(mirrorNewId, mirrorPos);
            data.AddBranchNode(mirrorEntry, parentId);

            return MirrorPairResult.Pair(originalNewId, mirrorNewId);
        }

        internal static bool HasCollisionAt(IReadOnlyList<SkillTreeData.SkillNodeEntry> nodes, Vector2 candidatePos, float tolerance)
        {
            if (nodes == null) return false;
            for (int i = 0; i < nodes.Count; i++)
            {
                if (Vector2.Distance(nodes[i].position, candidatePos) < tolerance)
                    return true;
            }
            return false;
        }
    }
}
