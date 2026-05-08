#if UNITY_EDITOR
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;
using RogueliteAutoBattler.UI.Toolkit.SkillTree;

namespace RogueliteAutoBattler.Tests.PlayMode
{
    public class SkillTreeNodeElementOrbVisualTests
    {
        [SetUp]
        public void SetUp()
        {
            SkillTreeNodeOrbResolver.ResetCache();
        }

        [Test]
        public void Constructor_AssignsBackgroundImage()
        {
            var node = new SkillTreeNodeElement(0);

            var assigned = node.style.backgroundImage;
            Assert.IsTrue(
                assigned.keyword == StyleKeyword.Undefined && assigned.value.texture != null,
                "ctor must set style.backgroundImage to the orb texture when Resources.Load succeeds.");
        }

        [Test]
        public void SetColorTag_AppliesTintColor()
        {
            var node = new SkillTreeNodeElement(0);

            node.SetColorTag(Color.blue);

            var tint = node.style.unityBackgroundImageTintColor;
            Assert.AreEqual(StyleKeyword.Undefined, tint.keyword,
                "unityBackgroundImageTintColor must be an inline value, not a keyword.");
            Assert.AreEqual(Color.blue.r, tint.value.r, 0.01f, "R channel must match Color.blue");
            Assert.AreEqual(Color.blue.g, tint.value.g, 0.01f, "G channel must match Color.blue");
            Assert.AreEqual(Color.blue.b, tint.value.b, 0.01f, "B channel must match Color.blue");
            Assert.AreEqual(Color.blue.a, tint.value.a, 0.01f, "A channel must match Color.blue");
        }

        [Test]
        public void SetColorTag_PreservesBackgroundColorContract()
        {
            var node = new SkillTreeNodeElement(0);

            node.SetColorTag(Color.blue);

            Assert.AreEqual(Color.blue, node.style.backgroundColor.value,
                "backgroundColor must still equal the color passed to SetColorTag (regression #303).");
            Assert.AreEqual(Color.blue, node.style.borderLeftColor.value,
                "borderLeftColor must still equal the color passed to SetColorTag (regression #303).");
            Assert.AreEqual(Color.blue, node.style.borderRightColor.value,
                "borderRightColor must still equal the color passed to SetColorTag (regression #303).");
            Assert.AreEqual(Color.blue, node.style.borderTopColor.value,
                "borderTopColor must still equal the color passed to SetColorTag (regression #303).");
            Assert.AreEqual(Color.blue, node.style.borderBottomColor.value,
                "borderBottomColor must still equal the color passed to SetColorTag (regression #303).");
        }
    }
}
#endif
