using RogueliteAutoBattler.Core;
using RogueliteAutoBattler.UI.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace RogueliteAutoBattler.Editor
{
    public static class BootstrapSceneBuilder
    {
        [MenuItem("Roguelite/Setup Game Scene")]
        public static void SetupGameScene()
        {
            Canvas existingCanvas = Object.FindFirstObjectByType<Canvas>(FindObjectsInactive.Include);
            if (existingCanvas != null)
            {
                if (!EditorUtility.DisplayDialog("Canvas Exists", "Replace existing Canvas?", "Replace", "Cancel"))
                    return;
            }

            Undo.IncrementCurrentGroup();
            int undoGroup = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Setup Game Scene");

            GameBootstrap existingBootstrap = Object.FindFirstObjectByType<GameBootstrap>(FindObjectsInactive.Include);
            if (existingBootstrap != null)
                Undo.DestroyObjectImmediate(existingBootstrap.gameObject);

            SetupNavigationSceneEditor.DestroyExistingSceneContent(existingCanvas);

            SetupNavigationSceneEditor.BuildSceneContent(out Canvas canvas, out Transform combatWorld,
                out NavigationManager navigationManager, out Camera mainCamera);

            var bootstrapGo = new GameObject("GameBootstrap");
            var bootstrap = bootstrapGo.AddComponent<GameBootstrap>();
            var bootstrapSo = new SerializedObject(bootstrap);
            EditorUIFactory.SetObj(bootstrapSo, "_canvas", canvas);
            EditorUIFactory.SetObj(bootstrapSo, "_combatWorld", combatWorld);
            EditorUIFactory.SetObj(bootstrapSo, "_navigationManager", navigationManager);
            EditorUIFactory.SetObj(bootstrapSo, "_mainCamera", mainCamera);
            bootstrapSo.ApplyModifiedProperties();
            Undo.RegisterCreatedObjectUndo(bootstrapGo, "GameBootstrap");

            Undo.CollapseUndoOperations(undoGroup);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Selection.activeGameObject = canvas.gameObject;
            Debug.Log("[SetupGameScene] Done. GameBootstrap wired with Canvas, CombatWorld, NavigationManager, MainCamera.");
        }
    }
}
