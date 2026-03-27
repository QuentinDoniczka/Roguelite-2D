using RogueliteAutoBattler.Data;
using UnityEditor;
using UnityEngine;

namespace RogueliteAutoBattler.Editor
{
    public class SettingsWindow : EditorWindow
    {
        private const string ConfigPath = "Assets/Data/DamageNumberConfig.asset";

        private DamageNumberConfig _config;
        private SerializedObject _serializedConfig;
        private Vector2 _scrollPos;

        [MenuItem("Roguelite/Settings")]
        private static void OpenWindow()
        {
            var window = GetWindow<SettingsWindow>("Settings");
            window.minSize = new Vector2(400, 340);
            window.Show();
        }

        private void OnEnable()
        {
            _config = AssetDatabase.LoadAssetAtPath<DamageNumberConfig>(ConfigPath);
            if (_config != null)
                _serializedConfig = new SerializedObject(_config);
        }

        private void OnDisable()
        {
            _serializedConfig = null;
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
            EditorGUILayout.Space(4f);

            EditorGUILayout.PropertyField(_serializedConfig.FindProperty("_enabled"), new GUIContent("Enabled"));
            EditorGUILayout.Space(8f);

            EditorGUILayout.PropertyField(_serializedConfig.FindProperty("_font"), new GUIContent("Font (TMP)"));
            EditorGUILayout.PropertyField(_serializedConfig.FindProperty("_fontSize"), new GUIContent("Font Size"));
            EditorGUILayout.PropertyField(_serializedConfig.FindProperty("_lifetime"), new GUIContent("Lifetime (s)"));
            EditorGUILayout.PropertyField(_serializedConfig.FindProperty("_slideDirection"), new GUIContent("Slide Direction"));
            EditorGUILayout.PropertyField(_serializedConfig.FindProperty("_slideDistance"), new GUIContent("Slide Distance"));
            EditorGUILayout.PropertyField(_serializedConfig.FindProperty("_spawnOffsetY"), new GUIContent("Spawn Offset Y"));
            EditorGUILayout.Space(8f);

            EditorGUILayout.PropertyField(_serializedConfig.FindProperty("_allyDamageColor"), new GUIContent("Ally Color"));
            EditorGUILayout.PropertyField(_serializedConfig.FindProperty("_enemyDamageColor"), new GUIContent("Enemy Color"));
            EditorGUILayout.Space(8f);

            EditorGUILayout.PropertyField(_serializedConfig.FindProperty("_sortingOrder"), new GUIContent("Sorting Order"));
            EditorGUILayout.PropertyField(_serializedConfig.FindProperty("_initialPoolSize"), new GUIContent("Pool Size"));

            EditorGUILayout.EndScrollView();

            if (_serializedConfig.ApplyModifiedProperties())
            {
                EditorUtility.SetDirty(_config);
                AssetDatabase.SaveAssets();
            }
        }
    }
}
