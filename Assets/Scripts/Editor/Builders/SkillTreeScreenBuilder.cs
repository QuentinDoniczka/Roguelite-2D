using RogueliteAutoBattler.Data;
using RogueliteAutoBattler.Economy;
using RogueliteAutoBattler.UI.Toolkit.SkillTree;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RogueliteAutoBattler.Editor.Builders
{
    internal static class SkillTreeScreenBuilder
    {
        internal static void SetupSkillTreeScreen(GameObject navigationHostGo,
                                                   GoldWallet goldWallet,
                                                   SkillPointWallet skillPointWallet)
        {
            SkillTreeScreenController existing =
                navigationHostGo.GetComponent<SkillTreeScreenController>();
            if (existing != null)
                Object.DestroyImmediate(existing);

            UIDocument uiDocument = navigationHostGo.GetComponent<UIDocument>();

            SkillTreeData data =
                AssetDatabase.LoadAssetAtPath<SkillTreeData>(EditorPaths.SkillTreeDataAsset);
            SkillTreeProgress progress = EnsureSkillTreeProgressAsset();

            SkillTreeScreenController controller =
                navigationHostGo.AddComponent<SkillTreeScreenController>();

            var so = new SerializedObject(controller);
            EditorUIFactory.SetObj(so, "_uiDocument", uiDocument);
            EditorUIFactory.SetObj(so, "_data", data);
            EditorUIFactory.SetObj(so, "_progress", progress);
            EditorUIFactory.SetObj(so, "_goldWallet", goldWallet);
            EditorUIFactory.SetObj(so, "_skillPointWallet", skillPointWallet);
            so.ApplyModifiedProperties();
        }

        private static SkillTreeProgress EnsureSkillTreeProgressAsset()
        {
            SkillTreeProgress existing =
                AssetDatabase.LoadAssetAtPath<SkillTreeProgress>(EditorPaths.SkillTreeProgressAsset);
            if (existing != null) return existing;

            EditorUIFactory.EnsureDirectoryExists(EditorPaths.SkillTreeProgressAsset);
            SkillTreeProgress asset = ScriptableObject.CreateInstance<SkillTreeProgress>();
            AssetDatabase.CreateAsset(asset, EditorPaths.SkillTreeProgressAsset);
            AssetDatabase.SaveAssets();
            return AssetDatabase.LoadAssetAtPath<SkillTreeProgress>(EditorPaths.SkillTreeProgressAsset);
        }
    }
}
