using System.Collections.Generic;
using NUnit.Framework;
using RogueliteAutoBattler.Data;
using UnityEngine;

namespace RogueliteAutoBattler.Tests.EditMode
{
    public class SkillTreeDesignerColorTagTests
    {
        private SkillTreeData _data;

        [SetUp]
        public void SetUp()
        {
            _data = ScriptableObject.CreateInstance<SkillTreeData>();
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(_data);
        }

        private static SkillTreeData.SkillNodeEntry MakeEntryWithDefaultColor(int id)
        {
            return new SkillTreeData.SkillNodeEntry
            {
                id = id,
                connectedNodeIds = new List<int>(),
                colorTag = NodeColorTag.Default
            };
        }

        [Test]
        public void SetNode_PersistsColorTagOnEntry()
        {
            _data.InitializeForTest(new List<SkillTreeData.SkillNodeEntry>
            {
                MakeEntryWithDefaultColor(0),
                MakeEntryWithDefaultColor(1)
            });

            var entry = _data.Nodes[1];
            entry.colorTag = NodeColorTag.Blue;

            _data.SetNode(1, entry);

            Assert.AreEqual(NodeColorTag.Blue, _data.Nodes[1].colorTag);
            Assert.AreEqual(NodeColorTag.Default, _data.Nodes[0].colorTag);
        }
    }
}
