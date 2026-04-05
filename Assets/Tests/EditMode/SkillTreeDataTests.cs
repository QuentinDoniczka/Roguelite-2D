using NUnit.Framework;
using RogueliteAutoBattler.Data;
using UnityEngine;

namespace RogueliteAutoBattler.Tests.EditMode
{
    public class SkillTreeDataTests
    {
        private SkillTreeData _skillTreeData;

        [SetUp]
        public void SetUp()
        {
            _skillTreeData = ScriptableObject.CreateInstance<SkillTreeData>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_skillTreeData);
        }

        [Test]
        public void GenerateNodes_CenterNodeAtZero()
        {
            _skillTreeData.RingNodeCount = 8;
            _skillTreeData.RingRadius = 5f;

            _skillTreeData.GenerateNodes();

            Assert.AreEqual(0, _skillTreeData.Nodes[0].id);
            Assert.AreEqual(Vector2.zero, _skillTreeData.Nodes[0].position);
        }

        [Test]
        public void GenerateNodes_TotalNodesEqualsRingNodeCountPlusOne()
        {
            _skillTreeData.RingNodeCount = 6;

            _skillTreeData.GenerateNodes();

            Assert.AreEqual(7, _skillTreeData.Nodes.Count);
        }

        [Test]
        public void GenerateNodes_RingNodesAtCorrectDistance()
        {
            _skillTreeData.RingNodeCount = 8;
            _skillTreeData.RingRadius = 3f;

            _skillTreeData.GenerateNodes();

            for (int i = 1; i <= 8; i++)
            {
                float distance = Vector2.Distance(_skillTreeData.Nodes[i].position, Vector2.zero);
                Assert.That(distance, Is.EqualTo(3f).Within(0.001f),
                    $"Node {i} distance from origin should be 3f");
            }
        }

        [Test]
        public void GenerateNodes_RingNodesEvenlySpaced()
        {
            _skillTreeData.RingNodeCount = 4;
            _skillTreeData.RingRadius = 5f;

            _skillTreeData.GenerateNodes();

            Assert.That(_skillTreeData.Nodes[1].position.x, Is.EqualTo(5f).Within(0.01f));
            Assert.That(_skillTreeData.Nodes[1].position.y, Is.EqualTo(0f).Within(0.01f));

            Assert.That(_skillTreeData.Nodes[2].position.x, Is.EqualTo(0f).Within(0.01f));
            Assert.That(_skillTreeData.Nodes[2].position.y, Is.EqualTo(5f).Within(0.01f));

            Assert.That(_skillTreeData.Nodes[3].position.x, Is.EqualTo(-5f).Within(0.01f));
            Assert.That(_skillTreeData.Nodes[3].position.y, Is.EqualTo(0f).Within(0.01f));

            Assert.That(_skillTreeData.Nodes[4].position.x, Is.EqualTo(0f).Within(0.01f));
            Assert.That(_skillTreeData.Nodes[4].position.y, Is.EqualTo(-5f).Within(0.01f));
        }

        [Test]
        public void GenerateNodes_IsDeterministic()
        {
            _skillTreeData.RingNodeCount = 8;
            _skillTreeData.RingRadius = 5f;

            _skillTreeData.GenerateNodes();
            var firstRunPositions = new Vector2[_skillTreeData.Nodes.Count];
            for (int i = 0; i < _skillTreeData.Nodes.Count; i++)
            {
                firstRunPositions[i] = _skillTreeData.Nodes[i].position;
            }

            _skillTreeData.GenerateNodes();

            for (int i = 0; i < _skillTreeData.Nodes.Count; i++)
            {
                Assert.AreEqual(firstRunPositions[i], _skillTreeData.Nodes[i].position,
                    $"Node {i} position should be identical between runs");
            }
        }

        [Test]
        public void GenerateNodes_ClearsExistingNodes()
        {
            _skillTreeData.RingNodeCount = 5;
            _skillTreeData.GenerateNodes();

            _skillTreeData.RingNodeCount = 3;
            _skillTreeData.GenerateNodes();

            Assert.AreEqual(4, _skillTreeData.Nodes.Count);
        }

        [Test]
        public void GenerateNodes_RingNodeIdsAreSequential()
        {
            _skillTreeData.RingNodeCount = 5;

            _skillTreeData.GenerateNodes();

            for (int i = 0; i < _skillTreeData.Nodes.Count; i++)
            {
                Assert.AreEqual(i, _skillTreeData.Nodes[i].id,
                    $"Node at index {i} should have id {i}");
            }
        }

        [Test]
        public void GenerateNodes_MinRingNodeCount_ProducesValidRing()
        {
            _skillTreeData.RingNodeCount = 3;
            _skillTreeData.RingRadius = 4f;

            _skillTreeData.GenerateNodes();

            Assert.AreEqual(4, _skillTreeData.Nodes.Count);

            for (int i = 1; i <= 3; i++)
            {
                float distance = Vector2.Distance(_skillTreeData.Nodes[i].position, Vector2.zero);
                Assert.That(distance, Is.EqualTo(4f).Within(0.001f),
                    $"Ring node {i} should be at distance 4f from origin");
            }
        }
    }
}
