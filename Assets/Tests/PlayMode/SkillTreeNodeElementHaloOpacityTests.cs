#if UNITY_EDITOR
using NUnit.Framework;
using UnityEngine.UIElements;
using RogueliteAutoBattler.Data;
using RogueliteAutoBattler.UI.Toolkit.SkillTree;

namespace RogueliteAutoBattler.Tests.PlayMode
{
    public class SkillTreeNodeElementHaloOpacityTests
    {
        private SkillTreeVisualSettingsProviderScope _scope;

        [TearDown]
        public void TearDown()
        {
            _scope?.Dispose();
            _scope = null;
        }

        private SkillTreeVisualSettingsProviderScope InstallScope(float size = 120f, float locked = 0f,
            float available = 0.6f, float purchased = 0.85f, float max = 1f)
        {
            _scope = new SkillTreeVisualSettingsProviderScope();
            _scope.Stub.SetForTesting(size, locked, available, purchased, max);
            return _scope;
        }

        [Test]
        public void SetState_AppliesOpacityForLocked()
        {
            InstallScope(locked: 0.1f);

            var node = new SkillTreeNodeElement(0);
            node.SetState(SkillTreeNodeVisualState.Locked);

            var halo = node.Q(className: "skill-tree-node__halo");
            Assert.AreEqual(0.1f, halo.style.opacity.value, 0.001f,
                "Halo opacity must match HaloOpacityLocked from settings.");
        }

        [Test]
        public void SetState_AppliesOpacityForAvailable()
        {
            InstallScope(available: 0.2f);

            var node = new SkillTreeNodeElement(0);
            node.SetState(SkillTreeNodeVisualState.Available);

            var halo = node.Q(className: "skill-tree-node__halo");
            Assert.AreEqual(0.2f, halo.style.opacity.value, 0.001f,
                "Halo opacity must match HaloOpacityAvailable from settings.");
        }

        [Test]
        public void SetState_AppliesOpacityForPurchased()
        {
            InstallScope(purchased: 0.3f);

            var node = new SkillTreeNodeElement(0);
            node.SetState(SkillTreeNodeVisualState.Purchased);

            var halo = node.Q(className: "skill-tree-node__halo");
            Assert.AreEqual(0.3f, halo.style.opacity.value, 0.001f,
                "Halo opacity must match HaloOpacityPurchased from settings.");
        }

        [Test]
        public void SetState_AppliesOpacityForMax()
        {
            InstallScope(max: 0.4f);

            var node = new SkillTreeNodeElement(0);
            node.SetState(SkillTreeNodeVisualState.Max);

            var halo = node.Q(className: "skill-tree-node__halo");
            Assert.AreEqual(0.4f, halo.style.opacity.value, 0.001f,
                "Halo opacity must match HaloOpacityMax from settings.");
        }

        [Test]
        public void Settings_Null_FallsBackToDefaults_Locked()
        {
            InstallScope();
            SkillTreeVisualSettingsResolver.Provider = () => null;
            SkillTreeVisualSettingsResolver.ResetCache();

            var node = new SkillTreeNodeElement(0);
            node.SetState(SkillTreeNodeVisualState.Locked);

            var halo = node.Q(className: "skill-tree-node__halo");
            Assert.AreEqual(0f, halo.style.opacity.value, 0.001f,
                "Fallback opacity for Locked must be 0f when settings asset is absent.");
        }

        [Test]
        public void Settings_Null_FallsBackToDefaults_Available()
        {
            InstallScope();
            SkillTreeVisualSettingsResolver.Provider = () => null;
            SkillTreeVisualSettingsResolver.ResetCache();

            var node = new SkillTreeNodeElement(0);
            node.SetState(SkillTreeNodeVisualState.Available);

            var halo = node.Q(className: "skill-tree-node__halo");
            Assert.AreEqual(0.6f, halo.style.opacity.value, 0.001f,
                "Fallback opacity for Available must be 0.6f when settings asset is absent.");
        }

        [Test]
        public void Constructor_AppliesHaloSize_FromSettings()
        {
            InstallScope(size: 150f);

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
