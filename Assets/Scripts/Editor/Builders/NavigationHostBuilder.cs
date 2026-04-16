using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using ToolkitNavigationHost = RogueliteAutoBattler.UI.Toolkit.NavigationHost;

namespace RogueliteAutoBattler.Editor
{
    internal static class NavigationHostBuilder
    {
        private const string PanelSettingsPath = "Assets/UI/MainPanelSettings.asset";
        private const string MainLayoutUxmlPath = "Assets/UI/Layouts/MainLayout.uxml";
        private const string InputAssetPath = "Assets/Settings/InputSystem_Actions.inputactions";

        internal static GameObject CreateNavigationHost()
        {
            PanelSettings panelSettings = LoadOrCreatePanelSettings();

            VisualTreeAsset mainLayout = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(MainLayoutUxmlPath);
            if (mainLayout == null)
                Debug.LogWarning($"[NavigationHostBuilder] UXML not found at {MainLayoutUxmlPath}.");

            var go = new GameObject("NavigationHost");

            UIDocument uiDocument = go.AddComponent<UIDocument>();
            uiDocument.panelSettings = panelSettings;
            uiDocument.visualTreeAsset = mainLayout;

            ToolkitNavigationHost navigationHost = go.AddComponent<ToolkitNavigationHost>();

            var so = new SerializedObject(navigationHost);
            EditorUIFactory.SetObj(so, "_uiDocument", uiDocument);
            so.ApplyModifiedProperties();

            WireCancelAction(navigationHost);

            return go;
        }

        private static PanelSettings LoadOrCreatePanelSettings()
        {
            PanelSettings existing = AssetDatabase.LoadAssetAtPath<PanelSettings>(PanelSettingsPath);
            if (existing != null)
                return existing;

            EditorUIFactory.EnsureDirectoryExists(PanelSettingsPath);

            PanelSettings panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
            panelSettings.scaleMode = PanelScaleMode.ScaleWithScreenSize;
            panelSettings.referenceResolution = new Vector2Int(1080, 1920);
            panelSettings.screenMatchMode = PanelScreenMatchMode.MatchWidthOrHeight;
            panelSettings.match = 0.5f;

            AssetDatabase.CreateAsset(panelSettings, PanelSettingsPath);
            AssetDatabase.SaveAssets();

            return panelSettings;
        }

        private static void WireCancelAction(ToolkitNavigationHost navigationHost)
        {
            InputActionReference cancelRef = FindActionRef(InputAssetPath, "UI", "Cancel");
            if (cancelRef == null)
            {
                Debug.LogWarning("[NavigationHostBuilder] UI/Cancel action not found.");
                return;
            }
            var so = new SerializedObject(navigationHost);
            EditorUIFactory.SetObj(so, "_cancelAction", cancelRef);
            so.ApplyModifiedProperties();
        }

        private static InputActionReference FindActionRef(string path, string map, string action)
        {
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
            foreach (Object asset in assets)
            {
                if (asset is InputActionReference r &&
                    r.action != null &&
                    r.action.actionMap != null &&
                    r.action.actionMap.name == map &&
                    r.action.name == action)
                {
                    return r;
                }
            }
            return null;
        }
    }
}
