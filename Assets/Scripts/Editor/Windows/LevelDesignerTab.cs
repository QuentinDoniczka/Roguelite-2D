using System.Collections.Generic;
using RogueliteAutoBattler.Data;
using UnityEditor;
using UnityEngine;

namespace RogueliteAutoBattler.Editor
{
    internal sealed class LevelDesignerTab
    {
        private const float LevelColumnStagesWidth       = 150f;
        private const float LevelColumnLevelsWidth       = 150f;
        private const float LevelColumnAutoBuilderWidth  = 180f;
        private const float LevelRowHeight          = 24f;
        private const float LevelSmallButtonWidth   = 24f;
        private const float LevelAddButtonHeight    = 22f;
        private const float LevelWaveHeaderHeight   = 26f;
        internal const string LevelDatabaseDefaultPath   = "Assets/Data/LevelDatabase.asset";
        private const string LevelDefaultEnemyPrefabPath = "Assets/Prefabs/Characters/sampleCharacterHuman.prefab";

        private const string LevelDefaultEnemyName       = "Enemy";
        private const int    LevelDefaultEnemyHp         = 50;
        private const int    LevelDefaultEnemyAtk        = 10;
        private const float  LevelDefaultEnemyAttackSpeed = 1f;
        private const float  LevelDefaultEnemyMoveSpeed  = 2f;
        private const float  LevelDefaultEnemyAttackRange = 0.5f;
        private const float  LevelDefaultEnemyColliderRadius = 0.10f;
        private const int    LevelDefaultEnemyGoldDrop       = 1;

        private static readonly Color PanelHeaderDividerColor = new Color(0.4f, 0.4f, 0.4f, 1f);
        private static readonly Color PanelDividerColor       = new Color(0.3f, 0.3f, 0.3f, 1f);
        private static readonly Color SelectedRowColor        = new Color(0.24f, 0.49f, 0.91f, 1f);

        private readonly EditorWindow _owner;

        private GUIStyle  _selectedRowStyle;
        private Texture2D _selectedRowTexture;

        private LevelDatabase    _levelDatabase;
        private SerializedObject _levelSerializedDatabase;
        private int _levelSelectedStageIndex = -1;
        private int _levelSelectedLevelIndex = -1;
        private int _levelSelectedStepIndex  = -1;
        private Vector2 _levelWavesScrollPos;
        private readonly Dictionary<string, bool> _levelFoldouts = new Dictionary<string, bool>();

        private int _autoBuilderStepCount = 10;
        private int _autoBuilderWavesPerStep = 1;
        private float _autoBuilderWaveSpawnDelay = 5f;
        private int _autoBuilderEnemiesPerWave = 3;
        private int _autoBuilderEnemyCountOverride = 5;
        private int _autoBuilderEnemyOverrideFrequency = 3;
        private int _autoBuilderSpecialStepFrequency = 5;
        private int _autoBuilderSpecialWavesPerStep = 1;
        private float _autoBuilderSpecialWaveSpawnDelay = 5f;
        private int _autoBuilderSpecialEnemiesPerWave = 5;
        private int _autoBuilderSpecialEnemyCountOverride = 8;
        private int _autoBuilderSpecialEnemyOverrideFrequency = 2;

        private const int AutoBuilderMinFrequency = 2;
        private const int StepTypeNormalEnumIndex      = (int)StepType.Normal;
        private const int StepTypeSpecialEnumIndex     = (int)StepType.Special;
        private const int AttackTypeMeleeEnumIndex     = (int)AttackType.Melee;

        internal LevelDesignerTab(EditorWindow owner)
        {
            _owner = owner;
        }

        internal void OnEnable()
        {
            LevelTryAutoLoadDatabase();
        }

        internal void OnDisable()
        {
            _levelSerializedDatabase = null;

            _selectedRowStyle = null;
            if (_selectedRowTexture != null)
            {
                Object.DestroyImmediate(_selectedRowTexture);
                _selectedRowTexture = null;
            }
        }

        internal void Draw()
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
                DrawAutoBuilderColumn(stagesProp);
                DrawDivider();
                DrawWavesPanel(stagesProp);
            }
            GUILayout.EndHorizontal();

            if (_levelSerializedDatabase.ApplyModifiedProperties())
                AssetDatabase.SaveAssets();
        }

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
            _owner.Repaint();
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
                newStep.FindPropertyRelative("stepType").enumValueIndex = StepTypeNormalEnumIndex;
                newStep.FindPropertyRelative("waves").arraySize = 0;
                _levelSelectedStepIndex = stepsProp.arraySize - 1;
                EditorUtility.SetDirty(_levelDatabase);
            }

            for (int i = 0; i < stepsProp.arraySize; i++)
            {
                var stepProp = stepsProp.GetArrayElementAtIndex(i);
                string stepLabel = stepProp.FindPropertyRelative("stepName").stringValue;
                if (string.IsNullOrEmpty(stepLabel)) stepLabel = $"Step {i + 1}";

                var stepTypeProp = stepProp.FindPropertyRelative("stepType");
                if (stepTypeProp != null && stepTypeProp.enumValueIndex == StepTypeSpecialEnumIndex)
                    stepLabel = $"[S] {stepLabel}";

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
                var selectedStep = stepsProp.GetArrayElementAtIndex(_levelSelectedStepIndex);

                var stepNameProp = selectedStep.FindPropertyRelative("stepName");
                GUILayout.Label("Step Name:", EditorStyles.miniLabel);
                stepNameProp.stringValue = EditorGUILayout.TextField(stepNameProp.stringValue);

                var selectedStepTypeProp = selectedStep.FindPropertyRelative("stepType");
                if (selectedStepTypeProp != null)
                    EditorGUILayout.PropertyField(selectedStepTypeProp);
            }
        }

        private void DrawAutoBuilderColumn(SerializedProperty stagesProp)
        {
            GUILayout.BeginVertical(GUILayout.Width(LevelColumnAutoBuilderWidth));
            {
                DrawPanelHeader("Auto-Builder");

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
                    GUILayout.EndVertical();
                    return;
                }

                _autoBuilderStepCount = Mathf.Max(1,
                    EditorGUILayout.IntField("Step Count", _autoBuilderStepCount));

                GUILayout.Space(4f);
                EditorGUILayout.LabelField("Normal Steps", EditorStyles.boldLabel);
                _autoBuilderWavesPerStep = Mathf.Max(1,
                    EditorGUILayout.IntField("Waves", _autoBuilderWavesPerStep));
                _autoBuilderWaveSpawnDelay = Mathf.Max(0f,
                    EditorGUILayout.FloatField("Wave Delay (s)", _autoBuilderWaveSpawnDelay));
                _autoBuilderEnemiesPerWave = Mathf.Max(1,
                    EditorGUILayout.IntField("Enemies", _autoBuilderEnemiesPerWave));
                _autoBuilderEnemyCountOverride = Mathf.Max(1,
                    EditorGUILayout.IntField("Override Count", _autoBuilderEnemyCountOverride));
                _autoBuilderEnemyOverrideFrequency = Mathf.Max(AutoBuilderMinFrequency,
                    EditorGUILayout.IntField("Override Every", _autoBuilderEnemyOverrideFrequency));

                GUILayout.Space(4f);
                EditorGUILayout.LabelField("Special Steps", EditorStyles.boldLabel);
                _autoBuilderSpecialStepFrequency = Mathf.Max(AutoBuilderMinFrequency,
                    EditorGUILayout.IntField("Special Every", _autoBuilderSpecialStepFrequency));
                _autoBuilderSpecialWavesPerStep = Mathf.Max(1,
                    EditorGUILayout.IntField("Waves", _autoBuilderSpecialWavesPerStep));
                _autoBuilderSpecialWaveSpawnDelay = Mathf.Max(0f,
                    EditorGUILayout.FloatField("Wave Delay (s)", _autoBuilderSpecialWaveSpawnDelay));
                _autoBuilderSpecialEnemiesPerWave = Mathf.Max(1,
                    EditorGUILayout.IntField("Enemies", _autoBuilderSpecialEnemiesPerWave));
                _autoBuilderSpecialEnemyCountOverride = Mathf.Max(1,
                    EditorGUILayout.IntField("Override Count", _autoBuilderSpecialEnemyCountOverride));
                _autoBuilderSpecialEnemyOverrideFrequency = Mathf.Max(AutoBuilderMinFrequency,
                    EditorGUILayout.IntField("Override Every", _autoBuilderSpecialEnemyOverrideFrequency));

                GUILayout.Space(6f);

                if (GUILayout.Button("Generate", GUILayout.Height(LevelRowHeight)))
                    ExecuteAutoBuilder(stepsProp);

                GUILayout.FlexibleSpace();
            }
            GUILayout.EndVertical();
        }

        private void ExecuteAutoBuilder(SerializedProperty stepsProp)
        {
            bool shouldGenerate = EditorUtility.DisplayDialog(
                "Generate Steps",
                "This will replace all existing steps in this level.",
                "Generate",
                "Cancel");

            if (!shouldGenerate)
                return;

            Undo.RecordObject(_levelDatabase, "Auto-Build Level Steps");

            stepsProp.arraySize = 0;
            int specialStepCounter = 0;

            for (int i = 0; i < _autoBuilderStepCount; i++)
            {
                int oneBasedIndex = i + 1;
                bool isSpecialStep = oneBasedIndex % _autoBuilderSpecialStepFrequency == 0;

                stepsProp.arraySize++;
                var stepElement = stepsProp.GetArrayElementAtIndex(i);

                string stepName = isSpecialStep
                    ? $"Step {oneBasedIndex} [Special]"
                    : $"Step {oneBasedIndex}";
                stepElement.FindPropertyRelative("stepName").stringValue = stepName;
                stepElement.FindPropertyRelative("stepType").enumValueIndex =
                    isSpecialStep ? StepTypeSpecialEnumIndex : StepTypeNormalEnumIndex;

                int enemyCount;
                int wavesCount;
                float waveDelay;
                if (isSpecialStep)
                {
                    specialStepCounter++;
                    bool hasSpecialOverride =
                        specialStepCounter % _autoBuilderSpecialEnemyOverrideFrequency == 0;
                    enemyCount = hasSpecialOverride
                        ? _autoBuilderSpecialEnemyCountOverride
                        : _autoBuilderSpecialEnemiesPerWave;
                    wavesCount = _autoBuilderSpecialWavesPerStep;
                    waveDelay = _autoBuilderSpecialWaveSpawnDelay;
                }
                else
                {
                    bool hasEnemyOverride =
                        oneBasedIndex % _autoBuilderEnemyOverrideFrequency == 0;
                    enemyCount = hasEnemyOverride
                        ? _autoBuilderEnemyCountOverride
                        : _autoBuilderEnemiesPerWave;
                    wavesCount = _autoBuilderWavesPerStep;
                    waveDelay = _autoBuilderWaveSpawnDelay;
                }

                var wavesProp = stepElement.FindPropertyRelative("waves");
                wavesProp.arraySize = wavesCount;

                for (int w = 0; w < wavesCount; w++)
                {
                    var waveElement = wavesProp.GetArrayElementAtIndex(w);
                    waveElement.FindPropertyRelative("waveName").stringValue = "Wave";
                    waveElement.FindPropertyRelative("spawnDelay").floatValue =
                        w == 0 ? 0f : waveDelay;

                    var enemiesProp = waveElement.FindPropertyRelative("enemies");
                    enemiesProp.arraySize = enemyCount;

                    for (int e = 0; e < enemyCount; e++)
                        InitEnemyDefaults(enemiesProp.GetArrayElementAtIndex(e));
                }
            }

            _levelSelectedStepIndex = 0;
            EditorUtility.SetDirty(_levelDatabase);
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

        private static readonly string[] EnemyCopyPropertyPaths =
        {
            "enemyName", "prefab", "hp", "atk", "attackSpeed", "moveSpeed",
            "attackRange", "attackType", "colliderRadius", "goldDrop",
            "appearance.headSprite", "appearance.hatSprite",
            "appearance.weaponSprite", "appearance.shieldSprite"
        };

        private static void CopyEnemyProperties(SerializedProperty source, SerializedProperty target)
        {
            foreach (string path in EnemyCopyPropertyPaths)
                CopySerializedProperty(source, target, path);
        }

        private static void CopySerializedProperty(
            SerializedProperty source, SerializedProperty target, string relativePath)
        {
            var srcProp = source.FindPropertyRelative(relativePath);
            var dstProp = target.FindPropertyRelative(relativePath);
            if (srcProp == null || dstProp == null)
                return;

            switch (srcProp.propertyType)
            {
                case SerializedPropertyType.String:
                    dstProp.stringValue = srcProp.stringValue;
                    break;
                case SerializedPropertyType.Integer:
                    dstProp.intValue = srcProp.intValue;
                    break;
                case SerializedPropertyType.Float:
                    dstProp.floatValue = srcProp.floatValue;
                    break;
                case SerializedPropertyType.Enum:
                    dstProp.enumValueIndex = srcProp.enumValueIndex;
                    break;
                case SerializedPropertyType.ObjectReference:
                    dstProp.objectReferenceValue = srcProp.objectReferenceValue;
                    break;
            }
        }

        private static void InitEnemyDefaults(SerializedProperty newEnemy)
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
            if (attackType != null) attackType.enumValueIndex = AttackTypeMeleeEnumIndex;

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

            EditorUIFactory.DrawAppearanceFields(enemyProp);

            EditorGUI.indentLevel--;
            GUILayout.Space(2f);

            return false;
        }

        private static void DrawPanelHeader(string title)
        {
            GUILayout.Label(title, EditorStyles.boldLabel);
            var rect = GUILayoutUtility.GetRect(1, 1, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(rect, PanelHeaderDividerColor);
            GUILayout.Space(4f);
        }

        private static void DrawDivider()
        {
            var rect = GUILayoutUtility.GetRect(1f, 1f, GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(true));
            EditorGUI.DrawRect(rect, PanelDividerColor);
        }

        private GUIStyle GetSelectedRowStyle()
        {
            if (_selectedRowStyle != null)
                return _selectedRowStyle;

            if (_selectedRowTexture == null)
            {
                _selectedRowTexture = new Texture2D(1, 1);
                _selectedRowTexture.SetPixel(0, 0, SelectedRowColor);
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
