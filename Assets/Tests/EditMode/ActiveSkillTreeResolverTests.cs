using NUnit.Framework;
using RogueliteAutoBattler.Data;
using UnityEditor;
using UnityEngine;

namespace RogueliteAutoBattler.Tests.EditMode
{
    [TestFixture]
    public class ActiveSkillTreeResolverTests
    {
        private const string ResourcesFolder = "Assets/Resources";
        private const string PointerAssetPath = "Assets/Resources/ActiveSkillTree.asset";
        private const string TempFolder = "Assets/Tests/Temp";
        private const string TempSkillTreePath = "Assets/Tests/Temp/TempSkillTreeData.asset";

        [SetUp]
        public void SetUp()
        {
            CleanupAssets();
        }

        [TearDown]
        public void TearDown()
        {
            CleanupAssets();
        }

        [Test]
        public void GetActive_ReturnsNull_WhenNoPointerAssetExists()
        {
            Assert.IsFalse(AssetDatabase.LoadAssetAtPath<ActiveSkillTreePointer>(PointerAssetPath) != null,
                "Precondition failed: pointer asset should not exist before this test.");

            var result = ActiveSkillTreeResolver.GetActive();

            Assert.IsNull(result);
        }

        [Test]
        public void GetActive_ReturnsNull_WhenPointerTargetIsNull()
        {
            EnsureFolder(ResourcesFolder);
            var pointer = ScriptableObject.CreateInstance<ActiveSkillTreePointer>();
            try
            {
                AssetDatabase.CreateAsset(pointer, PointerAssetPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                var result = ActiveSkillTreeResolver.GetActive();

                Assert.IsNull(result);
            }
            finally
            {
                CleanupAssets();
            }
        }

        [Test]
        public void GetActive_ReturnsTargetAsset_WhenPointerWired()
        {
            EnsureFolder(ResourcesFolder);
            EnsureFolder(TempFolder);

            var skillTree = ScriptableObject.CreateInstance<SkillTreeData>();
            var pointer = ScriptableObject.CreateInstance<ActiveSkillTreePointer>();
            try
            {
                AssetDatabase.CreateAsset(skillTree, TempSkillTreePath);
                AssetDatabase.CreateAsset(pointer, PointerAssetPath);

                var serialized = new SerializedObject(pointer);
                serialized.FindProperty("target").objectReferenceValue = skillTree;
                serialized.ApplyModifiedPropertiesWithoutUndo();

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                var result = ActiveSkillTreeResolver.GetActive();

                Assert.IsNotNull(result);
                Assert.AreEqual(skillTree, result);
            }
            finally
            {
                CleanupAssets();
            }
        }

        private static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder)) return;
            int lastSlash = folder.LastIndexOf('/');
            string parent = folder.Substring(0, lastSlash);
            string leaf = folder.Substring(lastSlash + 1);
            if (!AssetDatabase.IsValidFolder(parent))
                EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, leaf);
        }

        private static void CleanupAssets()
        {
            var cached = Resources.Load<ActiveSkillTreePointer>(ActiveSkillTreeResolver.ResourceName);
            if (cached != null)
                Resources.UnloadAsset(cached);

            if (AssetDatabase.LoadAssetAtPath<Object>(PointerAssetPath) != null)
                AssetDatabase.DeleteAsset(PointerAssetPath);

            if (AssetDatabase.IsValidFolder(TempFolder))
                AssetDatabase.DeleteAsset(TempFolder);

            AssetDatabase.Refresh();
            Resources.UnloadUnusedAssets();
        }
    }
}
