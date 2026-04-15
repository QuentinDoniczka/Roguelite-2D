using RogueliteAutoBattler.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;

namespace RogueliteAutoBattler.Editor
{
    internal static class NewGameSceneBuilder
    {
        [MenuItem("Roguelite/Setup New Game Scene")]
        private static void SetupNewGameScene()
        {
            GameObject existingWorld = GameObject.Find(GameBootstrap.CombatWorldName);
            if (existingWorld != null)
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

            CombatWorldBuilder.ConfigureMainCamera();
            GameObject combatWorld = CombatWorldBuilder.CreateCombatWorld();

            GameObject esGo = SetupNavigationSceneEditor.CreateEventSystem();
            Undo.RegisterCreatedObjectUndo(esGo, "EventSystem");

            Undo.CollapseUndoOperations(undoGroup);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Selection.activeGameObject = combatWorld;
            Debug.Log("[SetupNewGameScene] Done. CombatWorld + EventSystem created. GameBootstrap auto-discovers refs at runtime.");
        }
    }
}
