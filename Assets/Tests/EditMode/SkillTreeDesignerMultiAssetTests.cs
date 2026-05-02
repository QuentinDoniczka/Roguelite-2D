using NUnit.Framework;
using RogueliteAutoBattler.Data;
using RogueliteAutoBattler.Editor.Tools;
using UnityEditor;
using UnityEngine;

namespace RogueliteAutoBattler.Tests.EditMode
{
    [TestFixture]
    public class SkillTreeDesignerMultiAssetTests
    {
        private const string TempRoot = "Assets/Tests/Temp";
        private const string TempTreesFolder = "Assets/Tests/Temp/SkillTrees";
        private const string TempPointerFolder = "Assets/Tests/Temp/Pointer";
        private const string TempPointerPath = "Assets/Tests/Temp/Pointer/TempActiveSkillTree.asset";
        private const string OutsideTreePath = "Assets/Tests/Temp/OutsideTree.asset";

        private const string TreeNameA = "ATree";
        private const string TreeNameM = "MTree";
        private const string TreeNameZ = "ZTree";

        [SetUp]
        public void SetUp()
        {
            Cleanup();
        }

        [TearDown]
        public void TearDown()
        {
            Cleanup();
        }

        [Test]
        public void Enumerate_ReturnsAssetsUnderFolder_SortedByName()
        {
            EditorAssetFolders.EnsureFolder(TempTreesFolder);
            CreateTreeAsset(TempTreesFolder, TreeNameZ);
            CreateTreeAsset(TempTreesFolder, TreeNameA);
            CreateTreeAsset(TempTreesFolder, TreeNameM);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            var entries = SkillTreesEnumerator.Enumerate(TempTreesFolder);

            Assert.AreEqual(3, entries.Count);
            Assert.AreEqual(TreeNameA, entries[0].DisplayName);
            Assert.AreEqual(TreeNameM, entries[1].DisplayName);
            Assert.AreEqual(TreeNameZ, entries[2].DisplayName);
        }

        [Test]
        public void Enumerate_ExcludesAssetsOutsideFolder()
        {
            EditorAssetFolders.EnsureFolder(TempTreesFolder);
            CreateTreeAsset(TempTreesFolder, TreeNameA);

            EditorAssetFolders.EnsureFolder(TempRoot);
            var outside = ScriptableObject.CreateInstance<SkillTreeData>();
            AssetDatabase.CreateAsset(outside, OutsideTreePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            var entries = SkillTreesEnumerator.Enumerate(TempTreesFolder);

            Assert.AreEqual(1, entries.Count);
            Assert.AreEqual(TreeNameA, entries[0].DisplayName);
        }

        [Test]
        public void Enumerate_ReturnsEmpty_WhenFolderInvalid()
        {
            var entries = SkillTreesEnumerator.Enumerate("Assets/DoesNotExist/Nope");

            Assert.AreEqual(0, entries.Count);
        }

        [Test]
        public void SetActivePointer_WritesTargetViaSerializedProperty()
        {
            EditorAssetFolders.EnsureFolder(TempPointerFolder);
            EditorAssetFolders.EnsureFolder(TempTreesFolder);

            var pointer = ScriptableObject.CreateInstance<ActiveSkillTreePointer>();
            AssetDatabase.CreateAsset(pointer, TempPointerPath);
            var tree = CreateTreeAsset(TempTreesFolder, TreeNameA);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            bool result = SkillTreesEnumerator.SetActivePointer(TempPointerPath, tree);

            Assert.IsTrue(result);
            var reloaded = AssetDatabase.LoadAssetAtPath<ActiveSkillTreePointer>(TempPointerPath);
            Assert.IsNotNull(reloaded);
            Assert.AreEqual(tree, reloaded.Target);
        }

        [Test]
        public void SetActivePointer_ReturnsFalse_WhenPointerMissing()
        {
            EditorAssetFolders.EnsureFolder(TempTreesFolder);
            var tree = CreateTreeAsset(TempTreesFolder, TreeNameA);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            bool result = SkillTreesEnumerator.SetActivePointer("Assets/Tests/Temp/Pointer/Missing.asset", tree);

            Assert.IsFalse(result);
        }

        [Test]
        public void SetActivePointer_ReturnsFalse_WhenTargetNull()
        {
            EditorAssetFolders.EnsureFolder(TempPointerFolder);
            var pointer = ScriptableObject.CreateInstance<ActiveSkillTreePointer>();
            AssetDatabase.CreateAsset(pointer, TempPointerPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            bool result = SkillTreesEnumerator.SetActivePointer(TempPointerPath, null);

            Assert.IsFalse(result);
        }

        private static SkillTreeData CreateTreeAsset(string folder, string assetName)
        {
            var asset = ScriptableObject.CreateInstance<SkillTreeData>();
            var path = $"{folder}/{assetName}.asset";
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static void Cleanup()
        {
            if (AssetDatabase.IsValidFolder(TempRoot))
                AssetDatabase.DeleteAsset(TempRoot);
            AssetDatabase.Refresh();
            Resources.UnloadUnusedAssets();
        }
    }
}
