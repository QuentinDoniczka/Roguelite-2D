using RogueliteAutoBattler.Data;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace RogueliteAutoBattler.Editor
{
    public class GameDesignerWindow : EditorWindow
    {
        private static readonly string[] TabNames = { "Party Builder", "Level Designer" };
        private int _selectedTab;

        private GUIStyle  _selectedRowStyle;
        private Texture2D _selectedRowTexture;

        [MenuItem("Roguelite/Game Designer")]
        private static void OpenWindow()
        {
            var window = GetWindow<GameDesignerWindow>("Game Designer");
            window.minSize = new Vector2(640, 420);
            window.Show();
        }

        private void OnEnable()
        {
            TeamTryAutoLoadDatabase();
            LevelTryAutoLoadDatabase();
        }

        private void OnDisable()
        {
            _teamSerializedDatabase = null;

            _levelSerializedDatabase = null;

            _selectedRowStyle = null;
            if (_selectedRowTexture != null)
            {
                DestroyImmediate(_selectedRowTexture);
                _selectedRowTexture = null;
            }
        }

        private void OnGUI()
        {
            _selectedTab = GUILayout.Toolbar(_selectedTab, TabNames, EditorStyles.toolbarButton);
            GUILayout.Space(2f);

            switch (_selectedTab)
            {
                case 0: DrawTeamTab();  break;
                case 1: DrawLevelTab(); break;
            }
        }

        #region Team Builder

        private const float TeamRowHeight        = 24f;
        private const float TeamSmallButtonWidth = 24f;
        private const float TeamAddButtonHeight  = 22f;
        private const string TeamDefaultAllyPrefabPath = "Assets/Prefabs/Characters/sampleCharacterHuman.prefab";
        private const string TeamDatabaseDefaultPath   = "Assets/Data/TeamDatabase.asset";

        private const string TeamDefaultAllyName      = "Warrior";
        private const int    TeamDefaultMaxHp          = 100;
        private const int    TeamDefaultAtk            = 10;
        private const float  TeamDefaultAttackSpeed    = 1f;
        private const float  TeamDefaultMoveSpeed      = 2f;
        private const float  TeamDefaultRegenHpPerSec  = 0f;

        private TeamDatabase   _teamDatabase;
        private SerializedObject _teamSerializedDatabase;
        private Vector2 _teamScrollPos;
        private readonly Dictionary<string, bool> _teamFoldouts = new Dictionary<string, bool>();

        private void TeamTryAutoLoadDatabase()
        {
            var db = AssetDatabase.LoadAssetAtPath<TeamDatabase>(TeamDatabaseDefaultPath);
            if (db == null)
                db = TeamCreateDatabase(TeamDatabaseDefaultPath);
            TeamSetDatabase(db);
        }

        private static TeamDatabase TeamCreateDatabase(string path)
        {
            EditorUIFactory.EnsureDirectoryExists(path);
            var db = ScriptableObject.CreateInstance<TeamDatabase>();
            AssetDatabase.CreateAsset(db, path);
            AssetDatabase.SaveAssets();
            Debug.Log($"[TeamBuilder] Created TeamDatabase at {path}");
            return db;
        }

        private void TeamSetDatabase(TeamDatabase db)
        {
            _teamDatabase           = db;
            _teamSerializedDatabase = db != null ? new SerializedObject(db) : null;
            _teamFoldouts.Clear();
            Repaint();
        }

        private void DrawTeamTab()
        {
            DrawTeamToolbar();

            if (_teamDatabase == null)
            {
                EditorGUILayout.HelpBox(
                    "No TeamDatabase assigned. Use the field above or click 'New DB'.",
                    MessageType.Info);
                return;
            }

            _teamSerializedDatabase.Update();

            var alliesProp = _teamSerializedDatabase.FindProperty("allies");
            if (alliesProp == null)
            {
                Debug.LogError("[TeamBuilder] Property 'allies' not found on TeamDatabase.");
                EditorGUILayout.HelpBox("Property 'allies' not found — check TeamDatabase.", MessageType.Error);
                return;
            }

            TeamDrawAddButton(alliesProp);
            GUILayout.Space(4f);

            _teamScrollPos = EditorGUILayout.BeginScrollView(_teamScrollPos);
            {
                bool listModified = false;
                for (int i = 0; i < alliesProp.arraySize; i++)
                {
                    if (TeamDrawAlly(alliesProp, i))
                    {
                        listModified = true;
                        break;
                    }
                    GUILayout.Space(2f);
                }

                if (!listModified && alliesProp.arraySize == 0)
                    EditorGUILayout.HelpBox("No allies. Add one with '+ Add Ally'.", MessageType.None);
            }
            EditorGUILayout.EndScrollView();

            if (_teamSerializedDatabase.ApplyModifiedProperties())
                AssetDatabase.SaveAssets();
        }

        private void DrawTeamToolbar()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            {
                GUILayout.Label("PartyDatabase:", GUILayout.Width(95));

                EditorGUI.BeginChangeCheck();
                var newDb = (TeamDatabase)EditorGUILayout.ObjectField(
                    _teamDatabase, typeof(TeamDatabase), false, GUILayout.Width(180));
                if (EditorGUI.EndChangeCheck())
                    TeamSetDatabase(newDb);

                GUILayout.FlexibleSpace();

                if (GUILayout.Button("New DB", EditorStyles.toolbarButton, GUILayout.Width(55)))
                {
                    string path = EditorUtility.SaveFilePanelInProject(
                        "Create TeamDatabase", "TeamDatabase", "asset", "Choose location");
                    if (!string.IsNullOrEmpty(path))
                        TeamSetDatabase(TeamCreateDatabase(path));
                }
            }
            GUILayout.EndHorizontal();
        }

        private void TeamDrawAddButton(SerializedProperty alliesProp)
        {
            if (GUILayout.Button("+ Add Ally", GUILayout.Height(TeamAddButtonHeight)))
            {
                bool wasEmpty = alliesProp.arraySize == 0;
                alliesProp.arraySize++;
                if (wasEmpty)
                {
                    var newAlly = alliesProp.GetArrayElementAtIndex(alliesProp.arraySize - 1);
                    TeamInitAllyDefaults(newAlly);
                }
                EditorUtility.SetDirty(_teamDatabase);
            }
        }

        private static void TeamInitAllyDefaults(SerializedProperty allyProp)
        {
            var nameProp = allyProp.FindPropertyRelative("allyName");
            if (nameProp != null) nameProp.stringValue = TeamDefaultAllyName;

            var prefabProp = allyProp.FindPropertyRelative("prefab");
            if (prefabProp != null)
            {
                var defaultPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(TeamDefaultAllyPrefabPath);
                prefabProp.objectReferenceValue = defaultPrefab;
            }

            TeamSetIntSafe(allyProp,   "maxHp",              TeamDefaultMaxHp);
            TeamSetIntSafe(allyProp,   "atk",                TeamDefaultAtk);
            TeamSetFloatSafe(allyProp, "attackSpeed",        TeamDefaultAttackSpeed);
            TeamSetFloatSafe(allyProp, "moveSpeed",          TeamDefaultMoveSpeed);
            TeamSetFloatSafe(allyProp, "regenHpPerSecond",   TeamDefaultRegenHpPerSec);
            TeamSetFloatSafe(allyProp, "colliderRadius",    0.10f);
        }

        private bool TeamDrawAlly(SerializedProperty alliesProp, int index)
        {
            var allyProp = alliesProp.GetArrayElementAtIndex(index);
            var nameProp = allyProp.FindPropertyRelative("allyName");

            string foldoutKey = allyProp.propertyPath;
            if (!_teamFoldouts.ContainsKey(foldoutKey))
                _teamFoldouts[foldoutKey] = false;

            string label = nameProp != null && !string.IsNullOrEmpty(nameProp.stringValue)
                ? nameProp.stringValue
                : $"Ally {index + 1}";

            GUILayout.BeginHorizontal();
            {
                _teamFoldouts[foldoutKey] = EditorGUILayout.Foldout(
                    _teamFoldouts[foldoutKey], label, true);

                if (GUILayout.Button("-",
                        GUILayout.Width(TeamSmallButtonWidth),
                        GUILayout.Height(TeamRowHeight - 2)))
                {
                    if (EditorUtility.DisplayDialog("Remove Ally",
                            $"Remove '{label}'?", "Remove", "Cancel"))
                    {
                        alliesProp.DeleteArrayElementAtIndex(index);
                        _teamFoldouts.Remove(foldoutKey);
                        EditorUtility.SetDirty(_teamDatabase);
                        GUILayout.EndHorizontal();
                        return true;
                    }
                }
            }
            GUILayout.EndHorizontal();

            if (!_teamFoldouts[foldoutKey])
                return false;

            EditorGUI.indentLevel++;

            if (nameProp != null)
                nameProp.stringValue = EditorGUILayout.TextField("Name", nameProp.stringValue);
            else
                Debug.LogError("[TeamBuilder] Property 'allyName' not found on AllySpawnData.");

            var prefabProp = allyProp.FindPropertyRelative("prefab");
            if (prefabProp != null)
                prefabProp.objectReferenceValue = EditorGUILayout.ObjectField(
                    "Prefab", prefabProp.objectReferenceValue, typeof(GameObject), false);
            else
                Debug.LogError("[TeamBuilder] Property 'prefab' not found on AllySpawnData.");

            var maxHpProp = allyProp.FindPropertyRelative("maxHp");
            if (maxHpProp != null)
                maxHpProp.intValue = EditorGUILayout.IntField("HP", maxHpProp.intValue);
            else
                Debug.LogError("[TeamBuilder] Property 'maxHp' not found on AllySpawnData.");

            var atkProp = allyProp.FindPropertyRelative("atk");
            if (atkProp != null)
                atkProp.intValue = EditorGUILayout.IntField("ATK", atkProp.intValue);
            else
                Debug.LogError("[TeamBuilder] Property 'atk' not found on AllySpawnData.");

            var attackSpeedProp = allyProp.FindPropertyRelative("attackSpeed");
            if (attackSpeedProp != null)
                attackSpeedProp.floatValue = EditorGUILayout.FloatField("Attack Speed", attackSpeedProp.floatValue);
            else
                Debug.LogError("[TeamBuilder] Property 'attackSpeed' not found on AllySpawnData.");

            var moveSpeedProp = allyProp.FindPropertyRelative("moveSpeed");
            if (moveSpeedProp != null)
                moveSpeedProp.floatValue = EditorGUILayout.FloatField("Move Speed", moveSpeedProp.floatValue);
            else
                Debug.LogError("[TeamBuilder] Property 'moveSpeed' not found on AllySpawnData.");

            var regenProp = allyProp.FindPropertyRelative("regenHpPerSecond");
            if (regenProp != null)
                regenProp.floatValue = EditorGUILayout.FloatField("Regen HP/s", regenProp.floatValue);
            else
                Debug.LogError("[TeamBuilder] Property 'regenHpPerSecond' not found on AllySpawnData.");

            var colRadiusProp = allyProp.FindPropertyRelative("colliderRadius");
            if (colRadiusProp != null)
                colRadiusProp.floatValue = EditorGUILayout.FloatField("Collider Radius", colRadiusProp.floatValue);
            else
                Debug.LogError("[TeamBuilder] Property 'colliderRadius' not found on AllySpawnData.");

            DrawAppearanceFields(allyProp);

            EditorGUI.indentLevel--;
            GUILayout.Space(2f);

            return false;
        }

        private static void TeamSetIntSafe(SerializedProperty parent, string name, int value)
        {
            var p = parent.FindPropertyRelative(name);
            if (p != null) p.intValue = value;
            else Debug.LogError($"[TeamBuilder] Property '{name}' not found on AllySpawnData.");
        }

        private static void TeamSetFloatSafe(SerializedProperty parent, string name, float value)
        {
            var p = parent.FindPropertyRelative(name);
            if (p != null) p.floatValue = value;
            else Debug.LogError($"[TeamBuilder] Property '{name}' not found on AllySpawnData.");
        }

        #endregion

        #region Level Designer

        private const float LevelColumnStagesWidth  = 150f;
        private const float LevelColumnLevelsWidth  = 150f;
        private const float LevelRowHeight          = 24f;
        private const float LevelSmallButtonWidth   = 24f;
        private const float LevelAddButtonHeight    = 22f;
        private const float LevelWaveHeaderHeight   = 26f;
        private const string LevelDatabaseDefaultPath   = "Assets/Data/LevelDatabase.asset";
        private const string LevelDefaultEnemyPrefabPath = "Assets/Prefabs/Characters/sampleCharacterHuman.prefab";

        private const string LevelDefaultEnemyName       = "Enemy";
        private const int    LevelDefaultEnemyHp         = 50;
        private const int    LevelDefaultEnemyAtk        = 10;
        private const float  LevelDefaultEnemyAttackSpeed = 1f;
        private const float  LevelDefaultEnemyMoveSpeed  = 2f;
        private const float  LevelDefaultEnemyAttackRange = 0.5f;
        private const float  LevelDefaultEnemyColliderRadius = 0.10f;
        private const int    LevelDefaultEnemyGoldDrop       = 1;

        private LevelDatabase    _levelDatabase;
        private SerializedObject _levelSerializedDatabase;
        private int _levelSelectedStageIndex = -1;
        private int _levelSelectedLevelIndex = -1;
        private int _levelSelectedStepIndex  = -1;
        private Vector2 _levelWavesScrollPos;
        private readonly Dictionary<string, bool> _levelFoldouts = new Dictionary<string, bool>();

        private void LevelTryAutoLoadDatabase()
        {
            var db = AssetDatabase.LoadAssetAtPath<LevelDatabase>(LevelDatabaseDefaultPath);
            if (db == null)
                db = LevelCreateDatabase(LevelDatabaseDefaultPath);
            LevelSetDatabase(db);
        }

        private static LevelDatabase LevelCreateDatabase(string path)
        {
            EditorUIFactory.EnsureDirectoryExists(path);
            var db = ScriptableObject.CreateInstance<LevelDatabase>();
            AssetDatabase.CreateAsset(db, path);
            AssetDatabase.SaveAssets();
            Debug.Log($"[LevelDesigner] Created LevelDatabase at {path}");
            return db;
        }

        private void LevelSetDatabase(LevelDatabase db)
        {
            _levelDatabase           = db;
            _levelSerializedDatabase = db != null ? new SerializedObject(db) : null;
            _levelSelectedStageIndex = -1;
            _levelSelectedLevelIndex = -1;
            _levelSelectedStepIndex  = -1;
            _levelFoldouts.Clear();
            Repaint();
        }

        private void DrawLevelTab()
        {
            DrawLevelToolbar();

            if (_levelDatabase == null)
            {
                EditorGUILayout.HelpBox("No LevelDatabase assigned. Use the field above.", MessageType.Info);
                return;
            }

            _levelSerializedDatabase.Update();

            SerializedProperty stagesProp = _levelSerializedDatabase.FindProperty("stages");

            GUILayout.BeginHorizontal();
            {
                DrawStagesPanel(stagesProp);
                DrawDivider();
                DrawLevelsPanel(stagesProp);
                DrawDivider();
                DrawWavesPanel(stagesProp);
            }
            GUILayout.EndHorizontal();

            if (_levelSerializedDatabase.ApplyModifiedProperties())
                AssetDatabase.SaveAssets();
        }

        private void DrawLevelToolbar()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            {
                GUILayout.Label("LevelDatabase:", GUILayout.Width(100));

                EditorGUI.BeginChangeCheck();
                var newDb = (LevelDatabase)EditorGUILayout.ObjectField(
                    _levelDatabase, typeof(LevelDatabase), false, GUILayout.Width(220));
                if (EditorGUI.EndChangeCheck())
                    LevelSetDatabase(newDb);

                GUILayout.FlexibleSpace();

                if (GUILayout.Button("New DB", EditorStyles.toolbarButton, GUILayout.Width(60)))
                {
                    string path = EditorUtility.SaveFilePanelInProject(
                        "Create LevelDatabase", "LevelDatabase", "asset", "Choose location");
                    if (!string.IsNullOrEmpty(path))
                        LevelSetDatabase(LevelCreateDatabase(path));
                }
            }
            GUILayout.EndHorizontal();
        }

        private void DrawStagesPanel(SerializedProperty stagesProp)
        {
            GUILayout.BeginVertical(GUILayout.Width(LevelColumnStagesWidth));
            {
                DrawPanelHeader("Stages");

                if (stagesProp == null)
                {
                    Debug.LogError("[LevelDesigner] Property 'stages' not found on LevelDatabase.");
                    GUILayout.EndVertical();
                    return;
                }

                if (GUILayout.Button("+ Add Stage", GUILayout.Height(LevelAddButtonHeight)))
                {
                    stagesProp.arraySize++;
                    var newStage = stagesProp.GetArrayElementAtIndex(stagesProp.arraySize - 1);
                    newStage.FindPropertyRelative("stageName").stringValue = $"Stage {stagesProp.arraySize}";
                    var defaultTerrain = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Environment/grid_ground.png");
                    newStage.FindPropertyRelative("terrain").objectReferenceValue = defaultTerrain;
                    newStage.FindPropertyRelative("levels").arraySize = 0;
                    _levelSelectedStageIndex = stagesProp.arraySize - 1;
                    _levelSelectedLevelIndex = -1;
                    EditorUtility.SetDirty(_levelDatabase);
                }

                for (int i = 0; i < stagesProp.arraySize; i++)
                {
                    var stageProp = stagesProp.GetArrayElementAtIndex(i);
                    string label = stageProp.FindPropertyRelative("stageName").stringValue;
                    if (string.IsNullOrEmpty(label)) label = $"Stage {i + 1}";

                    bool isSelected = (i == _levelSelectedStageIndex);
                    GUIStyle style = isSelected ? GetSelectedRowStyle() : GUI.skin.button;

                    GUILayout.BeginHorizontal();
                    {
                        if (GUILayout.Button(label, style, GUILayout.Height(LevelRowHeight)))
                        {
                            if (_levelSelectedStageIndex != i)
                            {
                                _levelSelectedStageIndex = i;
                                _levelSelectedLevelIndex = -1;
                                _levelSelectedStepIndex  = -1;
                            }
                        }

                        if (GUILayout.Button("-",
                                GUILayout.Width(LevelSmallButtonWidth),
                                GUILayout.Height(LevelRowHeight)))
                        {
                            if (EditorUtility.DisplayDialog("Remove Stage",
                                    $"Remove '{label}'? This cannot be undone via the button.", "Remove", "Cancel"))
                            {
                                stagesProp.DeleteArrayElementAtIndex(i);
                                _levelSelectedStageIndex = Mathf.Clamp(_levelSelectedStageIndex, -1, stagesProp.arraySize - 1);
                                _levelSelectedLevelIndex = -1;
                                _levelSelectedStepIndex  = -1;
                                EditorUtility.SetDirty(_levelDatabase);
                                break;
                            }
                        }
                    }
                    GUILayout.EndHorizontal();
                }

                GUILayout.FlexibleSpace();

                if (_levelSelectedStageIndex >= 0 && _levelSelectedStageIndex < stagesProp.arraySize)
                {
                    var stageElement = stagesProp.GetArrayElementAtIndex(_levelSelectedStageIndex);
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

        private void DrawLevelsPanel(SerializedProperty stagesProp)
        {
            GUILayout.BeginVertical(GUILayout.Width(LevelColumnLevelsWidth));
            {
                DrawPanelHeader("Levels");

                if (stagesProp == null
                    || _levelSelectedStageIndex < 0
                    || _levelSelectedStageIndex >= stagesProp.arraySize)
                {
                    EditorGUILayout.HelpBox("Select a stage.", MessageType.None);
                    GUILayout.EndVertical();
                    return;
                }

                SerializedProperty levelsProp = stagesProp
                    .GetArrayElementAtIndex(_levelSelectedStageIndex)
                    .FindPropertyRelative("levels");

                if (levelsProp == null)
                {
                    Debug.LogError("[LevelDesigner] Property 'levels' not found on StageData.");
                    GUILayout.EndVertical();
                    return;
                }

                if (GUILayout.Button("+ Add Level", GUILayout.Height(LevelAddButtonHeight)))
                {
                    levelsProp.arraySize++;
                    var newLevel = levelsProp.GetArrayElementAtIndex(levelsProp.arraySize - 1);
                    newLevel.FindPropertyRelative("levelName").stringValue = $"Level {levelsProp.arraySize}";
                    newLevel.FindPropertyRelative("steps").arraySize = 0;
                    _levelSelectedLevelIndex = levelsProp.arraySize - 1;
                    _levelSelectedStepIndex  = -1;
                    EditorUtility.SetDirty(_levelDatabase);
                }

                for (int i = 0; i < levelsProp.arraySize; i++)
                {
                    var levelProp = levelsProp.GetArrayElementAtIndex(i);
                    string label = levelProp.FindPropertyRelative("levelName").stringValue;
                    if (string.IsNullOrEmpty(label)) label = $"Level {i + 1}";

                    bool isSelected = (i == _levelSelectedLevelIndex);
                    GUIStyle style = isSelected ? GetSelectedRowStyle() : GUI.skin.button;

                    GUILayout.BeginHorizontal();
                    {
                        if (GUILayout.Button(label, style, GUILayout.Height(LevelRowHeight)))
                        {
                            _levelSelectedLevelIndex = i;
                            _levelSelectedStepIndex  = -1;
                        }

                        if (GUILayout.Button("-",
                                GUILayout.Width(LevelSmallButtonWidth),
                                GUILayout.Height(LevelRowHeight)))
                        {
                            if (EditorUtility.DisplayDialog("Remove Level",
                                    $"Remove '{label}'?", "Remove", "Cancel"))
                            {
                                levelsProp.DeleteArrayElementAtIndex(i);
                                _levelSelectedLevelIndex = Mathf.Clamp(_levelSelectedLevelIndex, -1, levelsProp.arraySize - 1);
                                _levelSelectedStepIndex  = -1;
                                EditorUtility.SetDirty(_levelDatabase);
                                break;
                            }
                        }
                    }
                    GUILayout.EndHorizontal();
                }

                GUILayout.FlexibleSpace();

                if (_levelSelectedLevelIndex >= 0 && _levelSelectedLevelIndex < levelsProp.arraySize)
                {
                    var nameProp = levelsProp
                        .GetArrayElementAtIndex(_levelSelectedLevelIndex)
                        .FindPropertyRelative("levelName");
                    GUILayout.Label("Name:", EditorStyles.miniLabel);
                    nameProp.stringValue = EditorGUILayout.TextField(nameProp.stringValue);
                }
            }
            GUILayout.EndVertical();
        }

        private void DrawWavesPanel(SerializedProperty stagesProp)
        {
            GUILayout.BeginVertical();
            {
                DrawPanelHeader("Steps & Waves");

                if (stagesProp == null
                    || _levelSelectedStageIndex < 0
                    || _levelSelectedStageIndex >= stagesProp.arraySize)
                {
                    EditorGUILayout.HelpBox("Select a stage.", MessageType.None);
                    GUILayout.EndVertical();
                    return;
                }

                SerializedProperty levelsProp = stagesProp
                    .GetArrayElementAtIndex(_levelSelectedStageIndex)
                    .FindPropertyRelative("levels");

                if (levelsProp == null
                    || _levelSelectedLevelIndex < 0
                    || _levelSelectedLevelIndex >= levelsProp.arraySize)
                {
                    EditorGUILayout.HelpBox("Select a level.", MessageType.None);
                    GUILayout.EndVertical();
                    return;
                }

                SerializedProperty stepsProp = levelsProp
                    .GetArrayElementAtIndex(_levelSelectedLevelIndex)
                    .FindPropertyRelative("steps");

                if (stepsProp == null)
                {
                    Debug.LogError("[LevelDesigner] Property 'steps' not found on LevelData.");
                    GUILayout.EndVertical();
                    return;
                }

                DrawStepsList(stepsProp);

                GUILayout.Space(6f);

                if (_levelSelectedStepIndex >= 0 && _levelSelectedStepIndex < stepsProp.arraySize)
                {
                    SerializedProperty wavesProp = stepsProp
                        .GetArrayElementAtIndex(_levelSelectedStepIndex)
                        .FindPropertyRelative("waves");

                    if (wavesProp == null)
                    {
                        Debug.LogError("[LevelDesigner] Property 'waves' not found on StepData.");
                    }
                    else
                    {
                        DrawWavesList(wavesProp);
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox("Select a step to view its waves.", MessageType.None);
                }
            }
            GUILayout.EndVertical();
        }

        private void DrawStepsList(SerializedProperty stepsProp)
        {
            EditorGUILayout.LabelField("Steps", EditorStyles.boldLabel);

            if (GUILayout.Button("+ Add Step", GUILayout.Height(LevelAddButtonHeight)))
            {
                stepsProp.arraySize++;
                var newStep = stepsProp.GetArrayElementAtIndex(stepsProp.arraySize - 1);
                newStep.FindPropertyRelative("stepName").stringValue = $"Step {stepsProp.arraySize}";
                newStep.FindPropertyRelative("waves").arraySize = 0;
                _levelSelectedStepIndex = stepsProp.arraySize - 1;
                EditorUtility.SetDirty(_levelDatabase);
            }

            for (int i = 0; i < stepsProp.arraySize; i++)
            {
                var stepProp = stepsProp.GetArrayElementAtIndex(i);
                string stepLabel = stepProp.FindPropertyRelative("stepName").stringValue;
                if (string.IsNullOrEmpty(stepLabel)) stepLabel = $"Step {i + 1}";

                bool isSelected = (i == _levelSelectedStepIndex);
                GUIStyle style = isSelected ? GetSelectedRowStyle() : GUI.skin.button;

                GUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button(stepLabel, style, GUILayout.Height(LevelRowHeight)))
                        _levelSelectedStepIndex = i;

                    if (GUILayout.Button("-",
                            GUILayout.Width(LevelSmallButtonWidth),
                            GUILayout.Height(LevelRowHeight)))
                    {
                        if (EditorUtility.DisplayDialog("Remove Step",
                                $"Remove '{stepLabel}'?", "Remove", "Cancel"))
                        {
                            stepsProp.DeleteArrayElementAtIndex(i);
                            _levelSelectedStepIndex = Mathf.Clamp(_levelSelectedStepIndex, -1, stepsProp.arraySize - 1);
                            EditorUtility.SetDirty(_levelDatabase);
                            break;
                        }
                    }
                }
                GUILayout.EndHorizontal();
            }

            if (_levelSelectedStepIndex >= 0 && _levelSelectedStepIndex < stepsProp.arraySize)
            {
                var stepNameProp = stepsProp
                    .GetArrayElementAtIndex(_levelSelectedStepIndex)
                    .FindPropertyRelative("stepName");
                GUILayout.Label("Step Name:", EditorStyles.miniLabel);
                stepNameProp.stringValue = EditorGUILayout.TextField(stepNameProp.stringValue);
            }
        }

        private void DrawWavesList(SerializedProperty wavesProp)
        {
            EditorGUILayout.LabelField("Waves & Enemies", EditorStyles.boldLabel);

            if (GUILayout.Button("+ Add Wave", GUILayout.Height(LevelAddButtonHeight)))
            {
                wavesProp.arraySize++;
                var newWave = wavesProp.GetArrayElementAtIndex(wavesProp.arraySize - 1);
                newWave.FindPropertyRelative("waveName").stringValue  = $"Wave {wavesProp.arraySize}";
                newWave.FindPropertyRelative("spawnDelay").floatValue = 0f;
                newWave.FindPropertyRelative("enemies").arraySize     = 0;
                EditorUtility.SetDirty(_levelDatabase);
            }

            _levelWavesScrollPos = EditorGUILayout.BeginScrollView(_levelWavesScrollPos);
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

        private bool DrawWave(SerializedProperty wavesProp, int wi)
        {
            var waveProp    = wavesProp.GetArrayElementAtIndex(wi);
            var enemiesProp = waveProp.FindPropertyRelative("enemies");

            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox,
                GUILayout.Height(LevelWaveHeaderHeight));

            GUILayout.Label($"Wave {wi + 1}", EditorStyles.boldLabel, GUILayout.Width(60));

            var delayProp   = waveProp.FindPropertyRelative("spawnDelay");
            bool isFirstWave = (wi == 0);
            if (isFirstWave)
                delayProp.floatValue = 0f;

            EditorGUI.BeginDisabledGroup(isFirstWave);
            GUILayout.Label("Delay:", GUILayout.Width(40));
            delayProp.floatValue = EditorGUILayout.FloatField(delayProp.floatValue, GUILayout.Width(50));
            GUILayout.Label("s", GUILayout.Width(12));
            EditorGUI.EndDisabledGroup();

            if (GUILayout.Button("-",
                    GUILayout.Width(LevelSmallButtonWidth),
                    GUILayout.Height(LevelWaveHeaderHeight - 4)))
            {
                if (EditorUtility.DisplayDialog("Remove Wave", $"Remove Wave {wi + 1}?", "Remove", "Cancel"))
                {
                    wavesProp.DeleteArrayElementAtIndex(wi);
                    EditorUtility.SetDirty(_levelDatabase);
                    EditorGUILayout.EndHorizontal();
                    return true;
                }
            }

            EditorGUILayout.EndHorizontal();

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
                if (GUILayout.Button("+ Add Enemy", GUILayout.Height(LevelAddButtonHeight)))
                {
                    bool wasEmpty = enemiesProp.arraySize == 0;
                    enemiesProp.arraySize++;
                    if (wasEmpty)
                    {
                        var newEnemy = enemiesProp.GetArrayElementAtIndex(enemiesProp.arraySize - 1);
                        var lastEnemy = FindLastEnemyInDatabase(wi);
                        if (lastEnemy != null)
                            CopyEnemyProperties(lastEnemy, newEnemy);
                        else
                            InitEnemyDefaults(newEnemy);
                    }
                    EditorUtility.SetDirty(_levelDatabase);
                }
            }

            EditorGUI.indentLevel--;
            return false;
        }

        private SerializedProperty FindLastEnemyInDatabase(int currentWaveIndex)
        {
            var stagesProp = _levelSerializedDatabase.FindProperty("stages");
            if (stagesProp == null) return null;

            for (int si = _levelSelectedStageIndex; si >= 0; si--)
            {
                var stage  = stagesProp.GetArrayElementAtIndex(si);
                var levels = stage.FindPropertyRelative("levels");
                if (levels == null) continue;

                int startLevel = (si == _levelSelectedStageIndex) ? _levelSelectedLevelIndex : levels.arraySize - 1;
                for (int li = startLevel; li >= 0; li--)
                {
                    var level = levels.GetArrayElementAtIndex(li);
                    var steps = level.FindPropertyRelative("steps");
                    if (steps == null) continue;

                    int startStep = (si == _levelSelectedStageIndex && li == _levelSelectedLevelIndex)
                        ? _levelSelectedStepIndex
                        : steps.arraySize - 1;

                    for (int sti = startStep; sti >= 0; sti--)
                    {
                        var step  = steps.GetArrayElementAtIndex(sti);
                        var waves = step.FindPropertyRelative("waves");
                        if (waves == null) continue;

                        bool isCurrentStep = si == _levelSelectedStageIndex
                                             && li == _levelSelectedLevelIndex
                                             && sti == _levelSelectedStepIndex;
                        int startWave = isCurrentStep ? currentWaveIndex - 1 : waves.arraySize - 1;

                        for (int wi2 = startWave; wi2 >= 0; wi2--)
                        {
                            var wave    = waves.GetArrayElementAtIndex(wi2);
                            var enemies = wave.FindPropertyRelative("enemies");
                            if (enemies != null && enemies.arraySize > 0)
                                return enemies.GetArrayElementAtIndex(enemies.arraySize - 1);
                        }
                    }
                }
            }

            return null;
        }

        private static void CopyEnemyProperties(SerializedProperty source, SerializedProperty target)
        {
            var enemyName = target.FindPropertyRelative("enemyName");
            if (enemyName != null) enemyName.stringValue = source.FindPropertyRelative("enemyName").stringValue;

            var prefab = target.FindPropertyRelative("prefab");
            if (prefab != null) prefab.objectReferenceValue = source.FindPropertyRelative("prefab").objectReferenceValue;

            var hp = target.FindPropertyRelative("hp");
            if (hp != null) hp.intValue = source.FindPropertyRelative("hp").intValue;

            var atk = target.FindPropertyRelative("atk");
            if (atk != null) atk.intValue = source.FindPropertyRelative("atk").intValue;

            var attackSpeed = target.FindPropertyRelative("attackSpeed");
            if (attackSpeed != null) attackSpeed.floatValue = source.FindPropertyRelative("attackSpeed").floatValue;

            var moveSpeed = target.FindPropertyRelative("moveSpeed");
            if (moveSpeed != null) moveSpeed.floatValue = source.FindPropertyRelative("moveSpeed").floatValue;

            var attackRange = target.FindPropertyRelative("attackRange");
            if (attackRange != null) attackRange.floatValue = source.FindPropertyRelative("attackRange").floatValue;

            var attackType = target.FindPropertyRelative("attackType");
            if (attackType != null) attackType.enumValueIndex = source.FindPropertyRelative("attackType").enumValueIndex;

            var colRadius = target.FindPropertyRelative("colliderRadius");
            if (colRadius != null) colRadius.floatValue = source.FindPropertyRelative("colliderRadius").floatValue;

            var goldDrop = target.FindPropertyRelative("goldDrop");
            if (goldDrop != null) goldDrop.intValue = source.FindPropertyRelative("goldDrop").intValue;

            var headSprite = target.FindPropertyRelative("appearance.headSprite");
            if (headSprite != null) headSprite.objectReferenceValue = source.FindPropertyRelative("appearance.headSprite").objectReferenceValue;

            var hatSprite = target.FindPropertyRelative("appearance.hatSprite");
            if (hatSprite != null) hatSprite.objectReferenceValue = source.FindPropertyRelative("appearance.hatSprite").objectReferenceValue;

            var weaponSprite = target.FindPropertyRelative("appearance.weaponSprite");
            if (weaponSprite != null) weaponSprite.objectReferenceValue = source.FindPropertyRelative("appearance.weaponSprite").objectReferenceValue;

            var shieldSprite = target.FindPropertyRelative("appearance.shieldSprite");
            if (shieldSprite != null) shieldSprite.objectReferenceValue = source.FindPropertyRelative("appearance.shieldSprite").objectReferenceValue;
        }

        private void InitEnemyDefaults(SerializedProperty newEnemy)
        {
            var enemyName = newEnemy.FindPropertyRelative("enemyName");
            if (enemyName != null) enemyName.stringValue = LevelDefaultEnemyName;

            var hp = newEnemy.FindPropertyRelative("hp");
            if (hp != null) hp.intValue = LevelDefaultEnemyHp;

            var atk = newEnemy.FindPropertyRelative("atk");
            if (atk != null) atk.intValue = LevelDefaultEnemyAtk;

            var attackSpeed = newEnemy.FindPropertyRelative("attackSpeed");
            if (attackSpeed != null) attackSpeed.floatValue = LevelDefaultEnemyAttackSpeed;

            var moveSpeed = newEnemy.FindPropertyRelative("moveSpeed");
            if (moveSpeed != null) moveSpeed.floatValue = LevelDefaultEnemyMoveSpeed;

            var attackRange = newEnemy.FindPropertyRelative("attackRange");
            if (attackRange != null) attackRange.floatValue = LevelDefaultEnemyAttackRange;

            var attackType = newEnemy.FindPropertyRelative("attackType");
            if (attackType != null) attackType.enumValueIndex = 0;

            var colRadius = newEnemy.FindPropertyRelative("colliderRadius");
            if (colRadius != null) colRadius.floatValue = LevelDefaultEnemyColliderRadius;

            var goldDrop = newEnemy.FindPropertyRelative("goldDrop");
            if (goldDrop != null) goldDrop.intValue = LevelDefaultEnemyGoldDrop;

            var prefab = newEnemy.FindPropertyRelative("prefab");
            if (prefab != null)
                prefab.objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(LevelDefaultEnemyPrefabPath);
        }

        private bool DrawEnemy(SerializedProperty enemiesProp, int wi, int ei)
        {
            var enemyProp     = enemiesProp.GetArrayElementAtIndex(ei);
            var enemyNameProp = enemyProp.FindPropertyRelative("enemyName");
            var prefabProp    = enemyProp.FindPropertyRelative("prefab");

            string foldoutKey = enemyProp.propertyPath;
            if (!_levelFoldouts.ContainsKey(foldoutKey))
                _levelFoldouts[foldoutKey] = false;

            string foldoutLabel = string.IsNullOrEmpty(enemyNameProp.stringValue)
                ? $"Enemy {ei + 1}"
                : enemyNameProp.stringValue;

            GUILayout.BeginHorizontal();
            {
                _levelFoldouts[foldoutKey] = EditorGUILayout.Foldout(
                    _levelFoldouts[foldoutKey], foldoutLabel, true);

                if (GUILayout.Button("-",
                        GUILayout.Width(LevelSmallButtonWidth),
                        GUILayout.Height(LevelRowHeight - 2)))
                {
                    if (EditorUtility.DisplayDialog("Remove Enemy",
                            $"Remove '{foldoutLabel}' from Wave {wi + 1}?", "Remove", "Cancel"))
                    {
                        enemiesProp.DeleteArrayElementAtIndex(ei);
                        _levelFoldouts.Remove(foldoutKey);
                        EditorUtility.SetDirty(_levelDatabase);
                        GUILayout.EndHorizontal();
                        return true;
                    }
                }
            }
            GUILayout.EndHorizontal();

            if (!_levelFoldouts[foldoutKey])
                return false;

            EditorGUI.indentLevel++;

            enemyNameProp.stringValue = EditorGUILayout.TextField("Name", enemyNameProp.stringValue);

            prefabProp.objectReferenceValue = EditorGUILayout.ObjectField(
                "Prefab", prefabProp.objectReferenceValue, typeof(GameObject), false);

            var hpProp          = enemyProp.FindPropertyRelative("hp");
            var atkProp         = enemyProp.FindPropertyRelative("atk");
            var attackSpeedProp = enemyProp.FindPropertyRelative("attackSpeed");
            var moveSpeedProp   = enemyProp.FindPropertyRelative("moveSpeed");
            var attackRangeProp = enemyProp.FindPropertyRelative("attackRange");
            var attackTypeProp  = enemyProp.FindPropertyRelative("attackType");

            if (hpProp != null)
                hpProp.intValue = EditorGUILayout.IntField("HP", hpProp.intValue);
            else
                Debug.LogError("[LevelDesigner] Property 'hp' not found on EnemySpawnData.");

            if (atkProp != null)
                atkProp.intValue = EditorGUILayout.IntField("ATK", atkProp.intValue);
            else
                Debug.LogError("[LevelDesigner] Property 'atk' not found on EnemySpawnData.");

            if (attackSpeedProp != null)
                attackSpeedProp.floatValue = EditorGUILayout.FloatField("Attack Speed", attackSpeedProp.floatValue);
            else
                Debug.LogError("[LevelDesigner] Property 'attackSpeed' not found on EnemySpawnData.");

            if (moveSpeedProp != null)
                moveSpeedProp.floatValue = EditorGUILayout.FloatField("Move Speed", moveSpeedProp.floatValue);
            else
                Debug.LogError("[LevelDesigner] Property 'moveSpeed' not found on EnemySpawnData.");

            if (attackRangeProp != null)
                attackRangeProp.floatValue = EditorGUILayout.FloatField("Attack Range", attackRangeProp.floatValue);
            else
                Debug.LogError("[LevelDesigner] Property 'attackRange' not found on EnemySpawnData.");

            if (attackTypeProp != null)
                attackTypeProp.enumValueIndex = (int)(AttackType)EditorGUILayout.EnumPopup(
                    "Attack Type", (AttackType)attackTypeProp.enumValueIndex);
            else
                Debug.LogError("[LevelDesigner] Property 'attackType' not found on EnemySpawnData.");

            var colRadiusProp = enemyProp.FindPropertyRelative("colliderRadius");
            if (colRadiusProp != null)
                colRadiusProp.floatValue = EditorGUILayout.FloatField("Collider Radius", colRadiusProp.floatValue);
            else
                Debug.LogError("[LevelDesigner] Property 'colliderRadius' not found on EnemySpawnData.");

            var goldDropProp = enemyProp.FindPropertyRelative("goldDrop");
            if (goldDropProp != null)
                goldDropProp.intValue = EditorGUILayout.IntField("Gold Drop", goldDropProp.intValue);
            else
                Debug.LogError("[LevelDesigner] Property 'goldDrop' not found on EnemySpawnData.");

            DrawAppearanceFields(enemyProp);

            EditorGUI.indentLevel--;
            GUILayout.Space(2f);

            return false;
        }

        private static void DrawAppearanceFields(SerializedProperty parentProp)
        {
            var appearanceProp = parentProp.FindPropertyRelative("appearance");
            if (appearanceProp == null)
            {
                Debug.LogError("[GameDesigner] Property 'appearance' not found.");
                return;
            }

            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("Appearance", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(
                appearanceProp.FindPropertyRelative("headSprite"), new GUIContent("Head"));
            EditorGUILayout.PropertyField(
                appearanceProp.FindPropertyRelative("hatSprite"), new GUIContent("Hat / Armor"));
            EditorGUILayout.PropertyField(
                appearanceProp.FindPropertyRelative("weaponSprite"), new GUIContent("Weapon"));
            EditorGUILayout.PropertyField(
                appearanceProp.FindPropertyRelative("shieldSprite"), new GUIContent("Shield"));
        }

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

        #endregion

        #region Shared helpers

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

        #endregion
    }
}
