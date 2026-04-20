using RogueliteAutoBattler.Core;
using RogueliteAutoBattler.UI.Toolkit;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace RogueliteAutoBattler.Editor
{
    internal static class NewGameSceneBuilder
    {
        [MenuItem("Roguelite/Setup New Game Scene")]
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
            Undo.SetCurrentGroupName("Setup New Game Scene");

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

            var esGo = new GameObject("EventSystem");
            esGo.AddComponent<EventSystem>();
            esGo.AddComponent<InputSystemUIInputModule>();
            Undo.RegisterCreatedObjectUndo(esGo, "EventSystem");

            GameObject navHostGo = NavigationHostBuilder.CreateNavigationHost();
            Undo.RegisterCreatedObjectUndo(navHostGo, "NavigationHost");
            CombatHudBuilder.SetupToolkitCombatHud(navHostGo);

            Undo.CollapseUndoOperations(undoGroup);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Selection.activeGameObject = combatWorld;
            Debug.Log("[SetupNewGameScene] Done. CombatWorld + EventSystem + NavigationHost created.");
        }
    }
}
