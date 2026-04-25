using System.Collections.Generic;
using RogueliteAutoBattler.Data;
using UnityEditor;
using UnityEngine;

namespace RogueliteAutoBattler.Editor.Windows
{
    internal sealed class TeamBuilderTab
    {
        private const float TeamRowHeight        = 24f;
        private const float TeamSmallButtonWidth = 24f;
        private const float TeamAddButtonHeight  = 22f;
        internal const string TeamDefaultAllyPrefabPath = "Assets/Prefabs/Characters/sampleCharacterHuman.prefab";
        internal const string TeamDatabaseDefaultPath   = "Assets/Data/TeamDatabase.asset";

        internal const string TeamDefaultAllyName         = "Warrior";
        internal const int    TeamDefaultMaxHp            = 100;
        internal const int    TeamDefaultAtk              = 10;
        internal const float  TeamDefaultAttackSpeed      = 1f;
        internal const float  TeamDefaultMoveSpeed        = 2f;
        internal const float  TeamDefaultRegenHpPerSec    = 0f;
        internal const float  TeamDefaultColliderRadius   = 0.10f;

        private readonly EditorWindow _owner;

        private TeamDatabase   _teamDatabase;
        private SerializedObject _teamSerializedDatabase;
        private Vector2 _teamScrollPos;
        private readonly Dictionary<string, bool> _teamFoldouts = new Dictionary<string, bool>();

        internal TeamBuilderTab(EditorWindow owner)
        {
            _owner = owner;
        }

        internal void OnEnable()
        {
            TeamTryAutoLoadDatabase();
        }

        internal void OnDisable()
        {
            _teamSerializedDatabase = null;
        }

        internal void Draw()
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
            _owner.Repaint();
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
            TeamSetFloatSafe(allyProp, "colliderRadius",    TeamDefaultColliderRadius);
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

            EditorUIFactory.DrawAppearanceFields(allyProp);

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
    }
}
