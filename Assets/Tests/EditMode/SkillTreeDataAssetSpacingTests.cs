using NUnit.Framework;
using RogueliteAutoBattler.Data;
using UnityEditor;
using UnityEngine;

namespace RogueliteAutoBattler.Tests.EditMode
{
    public class SkillTreeDataAssetSpacingTests
    {
        private const float PositionTolerance = 0.01f;
        private const float MinimumExpectedRingRadius = 8f;

        private SkillTreeData _liveSkillTreeAsset;

        [SetUp]
        public void SetUp()
        {
            _liveSkillTreeAsset = AssetDatabase.LoadAssetAtPath<SkillTreeData>(SkillTreeData.DefaultAssetPath);
            Assert.IsNotNull(_liveSkillTreeAsset,
                $"Live SkillTreeData asset not found at {SkillTreeData.DefaultAssetPath}");
        }

        [Test]
        public void LiveAsset_RingRadius_IsAtLeastEight()
        {
            Assert.GreaterOrEqual(_liveSkillTreeAsset.RingRadius, MinimumExpectedRingRadius,
                $"Live asset ringRadius ({_liveSkillTreeAsset.RingRadius}) is below the minimum required spacing ({MinimumExpectedRingRadius}). " +
                "Do not lower this value without regenerating node positions.");
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
        public void LiveAsset_AllNodesAreOnRing()
        {
            Assert.IsTrue(_liveSkillTreeAsset.Nodes.Count > 0, "Live asset must contain at least one node");

            float expectedRadius = _liveSkillTreeAsset.RingRadius;
            for (int nodeIndex = 0; nodeIndex < _liveSkillTreeAsset.Nodes.Count; nodeIndex++)
            {
                Vector2 nodePosition = _liveSkillTreeAsset.Nodes[nodeIndex].position;
                float distanceFromOrigin = nodePosition.magnitude;
                Assert.That(Mathf.Abs(distanceFromOrigin - expectedRadius), Is.LessThan(PositionTolerance),
                    $"Node {nodeIndex} distance from origin ({distanceFromOrigin}) should equal ringRadius ({expectedRadius}). " +
                    "If ringRadius changed, regenerate node positions.");
            }
        }
    }
}
