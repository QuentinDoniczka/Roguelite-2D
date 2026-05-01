#if UNITY_EDITOR
using System.Collections.Generic;
using NUnit.Framework;
using RogueliteAutoBattler.Data;
using RogueliteAutoBattler.Editor.Windows.SkillTreeDesigner;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RogueliteAutoBattler.Tests.EditMode
{
    [TestFixture]
    public class SkillTreeCanvasElementTests
    {
        private const float UnitToPx = 64f;
        private const float NodeRadiusPx = 18f;
        private const int NodeAId = 1;
        private const int NodeBId = 2;

        private SkillTreeData CreateDataWithNodes(List<SkillTreeData.SkillNodeEntry> testNodes)
        {
            var data = ScriptableObject.CreateInstance<SkillTreeData>();
            data.InitializeForTest(testNodes);
            return data;
        }

        private SkillTreeCanvasElement CreateCanvasWithData(SkillTreeData data, int? selectedId = null)
        {
            var canvas = new SkillTreeCanvasElement();
            canvas.SetData(data, selectedId);
            return canvas;
        }

        [Test]
        public void HitTest_ReturnsNodeId_WhenInsideNodeCircle()
        {
            var nodes = new List<SkillTreeData.SkillNodeEntry>
            {
                new SkillTreeData.SkillNodeEntry { id = NodeAId, position = Vector2.zero }
            };
            var data = CreateDataWithNodes(nodes);
            var canvas = CreateCanvasWithData(data);

            var screenPos = canvas.DataToScreen(Vector2.zero);
            var result = canvas.HitTest(screenPos);

            Assert.AreEqual(NodeAId, result);
        }

        [Test]
        public void HitTest_ReturnsNull_WhenOutsideAnyNode()
        {
            var nodes = new List<SkillTreeData.SkillNodeEntry>
            {
                new SkillTreeData.SkillNodeEntry { id = NodeAId, position = Vector2.zero }
            };
            var data = CreateDataWithNodes(nodes);
            var canvas = CreateCanvasWithData(data);

            var farPoint = new Vector2(9999f, 9999f);
            var result = canvas.HitTest(farPoint);

            Assert.IsNull(result);
        }

        [Test]
        public void Zoom_ClampedToRange()
        {
            var canvas = new SkillTreeCanvasElement();

            canvas.SetZoomForTest(10f);
            Assert.AreEqual(4f, canvas.Zoom, 0.0001f);

            canvas.SetZoomForTest(0.05f);
            Assert.AreEqual(0.25f, canvas.Zoom, 0.0001f);
        }

        [Test]
        public void NodeClicked_FiresWithCorrectId_OnSimulateClickInsideNode()
        {
            var nodes = new List<SkillTreeData.SkillNodeEntry>
            {
                new SkillTreeData.SkillNodeEntry { id = NodeAId, position = Vector2.zero }
            };
            var data = CreateDataWithNodes(nodes);
            var canvas = CreateCanvasWithData(data);

            int? receivedId = null;
            canvas.NodeClicked += id => receivedId = id;

            var screenPos = canvas.DataToScreen(Vector2.zero);
            canvas.SimulateClickAt(screenPos);

            Assert.AreEqual(NodeAId, receivedId);
        }

        [Test]
        public void Window_UpdatesSelectedNodeId_OnNodeClicked()
        {
            var nodes = new List<SkillTreeData.SkillNodeEntry>
            {
                new SkillTreeData.SkillNodeEntry { id = NodeAId, position = Vector2.zero }
            };
            var data = CreateDataWithNodes(nodes);

            var window = ScriptableObject.CreateInstance<SkillTreeDesignerWindow>();
            var testRoot = new VisualElement();

            string[] guids = AssetDatabase.FindAssets("t:SkillTreeData");
            Assert.Greater(guids.Length, 0, "A SkillTreeData asset must exist in the project for this test.");

            window.BuildUI(testRoot);

            var canvas = window.Canvas;
            Assert.IsNotNull(canvas, "Canvas must be wired after BuildUI.");

            canvas.SetData(data, null);

            var screenPos = canvas.DataToScreen(Vector2.zero);
            canvas.SimulateClickAt(screenPos);

            Assert.AreEqual(NodeAId, window.SelectedNodeId);
        }

        [Test]
        public void HitTest_HandlesUnlaidOutCanvas_WithNonZeroDataPosition()
        {
            // The previous bug was masked when dataPos == Vector2.zero because NaN + 0 is still NaN
            // and DataToScreen-of-zero produced a NaN that round-tripped to "no hit". A non-zero dataPos
            // forces the NaN path to be exercised distinctly from the all-zero path, exposing that
            // contentRect.center == NaN on an unlaid-out canvas was poisoning the distance check.
            // Key: do NOT attach the canvas to a panel. We want to assert HitTest works without a layout pass.
            const int nodeId = 3;
            var nodes = new List<SkillTreeData.SkillNodeEntry>
            {
                new SkillTreeData.SkillNodeEntry { id = nodeId, position = new Vector2(2f, 3f) }
            };
            var data = CreateDataWithNodes(nodes);
            var canvas = CreateCanvasWithData(data);

            var clickPx = canvas.DataToScreen(new Vector2(2f, 3f));
            var hit = canvas.HitTest(clickPx);

            Assert.AreEqual(nodeId, hit);
        }

        [Test]
        public void Canvas_SetData_IncrementsMarkDirtyCount()
        {
            var nodes = new List<SkillTreeData.SkillNodeEntry>
            {
                new SkillTreeData.SkillNodeEntry { id = NodeAId, position = Vector2.zero }
            };
            var data = CreateDataWithNodes(nodes);
            var canvas = new SkillTreeCanvasElement();

            int before = canvas.MarkDirtyRepaintCount;
            canvas.SetData(data, null);
            int after = canvas.MarkDirtyRepaintCount;

            Assert.Greater(after, before);
        }

        [Test]
        public void NodeClicked_FiresWithoutPanel_OnSimulateClick()
        {
            // WHY: pins the panel-independent contract. The previous EventBase-based dispatch
            // silently no-oped when the canvas had no panel — this test would have caught that.
            const int nodeId = 1;
            var nodes = new List<SkillTreeData.SkillNodeEntry>
            {
                new SkillTreeData.SkillNodeEntry { id = nodeId, position = new Vector2(1f, 1f) }
            };
            var data = CreateDataWithNodes(nodes);
            var canvas = new SkillTreeCanvasElement();
            canvas.SetData(data, null);

            int? received = null;
            canvas.NodeClicked += id => received = id;

            canvas.SimulateClickAt(canvas.DataToScreen(new Vector2(1f, 1f)));

            Assert.That(received, Is.EqualTo(nodeId));
        }
    }
}
#endif
