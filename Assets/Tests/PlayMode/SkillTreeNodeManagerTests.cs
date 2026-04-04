using System.Collections;
using System.Linq;
using NUnit.Framework;
using RogueliteAutoBattler.UI.Screens.SkillTree;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace RogueliteAutoBattler.Tests.PlayMode
{
    public class SkillTreeNodeManagerTests : PlayModeTestBase
    {
        private GameObject _root;
        private SkillTreeNodeManager _manager;
        private RectTransform _content;

        [SetUp]
        public void SetUp()
        {
            (_root, _manager, _content) = TestCharacterFactory.CreateSkillTreeNodeManager();
            Track(_root);
        }

        [UnityTest]
        public IEnumerator Initialize_CreatesNodes()
        {
            _manager.Initialize();
            yield return null;

            Assert.AreEqual(10, _content.childCount);
        }

        [UnityTest]
        public IEnumerator Initialize_NodesHaveCorrectComponents()
        {
            _manager.Initialize();
            yield return null;

            for (int i = 0; i < _content.childCount; i++)
            {
                var child = _content.GetChild(i).gameObject;
                Assert.IsNotNull(child.GetComponent<SkillTreeNode>(),
                    $"Node {i} should have SkillTreeNode component");
                Assert.IsNotNull(child.GetComponent<Image>(),
                    $"Node {i} should have Image component");
            }
        }

        [UnityTest]
        public IEnumerator SelectNode_FiresOnNodeSelected()
        {
            _manager.Initialize();
            yield return null;

            SkillTreeNode selectedNode = null;
            _manager.OnNodeSelected += n => selectedNode = n;

            var firstNodeGo = _content.GetChild(0).gameObject;
            var eventData = new PointerEventData(EventSystem.current);
            ExecuteEvents.Execute<IPointerClickHandler>(firstNodeGo, eventData, ExecuteEvents.pointerClickHandler);
            yield return null;

            Assert.IsNotNull(selectedNode);
            Assert.AreEqual(0, selectedNode.NodeIndex);
        }

        [UnityTest]
        public IEnumerator SelectNode_DeselectsPrevious()
        {
            _manager.Initialize();
            yield return null;

            var firstNodeGo = _content.GetChild(0).gameObject;
            var secondNodeGo = _content.GetChild(1).gameObject;
            var firstNode = firstNodeGo.GetComponent<SkillTreeNode>();
            var secondNode = secondNodeGo.GetComponent<SkillTreeNode>();

            var eventData = new PointerEventData(EventSystem.current);
            ExecuteEvents.Execute<IPointerClickHandler>(firstNodeGo, eventData, ExecuteEvents.pointerClickHandler);
            yield return null;

            Assert.IsTrue(firstNode.IsSelected);

            eventData = new PointerEventData(EventSystem.current);
            ExecuteEvents.Execute<IPointerClickHandler>(secondNodeGo, eventData, ExecuteEvents.pointerClickHandler);
            yield return null;

            Assert.IsFalse(firstNode.IsSelected);
            Assert.IsTrue(secondNode.IsSelected);
        }

        [UnityTest]
        public IEnumerator DeselectAll_ClearsSelection()
        {
            _manager.Initialize();
            yield return null;

            var firstNodeGo = _content.GetChild(0).gameObject;
            var firstNode = firstNodeGo.GetComponent<SkillTreeNode>();

            var eventData = new PointerEventData(EventSystem.current);
            ExecuteEvents.Execute<IPointerClickHandler>(firstNodeGo, eventData, ExecuteEvents.pointerClickHandler);
            yield return null;

            Assert.IsTrue(firstNode.IsSelected);

            _manager.DeselectAll();
            yield return null;

            Assert.IsFalse(firstNode.IsSelected);

            var allNodes = _content.GetComponentsInChildren<SkillTreeNode>();
            Assert.IsTrue(allNodes.All(n => !n.IsSelected));
        }

        [UnityTest]
        public IEnumerator DeselectAll_WhenNothingSelected_DoesNotFireEvent()
        {
            _manager.Initialize();
            yield return null;

            bool deselectedFired = false;
            _manager.OnNodeDeselected += () => deselectedFired = true;

            _manager.DeselectAll();
            yield return null;

            Assert.IsFalse(deselectedFired);
        }
    }
}
