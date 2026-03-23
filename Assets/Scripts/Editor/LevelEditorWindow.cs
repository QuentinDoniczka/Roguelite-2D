using RogueliteAutoBattler.Data;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace RogueliteAutoBattler.Editor
{
    /// <summary>
    /// 3-column EditorWindow for authoring LevelDatabase assets.
    /// Columns: Stages | Levels | Waves + Enemies (scrollable).
    /// All mutations go through SerializedObject / SerializedProperty for full Undo support.
    /// </summary>
    public class LevelEditorWindow : EditorWindow
    {
        // ─── Layout constants ────────────────────────────────────────────────
        private const float ColumnStagesWidth  = 150f;
        private const float ColumnLevelsWidth  = 150f;
        private const float RowHeight          = 24f;
        private const float SmallButtonWidth   = 24f;
        private const float AddButtonHeight    = 22f;
        private const float WaveHeaderHeight   = 26f;
        private const string DatabaseDefaultPath = "Assets/Data/LevelDatabase.asset";
        private const string DefaultEnemyPrefabPath = "Assets/Prefabs/Characters/sampleCharacterHuman.prefab";

        // ─── State ───────────────────────────────────────────────────────────
        private LevelDatabase     _database;
        private SerializedObject  _serializedDatabase;

        private int _selectedStageIndex = -1;
        private int _selectedLevelIndex = -1;

        private Vector2 _wavesScrollPos;

        // Foldout states keyed by the SerializedProperty path of each enemy element.
        private readonly Dictionary<string, bool> _foldouts = new Dictionary<string, bool>();

        // Cached GUIStyle for selected rows (avoid allocating every OnGUI).
        private GUIStyle _selectedRowStyle;
        private Texture2D _selectedRowTexture;

        // ─── MenuItem ────────────────────────────────────────────────────────

        [MenuItem("Roguelite/Level Editor")]
        private static void OpenWindow()
        {
            var window = GetWindow<LevelEditorWindow>("Level Editor");
            window.minSize = new Vector2(600, 400);
            window.Show();
        }

        // ─── Lifecycle ───────────────────────────────────────────────────────

        private void OnEnable()
        {
            TryAutoLoadDatabase();
        }

        private void OnDisable()
        {
            _serializedDatabase = null;
            _selectedRowStyle   = null;

            if (_selectedRowTexture != null)
            {
                DestroyImmediate(_selectedRowTexture);
                _selectedRowTexture = null;
            }
        }

        // ─── Auto-load / create database ─────────────────────────────────────

        private void TryAutoLoadDatabase()
        {
            var db = AssetDatabase.LoadAssetAtPath<LevelDatabase>(DatabaseDefaultPath);
            if (db == null)
                db = CreateDatabase(DatabaseDefaultPath);

            SetDatabase(db);
        }

        private static LevelDatabase CreateDatabase(string path)
        {
            EditorUIFactory.EnsureDirectoryExists(path);

            var db = ScriptableObject.CreateInstance<LevelDatabase>();
            AssetDatabase.CreateAsset(db, path);
            AssetDatabase.SaveAssets();
            Debug.Log($"[LevelEditor] Created LevelDatabase at {path}");
            return db;
        }

        private void SetDatabase(LevelDatabase db)
        {
            _database           = db;
            _serializedDatabase = db != null ? new SerializedObject(db) : null;
            _selectedStageIndex = -1;
            _selectedLevelIndex = -1;
            _foldouts.Clear();
            Repaint();
        }

        // ─── OnGUI ───────────────────────────────────────────────────────────

        private void OnGUI()
        {
            DrawToolbar();

            if (_database == null)
            {
                EditorGUILayout.HelpBox("No LevelDatabase assigned. Use the field above.", MessageType.Info);
                return;
            }

            _serializedDatabase.Update();

            SerializedProperty stagesProp = _serializedDatabase.FindProperty("stages");

            GUILayout.BeginHorizontal();
            {
                DrawStagesPanel(stagesProp);
                DrawDivider();
                DrawLevelsPanel(stagesProp);
                DrawDivider();
                DrawWavesPanel(stagesProp);
            }
            GUILayout.EndHorizontal();

            _serializedDatabase.ApplyModifiedProperties();
        }

        // ─── Toolbar ─────────────────────────────────────────────────────────

        private void DrawToolbar()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            {
                GUILayout.Label("LevelDatabase:", GUILayout.Width(100));

                EditorGUI.BeginChangeCheck();
                var newDb = (LevelDatabase)EditorGUILayout.ObjectField(
                    _database, typeof(LevelDatabase), false, GUILayout.Width(220));
                if (EditorGUI.EndChangeCheck())
                    SetDatabase(newDb);

                GUILayout.FlexibleSpace();

                if (GUILayout.Button("New DB", EditorStyles.toolbarButton, GUILayout.Width(60)))
                {
                    string path = EditorUtility.SaveFilePanelInProject(
                        "Create LevelDatabase", "LevelDatabase", "asset", "Choose location");
                    if (!string.IsNullOrEmpty(path))
                        SetDatabase(CreateDatabase(path));
                }
            }
            GUILayout.EndHorizontal();
        }

        // ─── Stages panel ────────────────────────────────────────────────────

        private void DrawStagesPanel(SerializedProperty stagesProp)
        {
            GUILayout.BeginVertical(GUILayout.Width(ColumnStagesWidth));
            {
                DrawPanelHeader("Stages");

                if (stagesProp == null)
                {
                    Debug.LogError("[LevelEditor] Property 'stages' not found on LevelDatabase.");
                    GUILayout.EndVertical();
                    return;
                }

                // Add button
                if (GUILayout.Button("+ Add Stage", GUILayout.Height(AddButtonHeight)))
                {
                    stagesProp.arraySize++;
                    var newStage = stagesProp.GetArrayElementAtIndex(stagesProp.arraySize - 1);
                    newStage.FindPropertyRelative("stageName").stringValue = $"Stage {stagesProp.arraySize}";
                    var defaultTerrain = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Environment/grid_ground.png");
                    newStage.FindPropertyRelative("terrain").objectReferenceValue = defaultTerrain;
                    newStage.FindPropertyRelative("levels").arraySize = 0;
                    _selectedStageIndex = stagesProp.arraySize - 1;
                    _selectedLevelIndex = -1;
                    EditorUtility.SetDirty(_database);
                }

                // List
                for (int i = 0; i < stagesProp.arraySize; i++)
                {
                    var stageProp = stagesProp.GetArrayElementAtIndex(i);
                    string label = stageProp.FindPropertyRelative("stageName").stringValue;
                    if (string.IsNullOrEmpty(label)) label = $"Stage {i + 1}";

                    bool isSelected = (i == _selectedStageIndex);
                    GUIStyle style = isSelected ? GetSelectedRowStyle() : GUI.skin.button;

                    GUILayout.BeginHorizontal();
                    {
                        if (GUILayout.Button(label, style, GUILayout.Height(RowHeight)))
                        {
                            if (_selectedStageIndex != i)
                            {
                                _selectedStageIndex = i;
                                _selectedLevelIndex = -1;
                            }
                        }

                        if (GUILayout.Button("-", GUILayout.Width(SmallButtonWidth), GUILayout.Height(RowHeight)))
                        {
                            if (EditorUtility.DisplayDialog("Remove Stage",
                                    $"Remove '{label}'? This cannot be undone via the button.", "Remove", "Cancel"))
                            {
                                stagesProp.DeleteArrayElementAtIndex(i);
                                _selectedStageIndex = Mathf.Clamp(_selectedStageIndex, -1, stagesProp.arraySize - 1);
                                _selectedLevelIndex = -1;
                                EditorUtility.SetDirty(_database);
                                break;
                            }
                        }
                    }
                    GUILayout.EndHorizontal();
                }

                GUILayout.FlexibleSpace();

                // Inline rename + terrain for selected stage
                if (_selectedStageIndex >= 0 && _selectedStageIndex < stagesProp.arraySize)
                {
                    var stageElement = stagesProp.GetArrayElementAtIndex(_selectedStageIndex);

                    var nameProp    = stageElement.FindPropertyRelative("stageName");
                    var terrainProp = stageElement.FindPropertyRelative("terrain");

                    GUILayout.Label("Name:", EditorStyles.miniLabel);
                    nameProp.stringValue = EditorGUILayout.TextField(nameProp.stringValue);

                    GUILayout.Label("Terrain:", EditorStyles.miniLabel);
                    terrainProp.objectReferenceValue = EditorGUILayout.ObjectField(
                        terrainProp.objectReferenceValue, typeof(Sprite), false);
                }
            }
            GUILayout.EndVertical();
        }

        // ─── Levels panel ────────────────────────────────────────────────────

        private void DrawLevelsPanel(SerializedProperty stagesProp)
        {
            GUILayout.BeginVertical(GUILayout.Width(ColumnLevelsWidth));
            {
                DrawPanelHeader("Levels");

                if (stagesProp == null || _selectedStageIndex < 0 || _selectedStageIndex >= stagesProp.arraySize)
                {
                    EditorGUILayout.HelpBox("Select a stage.", MessageType.None);
                    GUILayout.EndVertical();
                    return;
                }

                SerializedProperty levelsProp = stagesProp
                    .GetArrayElementAtIndex(_selectedStageIndex)
                    .FindPropertyRelative("levels");

                if (levelsProp == null)
                {
                    Debug.LogError("[LevelEditor] Property 'levels' not found on StageData.");
                    GUILayout.EndVertical();
                    return;
                }

                // Add button
                if (GUILayout.Button("+ Add Level", GUILayout.Height(AddButtonHeight)))
                {
                    levelsProp.arraySize++;
                    var newLevel = levelsProp.GetArrayElementAtIndex(levelsProp.arraySize - 1);
                    newLevel.FindPropertyRelative("levelName").stringValue = $"Level {levelsProp.arraySize}";
                    newLevel.FindPropertyRelative("waves").arraySize = 0;
                    _selectedLevelIndex = levelsProp.arraySize - 1;
                    EditorUtility.SetDirty(_database);
                }

                // List
                for (int i = 0; i < levelsProp.arraySize; i++)
                {
                    var levelProp = levelsProp.GetArrayElementAtIndex(i);
                    string label = levelProp.FindPropertyRelative("levelName").stringValue;
                    if (string.IsNullOrEmpty(label)) label = $"Level {i + 1}";

                    bool isSelected = (i == _selectedLevelIndex);
                    GUIStyle style = isSelected ? GetSelectedRowStyle() : GUI.skin.button;

                    GUILayout.BeginHorizontal();
                    {
                        if (GUILayout.Button(label, style, GUILayout.Height(RowHeight)))
                            _selectedLevelIndex = i;

                        if (GUILayout.Button("-", GUILayout.Width(SmallButtonWidth), GUILayout.Height(RowHeight)))
                        {
                            if (EditorUtility.DisplayDialog("Remove Level",
                                    $"Remove '{label}'?", "Remove", "Cancel"))
                            {
                                levelsProp.DeleteArrayElementAtIndex(i);
                                _selectedLevelIndex = Mathf.Clamp(_selectedLevelIndex, -1, levelsProp.arraySize - 1);
                                EditorUtility.SetDirty(_database);
                                break;
                            }
                        }
                    }
                    GUILayout.EndHorizontal();
                }

                GUILayout.FlexibleSpace();

                // Inline rename for selected level
                if (_selectedLevelIndex >= 0 && _selectedLevelIndex < levelsProp.arraySize)
                {
                    var nameProp = levelsProp
                        .GetArrayElementAtIndex(_selectedLevelIndex)
                        .FindPropertyRelative("levelName");
                    GUILayout.Label("Name:", EditorStyles.miniLabel);
                    nameProp.stringValue = EditorGUILayout.TextField(nameProp.stringValue);
                }
            }
            GUILayout.EndVertical();
        }

        // ─── Waves panel ─────────────────────────────────────────────────────

        private void DrawWavesPanel(SerializedProperty stagesProp)
        {
            GUILayout.BeginVertical();
            {
                DrawPanelHeader("Waves & Enemies");

                if (stagesProp == null
                    || _selectedStageIndex < 0 || _selectedStageIndex >= stagesProp.arraySize)
                {
                    EditorGUILayout.HelpBox("Select a stage.", MessageType.None);
                    GUILayout.EndVertical();
                    return;
                }

                SerializedProperty levelsProp = stagesProp
                    .GetArrayElementAtIndex(_selectedStageIndex)
                    .FindPropertyRelative("levels");

                if (levelsProp == null
                    || _selectedLevelIndex < 0 || _selectedLevelIndex >= levelsProp.arraySize)
                {
                    EditorGUILayout.HelpBox("Select a level.", MessageType.None);
                    GUILayout.EndVertical();
                    return;
                }

                SerializedProperty wavesProp = levelsProp
                    .GetArrayElementAtIndex(_selectedLevelIndex)
                    .FindPropertyRelative("waves");

                if (wavesProp == null)
                {
                    Debug.LogError("[LevelEditor] Property 'waves' not found on LevelData.");
                    GUILayout.EndVertical();
                    return;
                }

                // Add wave button
                if (GUILayout.Button("+ Add Wave", GUILayout.Height(AddButtonHeight)))
                {
                    wavesProp.arraySize++;
                    var newWave = wavesProp.GetArrayElementAtIndex(wavesProp.arraySize - 1);
                    newWave.FindPropertyRelative("waveName").stringValue = $"Wave {wavesProp.arraySize}";
                    newWave.FindPropertyRelative("spawnDelay").floatValue = 0f;
                    newWave.FindPropertyRelative("enemies").arraySize    = 0;
                    EditorUtility.SetDirty(_database);
                }

                _wavesScrollPos = EditorGUILayout.BeginScrollView(_wavesScrollPos);
                {
                    bool waveListModified = false;
                    for (int wi = 0; wi < wavesProp.arraySize; wi++)
                    {
                        if (DrawWave(wavesProp, wi))
                        {
                            waveListModified = true;
                            break;
                        }
                        GUILayout.Space(4f);
                    }

                    if (!waveListModified && wavesProp.arraySize == 0)
                        EditorGUILayout.HelpBox("No waves. Add one above.", MessageType.None);
                }
                EditorGUILayout.EndScrollView();
            }
            GUILayout.EndVertical();
        }

        /// <summary>
        /// Draws a single wave block. Returns true if the wave list was structurally modified
        /// (caller must break the loop).
        /// </summary>
        private bool DrawWave(SerializedProperty wavesProp, int wi)
        {
            var waveProp    = wavesProp.GetArrayElementAtIndex(wi);
            var enemiesProp = waveProp.FindPropertyRelative("enemies");

            // ── Wave header ──────────────────────────────────────────────────
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox,
                GUILayout.Height(WaveHeaderHeight));

            GUILayout.Label($"Wave {wi + 1}", EditorStyles.boldLabel, GUILayout.Width(60));

            // Spawn delay — wave 1 is always 0 and non-editable
            var delayProp = waveProp.FindPropertyRelative("spawnDelay");
            bool isFirstWave = (wi == 0);
            if (isFirstWave)
                delayProp.floatValue = 0f;

            EditorGUI.BeginDisabledGroup(isFirstWave);
            GUILayout.Label("Delay:", GUILayout.Width(40));
            delayProp.floatValue = EditorGUILayout.FloatField(delayProp.floatValue, GUILayout.Width(50));
            GUILayout.Label("s", GUILayout.Width(12));
            EditorGUI.EndDisabledGroup();

            if (GUILayout.Button("-", GUILayout.Width(SmallButtonWidth), GUILayout.Height(WaveHeaderHeight - 4)))
            {
                if (EditorUtility.DisplayDialog("Remove Wave", $"Remove Wave {wi + 1}?", "Remove", "Cancel"))
                {
                    wavesProp.DeleteArrayElementAtIndex(wi);
                    EditorUtility.SetDirty(_database);
                    EditorGUILayout.EndHorizontal();
                    return true;
                }
            }

            EditorGUILayout.EndHorizontal();

            // ── Enemy list ───────────────────────────────────────────────────
            EditorGUI.indentLevel++;

            bool enemyListModified = false;
            for (int ei = 0; ei < enemiesProp.arraySize; ei++)
            {
                if (DrawEnemy(enemiesProp, wi, ei))
                {
                    enemyListModified = true;
                    break;
                }
            }

            if (!enemyListModified)
            {
                if (GUILayout.Button("+ Add Enemy", GUILayout.Height(AddButtonHeight)))
                {
                    enemiesProp.arraySize++;
                    var newEnemy = enemiesProp.GetArrayElementAtIndex(enemiesProp.arraySize - 1);
                    newEnemy.FindPropertyRelative("enemyName").stringValue   = "Enemy";
                    newEnemy.FindPropertyRelative("hp").intValue             = 50;
                    newEnemy.FindPropertyRelative("atk").intValue            = 10;
                    newEnemy.FindPropertyRelative("attackSpeed").floatValue  = 1f;
                    newEnemy.FindPropertyRelative("moveSpeed").floatValue    = 2f;
                    newEnemy.FindPropertyRelative("spawnOffset").vector2Value = Vector2.zero;

                    var defaultPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(DefaultEnemyPrefabPath);
                    newEnemy.FindPropertyRelative("prefab").objectReferenceValue = defaultPrefab;

                    EditorUtility.SetDirty(_database);
                }
            }

            EditorGUI.indentLevel--;
            return false;
        }

        /// <summary>
        /// Draws a single enemy foldout inside a wave. Returns true if the enemy list was modified.
        /// </summary>
        private bool DrawEnemy(SerializedProperty enemiesProp, int wi, int ei)
        {
            var enemyProp     = enemiesProp.GetArrayElementAtIndex(ei);
            var enemyNameProp = enemyProp.FindPropertyRelative("enemyName");
            var prefabProp    = enemyProp.FindPropertyRelative("prefab");

            string foldoutKey = enemyProp.propertyPath;
            if (!_foldouts.ContainsKey(foldoutKey))
                _foldouts[foldoutKey] = false;

            string foldoutLabel = string.IsNullOrEmpty(enemyNameProp.stringValue)
                ? $"Enemy {ei + 1}"
                : enemyNameProp.stringValue;

            GUILayout.BeginHorizontal();
            {
                _foldouts[foldoutKey] = EditorGUILayout.Foldout(
                    _foldouts[foldoutKey], foldoutLabel, true);

                if (GUILayout.Button("-", GUILayout.Width(SmallButtonWidth), GUILayout.Height(RowHeight - 2)))
                {
                    if (EditorUtility.DisplayDialog("Remove Enemy",
                            $"Remove '{foldoutLabel}' from Wave {wi + 1}?", "Remove", "Cancel"))
                    {
                        enemiesProp.DeleteArrayElementAtIndex(ei);
                        _foldouts.Remove(foldoutKey);
                        EditorUtility.SetDirty(_database);
                        GUILayout.EndHorizontal();
                        return true;
                    }
                }
            }
            GUILayout.EndHorizontal();

            if (!_foldouts[foldoutKey])
                return false;

            EditorGUI.indentLevel++;

            enemyNameProp.stringValue = EditorGUILayout.TextField("Name", enemyNameProp.stringValue);

            prefabProp.objectReferenceValue = EditorGUILayout.ObjectField(
                "Prefab", prefabProp.objectReferenceValue, typeof(GameObject), false);

            var hpProp              = enemyProp.FindPropertyRelative("hp");
            var atkProp             = enemyProp.FindPropertyRelative("atk");
            var attackSpeedProp     = enemyProp.FindPropertyRelative("attackSpeed");
            var moveSpeedProp       = enemyProp.FindPropertyRelative("moveSpeed");
            var spawnOffsetProp     = enemyProp.FindPropertyRelative("spawnOffset");

            if (hpProp != null)
                hpProp.intValue = EditorGUILayout.IntField("HP", hpProp.intValue);
            else
                Debug.LogError("[LevelEditor] Property 'hp' not found on EnemySpawnData.");

            if (atkProp != null)
                atkProp.intValue = EditorGUILayout.IntField("ATK", atkProp.intValue);
            else
                Debug.LogError("[LevelEditor] Property 'atk' not found on EnemySpawnData.");

            if (attackSpeedProp != null)
                attackSpeedProp.floatValue = EditorGUILayout.FloatField("Attack Speed", attackSpeedProp.floatValue);
            else
                Debug.LogError("[LevelEditor] Property 'attackSpeed' not found on EnemySpawnData.");

            if (moveSpeedProp != null)
                moveSpeedProp.floatValue = EditorGUILayout.FloatField("Move Speed", moveSpeedProp.floatValue);
            else
                Debug.LogError("[LevelEditor] Property 'moveSpeed' not found on EnemySpawnData.");

            if (spawnOffsetProp != null)
                spawnOffsetProp.vector2Value = EditorGUILayout.Vector2Field("Spawn Offset", spawnOffsetProp.vector2Value);
            else
                Debug.LogError("[LevelEditor] Property 'spawnOffset' not found on EnemySpawnData.");

            EditorGUI.indentLevel--;
            GUILayout.Space(2f);

            return false;
        }

        // ─── Helpers ─────────────────────────────────────────────────────────

        private static void DrawPanelHeader(string title)
        {
            GUILayout.Label(title, EditorStyles.boldLabel);
            var rect = GUILayoutUtility.GetRect(1, 1, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(rect, new Color(0.4f, 0.4f, 0.4f, 1f));
            GUILayout.Space(4f);
        }

        private static void DrawDivider()
        {
            var rect = GUILayoutUtility.GetRect(1f, 1f, GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(true));
            EditorGUI.DrawRect(rect, new Color(0.3f, 0.3f, 0.3f, 1f));
        }

        private GUIStyle GetSelectedRowStyle()
        {
            if (_selectedRowStyle != null)
                return _selectedRowStyle;

            if (_selectedRowTexture == null)
            {
                _selectedRowTexture = new Texture2D(1, 1);
                _selectedRowTexture.SetPixel(0, 0, new Color(0.24f, 0.49f, 0.91f, 1f));
                _selectedRowTexture.Apply();
            }

            _selectedRowStyle = new GUIStyle(GUI.skin.button);
            _selectedRowStyle.normal.background = _selectedRowTexture;
            _selectedRowStyle.normal.textColor  = Color.white;
            _selectedRowStyle.hover.background  = _selectedRowTexture;
            _selectedRowStyle.hover.textColor   = Color.white;
            return _selectedRowStyle;
        }
    }
}
