using NUnit.Framework;
using RogueliteAutoBattler.Data;
using RogueliteAutoBattler.Editor.Tools;
using UnityEngine;

namespace RogueliteAutoBattler.Tests.EditMode
{
    public class SkillTreeNodeFactorySnapDefaultsTests
    {
        private const float Tolerance = 1e-4f;

        [Test]
        public void CreateBranchNode_DefaultSnapEnabled_IsTrue()
        {
            var entry = SkillTreeNodeFactory.CreateBranchNode(1, Vector2.zero);

            Assert.That(entry.snapEnabled, Is.True);
        }

        [Test]
        public void CreateBranchNode_DefaultSnapThresholdUnits_Matches025()
        {
            var entry = SkillTreeNodeFactory.CreateBranchNode(1, Vector2.zero);

            Assert.That(entry.snapThresholdUnits, Is.EqualTo(0.25f).Within(Tolerance));
        }

        [Test]
        public void SkillTreeData_DefaultSnapEnabled_IsTrue()
        {
            Assert.That(SkillTreeData.DefaultSnapEnabled, Is.True);
        }

        [Test]
        public void SkillTreeData_DefaultSnapThresholdUnits_Matches025()
        {
            Assert.That(SkillTreeData.DefaultSnapThresholdUnits, Is.EqualTo(0.25f).Within(Tolerance));
        }
    }
}
