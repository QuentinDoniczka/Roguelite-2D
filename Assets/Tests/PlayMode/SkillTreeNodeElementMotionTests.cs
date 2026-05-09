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
    public class SkillTreeNodeElementMotionTests : PlayModeTestBase
    {
        private const string PanelSettingsPath = "Assets/UI/MainPanelSettings.asset";
        private const string MainStylePath = "Assets/UI/Styles/MainStyle.uss";
        private const string HaloBreatheOnClass = "skill-tree-node--halo-breathe-on";
        private const float OneBreathTickPlusMarginSeconds = 2.0f;
        private const float SparkleObservationSeconds = 0.5f;

        private UIDocument CreateDocument()
        {
            PanelSettings panelSettings = AssetDatabase.LoadAssetAtPath<PanelSettings>(PanelSettingsPath);
            Assert.IsNotNull(panelSettings, $"MainPanelSettings not found at {PanelSettingsPath}");

            var documentGo = Track(new GameObject("TestMotionUIDocument"));
            documentGo.SetActive(false);
            var doc = documentGo.AddComponent<UIDocument>();
            doc.panelSettings = panelSettings;
            documentGo.SetActive(true);
            return doc;
        }

        private static void AttachMainStyle(UIDocument doc)
        {
            StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(MainStylePath);
            Assert.IsNotNull(styleSheet, $"MainStyle USS not found at {MainStylePath}");
            doc.rootVisualElement.styleSheets.Add(styleSheet);
        }

        [UnityTest]
        public IEnumerator AvailableNode_TogglesHaloBreatheClass()
        {
            var doc = CreateDocument();
            yield return null;
            yield return null;

            var node = new SkillTreeNodeElement(0);
            node.SetState(SkillTreeNodeVisualState.Available);
            doc.rootVisualElement.Add(node);

            yield return new WaitForSeconds(OneBreathTickPlusMarginSeconds);

            Assert.IsTrue(node.ClassListContains(HaloBreatheOnClass),
                $"After ~1 breathe tick (1600 ms) an available node must have {HaloBreatheOnClass} toggled on.");
        }

        [UnityTest]
        public IEnumerator PurchasedNode_TogglesHaloBreatheClass()
        {
            var doc = CreateDocument();
            yield return null;
            yield return null;

            var node = new SkillTreeNodeElement(0);
            node.SetState(SkillTreeNodeVisualState.Purchased);
            doc.rootVisualElement.Add(node);

            yield return new WaitForSeconds(OneBreathTickPlusMarginSeconds);

            Assert.IsTrue(node.ClassListContains(HaloBreatheOnClass),
                $"After ~1 breathe tick (1600 ms) a purchased node must have {HaloBreatheOnClass} toggled on.");
        }

        [UnityTest]
        public IEnumerator MaxNode_TogglesHaloBreatheClass()
        {
            var doc = CreateDocument();
            yield return null;
            yield return null;

            var node = new SkillTreeNodeElement(0);
            node.SetState(SkillTreeNodeVisualState.Max);
            doc.rootVisualElement.Add(node);

            yield return new WaitForSeconds(OneBreathTickPlusMarginSeconds);

            Assert.IsTrue(node.ClassListContains(HaloBreatheOnClass),
                $"After ~1 breathe tick (1600 ms) a max node must have {HaloBreatheOnClass} toggled on.");
        }

        [UnityTest]
        public IEnumerator LockedNode_DoesNotReceiveHaloBreatheClass()
        {
            var doc = CreateDocument();
            yield return null;
            yield return null;

            var node = new SkillTreeNodeElement(0);
            node.SetState(SkillTreeNodeVisualState.Locked);
            doc.rootVisualElement.Add(node);

            yield return new WaitForSeconds(OneBreathTickPlusMarginSeconds);

            Assert.IsFalse(node.ClassListContains(HaloBreatheOnClass),
                $"A locked node must never receive {HaloBreatheOnClass}.");
        }

        [UnityTest]
        public IEnumerator Available_ThenLocked_HaloBreatheClassRemoved()
        {
            var doc = CreateDocument();
            yield return null;
            yield return null;

            var node = new SkillTreeNodeElement(0);
            node.SetState(SkillTreeNodeVisualState.Available);
            doc.rootVisualElement.Add(node);

            yield return new WaitForSeconds(OneBreathTickPlusMarginSeconds);

            Assert.IsTrue(node.ClassListContains(HaloBreatheOnClass),
                $"Precondition: {HaloBreatheOnClass} must be present before transitioning to Locked.");

            node.SetState(SkillTreeNodeVisualState.Locked);

            Assert.IsFalse(node.ClassListContains(HaloBreatheOnClass),
                $"SetState(Locked) must immediately remove {HaloBreatheOnClass} without waiting for the next scheduler tick.");
        }

        [UnityTest]
        public IEnumerator MaxNode_SparkleRotates()
        {
            var doc = CreateDocument();
            yield return null;
            yield return null;

            var node = new SkillTreeNodeElement(0);
            node.SetState(SkillTreeNodeVisualState.Max);
            doc.rootVisualElement.Add(node);

            yield return new WaitForSeconds(SparkleObservationSeconds);

            var sparkle = node.Q(className: "skill-tree-node__sparkle");
            var rotateStyle = sparkle.style.rotate;
            float angle = rotateStyle.keyword == StyleKeyword.Null || rotateStyle.keyword == StyleKeyword.Undefined
                ? 0f
                : rotateStyle.value.angle.value;

            Assert.Greater(angle, 5f,
                "After ~0.5s (~10 ticks at 1.5 deg/tick), sparkle rotation must exceed 5 degrees.");
        }

        [UnityTest]
        public IEnumerator AvailableNode_SparkleDoesNotRotate()
        {
            var doc = CreateDocument();
            yield return null;
            yield return null;

            var node = new SkillTreeNodeElement(0);
            node.SetState(SkillTreeNodeVisualState.Available);
            doc.rootVisualElement.Add(node);

            yield return new WaitForSeconds(SparkleObservationSeconds);

            var sparkle = node.Q(className: "skill-tree-node__sparkle");
            var rotateStyle = sparkle.style.rotate;
            float angle = rotateStyle.keyword == StyleKeyword.Null || rotateStyle.keyword == StyleKeyword.Undefined
                ? 0f
                : rotateStyle.value.angle.value;

            Assert.Less(angle, 0.5f,
                "An available node's sparkle must not rotate (angle must remain at 0).");
        }

        [UnityTest]
        public IEnumerator Max_ThenAvailable_SparkleRotationResets()
        {
            var doc = CreateDocument();
            yield return null;
            yield return null;

            var node = new SkillTreeNodeElement(0);
            node.SetState(SkillTreeNodeVisualState.Max);
            doc.rootVisualElement.Add(node);

            yield return new WaitForSeconds(SparkleObservationSeconds);

            node.SetState(SkillTreeNodeVisualState.Available);

            var sparkle = node.Q(className: "skill-tree-node__sparkle");
            var rotateStyle = sparkle.style.rotate;
            float angle = rotateStyle.keyword == StyleKeyword.Null || rotateStyle.keyword == StyleKeyword.Undefined
                ? 0f
                : rotateStyle.value.angle.value;

            Assert.AreEqual(0f, angle, 0.001f,
                "SetState(Available) after Max must immediately reset sparkle rotation to 0.");
        }
    }
}
#endif
