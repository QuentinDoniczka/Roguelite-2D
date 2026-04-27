using RogueliteAutoBattler.Editor.Builders;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace RogueliteAutoBattler.Editor.Tools
{
    internal static class RebuildNewGameScene
    {
        private const string MenuItemPath = "Roguelite/Rebuild NewGameScene from LevelDatabase";
        private const string ScenePath = "Assets/Scenes/NewGameScene.unity";

        [MenuItem(MenuItemPath)]
        private static void RebuildScene()
        {
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            NewGameSceneBuilder.SaveSceneAfterSetup();
        }
    }
}
