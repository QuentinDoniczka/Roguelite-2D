using System.Collections.Generic;
using RogueliteAutoBattler.Combat;
using RogueliteAutoBattler.Data;
using UnityEditor;
using UnityEngine;

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

        // Reusable dictionary for SyncOverrideList (avoid allocating every OnGUI).
        private readonly Dictionary<string, (bool enabled, int intVal, float floatVal)> _syncOverrideCache =
            new Dictionary<string, (bool enabled, int intVal, float floatVal)>();

        // Version counter incremented when CharacterStats fields are re-discovered.
        // SyncOverrideList is skipped for a property path that was already synced at the current version.
        private int _fieldsVersion;
        private readonly Dictionary<string, int> _syncedAtVersion = new Dictionary<string, int>();

        // Cached SerializedObject for the currently displayed baseStats (avoid recreating per frame).
        private CharacterStats _cachedBaseStatsRef;
        private SerializedObject _cachedBaseStatsSO;

        // Auto-discovered CharacterStats fields (cached once in OnEnable).
        private struct DiscoveredField
        {
            public string                  name;
            public string                  displayName;
            public SerializedPropertyType  propertyType;
        }
        private readonly List<DiscoveredField> _statsFields = new List<DiscoveredField>();

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
            DiscoverCharacterStatsFields();
            TryAutoLoadDatabase();
        }

        private void OnDisable()
        {
            _serializedDatabase = null;
            _selectedRowStyle = null;
            _cachedBaseStatsRef = null;
            _cachedBaseStatsSO = null;

            if (_selectedRowTexture != null)
            {
                DestroyImmediate(_selectedRowTexture);
                _selectedRowTexture = null;
            }
        }

        // ─── CharacterStats field discovery ──────────────────────────────────

        /// <summary>
        /// Instantiates a temporary CharacterStats SO, iterates its serialized properties,
        /// and caches every non-Unity-internal field for stat-override drawing.
        /// </summary>
        private void DiscoverCharacterStatsFields()
        {
            _statsFields.Clear();
            _fieldsVersion++;

            var tempSo = ScriptableObject.CreateInstance<CharacterStats>();
            var so = new SerializedObject(tempSo);

            SerializedProperty it = so.GetIterator();
            bool enter = true;
            while (it.NextVisible(enter))
            {
                enter = false;
                // Skip Unity internal properties (m_Script, etc.)
                if (it.name.StartsWith("m_")) continue;

                _statsFields.Add(new DiscoveredField
                {
                    name         = it.name,
                    displayName  = it.displayName,
                    propertyType = it.propertyType
                });
            }

            DestroyImmediate(tempSo);
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
            _syncedAtVersion.Clear();
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
                    newStage.FindPropertyRelative("stageName").stringValue =
                        $"Stage {stagesProp.arraySize}";
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

                        // Remove button
                        if (GUILayout.Button("-", GUILayout.Width(SmallButtonWidth), GUILayout.Height(RowHeight)))
                        {
                            if (EditorUtility.DisplayDialog("Remove Stage",
                                    $"Remove '{label}'? This cannot be undone via the button.", "Remove", "Cancel"))
                            {
                                stagesProp.DeleteArrayElementAtIndex(i);
                                _selectedStageIndex = Mathf.Clamp(_selectedStageIndex, -1, stagesProp.arraySize - 1);
                                _selectedLevelIndex = -1;
                                EditorUtility.SetDirty(_database);
                                break; // collection modified
                            }
                        }
                    }
                    GUILayout.EndHorizontal();
                }

                GUILayout.FlexibleSpace();

                // Inline rename for selected stage
                if (_selectedStageIndex >= 0 && _selectedStageIndex < stagesProp.arraySize)
                {
                    var nameProp = stagesProp
                        .GetArrayElementAtIndex(_selectedStageIndex)
                        .FindPropertyRelative("stageName");
                    GUILayout.Label("Name:", EditorStyles.miniLabel);
                    nameProp.stringValue = EditorGUILayout.TextField(nameProp.stringValue);
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
                    newLevel.FindPropertyRelative("levelName").stringValue =
                        $"Level {levelsProp.arraySize}";
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
                    newWave.FindPropertyRelative("waveName").stringValue  = "Wave";
                    newWave.FindPropertyRelative("terrain").stringValue   = "";
                    newWave.FindPropertyRelative("enemies").arraySize     = 0;
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
            var terrainProp = waveProp.FindPropertyRelative("terrain");
            var enemiesProp = waveProp.FindPropertyRelative("enemies");

            // ── Wave header ──────────────────────────────────────────────────
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox,
                GUILayout.Height(WaveHeaderHeight));

            GUILayout.Label($"Wave {wi + 1}", EditorStyles.boldLabel, GUILayout.Width(60));
            GUILayout.Label("Terrain:", GUILayout.Width(50));
            terrainProp.stringValue = EditorGUILayout.TextField(terrainProp.stringValue);

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
                    newEnemy.FindPropertyRelative("enemyName").stringValue        = "Enemy";
                    newEnemy.FindPropertyRelative("prefab").objectReferenceValue  = null;
                    newEnemy.FindPropertyRelative("baseStats").objectReferenceValue = null;
                    newEnemy.FindPropertyRelative("spawnOffset").vector2Value     = Vector2.zero;
                    // Clear overrides
                    newEnemy.FindPropertyRelative("statOverrides")
                            .FindPropertyRelative("overrides").arraySize = 0;
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
            var enemyProp      = enemiesProp.GetArrayElementAtIndex(ei);
            var enemyNameProp  = enemyProp.FindPropertyRelative("enemyName");
            var prefabProp     = enemyProp.FindPropertyRelative("prefab");
            var baseStatsProp  = enemyProp.FindPropertyRelative("baseStats");
            var offsetProp     = enemyProp.FindPropertyRelative("spawnOffset");

            string foldoutKey  = enemyProp.propertyPath;
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

            // Core fields
            enemyNameProp.stringValue = EditorGUILayout.TextField("Name", enemyNameProp.stringValue);

            prefabProp.objectReferenceValue = EditorGUILayout.ObjectField(
                "Prefab", prefabProp.objectReferenceValue, typeof(GameObject), false);

            baseStatsProp.objectReferenceValue = EditorGUILayout.ObjectField(
                "Base Stats", baseStatsProp.objectReferenceValue, typeof(CharacterStats), false);

            offsetProp.vector2Value = EditorGUILayout.Vector2Field("Spawn Offset", offsetProp.vector2Value);

            // Stat overrides
            GUILayout.Space(4f);
            GUILayout.Label("-- Stat Overrides --", EditorStyles.centeredGreyMiniLabel);

            var overridesListProp = enemyProp
                .FindPropertyRelative("statOverrides")
                .FindPropertyRelative("overrides");

            if (overridesListProp != null)
            {
                SyncOverrideList(overridesListProp);

                var baseStats = baseStatsProp.objectReferenceValue as CharacterStats;
                // Handle destroyed Unity objects that pass the 'as' cast but are logically null.
                if (baseStats == null) baseStats = null;
                SerializedObject baseStatsSO = GetCachedBaseStatsSO(baseStats);

                for (int oi = 0; oi < overridesListProp.arraySize; oi++)
                {
                    var overrideProp       = overridesListProp.GetArrayElementAtIndex(oi);
                    var fieldNameProp      = overrideProp.FindPropertyRelative("fieldName");
                    var enabledProp        = overrideProp.FindPropertyRelative("enabled");
                    var intValueProp       = overrideProp.FindPropertyRelative("intValue");
                    var floatValueProp     = overrideProp.FindPropertyRelative("floatValue");

                    // Find matching discovered field for displayName
                    string displayName = fieldNameProp.stringValue;
                    SerializedPropertyType fieldType = SerializedPropertyType.Float;

                    foreach (var df in _statsFields)
                    {
                        if (df.name == fieldNameProp.stringValue)
                        {
                            displayName = df.displayName;
                            fieldType   = df.propertyType;
                            break;
                        }
                    }

                    bool isIntField = (fieldType == SerializedPropertyType.Integer);

                    // Fallback base value from baseStats SO
                    float baseValueFloat = 0f;
                    int baseValueInt = 0;
                    if (baseStatsSO != null)
                    {
                        var baseProp = baseStatsSO.FindProperty(fieldNameProp.stringValue);
                        if (baseProp != null)
                        {
                            if (baseProp.propertyType == SerializedPropertyType.Integer)
                            {
                                baseValueInt = baseProp.intValue;
                                baseValueFloat = baseProp.intValue;
                            }
                            else
                            {
                                baseValueFloat = baseProp.floatValue;
                                baseValueInt = (int)baseProp.floatValue;
                            }
                        }
                    }

                    GUILayout.BeginHorizontal();
                    {
                        enabledProp.boolValue = EditorGUILayout.Toggle(enabledProp.boolValue, GUILayout.Width(16));

                        EditorGUI.BeginDisabledGroup(!enabledProp.boolValue);

                        if (isIntField)
                        {
                            int displayInt = enabledProp.boolValue ? intValueProp.intValue : baseValueInt;
                            int intVal = EditorGUILayout.IntField(displayName, displayInt);
                            if (enabledProp.boolValue)
                                intValueProp.intValue = intVal;
                        }
                        else
                        {
                            float displayFloat = enabledProp.boolValue ? floatValueProp.floatValue : baseValueFloat;
                            float floatVal = EditorGUILayout.FloatField(displayName, displayFloat);
                            if (enabledProp.boolValue)
                                floatValueProp.floatValue = floatVal;
                        }

                        EditorGUI.EndDisabledGroup();
                    }
                    GUILayout.EndHorizontal();
                }
            }
            else
            {
                Debug.LogError("[LevelEditor] 'statOverrides.overrides' property not found on EnemySpawnData.");
            }

            EditorGUI.indentLevel--;
            GUILayout.Space(2f);

            return false;
        }

        /// <summary>
        /// Ensures the overrides list contains exactly one entry per discovered CharacterStats field,
        /// in the same order. Adds missing entries; removes stale ones.
        /// Skips work if this property path was already synced at the current fields version.
        /// </summary>
        private void SyncOverrideList(SerializedProperty overridesProp)
        {
            string propPath = overridesProp.propertyPath;
            if (_syncedAtVersion.TryGetValue(propPath, out int ver) && ver == _fieldsVersion)
                return;

            // Build a lookup of current entries by fieldName.
            _syncOverrideCache.Clear();
            var existing = _syncOverrideCache;
            for (int i = 0; i < overridesProp.arraySize; i++)
            {
                var entry      = overridesProp.GetArrayElementAtIndex(i);
                string fn      = entry.FindPropertyRelative("fieldName").stringValue;
                bool enabled   = entry.FindPropertyRelative("enabled").boolValue;
                int intVal     = entry.FindPropertyRelative("intValue").intValue;
                float floatVal = entry.FindPropertyRelative("floatValue").floatValue;
                if (!existing.ContainsKey(fn))
                    existing[fn] = (enabled, intVal, floatVal);
            }

            // Rebuild the array to match discovered fields exactly (preserving existing values).
            overridesProp.arraySize = _statsFields.Count;
            for (int i = 0; i < _statsFields.Count; i++)
            {
                string fn   = _statsFields[i].name;
                var entry   = overridesProp.GetArrayElementAtIndex(i);
                entry.FindPropertyRelative("fieldName").stringValue = fn;

                if (existing.TryGetValue(fn, out var prev))
                {
                    entry.FindPropertyRelative("enabled").boolValue     = prev.enabled;
                    entry.FindPropertyRelative("intValue").intValue     = prev.intVal;
                    entry.FindPropertyRelative("floatValue").floatValue = prev.floatVal;
                }
                else
                {
                    entry.FindPropertyRelative("enabled").boolValue     = false;
                    entry.FindPropertyRelative("intValue").intValue     = 0;
                    entry.FindPropertyRelative("floatValue").floatValue = 0f;
                }
            }

            _syncedAtVersion[propPath] = _fieldsVersion;
        }

        // ─── Helpers ─────────────────────────────────────────────────────────

        /// <summary>
        /// Returns a cached SerializedObject for the given CharacterStats, recreating only when
        /// the reference changes. Avoids allocating a new SerializedObject every OnGUI frame.
        /// </summary>
        private SerializedObject GetCachedBaseStatsSO(CharacterStats stats)
        {
            if (stats == null)
                return null;

            if (_cachedBaseStatsRef != stats)
            {
                _cachedBaseStatsRef = stats;
                _cachedBaseStatsSO = new SerializedObject(stats);
            }
            else
            {
                _cachedBaseStatsSO.Update();
            }

            return _cachedBaseStatsSO;
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
            _selectedRowStyle.normal.background  = _selectedRowTexture;
            _selectedRowStyle.normal.textColor   = Color.white;
            _selectedRowStyle.hover.background   = _selectedRowTexture;
            _selectedRowStyle.hover.textColor    = Color.white;
            return _selectedRowStyle;
        }
    }
}
