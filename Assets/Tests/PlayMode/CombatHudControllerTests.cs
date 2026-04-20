#if UNITY_EDITOR
using System.Collections;
using NUnit.Framework;
using RogueliteAutoBattler.Economy;
using RogueliteAutoBattler.UI.Toolkit;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace RogueliteAutoBattler.Tests.PlayMode
{
    public class CombatHudControllerTests : PlayModeTestBase
    {
        private const string PanelSettingsPath = "Assets/UI/MainPanelSettings.asset";
        private const string MainLayoutPath = "Assets/UI/Layouts/MainLayout.uxml";
        private const string GoldLabelElementName = "gold-label";
        private const string UiDocumentFieldName = "_uiDocument";
        private const string GoldWalletFieldName = "_goldWallet";
        private const int AddedGoldAmount = 500;

        [UnityTest]
        public IEnumerator HudGoldLabel_UpdatesWhenWalletAddsGold()
        {
            PanelSettings panelSettings = AssetDatabase.LoadAssetAtPath<PanelSettings>(PanelSettingsPath);
            Assert.IsNotNull(panelSettings, $"MainPanelSettings must exist at {PanelSettingsPath}.");

            VisualTreeAsset visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(MainLayoutPath);
            Assert.IsNotNull(visualTree, $"MainLayout must exist at {MainLayoutPath}.");

            var walletGo = Track(new GameObject("GoldWallet"));
            GoldWallet wallet = walletGo.AddComponent<GoldWallet>();

            var hudGo = Track(new GameObject("CombatHud"));
            hudGo.SetActive(false);

            UIDocument uiDocument = hudGo.AddComponent<UIDocument>();
            uiDocument.panelSettings = panelSettings;
            uiDocument.visualTreeAsset = visualTree;

            CombatHudController hudController = hudGo.AddComponent<CombatHudController>();
            var hudSerialized = new SerializedObject(hudController);
            SerializedProperty uiDocumentProperty = hudSerialized.FindProperty(UiDocumentFieldName);
            Assert.IsNotNull(uiDocumentProperty, $"CombatHudController must expose a serialized {UiDocumentFieldName} field.");
            uiDocumentProperty.objectReferenceValue = uiDocument;

            SerializedProperty goldWalletProperty = hudSerialized.FindProperty(GoldWalletFieldName);
            Assert.IsNotNull(goldWalletProperty, $"CombatHudController must expose a serialized {GoldWalletFieldName} field.");
            goldWalletProperty.objectReferenceValue = wallet;

            hudSerialized.ApplyModifiedPropertiesWithoutUndo();

            hudGo.SetActive(true);

            yield return null;

            wallet.Add(AddedGoldAmount);

            yield return null;

            Label goldLabel = uiDocument.rootVisualElement.Q<Label>(GoldLabelElementName);
            Assert.IsNotNull(goldLabel, $"{GoldLabelElementName} must be present in MainLayout.uxml.");
            Assert.AreEqual(AddedGoldAmount.ToString(), goldLabel.text,
                "HUD gold label must reflect wallet total after Add (#215).");
        }
    }
}
#endif
