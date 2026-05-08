#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using RogueliteAutoBattler.Combat.Core;
using RogueliteAutoBattler.Data;
using RogueliteAutoBattler.Economy;
using RogueliteAutoBattler.UI.Toolkit.SkillTree;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace RogueliteAutoBattler.Tests.PlayMode
{
    public class SkillTreeScreenControllerColorTagTests : PlayModeTestBase
    {
        private const string PanelSettingsPath = "Assets/UI/MainPanelSettings.asset";
        private const string MainLayoutPath = "Assets/UI/Layouts/MainLayout.uxml";
        private const string UiDocumentFieldName = "_uiDocument";
        private const string DataFieldName = "_data";
        private const string ProgressFieldName = "_progress";
        private const string GoldWalletFieldName = "_goldWallet";
        private const string SkillPointWalletFieldName = "_skillPointWallet";

        private static void InjectPrivateField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(field, $"{target.GetType().Name} must expose private field {fieldName}.");
            field.SetValue(target, value);
        }

        private static SkillNodePalette CreateTwoEntryPalette()
        {
            SkillNodePalette palette = ScriptableObject.CreateInstance<SkillNodePalette>();
            SerializedObject serialized = new SerializedObject(palette);
            SerializedProperty entriesProperty = serialized.FindProperty(SkillNodePalette.FieldNames.Entries);
            Assert.IsNotNull(entriesProperty, $"SkillNodePalette must expose serialized field {SkillNodePalette.FieldNames.Entries}.");
            entriesProperty.arraySize = 2;
            SerializedProperty defaultEntry = entriesProperty.GetArrayElementAtIndex(0);
            defaultEntry.FindPropertyRelative("tag").enumValueIndex = (int)NodeColorTag.Default;
            defaultEntry.FindPropertyRelative("color").colorValue = Color.white;
            SerializedProperty redEntry = entriesProperty.GetArrayElementAtIndex(1);
            redEntry.FindPropertyRelative("tag").enumValueIndex = (int)NodeColorTag.Red;
            redEntry.FindPropertyRelative("color").colorValue = Color.red;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return palette;
        }

        private static SkillTreeData CreateTwoNodeDataWithColors()
        {
            SkillTreeData data = ScriptableObject.CreateInstance<SkillTreeData>();
            var entries = new List<SkillTreeData.SkillNodeEntry>
            {
                new SkillTreeData.SkillNodeEntry
                {
                    id = 0,
                    position = Vector2.zero,
                    connectedNodeIds = new List<int> { 1 },
                    costType = SkillTreeData.CostType.Gold,
                    maxLevel = 1,
                    baseCost = 1,
                    costMultiplierOdd = 1f,
                    costMultiplierEven = 1f,
                    costAdditivePerLevel = 0,
                    statModifierType = StatType.None,
                    statModifierMode = SkillTreeData.StatModifierMode.Flat,
                    statModifierValuePerLevel = 0f,
                    snapEnabled = true,
                    snapThresholdUnits = 0.25f,
                    colorTag = NodeColorTag.Default
                },
                new SkillTreeData.SkillNodeEntry
                {
                    id = 1,
                    position = new Vector2(2f, 0f),
                    connectedNodeIds = new List<int>(),
                    costType = SkillTreeData.CostType.Gold,
                    maxLevel = 1,
                    baseCost = 1,
                    costMultiplierOdd = 1f,
                    costMultiplierEven = 1f,
                    costAdditivePerLevel = 0,
                    statModifierType = StatType.None,
                    statModifierMode = SkillTreeData.StatModifierMode.Flat,
                    statModifierValuePerLevel = 0f,
                    snapEnabled = true,
                    snapThresholdUnits = 0.25f,
                    colorTag = NodeColorTag.Red
                }
            };
            data.InitializeForTest(entries);
            return data;
        }

        [UnityTest]
        public IEnumerator SpawnedNodes_ReceivePaletteColor()
        {
            PanelSettings panelSettings = AssetDatabase.LoadAssetAtPath<PanelSettings>(PanelSettingsPath);
            Assert.IsNotNull(panelSettings, $"MainPanelSettings must exist at {PanelSettingsPath}.");
            VisualTreeAsset visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(MainLayoutPath);
            Assert.IsNotNull(visualTree, $"MainLayout must exist at {MainLayoutPath}.");

            SkillNodePalette palette = CreateTwoEntryPalette();
            SkillTreeData data = CreateTwoNodeDataWithColors();
            SkillTreeProgress progress = ScriptableObject.CreateInstance<SkillTreeProgress>();
            GameObject goldWalletGo = Track(new GameObject("GoldWallet"));
            GoldWallet goldWallet = goldWalletGo.AddComponent<GoldWallet>();
            GameObject skillPointWalletGo = Track(new GameObject("SkillPointWallet"));
            SkillPointWallet skillPointWallet = skillPointWalletGo.AddComponent<SkillPointWallet>();

            GameObject controllerGo = Track(new GameObject("SkillTreeScreenController"));
            controllerGo.SetActive(false);
            UIDocument uiDocument = controllerGo.AddComponent<UIDocument>();
            uiDocument.panelSettings = panelSettings;
            uiDocument.visualTreeAsset = visualTree;
            SkillTreeScreenController controller = controllerGo.AddComponent<SkillTreeScreenController>();
            InjectPrivateField(controller, UiDocumentFieldName, uiDocument);
            InjectPrivateField(controller, DataFieldName, data);
            InjectPrivateField(controller, ProgressFieldName, progress);
            InjectPrivateField(controller, GoldWalletFieldName, goldWallet);
            InjectPrivateField(controller, SkillPointWalletFieldName, skillPointWallet);
            controller.Palette = palette;
            controllerGo.SetActive(true);

            yield return null;

            Assert.AreEqual(2, controller.NodeElements.Count, "Controller must spawn one element per data node.");
            SkillTreeNodeElement defaultNode = controller.NodeElements[0];
            SkillTreeNodeElement redNode = controller.NodeElements[1];
            Assert.AreEqual(0, defaultNode.NodeIndex, "First spawned element must map to data index 0 (central node).");
            Assert.AreEqual(1, redNode.NodeIndex, "Second spawned element must map to data index 1.");
            Assert.AreEqual(Color.white, defaultNode.CurrentColor,
                "Node with NodeColorTag.Default must receive Color.white from palette.");
            Assert.AreEqual(Color.red, redNode.CurrentColor,
                "Node with NodeColorTag.Red must receive Color.red from palette.");
        }
    }
}
#endif
