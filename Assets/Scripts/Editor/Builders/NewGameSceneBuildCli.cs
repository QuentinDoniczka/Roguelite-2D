using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace RogueliteAutoBattler.Editor
{
    public static class NewGameSceneBuildCli
    {
        [MenuItem("Roguelite/Internal/Rebuild And Save NewGameScene")]
        public static void BuildAndSave()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            NewGameSceneBuilder.SaveSceneAfterSetup();
            Debug.Log($"[{nameof(NewGameSceneBuildCli)}] BuildAndSave complete.");
            if (Application.isBatchMode)
                EditorApplication.Exit(0);
        }
    }
}
