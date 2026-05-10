#if UNITY_EDITOR
using System.Collections.Generic;
using NUnit.Framework;
using RogueliteAutoBattler.Combat.Core;
using RogueliteAutoBattler.Data;
using RogueliteAutoBattler.Editor.Tools;
using RogueliteAutoBattler.UI.Toolkit.SkillTree;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace RogueliteAutoBattler.Tests.EditMode
{
    [TestFixture]
    public class SkillTreesEnumeratorActivePointerSyncTests
    {
        private const string TempFolder = "Assets/Tests/Temp";
        private const string PointerAssetPath = "Assets/Tests/Temp/PointerForSyncTest.asset";
        private const string DataAPath = "Assets/Tests/Temp/SkillTreeDataA.asset";
        private const string DataBPath = "Assets/Tests/Temp/SkillTreeDataB.asset";
        private const string TempScenePath = "Assets/Tests/Temp/PointerSyncTestScene.unity";

        private Scene _tempScene;
        private bool _tempSceneCreated;

        [SetUp]
        public void SetUp()
        {
            EditorAssetFolders.EnsureFolder(TempFolder);
            CleanupAssets();
        }

        [TearDown]
        public void TearDown()
        {
            if (_tempSceneCreated && _tempScene.IsValid() && _tempScene.isLoaded)
            {
                EditorSceneManager.CloseScene(_tempScene, removeScene: true);
                _tempSceneCreated = false;
            }
            CleanupAssets();
            if (AssetDatabase.IsValidFolder(TempFolder))
                AssetDatabase.DeleteAsset(TempFolder);
            AssetDatabase.Refresh();
        }

        private static void CleanupAssets()
        {
            if (AssetDatabase.LoadAssetAtPath<Object>(PointerAssetPath) != null)
                AssetDatabase.DeleteAsset(PointerAssetPath);
            if (AssetDatabase.LoadAssetAtPath<Object>(DataAPath) != null)
                AssetDatabase.DeleteAsset(DataAPath);
            if (AssetDatabase.LoadAssetAtPath<Object>(DataBPath) != null)
                AssetDatabase.DeleteAsset(DataBPath);
            if (AssetDatabase.LoadAssetAtPath<Object>(TempScenePath) != null)
                AssetDatabase.DeleteAsset(TempScenePath);
        }

        private static SkillTreeData CreateAndPersistSkillTreeData(string assetPath)
        {
            var data = ScriptableObject.CreateInstance<SkillTreeData>();
            data.InitializeForTest(new List<SkillTreeData.SkillNodeEntry>
            {
                new SkillTreeData.SkillNodeEntry
                {
                    id = 0,
                    position = Vector2.zero,
                    connectedNodeIds = new List<int>(),
                    costType = SkillTreeData.CostType.Gold,
                    maxLevel = 1,
                    baseCost = 1,
                    costMultiplierOdd = 1f,
                    costMultiplierEven = 1f,
                    costAdditivePerLevel = 0,
                    statModifierType = StatType.Hp,
                    statModifierMode = SkillTreeData.StatModifierMode.Flat,
                    statModifierValuePerLevel = 1f
                }
            });
            AssetDatabase.CreateAsset(data, assetPath);
            return AssetDatabase.LoadAssetAtPath<SkillTreeData>(assetPath);
        }

        private static ActiveSkillTreePointer CreateAndPersistPointer(string assetPath, SkillTreeData initialTarget)
        {
            var pointer = ScriptableObject.CreateInstance<ActiveSkillTreePointer>();
            AssetDatabase.CreateAsset(pointer, assetPath);
            if (initialTarget != null)
            {
                var so = new SerializedObject(pointer);
                so.FindProperty(ActiveSkillTreePointer.FieldNames.Target).objectReferenceValue = initialTarget;
                so.ApplyModifiedPropertiesWithoutUndo();
                AssetDatabase.SaveAssets();
            }
            return AssetDatabase.LoadAssetAtPath<ActiveSkillTreePointer>(assetPath);
        }

        [Test]
        public void SetActivePointer_UpdatesOpenSceneControllerData()
        {
            _tempScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            _tempSceneCreated = true;

            var dataA = CreateAndPersistSkillTreeData(DataAPath);
            var dataB = CreateAndPersistSkillTreeData(DataBPath);
            var pointer = CreateAndPersistPointer(PointerAssetPath, dataA);
            Assert.IsNotNull(pointer, "Temp pointer asset must be created.");

            var controllerGo = new GameObject("SkillTreeScreenControllerForSyncTest");
            SceneManager.MoveGameObjectToScene(controllerGo, _tempScene);
            controllerGo.SetActive(false);
            controllerGo.AddComponent<UIDocument>();
            var controller = controllerGo.AddComponent<SkillTreeScreenController>();

            var so = new SerializedObject(controller);
            so.FindProperty(SkillTreeScreenController.FieldNames.Data).objectReferenceValue = dataA;
            so.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.SaveScene(_tempScene, TempScenePath);

            bool success = SkillTreesEnumerator.SetActivePointer(PointerAssetPath, dataB);
            Assert.IsTrue(success, "SetActivePointer must return true when given a valid pointer + target.");

            so.Update();
            var updatedReference = so.FindProperty(SkillTreeScreenController.FieldNames.Data).objectReferenceValue as SkillTreeData;
            Assert.AreSame(dataB, updatedReference,
                "SetActivePointer must rewrite _data on every open-scene SkillTreeScreenController to the new target (Bug #2 fix at the SetActivePointer layer).");

            Object.DestroyImmediate(controllerGo);
        }

        [Test]
        public void SetActivePointer_DoesNothing_WhenNoControllersOpen()
        {
            _tempScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            _tempSceneCreated = true;

            var dataB = CreateAndPersistSkillTreeData(DataBPath);
            var pointer = CreateAndPersistPointer(PointerAssetPath, initialTarget: null);
            Assert.IsNotNull(pointer, "Temp pointer asset must be created.");

            bool success = SkillTreesEnumerator.SetActivePointer(PointerAssetPath, dataB);

            Assert.IsTrue(success,
                "SetActivePointer must return true and not throw even when no SkillTreeScreenController exists in any open scene.");

            var reloadedPointer = AssetDatabase.LoadAssetAtPath<ActiveSkillTreePointer>(PointerAssetPath);
            Assert.AreSame(dataB, reloadedPointer.Target,
                "Pointer target must still be updated to the new data even when no controllers are present in open scenes.");
        }
    }
}
#endif
