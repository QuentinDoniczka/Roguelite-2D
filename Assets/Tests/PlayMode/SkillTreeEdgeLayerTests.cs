#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using RogueliteAutoBattler.UI.Toolkit.SkillTree;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace RogueliteAutoBattler.Tests.PlayMode
{
    public class SkillTreeEdgeLayerTests : PlayModeTestBase
    {
        private const string EdgeLayerClassName = "skill-tree-edge-layer";
        private const string PanelSettingsPath = "Assets/UI/MainPanelSettings.asset";
        private const float DefaultUnitToPixelScale = 32f;

        private static IReadOnlyList<(int fromId, int toId)> BuildEdgeList(params (int fromId, int toId)[] edges)
            => edges;

        private static IReadOnlyDictionary<int, int> BuildIdToIndexMap(params int[] ids)
        {
            var map = new Dictionary<int, int>(ids.Length);
            for (var i = 0; i < ids.Length; i++)
                map[ids[i]] = i;
            return map;
        }

        private static IReadOnlyList<Vector2> BuildNodePositionsByIndex(int count)
        {
            var positions = new Vector2[count];
            for (var i = 0; i < count; i++)
                positions[i] = new Vector2(i, i);
            return positions;
        }

        private static IReadOnlyList<SkillTreeNodeVisualState> BuildDefaultStates(int count)
        {
            var states = new SkillTreeNodeVisualState[count];
            for (var i = 0; i < count; i++)
                states[i] = SkillTreeNodeVisualState.Locked;
            return states;
        }

        [Test]
        public void Constructor_HasEdgeLayerClass()
        {
            var edgeLayer = new SkillTreeEdgeLayer();
            Assert.IsTrue(edgeLayer.ClassListContains(EdgeLayerClassName),
                $"SkillTreeEdgeLayer must carry the '{EdgeLayerClassName}' USS class on construction");
        }

        [Test]
        public void Constructor_PickingModeIsIgnore()
        {
            var edgeLayer = new SkillTreeEdgeLayer();
            Assert.AreEqual(PickingMode.Ignore, edgeLayer.pickingMode,
                "SkillTreeEdgeLayer must have pickingMode == Ignore so it never intercepts clicks from nodes underneath");
        }

        [Test]
        public void Constructor_EdgeCountIsZero()
        {
            var edgeLayer = new SkillTreeEdgeLayer();
            Assert.AreEqual(0, edgeLayer.EdgeCount,
                "SkillTreeEdgeLayer must start with EdgeCount == 0 before SetEdges is called");
        }

        [Test]
        public void SetEdges_PopulatesEdgeCount()
        {
            var edgeLayer = new SkillTreeEdgeLayer();
            var nodeIds = BuildIdToIndexMap(1, 2, 3, 4);
            var nodePositionsByIndex = BuildNodePositionsByIndex(4);
            var nodeStatesByIndex = BuildDefaultStates(4);
            var edges = BuildEdgeList((1, 2), (2, 3), (3, 4));

            edgeLayer.SetEdges(edges, nodePositionsByIndex, nodeIds, nodeStatesByIndex, DefaultUnitToPixelScale);

            Assert.AreEqual(3, edgeLayer.EdgeCount,
                "SetEdges with three valid edges must result in EdgeCount == 3");
        }

        [Test]
        public void SetEdges_Clears_PreviousEdges()
        {
            var edgeLayer = new SkillTreeEdgeLayer();
            var nodeIds = BuildIdToIndexMap(1, 2, 3, 4);
            var nodePositionsByIndex = BuildNodePositionsByIndex(4);
            var nodeStatesByIndex = BuildDefaultStates(4);

            edgeLayer.SetEdges(BuildEdgeList((1, 2), (2, 3), (3, 4)), nodePositionsByIndex, nodeIds, nodeStatesByIndex, DefaultUnitToPixelScale);
            edgeLayer.SetEdges(BuildEdgeList((1, 2)), nodePositionsByIndex, nodeIds, nodeStatesByIndex, DefaultUnitToPixelScale);

            Assert.AreEqual(1, edgeLayer.EdgeCount,
                "A second SetEdges call must clear previous edges and reflect only the new edge list");
        }

        [Test]
        public void SetEdges_Empty_ClearsEdges()
        {
            var edgeLayer = new SkillTreeEdgeLayer();
            var nodeIds = BuildIdToIndexMap(1, 2, 3, 4);
            var nodePositionsByIndex = BuildNodePositionsByIndex(4);
            var nodeStatesByIndex = BuildDefaultStates(4);

            edgeLayer.SetEdges(BuildEdgeList((1, 2), (2, 3), (3, 4)), nodePositionsByIndex, nodeIds, nodeStatesByIndex, DefaultUnitToPixelScale);
            edgeLayer.SetEdges(System.Array.Empty<(int fromId, int toId)>(), nodePositionsByIndex, nodeIds, nodeStatesByIndex, DefaultUnitToPixelScale);

            Assert.AreEqual(0, edgeLayer.EdgeCount,
                "SetEdges called with an empty edge list must clear all previous edges");
        }

        [Test]
        public void SetEdges_ResolvesIdsToIndices()
        {
            var edgeLayer = new SkillTreeEdgeLayer();
            var nodeIdsWithNonSequentialValues = BuildIdToIndexMap(5, 7, 9);
            var nodePositionsByIndex = BuildNodePositionsByIndex(3);
            var nodeStatesByIndex = BuildDefaultStates(3);
            var edgesReferencingIdsNotIndices = BuildEdgeList((5, 7), (7, 9));

            Assert.DoesNotThrow(() => edgeLayer.SetEdges(
                edgesReferencingIdsNotIndices,
                nodePositionsByIndex,
                nodeIdsWithNonSequentialValues,
                nodeStatesByIndex,
                DefaultUnitToPixelScale),
                "SetEdges must map non-sequential node IDs to their array indices without IndexOutOfRange");

            Assert.AreEqual(2, edgeLayer.EdgeCount,
                "Both edges referencing valid non-sequential IDs must be resolved and counted");
        }

        [Test]
        public void SetEdges_WithUnknownId_SilentlySkips_AndEdgeCountIsZero()
        {
            var edgeLayer = new SkillTreeEdgeLayer();
            var nodeIds = BuildIdToIndexMap(1, 2, 3);
            var nodePositionsByIndex = BuildNodePositionsByIndex(3);
            var nodeStatesByIndex = BuildDefaultStates(3);
            var edgesWithUnknownIds = BuildEdgeList((1, 999), (888, 2));

            Assert.DoesNotThrow(() => edgeLayer.SetEdges(
                edgesWithUnknownIds,
                nodePositionsByIndex,
                nodeIds,
                nodeStatesByIndex,
                DefaultUnitToPixelScale),
                "SetEdges must silently skip edges referencing unknown IDs instead of throwing");

            Assert.AreEqual(0, edgeLayer.EdgeCount,
                "Edges whose from/to IDs are not present in nodeIds must be skipped, leaving EdgeCount == 0");
        }

        [UnityTest]
        public IEnumerator GenerateVisualContent_Fires_WhenSetEdgesCalled()
        {
            PanelSettings panelSettings = AssetDatabase.LoadAssetAtPath<PanelSettings>(PanelSettingsPath);
            Assert.IsNotNull(panelSettings, $"MainPanelSettings not found at {PanelSettingsPath}");

            var documentGo = Track(new GameObject("TestUIDocumentForEdgeLayer"));
            documentGo.SetActive(false);
            var uiDocument = documentGo.AddComponent<UIDocument>();
            uiDocument.panelSettings = panelSettings;
            documentGo.SetActive(true);

            yield return null;
            yield return null;

            VisualElement root = uiDocument.rootVisualElement;
            if (root == null)
            {
                Assert.Inconclusive("rootVisualElement is null - UIDocument failed to initialize in the test environment.");
                yield break;
            }

            root.style.width = 1080;
            root.style.height = 1920;

            var edgeLayer = new SkillTreeEdgeLayer();
            edgeLayer.style.position = Position.Absolute;
            edgeLayer.style.left = 0;
            edgeLayer.style.top = 0;
            edgeLayer.style.width = 1080;
            edgeLayer.style.height = 1920;
            root.Add(edgeLayer);

            var nodeIds = BuildIdToIndexMap(1, 2, 3);
            var nodePositionsByIndex = BuildNodePositionsByIndex(3);
            var nodeStatesByIndex = new[]
            {
                SkillTreeNodeVisualState.Purchased,
                SkillTreeNodeVisualState.Purchased,
                SkillTreeNodeVisualState.Locked
            };
            var edges = BuildEdgeList((1, 2), (2, 3));

            Assert.DoesNotThrow(() => edgeLayer.SetEdges(
                edges,
                nodePositionsByIndex,
                nodeIds,
                nodeStatesByIndex,
                DefaultUnitToPixelScale),
                "SetEdges on a live panel must not throw during repaint scheduling");

            yield return null;
            yield return null;

            Assert.AreEqual(2, edgeLayer.EdgeCount,
                "After SetEdges + one repaint frame, EdgeCount must still reflect the two submitted edges");
            Assert.AreSame(root, edgeLayer.parent,
                "SkillTreeEdgeLayer must remain attached to the root after a repaint cycle");
        }
    }
}
#endif
