#if UNITY_EDITOR
using System;
using NUnit.Framework;
using RogueliteAutoBattler.Data;
using RogueliteAutoBattler.UI.Toolkit.SkillTree;
using UnityEngine;
using UnityEngine.UIElements;

namespace RogueliteAutoBattler.Tests.PlayMode
{
    public class SkillTreeNodeElementHaloOpacityTests
    {
        private Func<SkillTreeVisualSettings> _savedProvider;
        private SkillTreeVisualSettings _testSettings;

        [SetUp]
        public void SetUp()
        {
            _savedProvider = SkillTreeVisualSettingsResolver.Provider;
            SkillTreeVisualSettingsResolver.ResetCache();
        }

        [TearDown]
        public void TearDown()
        {
            SkillTreeVisualSettingsResolver.Provider = _savedProvider;
            SkillTreeVisualSettingsResolver.ResetCache();
            if (_testSettings != null)
                ScriptableObject.DestroyImmediate(_testSettings);
        }

        private SkillTreeVisualSettings CreateSettings(float size = 120f, float locked = 0f,
            float available = 0.6f, float purchased = 0.85f, float max = 1f)
        {
            var s = ScriptableObject.CreateInstance<SkillTreeVisualSettings>();
            s.SetForTesting(size, locked, available, purchased, max);
            return s;
        }

        [Test]
        public void SetState_AppliesOpacityForLocked()
        {
            _testSettings = CreateSettings(locked: 0.1f);
            SkillTreeVisualSettingsResolver.Provider = () => _testSettings;

            var node = new SkillTreeNodeElement(0);
            node.SetState(SkillTreeNodeVisualState.Locked);

            var halo = node.Q(className: "skill-tree-node__halo");
            Assert.AreEqual(0.1f, halo.style.opacity.value, 0.001f,
                "Halo opacity must match HaloOpacityLocked from settings.");
        }

        [Test]
        public void SetState_AppliesOpacityForAvailable()
        {
            _testSettings = CreateSettings(available: 0.2f);
            SkillTreeVisualSettingsResolver.Provider = () => _testSettings;

            var node = new SkillTreeNodeElement(0);
            node.SetState(SkillTreeNodeVisualState.Available);

            var halo = node.Q(className: "skill-tree-node__halo");
            Assert.AreEqual(0.2f, halo.style.opacity.value, 0.001f,
                "Halo opacity must match HaloOpacityAvailable from settings.");
        }

        [Test]
        public void SetState_AppliesOpacityForPurchased()
        {
            _testSettings = CreateSettings(purchased: 0.3f);
            SkillTreeVisualSettingsResolver.Provider = () => _testSettings;

            var node = new SkillTreeNodeElement(0);
            node.SetState(SkillTreeNodeVisualState.Purchased);

            var halo = node.Q(className: "skill-tree-node__halo");
            Assert.AreEqual(0.3f, halo.style.opacity.value, 0.001f,
                "Halo opacity must match HaloOpacityPurchased from settings.");
        }

        [Test]
        public void SetState_AppliesOpacityForMax()
        {
            _testSettings = CreateSettings(max: 0.4f);
            SkillTreeVisualSettingsResolver.Provider = () => _testSettings;

            var node = new SkillTreeNodeElement(0);
            node.SetState(SkillTreeNodeVisualState.Max);

            var halo = node.Q(className: "skill-tree-node__halo");
            Assert.AreEqual(0.4f, halo.style.opacity.value, 0.001f,
                "Halo opacity must match HaloOpacityMax from settings.");
        }

        [Test]
        public void Settings_Null_FallsBackToDefaults_Locked()
        {
            SkillTreeVisualSettingsResolver.Provider = () => null;

            var node = new SkillTreeNodeElement(0);
            node.SetState(SkillTreeNodeVisualState.Locked);

            var halo = node.Q(className: "skill-tree-node__halo");
            Assert.AreEqual(0f, halo.style.opacity.value, 0.001f,
                "Fallback opacity for Locked must be 0f when settings asset is absent.");
        }

        [Test]
        public void Settings_Null_FallsBackToDefaults_Available()
        {
            SkillTreeVisualSettingsResolver.Provider = () => null;

            var node = new SkillTreeNodeElement(0);
            node.SetState(SkillTreeNodeVisualState.Available);

            var halo = node.Q(className: "skill-tree-node__halo");
            Assert.AreEqual(0.6f, halo.style.opacity.value, 0.001f,
                "Fallback opacity for Available must be 0.6f when settings asset is absent.");
        }

        [Test]
        public void Constructor_AppliesHaloSize_FromSettings()
        {
            _testSettings = CreateSettings(size: 150f);
            SkillTreeVisualSettingsResolver.Provider = () => _testSettings;

            var node = new SkillTreeNodeElement(0);

            var halo = node.Q(className: "skill-tree-node__halo");
            Assert.AreEqual(150f, halo.style.width.value.value, 0.001f,
                "Halo width must match HaloSize from settings.");
            Assert.AreEqual(150f, halo.style.height.value.value, 0.001f,
                "Halo height must match HaloSize from settings.");
        }
    }
}
#endif
