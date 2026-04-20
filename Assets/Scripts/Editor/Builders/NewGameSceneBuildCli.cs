using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace RogueliteAutoBattler.Editor.Builders
{
    public static class NewGameSceneBuildCli
    {
        // Invoked via:
        // TODO: Unity.exe -batchmode -projectPath ... -executeMethod RogueliteAutoBattler.Editor.Builders.NewGameSceneBuildCli.BuildAndSave -quit
        [MenuItem("Roguelite/Internal/Rebuild And Save NewGameScene")]
        public static void BuildAndSave()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            NewGameSceneBuilder.SaveSceneAfterSetup();
            Debug.Log("[NewGameSceneBuildCli] BuildAndSave complete.");
            if (Application.isBatchMode)
                EditorApplication.Exit(0);
        }
    }
}
