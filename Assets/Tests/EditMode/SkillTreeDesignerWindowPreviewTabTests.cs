#if UNITY_EDITOR
using System.Reflection;
using NUnit.Framework;
using RogueliteAutoBattler.Editor.Windows;

namespace RogueliteAutoBattler.Tests.EditMode
{
    [TestFixture]
    public class SkillTreeDesignerWindowPreviewTabTests
    {
        private static string[] GetLabels(string fieldName)
        {
            var field = typeof(SkillTreeDesignerWindow).GetField(
                fieldName,
                BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNotNull(field, $"Could not find field '{fieldName}' via reflection.");
            return field.GetValue(null) as string[];
        }

        [Test]
        public void TabLabels_ContainPreview_WithoutBranch()
        {
            var labels = GetLabels("TabLabelsWithoutBranch");
            Assert.IsNotNull(labels);
            CollectionAssert.Contains(labels, "Preview", "TabLabelsWithoutBranch should contain 'Preview'.");
        }

        [Test]
        public void TabLabels_ContainPreview_WithBranch()
        {
            var labels = GetLabels("TabLabelsWithBranch");
            Assert.IsNotNull(labels);
            CollectionAssert.Contains(labels, "Preview", "TabLabelsWithBranch should contain 'Preview'.");
        }

        [Test]
        public void TabLabels_PreviewIsLast()
        {
            var labelsWithout = GetLabels("TabLabelsWithoutBranch");
            Assert.IsNotNull(labelsWithout);
            Assert.AreEqual("Preview", labelsWithout[labelsWithout.Length - 1],
                "'Preview' should be the last entry in TabLabelsWithoutBranch.");

            var labelsWith = GetLabels("TabLabelsWithBranch");
            Assert.IsNotNull(labelsWith);
            Assert.AreEqual("Preview", labelsWith[labelsWith.Length - 1],
                "'Preview' should be the last entry in TabLabelsWithBranch.");
        }
    }
}
#endif
