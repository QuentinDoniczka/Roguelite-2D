using RogueliteAutoBattler.Combat;
using RogueliteAutoBattler.Data;
using UnityEditor;
using UnityEngine;

namespace RogueliteAutoBattler.Editor
{
    public class SettingsWindow : EditorWindow
    {
        private const string ConfigPath = "Assets/Data/DamageNumberConfig.asset";
        private const float SectionSpacing = 8f;
        private const float HeaderSpacing = 4f;
        private static readonly Vector2 MinWindowSize = new Vector2(400, 340);

        private static readonly GUIContent LabelEnabled = new GUIContent("Enabled");
        private static readonly GUIContent LabelFont = new GUIContent("Font (TMP)");
        private static readonly GUIContent LabelFontSize = new GUIContent("Font Size");
        private static readonly GUIContent LabelLifetime = new GUIContent("Lifetime (s)");
        private static readonly GUIContent LabelSlideDirection = new GUIContent("Slide Direction");
        private static readonly GUIContent LabelSlideDistance = new GUIContent("Slide Distance");
        private static readonly GUIContent LabelSpawnOffsetY = new GUIContent("Spawn Offset Y");
        private static readonly GUIContent LabelAllyColor = new GUIContent("Ally Color");
        private static readonly GUIContent LabelEnemyColor = new GUIContent("Enemy Color");
        private static readonly GUIContent LabelOutlineWidth = new GUIContent("Outline Width");
        private static readonly GUIContent LabelOutlineColor = new GUIContent("Outline Color");
        private static readonly GUIContent LabelSortingOrder = new GUIContent("Sorting Order");
        private static readonly GUIContent LabelPoolSize = new GUIContent("Pool Size");

        private DamageNumberConfig _config;
        private SerializedObject _serializedConfig;
        private Vector2 _scrollPos;

        private SerializedProperty _propEnabled;
        private SerializedProperty _propFont;
        private SerializedProperty _propFontSize;
        private SerializedProperty _propLifetime;
        private SerializedProperty _propSlideDirection;
        private SerializedProperty _propSlideDistance;
        private SerializedProperty _propSpawnOffsetY;
        private SerializedProperty _propAllyDamageColor;
        private SerializedProperty _propEnemyDamageColor;
        private SerializedProperty _propOutlineWidth;
        private SerializedProperty _propOutlineColor;
        private SerializedProperty _propSortingOrder;
        private SerializedProperty _propInitialPoolSize;

        [MenuItem("Roguelite/Settings")]
        private static void OpenWindow()
        {
            var window = GetWindow<SettingsWindow>("Settings");
            window.minSize = MinWindowSize;
            window.Show();
        }

        private void OnEnable()
        {
            _config = AssetDatabase.LoadAssetAtPath<DamageNumberConfig>(ConfigPath);
            if (_config == null)
                return;

            _serializedConfig = new SerializedObject(_config);
            CacheSerializedProperties();
        }

        private void OnDisable()
        {
            _serializedConfig = null;
        }

        private void CacheSerializedProperties()
        {
            _propEnabled = _serializedConfig.FindProperty("_enabled");
            _propFont = _serializedConfig.FindProperty("_font");
            _propFontSize = _serializedConfig.FindProperty("_fontSize");
            _propLifetime = _serializedConfig.FindProperty("_lifetime");
            _propSlideDirection = _serializedConfig.FindProperty("_slideDirection");
            _propSlideDistance = _serializedConfig.FindProperty("_slideDistance");
            _propSpawnOffsetY = _serializedConfig.FindProperty("_spawnOffsetY");
            _propAllyDamageColor = _serializedConfig.FindProperty("_allyDamageColor");
            _propEnemyDamageColor = _serializedConfig.FindProperty("_enemyDamageColor");
            _propOutlineWidth = _serializedConfig.FindProperty("_outlineWidth");
            _propOutlineColor = _serializedConfig.FindProperty("_outlineColor");
            _propSortingOrder = _serializedConfig.FindProperty("_sortingOrder");
            _propInitialPoolSize = _serializedConfig.FindProperty("_initialPoolSize");
        }

        private void OnGUI()
        {
            if (_serializedConfig == null)
            {
                EditorGUILayout.HelpBox("DamageNumberConfig not found at " + ConfigPath, MessageType.Warning);
                if (GUILayout.Button("Reload"))
                    OnEnable();
                return;
            }

            _serializedConfig.Update();
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            EditorGUILayout.LabelField("Damage Numbers", EditorStyles.boldLabel);
            EditorGUILayout.Space(HeaderSpacing);

            EditorGUILayout.PropertyField(_propEnabled, LabelEnabled);
            EditorGUILayout.Space(SectionSpacing);

            EditorGUILayout.PropertyField(_propFont, LabelFont);
            EditorGUILayout.PropertyField(_propFontSize, LabelFontSize);
            EditorGUILayout.PropertyField(_propLifetime, LabelLifetime);
            EditorGUILayout.PropertyField(_propSlideDirection, LabelSlideDirection);
            EditorGUILayout.PropertyField(_propSlideDistance, LabelSlideDistance);
            EditorGUILayout.PropertyField(_propSpawnOffsetY, LabelSpawnOffsetY);
            EditorGUILayout.Space(SectionSpacing);

            EditorGUILayout.PropertyField(_propOutlineWidth, LabelOutlineWidth);
            EditorGUILayout.PropertyField(_propOutlineColor, LabelOutlineColor);
            EditorGUILayout.Space(SectionSpacing);

            EditorGUILayout.PropertyField(_propAllyDamageColor, LabelAllyColor);
            EditorGUILayout.PropertyField(_propEnemyDamageColor, LabelEnemyColor);
            EditorGUILayout.Space(SectionSpacing);

            EditorGUILayout.PropertyField(_propSortingOrder, LabelSortingOrder);
            EditorGUILayout.PropertyField(_propInitialPoolSize, LabelPoolSize);

            EditorGUILayout.EndScrollView();

            if (_serializedConfig.ApplyModifiedProperties())
            {
                DamageNumberSettingsPersistence.DeleteAll();
                EditorUtility.SetDirty(_config);
                AssetDatabase.SaveAssets();
            }
        }
    }
}
