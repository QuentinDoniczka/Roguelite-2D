using RogueliteAutoBattler.Data;
using RogueliteAutoBattler.UI.Screens.SkillTree;
using UnityEditor;
using UnityEngine;

namespace RogueliteAutoBattler.Editor
{
    internal sealed class SkillTreeDesignerWindow : EditorWindow
    {
        private const float MinZoom = 0.2f;
        private const float MaxZoom = 5f;
        private const float GridSpacing = 50f;
        private const float ConfigPanelWidthRatio = 0.3f;
        private const float NodeBorderThickness = 2f;
        private const float ZoomScrollSensitivity = 0.05f;
        private const float MinGridSpacingThreshold = 5f;

        private static readonly Color CanvasBackgroundColor = new Color(0.15f, 0.15f, 0.15f, 1f);
        private static readonly Color CrosshairColor = new Color(0.4f, 0.4f, 0.4f, 1f);
        private static readonly Color GridLineColor = new Color(0.2f, 0.2f, 0.2f, 1f);

        private SkillTreeData _data;
        private SerializedObject _serializedData;
        private Vector2 _canvasOffset;
        private float _canvasZoom = 1f;
        private Vector2 _configScrollPos;
        private GUIStyle _nodeLabelStyle;
        private string[] _nodeLabels;

        [MenuItem("Roguelite/Skill Tree Designer")]
        private static void OpenWindow()
        {
            var window = GetWindow<SkillTreeDesignerWindow>("Skill Tree Designer");
            window.minSize = new Vector2(800, 500);
            window.Show();
        }

        private void OnEnable()
        {
            _data = AssetDatabase.LoadAssetAtPath<SkillTreeData>(SkillTreeData.DefaultAssetPath);
            if (_data == null)
            {
                EditorUIFactory.EnsureDirectoryExists(SkillTreeData.DefaultAssetPath);
                _data = CreateInstance<SkillTreeData>();
                AssetDatabase.CreateAsset(_data, SkillTreeData.DefaultAssetPath);
                AssetDatabase.SaveAssets();
            }
            _serializedData = new SerializedObject(_data);
            RebuildNodeLabels();
        }

        private void OnGUI()
        {
            if (_data == null)
            {
                EditorGUILayout.HelpBox("No SkillTreeData asset found.", MessageType.Warning);
                return;
            }

            EditorGUILayout.BeginHorizontal();

            float configWidth = position.width * ConfigPanelWidthRatio;
            float canvasWidth = position.width - configWidth;

            Rect canvasRect = new Rect(0, 0, canvasWidth, position.height);
            DrawCanvas(canvasRect);
            HandleCanvasInput(canvasRect);

            GUILayout.BeginArea(new Rect(canvasWidth, 0, configWidth, position.height));
            DrawConfigPanel();
            GUILayout.EndArea();

            EditorGUILayout.EndHorizontal();
        }

        private void DrawCanvas(Rect canvasRect)
        {
            EditorGUI.DrawRect(canvasRect, CanvasBackgroundColor);

            GUI.BeginClip(canvasRect);

            Vector2 center = new Vector2(canvasRect.width * 0.5f, canvasRect.height * 0.5f);
            Vector2 origin = center + _canvasOffset;

            DrawGrid(canvasRect, origin);

            EditorGUI.DrawRect(new Rect(origin.x - 0.5f, 0, 1, canvasRect.height), CrosshairColor);
            EditorGUI.DrawRect(new Rect(0, origin.y - 0.5f, canvasRect.width, 1), CrosshairColor);

            if (_data.Nodes != null)
            {
                if (_nodeLabelStyle == null)
                {
                    _nodeLabelStyle = new GUIStyle(EditorStyles.miniLabel)
                    {
                        alignment = TextAnchor.MiddleCenter,
                        normal = { textColor = Color.white }
                    };
                }

                float scaledNodeSize = _data.NodeSize * _canvasZoom;
                float halfNode = scaledNodeSize * 0.5f;

                for (int i = 0; i < _data.Nodes.Count; i++)
                {
                    var entry = _data.Nodes[i];
                    Vector2 screenPos = origin + entry.position * _data.UnitSize * _canvasZoom;
                    Rect nodeRect = new Rect(screenPos.x - halfNode, screenPos.y - halfNode, scaledNodeSize, scaledNodeSize);

                    float scaledBorder = NodeBorderThickness * _canvasZoom;
                    Rect borderRect = new Rect(
                        nodeRect.x - scaledBorder,
                        nodeRect.y - scaledBorder,
                        nodeRect.width + scaledBorder * 2,
                        nodeRect.height + scaledBorder * 2);
                    EditorGUI.DrawRect(borderRect, _data.BorderNormalColor);

                    EditorGUI.DrawRect(nodeRect, _data.NodeColor);

                    string label = _nodeLabels != null && i < _nodeLabels.Length ? _nodeLabels[i] : entry.id.ToString();
                    GUI.Label(nodeRect, label, _nodeLabelStyle);
                }
            }

            GUI.EndClip();
        }

        private void DrawGrid(Rect canvasRect, Vector2 origin)
        {
            float spacing = GridSpacing * _canvasZoom;

            if (spacing < MinGridSpacingThreshold) return;

            float startX = origin.x % spacing;
            for (float x = startX; x < canvasRect.width; x += spacing)
                EditorGUI.DrawRect(new Rect(x, 0, 1, canvasRect.height), GridLineColor);

            float startY = origin.y % spacing;
            for (float y = startY; y < canvasRect.height; y += spacing)
                EditorGUI.DrawRect(new Rect(0, y, canvasRect.width, 1), GridLineColor);
        }

        private void HandleCanvasInput(Rect canvasRect)
        {
            Event evt = Event.current;
            if (!canvasRect.Contains(evt.mousePosition)) return;

            if (evt.type == EventType.ScrollWheel)
            {
                float zoomDelta = 1f - evt.delta.y * ZoomScrollSensitivity;
                _canvasZoom = Mathf.Clamp(_canvasZoom * zoomDelta, MinZoom, MaxZoom);
                evt.Use();
                Repaint();
            }

            if (evt.type == EventType.MouseDrag && (evt.button == 2 || (evt.button == 0 && evt.alt)))
            {
                _canvasOffset += evt.delta;
                evt.Use();
                Repaint();
            }
        }

        private void DrawConfigPanel()
        {
            _serializedData.Update();

            _configScrollPos = EditorGUILayout.BeginScrollView(_configScrollPos);

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Skill Tree Designer", EditorStyles.boldLabel);
            EditorGUILayout.Space(4);

            EditorGUI.BeginChangeCheck();
            var newData = (SkillTreeData)EditorGUILayout.ObjectField("Data Asset", _data, typeof(SkillTreeData), false);
            if (EditorGUI.EndChangeCheck() && newData != null)
            {
                _data = newData;
                _serializedData = new SerializedObject(_data);
            }

            EditorGUILayout.Space(12);
            EditorGUILayout.LabelField("Generation Parameters", EditorStyles.boldLabel);

            EditorGUILayout.IntSlider(_serializedData.FindProperty("nodeCount"), 1, 100, new GUIContent("Node Count"));
            EditorGUILayout.PropertyField(_serializedData.FindProperty("seed"), new GUIContent("Seed"));
            EditorGUILayout.Slider(_serializedData.FindProperty("placementRadius"), 1f, 20f, new GUIContent("Placement Radius"));

            EditorGUILayout.Space(12);
            EditorGUILayout.LabelField("Visual Settings", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(_serializedData.FindProperty("unitSize"), new GUIContent("Unit Size"));
            EditorGUILayout.PropertyField(_serializedData.FindProperty("nodeSize"), new GUIContent("Node Size"));
            EditorGUILayout.PropertyField(_serializedData.FindProperty("nodeColor"), new GUIContent("Node Color"));
            EditorGUILayout.PropertyField(_serializedData.FindProperty("borderNormalColor"), new GUIContent("Border Normal"));
            EditorGUILayout.PropertyField(_serializedData.FindProperty("borderSelectedColor"), new GUIContent("Border Selected"));

            EditorGUILayout.Space(16);

            if (GUILayout.Button("Generate", GUILayout.Height(30)))
            {
                _serializedData.ApplyModifiedProperties();
                _data.GenerateNodes();
                RebuildNodeLabels();
                EditorUtility.SetDirty(_data);
                AssetDatabase.SaveAssets();
                _serializedData.Update();
                Repaint();
            }

            EditorGUILayout.Space(8);

            if (GUILayout.Button("Apply to Scene", GUILayout.Height(30)))
            {
                ApplyToScene();
            }

            EditorGUILayout.Space(8);

            int nodeCountDisplay = _data.Nodes != null ? _data.Nodes.Count : 0;
            EditorGUILayout.HelpBox($"{nodeCountDisplay} nodes generated.", MessageType.Info);

            if (_serializedData.ApplyModifiedProperties())
                Repaint();

            EditorGUILayout.EndScrollView();
        }

        private void ApplyToScene()
        {
            var managers = FindObjectsByType<SkillTreeNodeManager>(FindObjectsSortMode.None);
            if (managers.Length == 0)
            {
                Debug.LogWarning("[SkillTreeDesigner] No SkillTreeNodeManager found in the scene. Run the scene builder first.");
                return;
            }

            var manager = managers[0];
            manager.ClearNodes();

            var managerSO = new SerializedObject(manager);
            EditorUIFactory.SetObj(managerSO, "_data", _data);
            managerSO.ApplyModifiedProperties();

            manager.Initialize();
            EditorUtility.SetDirty(manager);

            Debug.Log($"[SkillTreeDesigner] Applied {_data.Nodes.Count} nodes to scene SkillTreeNodeManager.");
        }

        private void RebuildNodeLabels()
        {
            if (_data == null || _data.Nodes == null || _data.Nodes.Count == 0)
            {
                _nodeLabels = null;
                return;
            }

            _nodeLabels = new string[_data.Nodes.Count];
            for (int i = 0; i < _data.Nodes.Count; i++)
                _nodeLabels[i] = _data.Nodes[i].id.ToString();
        }
    }
}
