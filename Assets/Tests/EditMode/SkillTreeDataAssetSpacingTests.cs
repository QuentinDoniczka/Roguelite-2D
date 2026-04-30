using NUnit.Framework;
using RogueliteAutoBattler.Data;
using RogueliteAutoBattler.Editor;
using UnityEditor;
using UnityEngine;

namespace RogueliteAutoBattler.Tests.EditMode
{
    public class SkillTreeDataAssetSpacingTests
    {
        private const float PositionTolerance = 0.01f;

        private SkillTreeData _liveSkillTreeAsset;

        [SetUp]
        public void SetUp()
        {
            _liveSkillTreeAsset = AssetDatabase.LoadAssetAtPath<SkillTreeData>(EditorPaths.SkillTreeDataAsset);
            Assert.IsNotNull(_liveSkillTreeAsset,
                $"Live SkillTreeData asset not found at {EditorPaths.SkillTreeDataAsset}");
        }

        [Test]
        public void LiveAsset_FirstNode_PositionMatchesRingRadius()
        {
            Assert.IsTrue(_liveSkillTreeAsset.Nodes.Count > 0, "Live asset must contain at least one node");

            Vector2 firstNodePosition = _liveSkillTreeAsset.Nodes[0].position;
            float expectedRadius = _liveSkillTreeAsset.RingRadius;

            Assert.That(Mathf.Abs(firstNodePosition.x - expectedRadius), Is.LessThan(PositionTolerance),
                $"First node x ({firstNodePosition.x}) should equal ringRadius ({expectedRadius}). " +
                "If ringRadius changed, regenerate node positions.");
            Assert.That(Mathf.Abs(firstNodePosition.y), Is.LessThan(PositionTolerance),
                $"First node y ({firstNodePosition.y}) should be 0.");
        }

        [Test]
        public void LiveAsset_HubNodesAreOnRing()
        {
            int hubCount = _liveSkillTreeAsset.RingNodeCount;
            Assert.GreaterOrEqual(_liveSkillTreeAsset.Nodes.Count, hubCount,
                "Live asset must contain at least RingNodeCount hub nodes");

            float expectedRadius = _liveSkillTreeAsset.RingRadius;
            for (int nodeIndex = 0; nodeIndex < hubCount; nodeIndex++)
            {
                Vector2 nodePosition = _liveSkillTreeAsset.Nodes[nodeIndex].position;
                float distanceFromOrigin = nodePosition.magnitude;
                Assert.That(Mathf.Abs(distanceFromOrigin - expectedRadius), Is.LessThan(PositionTolerance),
                    $"Hub node {nodeIndex} distance from origin ({distanceFromOrigin}) should equal ringRadius ({expectedRadius}). " +
                    "If ringRadius changed, regenerate hub positions.");
            }
        }
    }
}
