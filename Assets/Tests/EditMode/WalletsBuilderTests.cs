using NUnit.Framework;
using RogueliteAutoBattler.Economy;
using RogueliteAutoBattler.Editor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace RogueliteAutoBattler.Tests.EditMode
{
    [TestFixture]
    public class WalletsBuilderTests
    {
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
        public void FindOrCreateGoldWallet_CreatesOneWhenAbsent()
        {
            GoldWallet wallet = WalletsBuilder.FindOrCreateGoldWallet();

            Assert.IsNotNull(wallet, "FindOrCreateGoldWallet must never return null.");
            GoldWallet[] walletsInScene = Object.FindObjectsByType<GoldWallet>(FindObjectsSortMode.None);
            Assert.AreEqual(1, walletsInScene.Length,
                "Exactly one GoldWallet must exist in the scene after the first call.");
        }

        [Test]
        public void FindOrCreateGoldWallet_ReturnsExisting_DoesNotDuplicate()
        {
            GoldWallet first = WalletsBuilder.FindOrCreateGoldWallet();
            GoldWallet second = WalletsBuilder.FindOrCreateGoldWallet();

            GoldWallet[] walletsInScene = Object.FindObjectsByType<GoldWallet>(FindObjectsSortMode.None);
            Assert.AreEqual(1, walletsInScene.Length,
                "Calling FindOrCreateGoldWallet twice must not duplicate the wallet.");
            Assert.AreSame(first, second,
                "The second call must return the same GoldWallet instance as the first.");
        }

        [Test]
        public void FindOrCreateSkillPointWallet_CreatesOneWhenAbsent()
        {
            SkillPointWallet wallet = WalletsBuilder.FindOrCreateSkillPointWallet();

            Assert.IsNotNull(wallet, "FindOrCreateSkillPointWallet must never return null.");
            SkillPointWallet[] walletsInScene = Object.FindObjectsByType<SkillPointWallet>(FindObjectsSortMode.None);
            Assert.AreEqual(1, walletsInScene.Length,
                "Exactly one SkillPointWallet must exist in the scene after the first call.");
        }

        [Test]
        public void FindOrCreateSkillPointWallet_ReturnsExisting_DoesNotDuplicate()
        {
            SkillPointWallet first = WalletsBuilder.FindOrCreateSkillPointWallet();
            SkillPointWallet second = WalletsBuilder.FindOrCreateSkillPointWallet();

            SkillPointWallet[] walletsInScene = Object.FindObjectsByType<SkillPointWallet>(FindObjectsSortMode.None);
            Assert.AreEqual(1, walletsInScene.Length,
                "Calling FindOrCreateSkillPointWallet twice must not duplicate the wallet.");
            Assert.AreSame(first, second,
                "The second call must return the same SkillPointWallet instance as the first.");
        }
    }
}
