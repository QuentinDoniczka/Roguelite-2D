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
        public void NodeClickedEvent_FiresWithCorrectId_OnSimulateClickInsideNode()
        {
            var nodes = new List<SkillTreeData.SkillNodeEntry>
            {
                new SkillTreeData.SkillNodeEntry { id = NodeAId, position = Vector2.zero }
            };
            var data = CreateDataWithNodes(nodes);
            var canvas = CreateCanvasWithData(data);

            int? receivedId = null;
            var host = new VisualElement();
            host.Add(canvas);
            host.RegisterCallback<NodeClickedEvent>(evt => receivedId = evt.NodeId);

            var screenPos = canvas.DataToScreen(Vector2.zero);
            canvas.SimulateClickAt(screenPos);

            Assert.AreEqual(NodeAId, receivedId);
        }

        [Test]
        public void Window_UpdatesSelectedNodeId_OnNodeClickedEvent()
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
    }
}
#endif
