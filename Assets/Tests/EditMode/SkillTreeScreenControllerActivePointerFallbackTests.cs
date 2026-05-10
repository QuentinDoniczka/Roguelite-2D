#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using RogueliteAutoBattler.Combat.Core;
using RogueliteAutoBattler.Data;
using RogueliteAutoBattler.Economy;
using RogueliteAutoBattler.Tests.PlayMode;
using RogueliteAutoBattler.UI.Toolkit.SkillTree;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace RogueliteAutoBattler.Tests.EditMode
{
    public class SkillTreeScreenControllerActivePointerFallbackTests : PlayModeTestBase
    {
        private const string PanelSettingsPath = "Assets/UI/MainPanelSettings.asset";
        private const string MainLayoutPath = "Assets/UI/Layouts/MainLayout.uxml";
        private const string UiDocumentFieldName = "_uiDocument";
        private const string DataFieldName = "_data";
        private const string ProgressFieldName = "_progress";
        private const string GoldWalletFieldName = "_goldWallet";
        private const string SkillPointWalletFieldName = "_skillPointWallet";
        private const string PaletteFieldName = "_palette";

        private System.Func<SkillTreeData> _originalProvider;
        private SkillTreeData _stubData;

        [SetUp]
        public void SetUp()
        {
            _originalProvider = ActiveSkillTreeResolver.Provider;
        }

        public override void TearDown()
        {
            ActiveSkillTreeResolver.Provider = _originalProvider;
            if (_stubData != null) Object.DestroyImmediate(_stubData);
            _stubData = null;
            base.TearDown();
        }

        private static void InjectPrivateField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(field, $"{target.GetType().Name} must expose private field {fieldName}.");
            field.SetValue(target, value);
        }

        private static void InvokeAwakeViaReflection(SkillTreeScreenController controller)
        {
            MethodInfo awake = typeof(SkillTreeScreenController).GetMethod(
                "Awake", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(awake, "SkillTreeScreenController must declare a private Awake method.");
            awake.Invoke(controller, null);
        }

        private SkillTreeData CreateMinimalSkillTreeData()
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
            return data;
        }

        [UnityTest]
        public IEnumerator Awake_WhenDataIsNull_ResolvesFromActivePointer()
        {
            _stubData = CreateMinimalSkillTreeData();
            ActiveSkillTreeResolver.Provider = () => _stubData;

            var progress = ScriptableObject.CreateInstance<SkillTreeProgress>();
            var palette = ScriptableObject.CreateInstance<SkillNodePalette>();

            GameObject goldWalletGo = Track(new GameObject("GoldWallet"));
            GoldWallet goldWallet = goldWalletGo.AddComponent<GoldWallet>();
            goldWallet.Add(10000);

            GameObject skillPointWalletGo = Track(new GameObject("SkillPointWallet"));
            SkillPointWallet skillPointWallet = skillPointWalletGo.AddComponent<SkillPointWallet>();

            PanelSettings panelSettings = AssetDatabase.LoadAssetAtPath<PanelSettings>(PanelSettingsPath);
            Assert.IsNotNull(panelSettings, $"MainPanelSettings must exist at {PanelSettingsPath}.");
            VisualTreeAsset visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(MainLayoutPath);
            Assert.IsNotNull(visualTree, $"MainLayout must exist at {MainLayoutPath}.");

            GameObject controllerGo = Track(new GameObject("SkillTreeScreenController"));
            controllerGo.SetActive(false);

            UIDocument uiDocument = controllerGo.AddComponent<UIDocument>();
            uiDocument.panelSettings = panelSettings;
            uiDocument.visualTreeAsset = visualTree;

            SkillTreeScreenController controller = controllerGo.AddComponent<SkillTreeScreenController>();
            InjectPrivateField(controller, UiDocumentFieldName, uiDocument);
            InjectPrivateField(controller, DataFieldName, null);
            InjectPrivateField(controller, ProgressFieldName, progress);
            InjectPrivateField(controller, GoldWalletFieldName, goldWallet);
            InjectPrivateField(controller, SkillPointWalletFieldName, skillPointWallet);
            InjectPrivateField(controller, PaletteFieldName, palette);

            InvokeAwakeViaReflection(controller);
            yield return null;

            Assert.AreSame(_stubData, controller.Data,
                "Awake must fall back to ActiveSkillTreeResolver.GetActive() when _data is null (Bug #2 regression guard).");

            Object.DestroyImmediate(progress);
            Object.DestroyImmediate(palette);
        }

        [UnityTest]
        public IEnumerator Awake_WhenDataIsAssigned_DoesNotOverrideWithActivePointer()
        {
            _stubData = CreateMinimalSkillTreeData();
            var assignedData = CreateMinimalSkillTreeData();
            ActiveSkillTreeResolver.Provider = () => _stubData;

            var progress = ScriptableObject.CreateInstance<SkillTreeProgress>();
            var palette = ScriptableObject.CreateInstance<SkillNodePalette>();

            GameObject goldWalletGo = Track(new GameObject("GoldWallet"));
            GoldWallet goldWallet = goldWalletGo.AddComponent<GoldWallet>();
            goldWallet.Add(10000);

            GameObject skillPointWalletGo = Track(new GameObject("SkillPointWallet"));
            SkillPointWallet skillPointWallet = skillPointWalletGo.AddComponent<SkillPointWallet>();

            PanelSettings panelSettings = AssetDatabase.LoadAssetAtPath<PanelSettings>(PanelSettingsPath);
            VisualTreeAsset visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(MainLayoutPath);

            GameObject controllerGo = Track(new GameObject("SkillTreeScreenController"));
            controllerGo.SetActive(false);

            UIDocument uiDocument = controllerGo.AddComponent<UIDocument>();
            uiDocument.panelSettings = panelSettings;
            uiDocument.visualTreeAsset = visualTree;

            SkillTreeScreenController controller = controllerGo.AddComponent<SkillTreeScreenController>();
            InjectPrivateField(controller, UiDocumentFieldName, uiDocument);
            InjectPrivateField(controller, DataFieldName, assignedData);
            InjectPrivateField(controller, ProgressFieldName, progress);
            InjectPrivateField(controller, GoldWalletFieldName, goldWallet);
            InjectPrivateField(controller, SkillPointWalletFieldName, skillPointWallet);
            InjectPrivateField(controller, PaletteFieldName, palette);

            InvokeAwakeViaReflection(controller);
            yield return null;

            Assert.AreSame(assignedData, controller.Data,
                "Awake must NOT override an already-assigned _data with the ActivePointer fallback.");

            Object.DestroyImmediate(progress);
            Object.DestroyImmediate(palette);
            Object.DestroyImmediate(assignedData);
        }
    }
}
#endif
