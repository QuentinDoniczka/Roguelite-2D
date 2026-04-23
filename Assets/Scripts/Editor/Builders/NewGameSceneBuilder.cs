using RogueliteAutoBattler.Core;
using RogueliteAutoBattler.Economy;
using RogueliteAutoBattler.UI.Toolkit;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;

namespace RogueliteAutoBattler.Editor
{
    internal static class NewGameSceneBuilder
    {
        private const string SetupLogTag = "[SetupNewGameScene]";
        private const string SaveLogTag = "[SaveSceneAfterSetup]";
        private const string SetupMenuItemPath = "Roguelite/Setup New Game Scene";
        private const string UndoGroupName = "Setup New Game Scene";
        private const string EventSystemGameObjectName = "EventSystem";
        private const string NavigationHostUndoLabel = "NavigationHost";
        private const string DefaultScenePath = "Assets/Scenes/NewGameScene.unity";

        [MenuItem(SetupMenuItemPath)]
        private static void SetupNewGameScene()
        {
            GameObject existingWorld = GameObject.Find(GameBootstrap.CombatWorldName);
            if (existingWorld != null && !Application.isBatchMode)
            {
                if (!EditorUtility.DisplayDialog("CombatWorld Exists", "Replace existing CombatWorld?", "Replace", "Cancel"))
                    return;
            }

            Undo.IncrementCurrentGroup();
            int undoGroup = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName(UndoGroupName);

            if (existingWorld != null)
                Undo.DestroyObjectImmediate(existingWorld);

            EventSystem existingEs = Object.FindFirstObjectByType<EventSystem>(FindObjectsInactive.Include);
            if (existingEs != null)
                Undo.DestroyObjectImmediate(existingEs.gameObject);

            NavigationHost existingHost = Object.FindFirstObjectByType<NavigationHost>(FindObjectsInactive.Include);
            if (existingHost != null)
                Undo.DestroyObjectImmediate(existingHost.gameObject);

            CombatWorldBuilder.ConfigureMainCamera();
            GameObject combatWorld = CombatWorldBuilder.CreateCombatWorld();

            var esGo = new GameObject(EventSystemGameObjectName);
            esGo.AddComponent<EventSystem>();
            esGo.AddComponent<InputSystemUIInputModule>();
            Undo.RegisterCreatedObjectUndo(esGo, EventSystemGameObjectName);

            GameObject navHostGo = NavigationHostBuilder.CreateNavigationHost();
            Undo.RegisterCreatedObjectUndo(navHostGo, NavigationHostUndoLabel);
            GoldWallet goldWallet = WalletsBuilder.FindOrCreateGoldWallet();
            SkillPointWallet skillPointWallet = WalletsBuilder.FindOrCreateSkillPointWallet();
            CombatHudBuilder.SetupToolkitCombatHud(navHostGo, goldWallet, skillPointWallet);
            SkillTreeScreenBuilder.SetupSkillTreeScreen(navHostGo, goldWallet, skillPointWallet);

            Undo.CollapseUndoOperations(undoGroup);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Selection.activeGameObject = combatWorld;
            Debug.Log($"{SetupLogTag} Done. CombatWorld + EventSystem + NavigationHost created.");
        }

        public static void SaveSceneAfterSetup()
        {
            SetupNewGameScene();
            Scene activeScene = EditorSceneManager.GetActiveScene();
            string scenePath = activeScene.path;
            if (string.IsNullOrEmpty(scenePath))
                scenePath = DefaultScenePath;
            EditorSceneManager.SaveScene(activeScene, scenePath);
            Debug.Log($"{SaveLogTag} Scene saved to: {scenePath}");
        }
    }
}
