using System.Collections;
using NUnit.Framework;
using RogueliteAutoBattler.UI.Screens.SkillTree;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;

namespace RogueliteAutoBattler.Tests.PlayMode
{
    public class SkillTreeNodeTests : PlayModeTestBase
    {
        private GameObject _nodeGo;
        private SkillTreeNode _node;

        [SetUp]
        public void SetUp()
        {
            (_nodeGo, _node) = TestCharacterFactory.CreateSkillTreeNode(0);
            Track(_nodeGo);
        }

        [UnityTest]
        public IEnumerator OnPointerClick_FiresOnNodeClicked()
        {
            yield return null;

            SkillTreeNode clickedNode = null;
            _node.OnNodeClicked += n => clickedNode = n;

            var eventData = new PointerEventData(EventSystem.current);
            ExecuteEvents.Execute<IPointerClickHandler>(_nodeGo, eventData, ExecuteEvents.pointerClickHandler);
            yield return null;

            Assert.AreEqual(_node, clickedNode);
        }

        [Test]
        public void SetSelected_True_ChangesBorderColorToSelected()
        {
            _node.SetSelected(true);

            var borderImage = _nodeGo.transform.GetChild(0).GetComponent<UnityEngine.UI.Image>();
            Assert.AreEqual(Color.yellow, borderImage.color);
        }

        [Test]
        public void SetSelected_False_ResetsBorderColorToNormal()
        {
            _node.SetSelected(true);
            _node.SetSelected(false);

            var borderImage = _nodeGo.transform.GetChild(0).GetComponent<UnityEngine.UI.Image>();
            Assert.AreEqual(Color.gray, borderImage.color);
        }

        [Test]
        public void Initialize_SetsNodeIndex()
        {
            var (go, node) = TestCharacterFactory.CreateSkillTreeNode(7);
            Track(go);

            Assert.AreEqual(7, node.NodeIndex);
        }

        [Test]
        public void IsSelected_ReflectsState()
        {
            Assert.IsFalse(_node.IsSelected);

            _node.SetSelected(true);
            Assert.IsTrue(_node.IsSelected);

            _node.SetSelected(false);
            Assert.IsFalse(_node.IsSelected);
        }
    }
}
