using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using RogueliteAutoBattler.Data;
using RogueliteAutoBattler.Editor.Tools;

namespace RogueliteAutoBattler.Tests.EditMode
{
    [TestFixture]
    public class StatTypeValidatorTests
    {
        [Test]
        public void Run_WithEmptyAssetList_ReportsZeroNodesAndZeroIssues()
        {
            var report = StatTypeValidator.Run(new List<SkillTreeData>());

            Assert.AreEqual(0, report.ScannedAssets);
            Assert.AreEqual(0, report.ScannedNodes);
            Assert.AreEqual(0, report.Issues.Count);
        }

        [Test]
        public void Run_WithFreshGeneratedAsset_ReportsZeroIssues()
        {
            var asset = ScriptableObject.CreateInstance<SkillTreeData>();
            try
            {
                var report = StatTypeValidator.Run(new[] { asset });

                Assert.AreEqual(1, report.ScannedAssets);
                Assert.GreaterOrEqual(report.ScannedNodes, 1);
                Assert.AreEqual(0, report.Issues.Count, string.Join("\n", report.Issues));
            }
            finally
            {
                Object.DestroyImmediate(asset);
            }
        }

        [Test]
        public void Run_WithRealProjectAsset_ReportsZeroIssues()
        {
            var realAsset = AssetDatabase.LoadAssetAtPath<SkillTreeData>("Assets/Data/SkillTreeData.asset");
            Assert.IsNotNull(realAsset, "Expected real SkillTreeData asset at Assets/Data/SkillTreeData.asset");

            var report = StatTypeValidator.Run(new[] { realAsset });

            Assert.AreEqual(1, report.ScannedAssets);
            Assert.AreEqual(0, report.Issues.Count, string.Join("\n", report.Issues));
        }

        [Test]
        public void Run_ValidationReport_FieldsAreInitializedCorrectly()
        {
            var issues = new List<string> { "fake" };

            var report = new StatTypeValidator.ValidationReport(2, 5, issues);

            Assert.AreEqual(2, report.ScannedAssets);
            Assert.AreEqual(5, report.ScannedNodes);
            Assert.AreEqual(1, report.Issues.Count);
            Assert.AreEqual("fake", report.Issues[0]);
        }
    }
}
