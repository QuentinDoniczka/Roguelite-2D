using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace RogueliteAutoBattler.Editor
{
    internal static class BootstrapSceneBuilder
    {
        [MenuItem("Roguelite/Setup Game Scene")]
        private static void SetupGameScene()
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

            SetupNavigationSceneEditor.DestroyExistingSceneContent(existingCanvas);

            SetupNavigationSceneEditor.BuildSceneContent(out Canvas canvas, out _,
                out _, out _);

            Undo.CollapseUndoOperations(undoGroup);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Selection.activeGameObject = canvas.gameObject;
            Debug.Log("[SetupGameScene] Done. GameBootstrap auto-discovers refs at runtime via [RuntimeInitializeOnLoadMethod].");
        }
    }
}
