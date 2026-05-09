using System.IO;
using NUnit.Framework;
using RogueliteAutoBattler.Data;
using RogueliteAutoBattler.Editor.Tools;
using UnityEditor;
using UnityEngine;

namespace RogueliteAutoBattler.Tests.EditMode
{
    public class SkillTreeVisualSettingsTests
    {
        private SkillTreeVisualSettings _instance;

        [TearDown]
        public void TearDown()
        {
            if (_instance != null)
                ScriptableObject.DestroyImmediate(_instance);
        }

        [Test]
        public void Defaults_AreReasonable()
        {
            _instance = ScriptableObject.CreateInstance<SkillTreeVisualSettings>();

            Assert.AreEqual(120f, _instance.HaloSize);
            Assert.AreEqual(0f, _instance.HaloOpacityLocked);
            Assert.AreEqual(0.6f, _instance.HaloOpacityAvailable);
            Assert.AreEqual(0.85f, _instance.HaloOpacityPurchased);
            Assert.AreEqual(1f, _instance.HaloOpacityMax);
        }

        [Test]
        public void AssetCreator_CreatesAtExpectedPath_AndIsIdempotent()
        {
            var first = SkillTreeVisualSettingsAssetCreator.EnsureExists();

            Assert.IsNotNull(
                AssetDatabase.LoadAssetAtPath<SkillTreeVisualSettings>(SkillTreeVisualSettingsAssetCreator.AssetPath),
                $"Asset must exist at {SkillTreeVisualSettingsAssetCreator.AssetPath} after EnsureExists.");

            Assert.IsTrue(
                File.Exists(SkillTreeVisualSettingsAssetCreator.AssetPath),
                $"File must exist on disk at {SkillTreeVisualSettingsAssetCreator.AssetPath}.");

            var second = SkillTreeVisualSettingsAssetCreator.EnsureExists();

            Assert.AreEqual(first.GetInstanceID(), second.GetInstanceID(),
                "EnsureExists must be idempotent.");
        }
    }
}
