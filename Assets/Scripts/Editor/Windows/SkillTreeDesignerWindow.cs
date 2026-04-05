using System.Collections.Generic;
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

        private static readonly GUIContent LabelRingNodeCount = new GUIContent("Ring Node Count");
        private static readonly GUIContent LabelRingRadius = new GUIContent("Ring Radius");
        private static readonly GUIContent LabelUnitSize = new GUIContent("Unit Size");
        private static readonly GUIContent LabelNodeSize = new GUIContent("Node Size");
        private static readonly GUIContent LabelNodeColor = new GUIContent("Node Color");
        private static readonly GUIContent LabelBorderNormal = new GUIContent("Border Normal");
        private static readonly GUIContent LabelBorderSelected = new GUIContent("Border Selected");
        private static readonly GUIContent LabelBaseCost = new GUIContent("Base Cost");
        private static readonly GUIContent LabelCostMultiplier = new GUIContent("Multiplier / Level");
        private static readonly GUIContent LabelCostAdditive = new GUIContent("Additive / Level");
        private static readonly GUIContent LabelEdgeColor = new GUIContent("Edge Color");
        private static readonly GUIContent LabelRingGuideColor = new GUIContent("Ring Guide Color");
        private static readonly GUIContent LabelEdgeThickness = new GUIContent("Edge Thickness");

        private SkillTreeData _data;
        private SerializedObject _serializedData;
        private SerializedProperty _propRingNodeCount;
        private SerializedProperty _propRingRadius;
        private SerializedProperty _propUnitSize;
        private SerializedProperty _propNodeSize;
        private SerializedProperty _propNodeColor;
        private SerializedProperty _propBorderNormalColor;
        private SerializedProperty _propBorderSelectedColor;
        private SerializedProperty _propBaseCost;
        private SerializedProperty _propCostMultiplierPerLevel;
        private SerializedProperty _propCostAdditivePerLevel;
        private SerializedProperty _propEdgeColor;
        private SerializedProperty _propRingGuideColor;
        private SerializedProperty _propEdgeThickness;
        private Vector2 _canvasOffset;
        private float _canvasZoom = 1f;
        private Vector2 _configScrollPos;
        private GUIStyle _nodeLabelStyle;
        private string[] _nodeLabels;
        private int _selectedNodeIndex = -1;

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
            CacheSerializedProperties();
            RebuildNodeLabels();
        }

        private void CacheSerializedProperties()
        {
            _propRingNodeCount = _serializedData.FindProperty("ringNodeCount");
            _propRingRadius = _serializedData.FindProperty("ringRadius");
            _propUnitSize = _serializedData.FindProperty("unitSize");
            _propNodeSize = _serializedData.FindProperty("nodeSize");
            _propNodeColor = _serializedData.FindProperty("nodeColor");
            _propBorderNormalColor = _serializedData.FindProperty("borderNormalColor");
            _propBorderSelectedColor = _serializedData.FindProperty("borderSelectedColor");
            _propBaseCost = _serializedData.FindProperty("baseCost");
            _propCostMultiplierPerLevel = _serializedData.FindProperty("costMultiplierPerLevel");
            _propCostAdditivePerLevel = _serializedData.FindProperty("costAdditivePerLevel");
            _propEdgeColor = _serializedData.FindProperty("edgeColor");
            _propRingGuideColor = _serializedData.FindProperty("ringGuideColor");
            _propEdgeThickness = _serializedData.FindProperty("edgeThickness");
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

            if (_nodeLabelStyle == null)
            {
                _nodeLabelStyle = new GUIStyle(EditorStyles.miniLabel)
                {
                    alignment = TextAnchor.MiddleCenter,
                    normal = { textColor = Color.white }
                };
            }

            float scaledUnit = _data.UnitSize * _canvasZoom;
            float scaledNodeSize = _data.NodeSize * _canvasZoom;
            float halfNode = scaledNodeSize * 0.5f;
            float scaledBorder = NodeBorderThickness * _canvasZoom;

            Handles.BeginGUI();

            Handles.color = _data.RingGuideColor;
            float ringRadiusPixels = _data.RingRadius * scaledUnit;
            Handles.DrawWireDisc(new Vector3(origin.x, origin.y, 0f), Vector3.forward, ringRadiusPixels);

            Handles.color = _data.EdgeColor;
            var edges = _data.GetEdges();
            foreach (var (fromId, toId) in edges)
            {
                if (fromId >= _data.Nodes.Count || toId >= _data.Nodes.Count) continue;
                Vector2 fromPos = origin + _data.Nodes[fromId].position * scaledUnit;
                Vector2 toPos = origin + _data.Nodes[toId].position * scaledUnit;
                Handles.DrawLine(new Vector3(fromPos.x, fromPos.y, 0f), new Vector3(toPos.x, toPos.y, 0f));
            }

            for (int i = 0; i < _data.Nodes.Count; i++)
            {
                var entry = _data.Nodes[i];
                Vector2 screenPos = origin + entry.position * scaledUnit;
                Vector3 center3D = new Vector3(screenPos.x, screenPos.y, 0f);
                Rect nodeRect = new Rect(screenPos.x - halfNode, screenPos.y - halfNode, scaledNodeSize, scaledNodeSize);

                Handles.color = (i == _selectedNodeIndex) ? _data.BorderSelectedColor : _data.BorderNormalColor;
                Handles.DrawSolidDisc(center3D, Vector3.forward, halfNode + scaledBorder);

                Handles.color = _data.NodeColor;
                Handles.DrawSolidDisc(center3D, Vector3.forward, halfNode);

                string label = _nodeLabels != null && i < _nodeLabels.Length ? _nodeLabels[i] : entry.id.ToString();
                GUI.Label(nodeRect, label, _nodeLabelStyle);
            }
            Handles.EndGUI();

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

            if (evt.type == EventType.MouseDown && evt.button == 0 && !evt.alt)
            {
                Vector2 mouseInCanvas = evt.mousePosition;
                Vector2 center = new Vector2(canvasRect.width * 0.5f, canvasRect.height * 0.5f);
                Vector2 origin = center + _canvasOffset;

                int hitIndex = HitTestNode(mouseInCanvas, origin, _data.Nodes, _data.UnitSize, _data.NodeSize, _canvasZoom);
                _selectedNodeIndex = hitIndex;
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
                CacheSerializedProperties();
            }

            EditorGUILayout.Space(12);
            EditorGUILayout.LabelField("Generation Parameters", EditorStyles.boldLabel);

            EditorGUILayout.IntSlider(_propRingNodeCount, 3, 24, LabelRingNodeCount);
            EditorGUILayout.Slider(_propRingRadius, 1f, 20f, LabelRingRadius);

            EditorGUILayout.Space(12);
            EditorGUILayout.LabelField("Visual Settings", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(_propUnitSize, LabelUnitSize);
            EditorGUILayout.PropertyField(_propNodeSize, LabelNodeSize);
            EditorGUILayout.PropertyField(_propNodeColor, LabelNodeColor);
            EditorGUILayout.PropertyField(_propBorderNormalColor, LabelBorderNormal);
            EditorGUILayout.PropertyField(_propBorderSelectedColor, LabelBorderSelected);

            EditorGUILayout.Space(12);
            EditorGUILayout.LabelField("Cost Formula", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("cost(lvl) = floor(base * mult^lvl) + add * lvl", MessageType.None);
            EditorGUILayout.PropertyField(_propBaseCost, LabelBaseCost);
            EditorGUILayout.PropertyField(_propCostMultiplierPerLevel, LabelCostMultiplier);
            EditorGUILayout.PropertyField(_propCostAdditivePerLevel, LabelCostAdditive);

            EditorGUILayout.Space(12);
            EditorGUILayout.LabelField("Edge Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_propEdgeColor, LabelEdgeColor);
            EditorGUILayout.PropertyField(_propRingGuideColor, LabelRingGuideColor);
            EditorGUILayout.PropertyField(_propEdgeThickness, LabelEdgeThickness);

            EditorGUILayout.Space(16);

            if (GUILayout.Button("Generate", GUILayout.Height(30)))
            {
                _serializedData.ApplyModifiedProperties();
                _data.GenerateNodes();
                _selectedNodeIndex = -1;
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

            EditorGUILayout.HelpBox($"{_data.Nodes.Count} nodes generated.", MessageType.Info);

            if (_selectedNodeIndex >= 0 && _selectedNodeIndex < _data.Nodes.Count)
            {
                EditorGUILayout.Space(16);
                EditorGUILayout.LabelField($"Node {_selectedNodeIndex} Properties", EditorStyles.boldLabel);

                var node = _data.Nodes[_selectedNodeIndex];

                EditorGUI.BeginChangeCheck();

                var newNodeType = (SkillTreeData.NodeType)EditorGUILayout.EnumPopup("Node Type", node.nodeType);
                var newCostType = (SkillTreeData.CostType)EditorGUILayout.EnumPopup("Cost Type", node.costType);
                int newMaxLevel = EditorGUILayout.IntField("Max Level", node.maxLevel);
                var newStatModType = (SkillTreeData.StatModifierType)EditorGUILayout.EnumPopup("Stat Modifier", node.statModifierType);
                float newStatModValue = EditorGUILayout.FloatField("Value Per Level", node.statModifierValuePerLevel);

                if (EditorGUI.EndChangeCheck())
                {
                    var updated = node;
                    updated.nodeType = newNodeType;
                    updated.costType = newCostType;
                    updated.maxLevel = newMaxLevel;
                    updated.statModifierType = newStatModType;
                    updated.statModifierValuePerLevel = newStatModValue;

                    _data.SetNode(_selectedNodeIndex, updated);
                    EditorUtility.SetDirty(_data);
                    AssetDatabase.SaveAssets();
                }

                EditorGUILayout.Space(8);
                EditorGUILayout.LabelField("Cost Preview", EditorStyles.boldLabel);
                for (int lvl = 0; lvl < Mathf.Min(node.maxLevel, 10); lvl++)
                {
                    int cost = _data.ComputeNodeCost(lvl);
                    EditorGUILayout.LabelField($"  Level {lvl} → {lvl + 1}", $"{cost} {node.costType}");
                }
            }

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
            EditorUIFactory.SetColor(managerSO, "_edgeColor", _data.EdgeColor);
            EditorUIFactory.SetFloat(managerSO, "_edgeThickness", _data.EdgeThickness);
            managerSO.ApplyModifiedProperties();

            manager.Initialize();
            EditorUtility.SetDirty(manager);

            Debug.Log($"[SkillTreeDesigner] Applied {_data.Nodes.Count} nodes to scene SkillTreeNodeManager.");
        }

        internal static int HitTestNode(Vector2 mousePos, Vector2 origin, IReadOnlyList<SkillTreeData.SkillNodeEntry> nodes, float unitSize, float nodeSize, float zoom)
        {
            float scaledUnit = unitSize * zoom;
            float halfNode = nodeSize * zoom * 0.5f;
            float halfNodeSq = halfNode * halfNode;

            for (int i = nodes.Count - 1; i >= 0; i--)
            {
                Vector2 nodeScreenPos = origin + nodes[i].position * scaledUnit;
                float distSq = (mousePos - nodeScreenPos).sqrMagnitude;
                if (distSq <= halfNodeSq)
                    return i;
            }
            return -1;
        }

        private void RebuildNodeLabels()
        {
            if (_data == null || _data.Nodes.Count == 0)
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
