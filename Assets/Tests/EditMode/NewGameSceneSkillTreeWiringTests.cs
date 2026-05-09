using NUnit.Framework;
using RogueliteAutoBattler.UI.Toolkit.SkillTree;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace RogueliteAutoBattler.Tests.EditMode
{
    [TestFixture]
    public class NewGameSceneSkillTreeWiringTests
    {
        private const string ScenePath = "Assets/Scenes/NewGameScene.unity";

        [Test]
        public void NewGameScene_HasSkillTreeScreenController_FullyWired()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            try
            {
                SkillTreeScreenController controller = null;
                foreach (var root in scene.GetRootGameObjects())
                {
                    controller = root.GetComponentInChildren<SkillTreeScreenController>(includeInactive: true);
                    if (controller != null) break;
                }

                Assert.IsNotNull(controller, $"SkillTreeScreenController must exist in {ScenePath}.");

                var so = new SerializedObject(controller);
                AssertSerializedFieldNotNull(so, "_uiDocument");
                AssertSerializedFieldNotNull(so, "_data");
                AssertSerializedFieldNotNull(so, "_progress");
                AssertSerializedFieldNotNull(so, "_goldWallet");
                AssertSerializedFieldNotNull(so, "_skillPointWallet");
            }
            finally
            {
                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            }
        }

        private static void AssertSerializedFieldNotNull(SerializedObject so, string fieldName)
        {
            var prop = so.FindProperty(fieldName);
            Assert.IsNotNull(prop, $"SerializedObject must expose field {fieldName}.");
            Assert.AreEqual(SerializedPropertyType.ObjectReference, prop.propertyType,
                $"{fieldName} must be an Object reference.");
            Assert.IsNotNull(prop.objectReferenceValue,
                $"{fieldName} on SkillTreeScreenController must be assigned in the scene.");
        }
    }
}
