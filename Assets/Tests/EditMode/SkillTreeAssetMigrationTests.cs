using NUnit.Framework;
using RogueliteAutoBattler.Data;
using RogueliteAutoBattler.Editor.Tools;
using UnityEditor;
using UnityEngine;

namespace RogueliteAutoBattler.Tests.EditMode
{
    [TestFixture]
    public class SkillTreeAssetMigrationTests
    {
        private const string TempLegacyPath = "Assets/Tests/Temp/LegacySkillTreeData.asset";
        private const string TempMigratedPath = "Assets/Tests/Temp/SkillTrees/Default.asset";
        private const string TempPointerPath = "Assets/Tests/Temp/Resources/ActiveSkillTree.asset";
        private const string TempTreesFolder = "Assets/Tests/Temp/SkillTrees";
        private const string TempRootFolder = "Assets/Tests/Temp";

        [TearDown]
        public void TearDown()
        {
            if (AssetDatabase.IsValidFolder(TempRootFolder))
                AssetDatabase.DeleteAsset(TempRootFolder);
        }

        [Test]
        public void RunForTest_MovesLegacyAssetAndWiresPointer_WhenLegacyExists()
        {
            AssetFolderUtils.EnsureFolder(TempRootFolder);
            var legacy = ScriptableObject.CreateInstance<SkillTreeData>();
            AssetDatabase.CreateAsset(legacy, TempLegacyPath);

            SkillTreeAssetMigration.RunForTest(TempLegacyPath, TempMigratedPath, TempPointerPath, TempTreesFolder);

            Assert.IsNull(AssetDatabase.LoadAssetAtPath<SkillTreeData>(TempLegacyPath),
                "Legacy asset should no longer exist at original path.");
            Assert.IsNotNull(AssetDatabase.LoadAssetAtPath<SkillTreeData>(TempMigratedPath),
                "Migrated asset should exist at new path.");
            var pointer = AssetDatabase.LoadAssetAtPath<ActiveSkillTreePointer>(TempPointerPath);
            Assert.IsNotNull(pointer, "Pointer asset should exist.");
            Assert.IsNotNull(pointer.Target, "Pointer.Target should be wired.");
            Assert.AreEqual(AssetDatabase.GetAssetPath(pointer.Target), TempMigratedPath,
                "Pointer.Target should point to the migrated asset.");
        }

        [Test]
        public void RunForTest_IsIdempotent_WhenAlreadyMigrated()
        {
            AssetFolderUtils.EnsureFolder(TempRootFolder);
            AssetFolderUtils.EnsureFolder(TempTreesFolder);
            AssetFolderUtils.EnsureFolder("Assets/Tests/Temp/Resources");

            var data = ScriptableObject.CreateInstance<SkillTreeData>();
            AssetDatabase.CreateAsset(data, TempMigratedPath);

            var pointer = ScriptableObject.CreateInstance<ActiveSkillTreePointer>();
            AssetDatabase.CreateAsset(pointer, TempPointerPath);
            var so = new SerializedObject(pointer);
            so.FindProperty(ActiveSkillTreePointer.FieldNames.Target).objectReferenceValue = data;
            so.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssets();

            var guidBefore = AssetDatabase.AssetPathToGUID(TempPointerPath);

            SkillTreeAssetMigration.RunForTest(TempLegacyPath, TempMigratedPath, TempPointerPath, TempTreesFolder);
            SkillTreeAssetMigration.RunForTest(TempLegacyPath, TempMigratedPath, TempPointerPath, TempTreesFolder);

            var guidAfter = AssetDatabase.AssetPathToGUID(TempPointerPath);
            Assert.AreEqual(guidBefore, guidAfter, "Pointer asset GUID should not change after repeated runs.");
            var reloaded = AssetDatabase.LoadAssetAtPath<ActiveSkillTreePointer>(TempPointerPath);
            Assert.IsNotNull(reloaded.Target, "Pointer.Target should still be wired after idempotent run.");
        }

        [Test]
        public void RunForTest_CreatesEmptyAsset_WhenNoLegacy()
        {
            AssetFolderUtils.EnsureFolder(TempRootFolder);

            SkillTreeAssetMigration.RunForTest(TempLegacyPath, TempMigratedPath, TempPointerPath, TempTreesFolder);

            var created = AssetDatabase.LoadAssetAtPath<SkillTreeData>(TempMigratedPath);
            Assert.IsNotNull(created, "An empty SkillTreeData should be created when no legacy asset exists.");
            var pointer = AssetDatabase.LoadAssetAtPath<ActiveSkillTreePointer>(TempPointerPath);
            Assert.IsNotNull(pointer, "Pointer asset should be created.");
            Assert.IsNotNull(pointer.Target, "Pointer.Target should be wired to the empty asset.");
        }

    }
}
