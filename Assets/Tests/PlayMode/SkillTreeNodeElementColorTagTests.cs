#if UNITY_EDITOR
using NUnit.Framework;
using UnityEngine;
using RogueliteAutoBattler.UI.Toolkit.SkillTree;

namespace RogueliteAutoBattler.Tests.PlayMode
{
    public class SkillTreeNodeElementColorTagTests
    {
        [Test]
        public void SetColorTag_AppliesInlineColors_AndSurvivesSetState()
        {
            var node = new SkillTreeNodeElement(0);

            node.SetColorTag(Color.red);

            Assert.AreEqual(Color.red, node.CurrentColor,
                "CurrentColor must equal the color passed to SetColorTag.");
            Assert.AreEqual(Color.red, node.style.backgroundColor.value,
                "Inline backgroundColor must equal the color passed to SetColorTag.");
            Assert.AreEqual(Color.red, node.style.borderLeftColor.value,
                "Inline borderLeftColor must equal the color passed to SetColorTag.");
            Assert.AreEqual(Color.red, node.style.borderRightColor.value,
                "Inline borderRightColor must equal the color passed to SetColorTag.");
            Assert.AreEqual(Color.red, node.style.borderTopColor.value,
                "Inline borderTopColor must equal the color passed to SetColorTag.");
            Assert.AreEqual(Color.red, node.style.borderBottomColor.value,
                "Inline borderBottomColor must equal the color passed to SetColorTag.");

            node.SetState(SkillTreeNodeVisualState.Available);

            Assert.AreEqual(Color.red, node.style.backgroundColor.value,
                "Inline backgroundColor must survive a SetState class swap.");
            Assert.AreEqual(Color.red, node.style.borderLeftColor.value,
                "Inline borderLeftColor must survive a SetState class swap.");
            Assert.AreEqual(Color.red, node.style.borderRightColor.value,
                "Inline borderRightColor must survive a SetState class swap.");
            Assert.AreEqual(Color.red, node.style.borderTopColor.value,
                "Inline borderTopColor must survive a SetState class swap.");
            Assert.AreEqual(Color.red, node.style.borderBottomColor.value,
                "Inline borderBottomColor must survive a SetState class swap.");
            Assert.AreEqual(Color.red, node.CurrentColor,
                "CurrentColor must remain unchanged across SetState calls.");
        }
    }
}
#endif
