using NUnit.Framework;
using RogueliteAutoBattler.Combat.Core;

namespace RogueliteAutoBattler.Tests.EditMode
{
    [TestFixture]
    public class ModifierSourcesTests
    {
        [Test]
        public void Constants_AreNonEmpty()
        {
            Assert.IsFalse(string.IsNullOrEmpty(ModifierSources.Base));
            Assert.IsFalse(string.IsNullOrEmpty(ModifierSources.TechTree));
            Assert.IsFalse(string.IsNullOrEmpty(ModifierSources.Item));
            Assert.IsFalse(string.IsNullOrEmpty(ModifierSources.Blessing));
            Assert.IsFalse(string.IsNullOrEmpty(ModifierSources.LevelUp));
        }

        [Test]
        public void ItemSource_PrefixesInstanceId()
        {
            Assert.AreEqual("item:sword42", ModifierSources.ItemSource("sword42"));
        }

        [Test]
        public void TechTreeNode_FormatsAsTechtreeColonNodeId()
        {
            Assert.AreEqual("techtree:node0", ModifierSources.TechTreeNode(0));
            Assert.AreEqual("techtree:node7", ModifierSources.TechTreeNode(7));
            Assert.AreEqual("techtree:node42", ModifierSources.TechTreeNode(42));
        }
    }
}
