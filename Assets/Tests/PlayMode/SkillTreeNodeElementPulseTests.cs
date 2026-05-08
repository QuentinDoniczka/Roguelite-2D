#if UNITY_EDITOR
using System.Collections;
using NUnit.Framework;
using RogueliteAutoBattler.UI.Toolkit.SkillTree;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace RogueliteAutoBattler.Tests.PlayMode
{
    public class SkillTreeNodeElementPulseTests : PlayModeTestBase
    {
        private const string PanelSettingsPath = "Assets/UI/MainPanelSettings.asset";
        private const string PulseOnClass = "skill-tree-node--pulse-on";
        private const string AvailableClass = "skill-tree-node--available";
        private const string LockedClass = "skill-tree-node--locked";

        private UIDocument CreateDocument()
        {
            PanelSettings panelSettings = AssetDatabase.LoadAssetAtPath<PanelSettings>(PanelSettingsPath);
            Assert.IsNotNull(panelSettings, $"MainPanelSettings not found at {PanelSettingsPath}");

            var documentGo = Track(new GameObject("TestPulseUIDocument"));
            documentGo.SetActive(false);
            var doc = documentGo.AddComponent<UIDocument>();
            doc.panelSettings = panelSettings;
            documentGo.SetActive(true);
            return doc;
        }

        [UnityTest]
        public IEnumerator LockedNode_DoesNotReceivePulseClass_AfterScheduler()
        {
            var doc = CreateDocument();
            yield return null;
            yield return null;

            var node = new SkillTreeNodeElement(0);
            node.SetState(SkillTreeNodeVisualState.Locked);
            doc.rootVisualElement.Add(node);

            yield return new WaitForSeconds(1.2f);

            Assert.IsFalse(node.ClassListContains(PulseOnClass),
                $"A locked node must never receive {PulseOnClass}.");
        }

        [UnityTest]
        public IEnumerator AvailableNode_TogglesPulseClass()
        {
            var doc = CreateDocument();
            yield return null;
            yield return null;

            var node = new SkillTreeNodeElement(0);
            node.SetState(SkillTreeNodeVisualState.Available);
            doc.rootVisualElement.Add(node);

            yield return new WaitForSeconds(1.2f);

            Assert.IsTrue(node.ClassListContains(PulseOnClass),
                $"After ~1 tick (800 ms) an available node must have {PulseOnClass} toggled on (observed between tick 1 and tick 2).");
        }

        [UnityTest]
        public IEnumerator Available_ThenLockedAfterPulse_PulseClassRemoved()
        {
            var doc = CreateDocument();
            yield return null;
            yield return null;

            var node = new SkillTreeNodeElement(0);
            node.SetState(SkillTreeNodeVisualState.Available);
            doc.rootVisualElement.Add(node);

            yield return new WaitForSeconds(1.2f);

            Assert.IsTrue(node.ClassListContains(PulseOnClass),
                $"Precondition: {PulseOnClass} must be present before transitioning away from Available.");

            node.SetState(SkillTreeNodeVisualState.Locked);

            Assert.IsFalse(node.ClassListContains(PulseOnClass),
                $"SetState(Locked) must immediately remove {PulseOnClass} without waiting for the next scheduler tick.");
        }
    }
}
#endif
