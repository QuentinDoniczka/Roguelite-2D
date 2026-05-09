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
        private const float RaysObservationSeconds = 2.0f;

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
        public IEnumerator MaxNode_RaysRotate()
        {
            var doc = CreateDocument();
            yield return null;
            yield return null;
            Assert.IsNotNull(doc.rootVisualElement);
            AttachMainStyle(doc);

            var node = new SkillTreeNodeElement(0);
            node.SetState(SkillTreeNodeVisualState.Max);
            doc.rootVisualElement.Add(node);

            yield return new WaitForSeconds(RaysObservationSeconds);

            var rays = node.Q(className: "skill-tree-node__rays");
            float angle = rays.transform.rotation.eulerAngles.z;

            Assert.Greater(angle, 5f,
                "After ~2s (~40 ticks at 0.3 deg/tick = 12 deg), rays rotation must exceed 5 degrees.");
        }

        [UnityTest]
        public IEnumerator AvailableNode_RaysDoNotRotate()
        {
            var doc = CreateDocument();
            yield return null;
            yield return null;
            Assert.IsNotNull(doc.rootVisualElement);
            AttachMainStyle(doc);

            var node = new SkillTreeNodeElement(0);
            node.SetState(SkillTreeNodeVisualState.Available);
            doc.rootVisualElement.Add(node);

            yield return new WaitForSeconds(RaysObservationSeconds);

            var rays = node.Q(className: "skill-tree-node__rays");
            float angle = rays.transform.rotation.eulerAngles.z;

            Assert.Less(angle, 0.5f,
                "An available node's rays must not rotate (angle must remain at 0).");
        }

        [UnityTest]
        public IEnumerator Max_ThenAvailable_RaysRotationResets()
        {
            var doc = CreateDocument();
            yield return null;
            yield return null;
            Assert.IsNotNull(doc.rootVisualElement);
            AttachMainStyle(doc);

            var node = new SkillTreeNodeElement(0);
            node.SetState(SkillTreeNodeVisualState.Max);
            doc.rootVisualElement.Add(node);

            yield return new WaitForSeconds(RaysObservationSeconds);

            node.SetState(SkillTreeNodeVisualState.Available);

            var rays = node.Q(className: "skill-tree-node__rays");
            float angle = rays.transform.rotation.eulerAngles.z;

            Assert.AreEqual(0f, angle, 0.001f,
                "SetState(Available) after Max must immediately reset rays rotation to 0.");
        }

        [UnityTest]
        public IEnumerator MaxNode_RaysAngleAdvances_BetweenTwoObservations()
        {
            var doc = CreateDocument();
            yield return null;
            yield return null;
            Assert.IsNotNull(doc.rootVisualElement);
            AttachMainStyle(doc);

            var node = new SkillTreeNodeElement(0);
            doc.rootVisualElement.Add(node);
            node.SetState(SkillTreeNodeVisualState.Max);

            yield return new WaitForSeconds(0.6f);
            var rays = node.Q(className: "skill-tree-node__rays");
            var angleAt06 = rays.transform.rotation.eulerAngles.z;

            yield return new WaitForSeconds(1.2f);
            var angleAt18 = rays.transform.rotation.eulerAngles.z;

            Assert.Greater(angleAt18, angleAt06,
                $"Rays angle must advance over time. T+0.6s: {angleAt06}, T+1.8s: {angleAt18}.");
            Assert.Greater(angleAt18 - angleAt06, 2f,
                $"Rays angle must advance by at least 2° between 0.6s and 1.8s. Delta: {angleAt18 - angleAt06}");
        }
    }
}
#endif
