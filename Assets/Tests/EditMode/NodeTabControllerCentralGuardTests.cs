using System.Collections.Generic;
using NUnit.Framework;
using RogueliteAutoBattler.Combat.Core;
using RogueliteAutoBattler.Data;
using RogueliteAutoBattler.Editor.Windows.SkillTreeDesigner;
using UnityEngine;
using UnityEngine.UIElements;

namespace RogueliteAutoBattler.Tests.EditMode
{
    public class NodeTabControllerCentralGuardTests
    {
        private SkillTreeData _data;
        private SkillTreeCanvasElement _canvas;
        private int? _selectedId;

        [SetUp]
        public void SetUp()
        {
            _data = ScriptableObject.CreateInstance<SkillTreeData>();
            _canvas = new SkillTreeCanvasElement();
            _canvas.SetData(_data, null);
            _selectedId = null;
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_data);
        }

        private NodeTabController MakeController()
        {
            var tabContent = new VisualElement();
            return new NodeTabController(
                tabContent,
                _data,
                _canvas,
                () => _selectedId,
                id => _selectedId = id);
        }

        private static SkillTreeData.SkillNodeEntry MakeNode(int id, Vector2 position)
        {
            return new SkillTreeData.SkillNodeEntry
            {
                id = id,
                position = position,
                connectedNodeIds = new List<int>(),
                costType = SkillTreeData.CostType.Gold,
                maxLevel = 1,
                baseCost = 10,
                costMultiplierOdd = 1f,
                costMultiplierEven = 1f,
                costAdditivePerLevel = 0,
                statModifierType = StatType.Hp,
                statModifierMode = SkillTreeData.StatModifierMode.Flat,
                statModifierValuePerLevel = 1f
            };
        }

        [Test]
        public void DeleteButton_Disabled_WhenSelectedIsCentral()
        {
            int centralId = SkillTreeData.CentralNodeId;
            _data.InitializeForTest(new List<SkillTreeData.SkillNodeEntry>
            {
                MakeNode(centralId, Vector2.zero)
            });
            _selectedId = centralId;
            NodeTabController nodeTab = MakeController();

            nodeTab.OnSelectionChanged(centralId);

            Assert.IsFalse(nodeTab.IsDeleteEnabled);
        }

        [Test]
        public void DeleteButton_Disabled_WhenNoSelection()
        {
            _data.InitializeForTest(new List<SkillTreeData.SkillNodeEntry>());
            _selectedId = null;
            NodeTabController nodeTab = MakeController();

            nodeTab.OnSelectionChanged(null);

            Assert.IsFalse(nodeTab.IsDeleteEnabled);
        }

        [Test]
        public void DeleteButton_Enabled_WhenSelectedIsNonCentral()
        {
            int centralId = SkillTreeData.CentralNodeId;
            int childId = 1;
            _data.InitializeForTest(new List<SkillTreeData.SkillNodeEntry>
            {
                MakeNode(centralId, Vector2.zero),
                MakeNode(childId, new Vector2(1f, 0f))
            });
            _selectedId = childId;
            NodeTabController nodeTab = MakeController();

            nodeTab.OnSelectionChanged(childId);

            Assert.IsTrue(nodeTab.IsDeleteEnabled);
        }

        [Test]
        public void Delete_RemovesNonCentralNode_FromData()
        {
            int centralId = SkillTreeData.CentralNodeId;
            int childId = 1;
            _data.InitializeForTest(new List<SkillTreeData.SkillNodeEntry>
            {
                MakeNode(centralId, Vector2.zero),
                MakeNode(childId, new Vector2(1f, 0f))
            });
            _selectedId = childId;
            NodeTabController nodeTab = MakeController();
            nodeTab.OnSelectionChanged(childId);

            nodeTab.Delete();

            bool childStillExists = false;
            foreach (var node in _data.Nodes)
            {
                if (node.id == childId)
                {
                    childStillExists = true;
                    break;
                }
            }
            Assert.IsFalse(childStillExists, "Node with id 1 should have been removed");
        }

        [Test]
        public void Delete_OnCentral_NoOps_AndDoesNotRemoveCentral()
        {
            int centralId = SkillTreeData.CentralNodeId;
            _data.InitializeForTest(new List<SkillTreeData.SkillNodeEntry>
            {
                MakeNode(centralId, Vector2.zero)
            });
            _selectedId = centralId;
            NodeTabController nodeTab = MakeController();
            nodeTab.OnSelectionChanged(centralId);

            nodeTab.Delete();

            bool centralExists = false;
            foreach (var node in _data.Nodes)
            {
                if (node.id == centralId)
                {
                    centralExists = true;
                    break;
                }
            }
            Assert.IsTrue(centralExists, "Central node must not be removed");
        }
    }
}
