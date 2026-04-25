#if UNITY_EDITOR
using NUnit.Framework;
using RogueliteAutoBattler.Data;
using RogueliteAutoBattler.Editor;
using RogueliteAutoBattler.UI.Toolkit;
using RogueliteAutoBattler.UI.Toolkit.SkillTree;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace RogueliteAutoBattler.Tests.EditMode
{
    [TestFixture]
    public class SkillTreeScreenBuilderTests
    {
        private const string SetupMenuItemPath = "Roguelite/Setup New Game Scene";

        [SetUp]
        public void SetUp()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        }

        [TearDown]
        public void TearDown()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        }

        [Test]
        public void AfterSetup_NavigationHostGO_HasSkillTreeScreenController()
        {
            bool executed = EditorApplication.ExecuteMenuItem(SetupMenuItemPath);
            Assert.IsTrue(executed, $"MenuItem '{SetupMenuItemPath}' must be registered and executable.");

            GameObject navigationHostGo = FindNavigationHostGameObject();
            SkillTreeScreenController controller = navigationHostGo.GetComponent<SkillTreeScreenController>();

            Assert.IsNotNull(controller,
                "NavigationHost GameObject must carry a SkillTreeScreenController after scene setup (#183).");
        }

        [Test]
        public void AfterSetup_Controller_HasAllSerializeFieldsAssigned()
        {
            EditorApplication.ExecuteMenuItem(SetupMenuItemPath);

            SkillTreeScreenController controller = FindSkillTreeScreenController();
            var serializedController = new SerializedObject(controller);

            AssertSerializedFieldAssigned(serializedController, "_uiDocument");
            AssertSerializedFieldAssigned(serializedController, "_data");
            AssertSerializedFieldAssigned(serializedController, "_progress");
            AssertSerializedFieldAssigned(serializedController, "_goldWallet");
            AssertSerializedFieldAssigned(serializedController, "_skillPointWallet");
        }

        [Test]
        public void AfterSetup_ControllerDataRef_PointsToDefaultAsset()
        {
            EditorApplication.ExecuteMenuItem(SetupMenuItemPath);

            SkillTreeScreenController controller = FindSkillTreeScreenController();
            var serializedController = new SerializedObject(controller);
            SerializedProperty dataProperty = serializedController.FindProperty("_data");

            Assert.IsNotNull(dataProperty, "SkillTreeScreenController must expose a serialized _data field.");
            Assert.IsNotNull(dataProperty.objectReferenceValue,
                "SkillTreeScreenController._data must be wired after setup.");

            string assetPath = AssetDatabase.GetAssetPath(dataProperty.objectReferenceValue);
            Assert.AreEqual(EditorPaths.SkillTreeDataAsset, assetPath,
                $"SkillTreeScreenController._data must reference the default asset at '{EditorPaths.SkillTreeDataAsset}'.");
        }

        [Test]
        public void AfterSetup_ControllerProgressRef_PointsToDefaultAsset()
        {
            EditorApplication.ExecuteMenuItem(SetupMenuItemPath);

            SkillTreeScreenController controller = FindSkillTreeScreenController();
            var serializedController = new SerializedObject(controller);
            SerializedProperty progressProperty = serializedController.FindProperty("_progress");

            Assert.IsNotNull(progressProperty, "SkillTreeScreenController must expose a serialized _progress field.");
            Assert.IsNotNull(progressProperty.objectReferenceValue,
                "SkillTreeScreenController._progress must be wired after setup.");

            string assetPath = AssetDatabase.GetAssetPath(progressProperty.objectReferenceValue);
            Assert.AreEqual(EditorPaths.SkillTreeProgressAsset, assetPath,
                $"SkillTreeScreenController._progress must reference the default asset at '{EditorPaths.SkillTreeProgressAsset}'.");
        }

        [Test]
        public void AfterSetup_Idempotent_NoDuplicateControllerComponent()
        {
            EditorApplication.ExecuteMenuItem(SetupMenuItemPath);
            EditorApplication.ExecuteMenuItem(SetupMenuItemPath);

            GameObject navigationHostGo = FindNavigationHostGameObject();
            SkillTreeScreenController[] controllers =
                navigationHostGo.GetComponents<SkillTreeScreenController>();

            Assert.AreEqual(1, controllers.Length,
                "Running scene setup twice must not duplicate SkillTreeScreenController on the NavigationHost GameObject.");
        }

        private static GameObject FindNavigationHostGameObject()
        {
            NavigationHost navigationHost =
                Object.FindFirstObjectByType<NavigationHost>(FindObjectsInactive.Include);
            Assert.IsNotNull(navigationHost,
                "NavigationHost must exist in the scene before searching its GameObject.");
            return navigationHost.gameObject;
        }

        private static SkillTreeScreenController FindSkillTreeScreenController()
        {
            GameObject navigationHostGo = FindNavigationHostGameObject();
            SkillTreeScreenController controller =
                navigationHostGo.GetComponent<SkillTreeScreenController>();
            Assert.IsNotNull(controller,
                "SkillTreeScreenController must exist on the NavigationHost GameObject before this assertion runs.");
            return controller;
        }

        private static void AssertSerializedFieldAssigned(SerializedObject serializedController, string fieldName)
        {
            SerializedProperty fieldProperty = serializedController.FindProperty(fieldName);
            Assert.IsNotNull(fieldProperty,
                $"SkillTreeScreenController must expose a serialized {fieldName} field.");
            Assert.IsNotNull(fieldProperty.objectReferenceValue,
                $"SkillTreeScreenController.{fieldName} must be wired (objectReferenceValue != null) after scene setup.");
        }
    }
}
#endif
