using UnityEditor;
using UnityEngine;

namespace RogueliteAutoBattler.Editor.Windows
{
    internal sealed class GameDesignerWindow : EditorWindow
    {
        private static readonly string[] TabNames = { "Party Builder", "Level Designer" };
        private int _selectedTab;

        private TeamBuilderTab _teamTab;
        private LevelDesignerTab _levelTab;

        [MenuItem("Roguelite/Game Designer")]
        private static void OpenWindow()
        {
            var window = GetWindow<GameDesignerWindow>("Game Designer");
            window.minSize = new Vector2(640, 420);
            window.Show();
        }

        private void OnEnable()
        {
            _teamTab = new TeamBuilderTab(this);
            _levelTab = new LevelDesignerTab(this);
            _teamTab.OnEnable();
            _levelTab.OnEnable();
        }

        private void OnDisable()
        {
            _teamTab.OnDisable();
            _levelTab.OnDisable();
        }

        private void OnGUI()
        {
            _selectedTab = GUILayout.Toolbar(_selectedTab, TabNames, EditorStyles.toolbarButton);
            GUILayout.Space(2f);

            switch (_selectedTab)
            {
                case 0: _teamTab.Draw();  break;
                case 1: _levelTab.Draw(); break;
            }
        }
    }
}
