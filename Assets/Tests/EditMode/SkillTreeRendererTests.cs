#if UNITY_EDITOR
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using RogueliteAutoBattler.Combat.Core;
using RogueliteAutoBattler.Data;
using RogueliteAutoBattler.Tests.PlayMode;
using RogueliteAutoBattler.UI.Toolkit.SkillTree;
using UnityEngine;
using UnityEngine.UIElements;

namespace RogueliteAutoBattler.Tests.EditMode
{
    [TestFixture]
    public class SkillTreeRendererTests
    {
        private const float PositionTolerance = 0.001f;

        private SkillTreeData _data;

        [TearDown]
        public void TearDown()
        {
            if (_data != null) Object.DestroyImmediate(_data);
            _data = null;
        }

        private static SkillTreeData.SkillNodeEntry MakeNode(int id, Vector2 position, params int[] childIds)
        {
            var connected = new List<int>();
            if (childIds != null) connected.AddRange(childIds);
            return new SkillTreeData.SkillNodeEntry
            {
                id = id,
                position = position,
                connectedNodeIds = connected,
                costType = SkillTreeData.CostType.Gold,
                maxLevel = 1,
                baseCost = 1,
                costMultiplierOdd = 1f,
                costMultiplierEven = 1f,
                costAdditivePerLevel = 0,
                statModifierType = StatType.Hp,
                statModifierMode = SkillTreeData.StatModifierMode.Flat,
                statModifierValuePerLevel = 1f
            };
        }

        private static List<SkillTreeNodeVisualState> BuildLockedStates(int count)
        {
            var states = new List<SkillTreeNodeVisualState>(count);
            for (int i = 0; i < count; i++)
                states.Add(SkillTreeNodeVisualState.Locked);
            return states;
        }

        [Test]
        public void RenderNodes_PositionsNodesAtUnitToPixelScale()
        {
            using var scope = new SkillTreeVisualSettingsProviderScope();

            _data = ScriptableObject.CreateInstance<SkillTreeData>();
            _data.InitializeForTest(new List<SkillTreeData.SkillNodeEntry>
            {
                MakeNode(0, Vector2.zero),
                MakeNode(1, new Vector2(2f, 1f))
            });

            var renderer = new SkillTreeRenderer(palette: null);
            var nodesLayer = new VisualElement();
            var states = BuildLockedStates(_data.Nodes.Count);

            renderer.RenderNodes(_data, states, nodesLayer);

            Assert.AreEqual(2, renderer.NodeElements.Count, "Renderer must produce two node elements.");

            float expectedLeft0 = 0f * SkillTreeRenderer.UnitToPixelScale - SkillTreeRenderer.NodeHalfSize;
            float expectedTop0 = 0f * SkillTreeRenderer.UnitToPixelScale - SkillTreeRenderer.NodeHalfSize;
            Assert.AreEqual(expectedLeft0, renderer.NodeElements[0].style.left.value.value, PositionTolerance,
                "Node 0 style.left must equal dataPos.x * UnitToPixelScale - NodeHalfSize.");
            Assert.AreEqual(expectedTop0, renderer.NodeElements[0].style.top.value.value, PositionTolerance,
                "Node 0 style.top must equal dataPos.y * UnitToPixelScale - NodeHalfSize.");

            float expectedLeft1 = 2f * SkillTreeRenderer.UnitToPixelScale - SkillTreeRenderer.NodeHalfSize;
            float expectedTop1 = 1f * SkillTreeRenderer.UnitToPixelScale - SkillTreeRenderer.NodeHalfSize;
            Assert.AreEqual(expectedLeft1, renderer.NodeElements[1].style.left.value.value, PositionTolerance,
                "Node 1 style.left must equal dataPos.x * UnitToPixelScale - NodeHalfSize.");
            Assert.AreEqual(expectedTop1, renderer.NodeElements[1].style.top.value.value, PositionTolerance,
                "Node 1 style.top must equal dataPos.y * UnitToPixelScale - NodeHalfSize.");
        }

        [Test]
        public void RenderNodes_AppendsExpectedCountToLayer()
        {
            using var scope = new SkillTreeVisualSettingsProviderScope();

            _data = ScriptableObject.CreateInstance<SkillTreeData>();
            _data.InitializeForTest(new List<SkillTreeData.SkillNodeEntry>
            {
                MakeNode(0, Vector2.zero),
                MakeNode(1, new Vector2(1f, 0f)),
                MakeNode(2, new Vector2(2f, 0f))
            });

            var renderer = new SkillTreeRenderer(palette: null);
            var nodesLayer = new VisualElement();
            var states = BuildLockedStates(_data.Nodes.Count);

            renderer.RenderNodes(_data, states, nodesLayer);

            Assert.AreEqual(3, nodesLayer.childCount, "nodesLayer must contain one child per data node.");
            Assert.AreEqual(3, renderer.NodeElements.Count, "Renderer must expose one element per data node.");
        }

        [Test]
        public void RenderEdges_FeedsExpectedEdgeCount()
        {
            using var scope = new SkillTreeVisualSettingsProviderScope();

            _data = ScriptableObject.CreateInstance<SkillTreeData>();
            _data.InitializeForTest(new List<SkillTreeData.SkillNodeEntry>
            {
                MakeNode(0, Vector2.zero, 1),
                MakeNode(1, new Vector2(1f, 0f), 2),
                MakeNode(2, new Vector2(2f, 0f))
            });

            var renderer = new SkillTreeRenderer(palette: null);
            var edgeLayer = new SkillTreeEdgeLayer();
            var states = BuildLockedStates(_data.Nodes.Count);

            var idToIndex = new Dictionary<int, int>();
            for (int i = 0; i < _data.Nodes.Count; i++)
                idToIndex[_data.Nodes[i].id] = i;

            renderer.RenderEdges(_data, idToIndex, states, edgeLayer);

            Assert.AreEqual(2, edgeLayer.EdgeCount,
                "Edge layer must contain one entry per parent->child connection (0->1 and 1->2).");
        }

        [Test]
        public void RenderNodes_RegistersClickCallback_WhenProvided()
        {
            using var scope = new SkillTreeVisualSettingsProviderScope();

            _data = ScriptableObject.CreateInstance<SkillTreeData>();
            _data.InitializeForTest(new List<SkillTreeData.SkillNodeEntry>
            {
                MakeNode(0, Vector2.zero),
                MakeNode(1, new Vector2(1f, 0f))
            });

            var renderer = new SkillTreeRenderer(palette: null);
            var nodesLayer = new VisualElement();
            var states = BuildLockedStates(_data.Nodes.Count);

            int observedNodeIndex = -1;
            int callCount = 0;
            System.Action<int> callback = idx =>
            {
                observedNodeIndex = idx;
                callCount++;
            };

            renderer.RenderNodes(_data, states, nodesLayer, callback);

            var clickedField = typeof(SkillTreeNodeElement).GetField("Clicked",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(clickedField,
                "SkillTreeNodeElement must expose a 'Clicked' event field via reflection.");

            var subscribedDelegate = (System.Delegate)clickedField.GetValue(renderer.NodeElements[1]);
            Assert.IsNotNull(subscribedDelegate,
                "Renderer must subscribe the supplied callback to the Clicked event when one is provided.");

            subscribedDelegate.DynamicInvoke(1);

            Assert.AreEqual(1, callCount, "Callback must be invoked exactly once when the node's Clicked event fires.");
            Assert.AreEqual(1, observedNodeIndex, "Callback must receive the clicked node's index.");
        }
    }
}
#endif
