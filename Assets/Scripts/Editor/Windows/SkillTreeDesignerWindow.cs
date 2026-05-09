using System.Collections.Generic;
using RogueliteAutoBattler.Combat.Core;
using RogueliteAutoBattler.Data;
using RogueliteAutoBattler.Editor.Tools;
using RogueliteAutoBattler.UI.Toolkit.SkillTree;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RogueliteAutoBattler.Editor.Windows
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
        private const float MinBranchPreviewDistance = 0.5f;
        private const float MaxBranchPreviewDistance = 10f;
        private const float MinBranchAngleDegrees = 0f;
        private const float MaxBranchAngleDegrees = 360f;
        private const float BranchPreviewDottedSegmentSize = 6f;
        private const int CostPreviewMaxRows = 10;
        private const float MinWindowWidth = 800f;
        private const float MinWindowHeight = 500f;
        private const int LeftMouseButton = 0;
        private const int MiddleMouseButton = 2;
        private const float SectionSpacingSmall = 8f;
        private const float SectionSpacingMedium = 12f;
        private const float SectionSpacingLarge = 16f;
        private const float CrosshairLineThickness = 1f;
        private const float CrosshairHalfPixelOffset = 0.5f;
        private const float DragStartThresholdPx = 4f;
        private const float TopAndSubTabBarReservedHeightPixels = 60f;
        private const float VisualTabCanvasFlexGrow = 0.6f;
        private const float VisualTabInspectorFlexGrow = 0.4f;
        private const float VisualTabInspectorMinWidthPixels = 280f;
        private const float VisualTabInspectorPaddingPixels = 8f;

        private enum DesignerTab
        {
            SkillTree,
            Preview,
            Node,
            Branch
        }

        internal enum TopLevelTab
        {
            Designer,
            Visual
        }

        private static readonly DesignerTab[] VisibleTabsWithoutBranch =
        {
            DesignerTab.SkillTree, DesignerTab.Preview, DesignerTab.Node
        };

        private static readonly DesignerTab[] VisibleTabsWithBranch =
        {
            DesignerTab.SkillTree, DesignerTab.Preview, DesignerTab.Node, DesignerTab.Branch
        };

        private static readonly Color CanvasBackgroundColor = new Color(0.15f, 0.15f, 0.15f, 1f);
        private static readonly Color CrosshairColor = new Color(0.4f, 0.4f, 0.4f, 1f);
        private static readonly Color GridLineColor = new Color(0.2f, 0.2f, 0.2f, 1f);
        private static readonly Color BranchPreviewTintColor = new Color(1f, 1f, 1f, 0.4f);
        private static readonly Color MirrorAccentRgb = new Color(1f, 0.92f, 0.016f);
        private static readonly Color MirrorAxisLineColor = new Color(MirrorAccentRgb.r, MirrorAccentRgb.g, MirrorAccentRgb.b, 0.6f);
        private static readonly Color MirrorPreviewTintColor = new Color(0f, 1f, 1f, 0.4f);
        private static readonly Color MirrorSourceRingColor = new Color(MirrorAccentRgb.r, MirrorAccentRgb.g, MirrorAccentRgb.b, 0.5f);
        private const float MirrorSourceRingThicknessMultiplier = 1.5f;
        private static readonly Color AlignmentRadiusCircleColor = new Color(0.5f, 0.7f, 1f, 0.3f);

        private static readonly GUIContent LabelUnitSize = new GUIContent("Unit Size");
        private static readonly GUIContent LabelNodeSize = new GUIContent("Node Size");
        private static readonly GUIContent LabelNodeColor = new GUIContent("Node Color");
        private static readonly GUIContent LabelBorderNormal = new GUIContent("Border Normal");
        private static readonly GUIContent LabelBorderSelected = new GUIContent("Border Selected");
        private static readonly GUIContent LabelBaseCost = new GUIContent("Base Cost");
        private static readonly GUIContent LabelCostMultiplierOdd = new GUIContent("Multiplier (Odd Levels)");
        private static readonly GUIContent LabelCostMultiplierEven = new GUIContent("Multiplier (Even Levels)");
        private static readonly GUIContent LabelCostAdditive = new GUIContent("Additive / Level");
        private static readonly GUIContent LabelDefaultGeneratedMaxLevel = new GUIContent("Default Max Level (0 = unlimited)");
        private static readonly GUIContent LabelEdgeColor = new GUIContent("Edge Color");
        private static readonly GUIContent LabelEdgeThickness = new GUIContent("Edge Thickness");

        private const string AngleSliderLabel = "Angle (deg, 0=N, 90=E)";
        private const float MinMirrorAxisDegrees = 0f;
        private const float MaxMirrorAxisDegrees = 360f;
        private const string MirrorEnabledLabel = "Mirror";
        private const string MirrorAxisSliderLabel = "Mirror Axis (deg, 0=vertical)";
        private const string PickMirrorSourceLabel = "Pick Mirror Source";
        private const string CancelPickLabel = "Cancel Pick";
        private const string MirrorSourceStatusWorkingNode = "Source: working node";
        private const string MirrorSourceStatusFormatNode = "Source: Node {0}";
        private const string AngleRelativeToggleLabel = "Angle from mirror axis";
        private const string ResolvedAbsoluteAngleFormat = "Resolved absolute angle: {0:F1}°";

        private const float MinSnapThresholdUnits = 0f;
        private const float MaxSnapThresholdUnits = 2f;
        private const float CoordLabelOffsetXPixels = 4f;
        private const float CoordLabelOffsetYPixels = 8f;
        private const float CoordLabelWidthPixels = 120f;
        private const float CoordLabelHeightPixels = 16f;
        private const float ColorSwatchWidth = 24f;
        private const float ColorSwatchHeight = 18f;
        private static readonly Color SnapGuideLineColor = new Color(0.4f, 0.85f, 1f, 0.7f);
        private const string SnapEnabledLabel = "Snap to Nearby Node";
        private const string SnapThresholdLabel = "Snap Threshold (units)";
        private const string AlignmentRadiusLabel = "Alignment Radius (units)";
        private const string AlignmentRadiusVisibleLabel = "Show Radius During Drag";
        private const float MinAlignmentRadiusUnits = 0f;
        private const float MaxAlignmentRadiusUnits = 20f;

        private const string SelectedAssetGuidEditorPrefKey = "SkillTreeDesigner.SelectedAssetGuid";
        private const string TopLevelTabEditorPrefKey = "SkillTreeDesigner.TopLevelTab";
        private const string ActiveLabelPrefix = "Active: ";
        private const string NoActiveLabel = "<none>";

        private const string DefaultNewTreeFileName = "NewSkillTree";
        private const string AssetExtension = "asset";
        private const string DialogTitleCreateTree = "Create Skill Tree";
        private const string DialogTitleInvalidPath = "Invalid Path";
        private const string DialogTitleDeleteTree = "Delete Skill Tree";
        private const string DialogConfirmOk = "OK";
        private const string DialogConfirmDelete = "Delete";
        private const string DialogConfirmCancel = "Cancel";
        private const string ButtonLabelSetActive = "Set as Active";
        private const string ButtonLabelNew = "New";
        private const string ButtonLabelDuplicate = "Duplicate";
        private const string ButtonLabelDelete = "Delete";
        private const string DisabledTooltipNoSelection = "No skill tree selected.";
        private const string DisabledTooltipAlreadyActive = "This tree is already active.";
        private const string DisabledTooltipCannotDeleteActive = "Cannot delete the active skill tree. Switch to another tree first.";
        private const string DisabledTooltipCannotDeleteLast = "Cannot delete the last skill tree.";
        private const string DisabledTooltipDeleteEnabled = "Delete this skill tree.";
        private const int MinimumRetainedTreeCount = 1;

        private static readonly string[] TabLabelsWithoutBranch = { "Skill Tree", "Preview", "Node" };
        private static readonly string[] TabLabelsWithBranch = { "Skill Tree", "Preview", "Node", "Branch" };
        private static readonly string[] TopLevelTabLabels = { "Designer", "Visual" };

        private IReadOnlyList<SkillTreesEnumerator.TreeEntry> _treeEntries;
        private string[] _treeDisplayNames;
        private int _selectedTreeIndex = -1;
        private SkillTreeData _activePointerTarget;

        private SkillTreeData _data;
        private SerializedObject _serializedData;
        private SerializedProperty _propUnitSize;
        private SerializedProperty _propNodeSize;
        private SerializedProperty _propNodeColor;
        private SerializedProperty _propBorderNormalColor;
        private SerializedProperty _propBorderSelectedColor;
        private SerializedProperty _propBaseCost;
        private SerializedProperty _propCostMultiplierOdd;
        private SerializedProperty _propCostMultiplierEven;
        private SerializedProperty _propCostAdditivePerLevel;
        private SerializedProperty _propDefaultGeneratedMaxLevel;
        private SerializedProperty _propEdgeColor;
        private SerializedProperty _propEdgeThickness;
        private Vector2 _canvasOffset;
        private float _canvasZoom = 1f;
        private Vector2 _configScrollPos;
        private GUIStyle _nodeLabelStyle;
        private string[] _nodeLabels;
        private int _selectedNodeIndex = -1;
        private NodeDragController.DragState _dragState = NodeDragController.DragState.Inactive;
        private NodeSnapEngine.SnapResult _lastSnapResult;
        private bool _pendingDragArm;
        private Vector2 _pendingDragMousePx;
        private int _pendingDragNodeIndex = -1;
        private int _pendingDragMirrorPartnerIndex = -1;
        private Vector2 _pendingDragMirrorPartnerStartPositionUnits;
        private static GUIStyle _coordLabelStyle;
        private DesignerTab _activeTab = DesignerTab.SkillTree;
        private TopLevelTab _topLevelTab = TopLevelTab.Designer;
        private bool _branchPreviewActive;
        private int _branchPreviewParentIndex = -1;
        private DesignerTab _branchPreviewPreviousTab = DesignerTab.SkillTree;
        private TopLevelTab _branchPreviewPreviousTopLevelTab = TopLevelTab.Designer;
        private BranchPreviewSettings _branchPreviewSettings = BranchPreviewSettings.Defaults;
        private BranchPreviewSettings _lastBranchPreviewSettings = BranchPreviewSettings.Defaults;
        private int _mirrorSourceNodeIndex = BranchPlacement.NoMirrorSourceOverride;
        private bool _angleIsRelativeToMirrorAxis;
        private bool _pickMirrorSourceMode;
        private string _lastMirrorWarning;
        private bool _connectionsFoldoutOpen = true;
        private readonly List<NodeConnectionsInspector.ConnectionRow> _connectionsBuffer = new List<NodeConnectionsInspector.ConnectionRow>();
        private SkillNodePalette _cachedNodePalette;

        private SkillTreePreviewPanel _previewPanel;
        private VisualElement _previewRoot;
        private VisualElement _previewPanelHost;

        private SkillTreeVisualSettings _visualSettings;
        private SerializedObject _visualSettingsSerializedObject;
        private VisualElement _visualTabRoot;
        private VisualElement _visualTabCanvasHost;
        private VisualElement _visualTabInspectorHost;
        private SkillTreePreviewPanel _visualTabPreviewPanel;

        private static void LogError(string message)
        {
            Debug.LogError($"[{nameof(SkillTreeDesignerWindow)}] {message}");
        }

        private static void DrawToolbarButton(string label, bool enabled, string disabledTooltip, System.Action onClick)
        {
            using (new EditorGUI.DisabledScope(!enabled))
            {
                var content = enabled ? new GUIContent(label) : new GUIContent(label, disabledTooltip);
                if (GUILayout.Button(content))
                    onClick();
            }
        }

        [MenuItem("Roguelite/Skill Tree Designer")]
        private static void OpenWindow()
        {
            var window = GetWindow<SkillTreeDesignerWindow>("Skill Tree Designer");
            window.minSize = new Vector2(MinWindowWidth, MinWindowHeight);
            window.Show();
        }

        private void OnEnable()
        {
            RefreshTreeList();
            var initialEntry = ResolveInitialTreeEntry();
            BindAsset(initialEntry);
            RefreshActivePointerCache();
            _lastBranchPreviewSettings = BranchPreviewSettingsPersistence.Load();
            MirrorAxisPersistence.ApplyTo(ref _lastBranchPreviewSettings);
            _branchPreviewSettings = _lastBranchPreviewSettings;
            _cachedNodePalette = ActiveSkillNodePaletteResolver.GetActive();
            InitializePreviewPanel();
            InitializeVisualTabRoot();
            Undo.undoRedoPerformed += OnUndoRedoPerformed;
            LoadTopLevelTabFromPrefs();
        }

        private void LoadTopLevelTabFromPrefs()
        {
            _topLevelTab = (TopLevelTab)EditorPrefs.GetInt(TopLevelTabEditorPrefKey, (int)TopLevelTab.Designer);
        }

        private void SaveTopLevelTabToPrefs()
        {
            EditorPrefs.SetInt(TopLevelTabEditorPrefKey, (int)_topLevelTab);
        }

        private VisualElement CreateTopLevelOverlayRoot(FlexDirection flexDirection)
        {
            var overlay = new VisualElement();
            overlay.style.position = Position.Absolute;
            overlay.style.top = TopAndSubTabBarReservedHeightPixels;
            overlay.style.left = 0f;
            overlay.style.right = 0f;
            overlay.style.bottom = 0f;
            overlay.style.flexDirection = flexDirection;
            overlay.style.display = DisplayStyle.None;
            rootVisualElement.Add(overlay);
            return overlay;
        }

        private void InitializePreviewPanel()
        {
            _previewRoot = CreateTopLevelOverlayRoot(FlexDirection.Column);
            _previewRoot.style.flexGrow = 1;

            _previewPanel = new SkillTreePreviewPanel(_data, _cachedNodePalette);
            _previewPanelHost = _previewPanel.BuildRoot();
            _previewRoot.Add(_previewPanelHost);
        }

        private void InitializeVisualTabRoot()
        {
            _visualSettings = SkillTreeVisualSettingsResolver.Get();

            _visualTabRoot = CreateTopLevelOverlayRoot(FlexDirection.Row);

            _visualTabCanvasHost = new VisualElement();
            _visualTabCanvasHost.style.flexGrow = VisualTabCanvasFlexGrow;
            _visualTabCanvasHost.style.minWidth = 0;
            _visualTabRoot.Add(_visualTabCanvasHost);

            _visualTabInspectorHost = new VisualElement();
            _visualTabInspectorHost.style.flexGrow = VisualTabInspectorFlexGrow;
            _visualTabInspectorHost.style.minWidth = VisualTabInspectorMinWidthPixels;
            _visualTabInspectorHost.style.paddingLeft = VisualTabInspectorPaddingPixels;
            _visualTabInspectorHost.style.paddingRight = VisualTabInspectorPaddingPixels;
            _visualTabInspectorHost.style.paddingTop = VisualTabInspectorPaddingPixels;
            _visualTabInspectorHost.style.paddingBottom = VisualTabInspectorPaddingPixels;
            _visualTabRoot.Add(_visualTabInspectorHost);

            if (_visualSettings == null)
            {
                var error = new IMGUIContainer(() =>
                    EditorGUILayout.HelpBox("SkillTreeVisualSettings asset not found at Resources/Data/SkillTreeVisualSettings.asset.", MessageType.Error));
                _visualTabInspectorHost.Add(error);
                return;
            }

            _visualSettingsSerializedObject = new SerializedObject(_visualSettings);

            _visualTabPreviewPanel = new SkillTreePreviewPanel(_data, _cachedNodePalette);
            _visualTabCanvasHost.Add(_visualTabPreviewPanel.BuildRoot());

            var inspector = new IMGUIContainer(DrawVisualTabInspector);
            inspector.style.flexGrow = 1;
            _visualTabInspectorHost.Add(inspector);
        }

        private void DrawVisualTabInspector()
        {
            if (_visualSettingsSerializedObject == null) return;

            _visualSettingsSerializedObject.Update();

            EditorGUI.BeginChangeCheck();

            var iterator = _visualSettingsSerializedObject.GetIterator();
            iterator.NextVisible(true);
            while (iterator.NextVisible(false))
            {
                EditorGUILayout.PropertyField(iterator, true);
            }

            bool changed = EditorGUI.EndChangeCheck();

            _visualSettingsSerializedObject.ApplyModifiedProperties();

            if (changed)
                OnVisualSettingsChanged();
        }

        private void OnVisualSettingsChanged()
        {
            if (_visualSettings == null) return;

            EditorUtility.SetDirty(_visualSettings);
            AssetDatabase.SaveAssets();
            SkillTreeVisualSettingsResolver.ResetCache();
            _visualTabPreviewPanel?.Rebuild();
            _previewPanel?.Rebuild();
        }

        private void OnUndoRedoPerformed()
        {
            _visualSettingsSerializedObject?.Update();
            _visualTabPreviewPanel?.Rebuild();
            _previewPanel?.Rebuild();
            Repaint();
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndoRedoPerformed;
            SaveTopLevelTabToPrefs();
            if (_previewRoot != null && _previewRoot.parent != null)
                _previewRoot.RemoveFromHierarchy();
            _previewRoot = null;
            _previewPanel = null;
            _previewPanelHost = null;
            if (_visualTabRoot != null && _visualTabRoot.parent != null)
                _visualTabRoot.RemoveFromHierarchy();
            _visualTabRoot = null;
            _visualTabCanvasHost = null;
            _visualTabInspectorHost = null;
            _visualTabPreviewPanel = null;
            _visualSettingsSerializedObject?.Dispose();
            _visualSettingsSerializedObject = null;
            _visualSettings = null;
        }

        private void RefreshTreeList()
        {
            _treeEntries = SkillTreesEnumerator.Enumerate(EditorPaths.SkillTreesFolder);
            _treeDisplayNames = new string[_treeEntries.Count];
            for (int i = 0; i < _treeEntries.Count; i++)
                _treeDisplayNames[i] = _treeEntries[i].DisplayName;
        }

        private SkillTreesEnumerator.TreeEntry? ResolveInitialTreeEntry()
        {
            if (_treeEntries == null || _treeEntries.Count == 0) return null;

            var savedGuid = EditorPrefs.GetString(SelectedAssetGuidEditorPrefKey, string.Empty);
            if (!string.IsNullOrEmpty(savedGuid))
            {
                var idx = IndexOfGuid(savedGuid);
                if (idx >= 0) return _treeEntries[idx];
            }

            var active = ActiveSkillTreeResolver.GetActive();
            if (active != null)
            {
                var activePath = AssetDatabase.GetAssetPath(active);
                var activeGuid = AssetDatabase.AssetPathToGUID(activePath);
                var idx = IndexOfGuid(activeGuid);
                if (idx >= 0) return _treeEntries[idx];
            }

            return _treeEntries[0];
        }

        private int IndexOfGuid(string guid)
        {
            for (int i = 0; i < _treeEntries.Count; i++)
                if (_treeEntries[i].Guid == guid) return i;
            return -1;
        }

        private void BindAsset(SkillTreesEnumerator.TreeEntry? entry)
        {
            _lastMirrorWarning = null;
            if (!entry.HasValue)
            {
                _data = null;
                _serializedData = null;
                _selectedTreeIndex = -1;
                _nodeLabels = null;
                return;
            }
            var resolvedEntry = entry.Value;
            _selectedTreeIndex = IndexOfGuid(resolvedEntry.Guid);
            _data = resolvedEntry.Asset;
            _serializedData = new SerializedObject(_data);
            CacheSerializedProperties();
            RebuildNodeLabels();
            EditorPrefs.SetString(SelectedAssetGuidEditorPrefKey, resolvedEntry.Guid);
            RebuildPreviewPanelForCurrentTree();
        }

        private void RebuildPreviewPanelForCurrentTree()
        {
            RebuildPanel(ref _previewPanel, _previewRoot, _previewPanelHost, (host, panel) =>
            {
                _previewPanelHost = panel.BuildRoot();
                host.Add(_previewPanelHost);
            });

            if (_visualTabCanvasHost != null)
            {
                _visualTabCanvasHost.Clear();
                _visualTabPreviewPanel = new SkillTreePreviewPanel(_data, _cachedNodePalette);
                _visualTabCanvasHost.Add(_visualTabPreviewPanel.BuildRoot());
            }
        }

        private void RebuildPanel(ref SkillTreePreviewPanel panel, VisualElement host, VisualElement oldHost, System.Action<VisualElement, SkillTreePreviewPanel> reseat)
        {
            if (host == null) return;
            if (oldHost != null && oldHost.parent != null)
                oldHost.RemoveFromHierarchy();
            panel = new SkillTreePreviewPanel(_data, _cachedNodePalette);
            reseat(host, panel);
        }

        private void RefreshActivePointerCache()
        {
            var pointer = AssetDatabase.LoadAssetAtPath<ActiveSkillTreePointer>(EditorPaths.ActiveSkillTreePointerAsset);
            _activePointerTarget = pointer != null ? pointer.Target : null;
        }

        private void SetActivePointerToCurrent()
        {
            if (_data == null) return;
            if (!SkillTreesEnumerator.SetActivePointer(EditorPaths.ActiveSkillTreePointerAsset, _data))
            {
                LogError($"Active pointer asset missing at {EditorPaths.ActiveSkillTreePointerAsset}");
                return;
            }
            RefreshActivePointerCache();
        }

        private void CreateNewTree()
        {
            var path = EditorUtility.SaveFilePanel(DialogTitleCreateTree, EditorPaths.SkillTreesFolder, DefaultNewTreeFileName, AssetExtension);
            if (string.IsNullOrEmpty(path)) return;
            if (!SkillTreesEnumerator.IsPathUnderSkillTreesFolder(path, EditorPaths.SkillTreesFolder))
            {
                EditorUtility.DisplayDialog(DialogTitleInvalidPath, $"Skill trees must live under {EditorPaths.SkillTreesFolder}.", DialogConfirmOk);
                return;
            }
            var assetRelative = SkillTreesEnumerator.ConvertAbsoluteToAssetRelative(path);
            var asset = ScriptableObject.CreateInstance<SkillTreeData>();
            AssetDatabase.CreateAsset(asset, assetRelative);
            AssetDatabase.SaveAssets();
            var guid = AssetDatabase.AssetPathToGUID(assetRelative);
            RefreshTreeList();
            BindAsset(FindEntryByGuid(guid));
        }

        private void DuplicateCurrentTree()
        {
            if (_data == null) return;
            var sourcePath = AssetDatabase.GetAssetPath(_data);
            var duplicatePath = SkillTreesEnumerator.MakeUniqueDuplicatePath(sourcePath);
            if (string.IsNullOrEmpty(duplicatePath)) return;
            if (!AssetDatabase.CopyAsset(sourcePath, duplicatePath))
            {
                LogError($"Failed to duplicate {sourcePath} to {duplicatePath}");
                return;
            }
            AssetDatabase.SaveAssets();
            var guid = AssetDatabase.AssetPathToGUID(duplicatePath);
            RefreshTreeList();
            BindAsset(FindEntryByGuid(guid));
        }

        private void DeleteCurrentTree()
        {
            if (_data == null) return;
            if (_treeEntries.Count <= MinimumRetainedTreeCount) return;
            if (_data == _activePointerTarget) return;
            var assetName = _data.name;
            if (!EditorUtility.DisplayDialog(DialogTitleDeleteTree, $"Delete '{assetName}'? This cannot be undone.", DialogConfirmDelete, DialogConfirmCancel))
                return;
            var path = AssetDatabase.GetAssetPath(_data);
            if (!AssetDatabase.DeleteAsset(path))
            {
                LogError($"Failed to delete {path}");
                return;
            }
            AssetDatabase.SaveAssets();
            RefreshTreeList();
            BindAsset(_treeEntries.Count > 0 ? _treeEntries[0] : (SkillTreesEnumerator.TreeEntry?)null);
        }

        private SkillTreesEnumerator.TreeEntry? FindEntryByGuid(string guid)
        {
            var idx = IndexOfGuid(guid);
            return idx >= 0 ? _treeEntries[idx] : (SkillTreesEnumerator.TreeEntry?)null;
        }

        private void CacheSerializedProperties()
        {
            _propUnitSize = _serializedData.FindProperty(SkillTreeData.FieldNames.UnitSize);
            _propNodeSize = _serializedData.FindProperty(SkillTreeData.FieldNames.NodeSize);
            _propNodeColor = _serializedData.FindProperty(SkillTreeData.FieldNames.NodeColor);
            _propBorderNormalColor = _serializedData.FindProperty(SkillTreeData.FieldNames.BorderNormalColor);
            _propBorderSelectedColor = _serializedData.FindProperty(SkillTreeData.FieldNames.BorderSelectedColor);
            _propBaseCost = _serializedData.FindProperty(SkillTreeData.FieldNames.BaseCost);
            _propCostMultiplierOdd = _serializedData.FindProperty(SkillTreeData.FieldNames.CostMultiplierOdd);
            _propCostMultiplierEven = _serializedData.FindProperty(SkillTreeData.FieldNames.CostMultiplierEven);
            _propCostAdditivePerLevel = _serializedData.FindProperty(SkillTreeData.FieldNames.CostAdditivePerLevel);
            _propDefaultGeneratedMaxLevel = _serializedData.FindProperty(SkillTreeData.FieldNames.DefaultGeneratedMaxLevel);
            _propEdgeColor = _serializedData.FindProperty(SkillTreeData.FieldNames.EdgeColor);
            _propEdgeThickness = _serializedData.FindProperty(SkillTreeData.FieldNames.EdgeThickness);
        }

        private bool IsPreviewTabActive()
        {
            return _activeTab == DesignerTab.Preview;
        }

        private DesignerTab[] GetVisibleTabs()
        {
            return _branchPreviewActive ? VisibleTabsWithBranch : VisibleTabsWithoutBranch;
        }

        private int GetActiveTabSelectionIndex()
        {
            var visible = GetVisibleTabs();
            for (int i = 0; i < visible.Length; i++)
                if (visible[i] == _activeTab)
                    return i;
            return 0;
        }

        private void SetActiveTabFromSelectionIndex(int selectionIndex)
        {
            var visible = GetVisibleTabs();
            if (selectionIndex < 0 || selectionIndex >= visible.Length) return;
            _activeTab = visible[selectionIndex];
        }

        private void DrawTopLevelTabBar()
        {
            int currentIndex = (int)_topLevelTab;
            using (new EditorGUI.DisabledScope(_branchPreviewActive))
            {
                int newIndex = GUILayout.Toolbar(currentIndex, TopLevelTabLabels);
                if (newIndex != currentIndex)
                {
                    _topLevelTab = (TopLevelTab)newIndex;
                    SaveTopLevelTabToPrefs();
                }
            }
        }

        private void OnGUI()
        {
            if (_data == null)
            {
                EditorGUILayout.HelpBox("No SkillTreeData asset found.", MessageType.Warning);
                return;
            }

            DrawTopLevelTabBar();

            if (_topLevelTab == TopLevelTab.Visual)
            {
                if (_previewRoot != null) _previewRoot.style.display = DisplayStyle.None;
                if (_visualTabRoot != null) _visualTabRoot.style.display = DisplayStyle.Flex;
                return;
            }

            if (_visualTabRoot != null) _visualTabRoot.style.display = DisplayStyle.None;

            if (IsPreviewTabActive())
            {
                if (_previewRoot != null)
                    _previewRoot.style.display = DisplayStyle.Flex;
                DrawPreviewTabBar();
                return;
            }

            if (_previewRoot != null)
                _previewRoot.style.display = DisplayStyle.None;

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

        private void DrawPreviewTabBar()
        {
            if (_data == null) return;
            _serializedData.Update();
            string[] tabLabels = _branchPreviewActive ? TabLabelsWithBranch : TabLabelsWithoutBranch;
            int newSelection = GUILayout.Toolbar(GetActiveTabSelectionIndex(), tabLabels);
            SetActiveTabFromSelectionIndex(newSelection);
            _serializedData.ApplyModifiedProperties();
        }

        private void DrawCanvas(Rect canvasRect)
        {
            EditorGUI.DrawRect(canvasRect, CanvasBackgroundColor);

            GUI.BeginClip(canvasRect);

            Vector2 origin = ComputeCanvasOrigin(canvasRect);

            DrawGrid(canvasRect, origin);

            EditorGUI.DrawRect(new Rect(origin.x - CrosshairHalfPixelOffset, 0, CrosshairLineThickness, canvasRect.height), CrosshairColor);
            EditorGUI.DrawRect(new Rect(0, origin.y - CrosshairHalfPixelOffset, canvasRect.width, CrosshairLineThickness), CrosshairColor);

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

                if (_branchPreviewActive && _branchPreviewSettings.mirrorEnabled
                    && _mirrorSourceNodeIndex == i && i != _branchPreviewParentIndex)
                {
                    Handles.color = MirrorSourceRingColor;
                    Handles.DrawSolidDisc(center3D, Vector3.forward, halfNode + scaledBorder * MirrorSourceRingThicknessMultiplier);
                }

                Handles.color = (i == _selectedNodeIndex) ? _data.BorderSelectedColor : _data.BorderNormalColor;
                Handles.DrawSolidDisc(center3D, Vector3.forward, halfNode + scaledBorder);

                Handles.color = _cachedNodePalette != null
                    ? _cachedNodePalette.GetColor(entry.colorTag)
                    : _data.NodeColor;
                Handles.DrawSolidDisc(center3D, Vector3.forward, halfNode);

                string label = _nodeLabels != null && i < _nodeLabels.Length ? _nodeLabels[i] : entry.id.ToString();
                GUI.Label(nodeRect, label, _nodeLabelStyle);

                if (_dragState.IsActive && i == _dragState.NodeIndex)
                {
                    if (_coordLabelStyle == null)
                    {
                        _coordLabelStyle = new GUIStyle(EditorStyles.miniLabel)
                        {
                            alignment = TextAnchor.MiddleLeft,
                            normal = { textColor = Color.white }
                        };
                    }
                    var (dx, dy) = SkillTreeGrid.ToDisplay(entry.position);
                    string coordText = $"({dx}, {dy})";
                    Rect coordRect = new Rect(
                        screenPos.x + halfNode + CoordLabelOffsetXPixels,
                        screenPos.y - CoordLabelOffsetYPixels,
                        CoordLabelWidthPixels,
                        CoordLabelHeightPixels);
                    GUI.Label(coordRect, coordText, _coordLabelStyle);
                }
            }
            if (_branchPreviewActive && _branchPreviewParentIndex >= 0 && _branchPreviewParentIndex < _data.Nodes.Count)
            {
                var (parentPos, mirrorSourcePos, resolvedAngle, mirrorBranchAngle) = ComputeResolvedPreview();
                Vector2 previewPos = BranchPlacement.ComputeBranchPosition(parentPos, _branchPreviewSettings.distance, resolvedAngle);
                Vector2 parentScreen = origin + parentPos * scaledUnit;
                Vector2 previewScreen = origin + previewPos * scaledUnit;
                Handles.color = BranchPreviewTintColor;
                Handles.DrawDottedLine(
                    new Vector3(parentScreen.x, parentScreen.y, 0f),
                    new Vector3(previewScreen.x, previewScreen.y, 0f),
                    BranchPreviewDottedSegmentSize);
                Handles.DrawWireDisc(new Vector3(previewScreen.x, previewScreen.y, 0f), Vector3.forward, halfNode);

                if (_branchPreviewSettings.mirrorEnabled)
                {
                    Vector2 mirrorSourceScreen = origin + mirrorSourcePos * scaledUnit;
                    float halfSpan = Mathf.Max(canvasRect.width, canvasRect.height);
                    MirrorAxisGeometry.ComputeAxisEndpoints(mirrorSourceScreen, _branchPreviewSettings.mirrorAxisDegrees, halfSpan, out var axisStart, out var axisEnd);
                    Handles.color = MirrorAxisLineColor;
                    Handles.DrawDottedLine(new Vector3(axisStart.x, axisStart.y, 0f), new Vector3(axisEnd.x, axisEnd.y, 0f), BranchPreviewDottedSegmentSize);

                    Vector2 mirrorPos = BranchPlacement.ComputeBranchPosition(mirrorSourcePos, _branchPreviewSettings.distance, mirrorBranchAngle);
                    Vector2 mirrorScreen = origin + mirrorPos * scaledUnit;
                    Handles.color = MirrorPreviewTintColor;
                    Handles.DrawDottedLine(
                        new Vector3(mirrorSourceScreen.x, mirrorSourceScreen.y, 0f),
                        new Vector3(mirrorScreen.x, mirrorScreen.y, 0f),
                        BranchPreviewDottedSegmentSize);
                    Handles.DrawWireDisc(new Vector3(mirrorScreen.x, mirrorScreen.y, 0f), Vector3.forward, halfNode);
                }
            }
            if (_dragState.IsActive
                && _branchPreviewSettings.alignmentRadiusVisible
                && _branchPreviewSettings.alignmentRadiusUnits > 0f
                && _dragState.NodeIndex >= 0
                && _dragState.NodeIndex < _data.Nodes.Count)
            {
                Vector2 nodePos = _data.Nodes[_dragState.NodeIndex].position;
                Vector2 centerScreen = AlignmentOverlayGeometry.ComputeRadiusCircleCenterScreen(
                    nodePos, origin, scaledUnit);
                float radiusScreen = AlignmentOverlayGeometry.ComputeRadiusCircleScreenRadius(
                    _branchPreviewSettings.alignmentRadiusUnits, _data.UnitSize, _canvasZoom);
                Color prevCircleColor = Handles.color;
                Handles.color = AlignmentRadiusCircleColor;
                Handles.DrawWireDisc(centerScreen, Vector3.forward, radiusScreen);
                Handles.color = prevCircleColor;
            }

            if (_dragState.IsActive && _lastSnapResult.SnappedAxis != NodeSnapEngine.SnapAxis.None)
            {
                Color prevColor = Handles.color;
                Handles.color = SnapGuideLineColor;
                if (_lastSnapResult.SnappedAxis == NodeSnapEngine.SnapAxis.X
                    || _lastSnapResult.SnappedAxis == NodeSnapEngine.SnapAxis.Y)
                {
                    DrawAxisAlignedSnapGuide(
                        _lastSnapResult.SnappedAxis,
                        _data.Nodes[_lastSnapResult.TargetNodeIndex].position,
                        origin,
                        scaledUnit,
                        canvasRect);
                }
                else if (_lastSnapResult.SnappedAxis == NodeSnapEngine.SnapAxis.LineCardinal)
                {
                    Vector2 targetPos = _data.Nodes[_lastSnapResult.TargetNodeIndex].position;
                    Vector2 resolved = _lastSnapResult.ResolvedPosition;
                    bool alignedOnX = Mathf.Abs(targetPos.x - resolved.x) < Mathf.Abs(targetPos.y - resolved.y);
                    if (alignedOnX)
                    {
                        DrawVerticalSnapGuide(targetPos.x, origin, scaledUnit, canvasRect);
                    }
                    else
                    {
                        DrawHorizontalSnapGuide(targetPos.y, origin, scaledUnit, canvasRect);
                    }
                }
                else if (_lastSnapResult.SnappedAxis == NodeSnapEngine.SnapAxis.LineCollinear
                    && _lastSnapResult.SecondaryTargetNodeIndex >= 0)
                {
                    Vector2 primaryPos = _data.Nodes[_lastSnapResult.TargetNodeIndex].position;
                    Vector2 secondaryPos = _data.Nodes[_lastSnapResult.SecondaryTargetNodeIndex].position;
                    Vector2 primaryScreen = origin + primaryPos * scaledUnit;
                    Vector2 secondaryScreen = origin + secondaryPos * scaledUnit;
                    Vector2 lineDir = (secondaryScreen - primaryScreen).normalized;
                    float halfSpan = Mathf.Max(canvasRect.width, canvasRect.height);
                    Vector2 lineStart = primaryScreen - lineDir * halfSpan;
                    Vector2 lineEnd = primaryScreen + lineDir * halfSpan;
                    Handles.DrawDottedLine(
                        new Vector3(lineStart.x, lineStart.y, 0f),
                        new Vector3(lineEnd.x, lineEnd.y, 0f),
                        BranchPreviewDottedSegmentSize);
                }

                if (_lastSnapResult.CrossAxis != NodeSnapEngine.SnapAxis.None
                    && _lastSnapResult.CrossTargetNodeIndex >= 0
                    && _lastSnapResult.CrossTargetNodeIndex < _data.Nodes.Count)
                {
                    DrawAxisAlignedSnapGuide(
                        _lastSnapResult.CrossAxis,
                        _data.Nodes[_lastSnapResult.CrossTargetNodeIndex].position,
                        origin,
                        scaledUnit,
                        canvasRect);
                }
                Handles.color = prevColor;
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

        private void DrawAxisAlignedSnapGuide(NodeSnapEngine.SnapAxis axis, Vector2 nodePos, Vector2 origin, float scaledUnit, Rect canvasRect)
        {
            if (axis == NodeSnapEngine.SnapAxis.X)
                DrawVerticalSnapGuide(nodePos.x, origin, scaledUnit, canvasRect);
            else if (axis == NodeSnapEngine.SnapAxis.Y)
                DrawHorizontalSnapGuide(nodePos.y, origin, scaledUnit, canvasRect);
        }

        private void DrawVerticalSnapGuide(float worldX, Vector2 origin, float scaledUnit, Rect canvasRect)
        {
            float snapScreenX = origin.x + worldX * scaledUnit;
            Handles.DrawDottedLine(
                new Vector3(snapScreenX, 0f, 0f),
                new Vector3(snapScreenX, canvasRect.height, 0f),
                BranchPreviewDottedSegmentSize);
        }

        private void DrawHorizontalSnapGuide(float worldY, Vector2 origin, float scaledUnit, Rect canvasRect)
        {
            float snapScreenY = origin.y + worldY * scaledUnit;
            Handles.DrawDottedLine(
                new Vector3(0f, snapScreenY, 0f),
                new Vector3(canvasRect.width, snapScreenY, 0f),
                BranchPreviewDottedSegmentSize);
        }

        // TODO #283 follow-up: extract NodeDragOrchestrator owning _dragState/_pendingDrag* /_lastSnapResult
        // and exposing OnMouseDown/OnMouseDrag/OnMouseUp. Deferred from this PR because the extraction
        // touches mirror partner discovery, snap, undo, dirty/save and shared _branchPreviewSettings —
        // higher risk than is appropriate for the snap-feature ticket.
        private void HandleCanvasInput(Rect canvasRect)
        {
            Event evt = Event.current;

            if (_pickMirrorSourceMode && evt.type == EventType.KeyDown && evt.keyCode == KeyCode.Escape)
            {
                CancelMirrorSourcePickMode();
                evt.Use();
                return;
            }

            if (!canvasRect.Contains(evt.mousePosition)) return;

            if (_pickMirrorSourceMode && evt.type == EventType.MouseDown && evt.button == LeftMouseButton && !evt.alt)
            {
                int pickHitIndex = HitTestNodeAt(evt.mousePosition, canvasRect);
                if (pickHitIndex >= 0 && pickHitIndex != _branchPreviewParentIndex)
                    _mirrorSourceNodeIndex = pickHitIndex;
                CancelMirrorSourcePickMode();
                evt.Use();
                return;
            }

            if (evt.type == EventType.ScrollWheel)
            {
                float zoomDelta = 1f - evt.delta.y * ZoomScrollSensitivity;
                _canvasZoom = Mathf.Clamp(_canvasZoom * zoomDelta, MinZoom, MaxZoom);
                evt.Use();
                Repaint();
            }

            if (evt.type == EventType.MouseDown && evt.button == LeftMouseButton && !evt.alt)
            {
                int hitIndex = HitTestNodeAt(evt.mousePosition, canvasRect);
                _selectedNodeIndex = hitIndex;
                if (!_branchPreviewActive)
                    _activeTab = hitIndex >= 0 ? DesignerTab.Node : DesignerTab.SkillTree;

                if (hitIndex >= 0 && _data.Nodes[hitIndex].id != SkillTreeData.CentralNodeId)
                {
                    _pendingDragArm = true;
                    _pendingDragMousePx = evt.mousePosition;
                    _pendingDragNodeIndex = hitIndex;
                    if (_branchPreviewSettings.mirrorEnabled)
                    {
                        _pendingDragMirrorPartnerIndex = MirrorPartnerFinder.FindPartnerIndex(
                            _data.Nodes, hitIndex, _branchPreviewSettings.mirrorAxisDegrees,
                            MirrorPartnerFinder.DefaultMatchToleranceUnits);
                        _pendingDragMirrorPartnerStartPositionUnits = _pendingDragMirrorPartnerIndex >= 0
                            ? _data.Nodes[_pendingDragMirrorPartnerIndex].position
                            : Vector2.zero;
                    }
                    else
                    {
                        _pendingDragMirrorPartnerIndex = -1;
                        _pendingDragMirrorPartnerStartPositionUnits = Vector2.zero;
                    }
                }

                evt.Use();
                Repaint();
            }

            if (evt.type == EventType.MouseDrag && evt.button == LeftMouseButton && !evt.alt)
            {
                if (_pendingDragArm && (evt.mousePosition - _pendingDragMousePx).magnitude >= DragStartThresholdPx)
                {
                    Undo.RegisterCompleteObjectUndo(_data, "Move Node");
                    var startNode = _data.Nodes[_pendingDragNodeIndex];
                    _dragState = new NodeDragController.DragState(
                        _pendingDragNodeIndex,
                        startNode.position,
                        _pendingDragMousePx,
                        _pendingDragMirrorPartnerIndex,
                        _pendingDragMirrorPartnerStartPositionUnits);
                    _pendingDragArm = false;
                }

                if (_dragState.IsActive)
                {
                    Vector2 rawNewPos = NodeDragController.ComputeNewNodePosition(
                        _dragState, evt.mousePosition, _data.UnitSize, _canvasZoom);
                    var draggedNode = _data.Nodes[_dragState.NodeIndex];
                    bool snapAllowed = draggedNode.snapEnabled && !evt.shift;
                    _lastSnapResult = snapAllowed
                        ? NodeSnapEngine.Resolve(rawNewPos, _dragState.NodeIndex, _data.Nodes, draggedNode.snapThresholdUnits, _branchPreviewSettings.alignmentRadiusUnits, _lastSnapResult)
                        : NodeSnapEngine.SnapResult.NoSnap(rawNewPos);

                    var updated = _data.Nodes[_dragState.NodeIndex];
                    updated.position = _lastSnapResult.ResolvedPosition;
                    _data.SetNode(_dragState.NodeIndex, updated);

                    if (_dragState.MirrorPartnerIndex >= 0 && _dragState.MirrorPartnerIndex < _data.Nodes.Count)
                    {
                        Vector2 delta = _lastSnapResult.ResolvedPosition - _dragState.NodeStartPositionUnits;
                        Vector2 mirroredDelta = MirrorAxisGeometry.ReflectAcrossClockwiseFromNorthAxis(delta, _branchPreviewSettings.mirrorAxisDegrees);
                        Vector2 partnerNew = _dragState.MirrorPartnerStartPositionUnits + mirroredDelta;
                        var partner = _data.Nodes[_dragState.MirrorPartnerIndex];
                        partner.position = partnerNew;
                        _data.SetNode(_dragState.MirrorPartnerIndex, partner);
                    }

                    _serializedData?.Update();
                    Repaint();
                }

                evt.Use();
            }

            if (evt.type == EventType.MouseUp && evt.button == LeftMouseButton)
            {
                bool wasDragging = _dragState.IsActive;
                if (wasDragging)
                {
                    _dragState = NodeDragController.DragState.Inactive;
                    _lastSnapResult = NodeSnapEngine.SnapResult.NoSnap(Vector2.zero);
                    EditorUtility.SetDirty(_data);
                    AssetDatabase.SaveAssets();
                    Repaint();
                }
                _pendingDragArm = false;
                if (wasDragging)
                    evt.Use();
            }

            if (evt.type == EventType.MouseDrag && (evt.button == MiddleMouseButton || (evt.button == LeftMouseButton && evt.alt)))
            {
                _canvasOffset += evt.delta;
                evt.Use();
                Repaint();
            }
        }

        private void DrawConfigPanel()
        {
            _serializedData.Update();

            string[] tabLabels = _branchPreviewActive ? TabLabelsWithBranch : TabLabelsWithoutBranch;
            int newSelection = GUILayout.Toolbar(GetActiveTabSelectionIndex(), tabLabels);
            SetActiveTabFromSelectionIndex(newSelection);

            _configScrollPos = EditorGUILayout.BeginScrollView(_configScrollPos);

            if (_activeTab == DesignerTab.SkillTree)
                DrawSkillTreeTab();
            else if (_activeTab == DesignerTab.Node)
                DrawNodeTab();
            else
                DrawBranchTab();

            if (_serializedData.ApplyModifiedProperties())
                Repaint();

            EditorGUILayout.EndScrollView();
        }

        private void DrawSkillTreeTab()
        {
            EditorGUILayout.Space(SectionSpacingSmall);

            if (_treeEntries == null || _treeEntries.Count == 0)
            {
                EditorGUILayout.HelpBox($"No SkillTreeData assets found under {EditorPaths.SkillTreesFolder}.", MessageType.Warning);
            }
            else
            {
                EditorGUI.BeginChangeCheck();
                int newIndex = EditorGUILayout.Popup("Skill Tree", _selectedTreeIndex, _treeDisplayNames);
                if (EditorGUI.EndChangeCheck() && newIndex != _selectedTreeIndex && newIndex >= 0 && newIndex < _treeEntries.Count)
                    BindAsset(_treeEntries[newIndex]);

                string activeName = _activePointerTarget != null ? _activePointerTarget.name : NoActiveLabel;
                EditorGUILayout.LabelField(ActiveLabelPrefix + activeName);

                EditorGUILayout.BeginHorizontal();
                bool isActive = _data == _activePointerTarget;
                bool canSetActive = _data != null && !isActive;
                string setActiveTooltip = _data == null ? DisabledTooltipNoSelection : DisabledTooltipAlreadyActive;
                DrawToolbarButton(ButtonLabelSetActive, canSetActive, setActiveTooltip, SetActivePointerToCurrent);

                DrawToolbarButton(ButtonLabelNew, true, null, CreateNewTree);

                DrawToolbarButton(ButtonLabelDuplicate, _data != null, DisabledTooltipNoSelection, DuplicateCurrentTree);

                bool canDelete = _data != null && _treeEntries != null && _treeEntries.Count > MinimumRetainedTreeCount && !isActive;
                string deleteTooltip = canDelete
                    ? DisabledTooltipDeleteEnabled
                    : (_data == null ? DisabledTooltipNoSelection
                        : isActive ? DisabledTooltipCannotDeleteActive
                        : DisabledTooltipCannotDeleteLast);
                DrawToolbarButton(ButtonLabelDelete, canDelete, deleteTooltip, DeleteCurrentTree);
                EditorGUILayout.EndHorizontal();
            }

            if (_data == null) return;

            EditorGUILayout.Space(SectionSpacingMedium);
            EditorGUILayout.LabelField("Visual Settings", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(_propUnitSize, LabelUnitSize);
            EditorGUILayout.PropertyField(_propNodeSize, LabelNodeSize);
            EditorGUILayout.PropertyField(_propNodeColor, LabelNodeColor);
            EditorGUILayout.PropertyField(_propBorderNormalColor, LabelBorderNormal);
            EditorGUILayout.PropertyField(_propBorderSelectedColor, LabelBorderSelected);

            EditorGUILayout.Space(SectionSpacingMedium);
            EditorGUILayout.LabelField("Cost Defaults (applied on Generate)", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("cost(0) = base, cost(n) = floor(prev \u00d7 mult) + add\nmult alternates: odd/even", MessageType.None);
            EditorGUILayout.PropertyField(_propBaseCost, LabelBaseCost);
            EditorGUILayout.PropertyField(_propCostMultiplierOdd, LabelCostMultiplierOdd);
            EditorGUILayout.PropertyField(_propCostMultiplierEven, LabelCostMultiplierEven);
            EditorGUILayout.PropertyField(_propCostAdditivePerLevel, LabelCostAdditive);
            EditorGUILayout.PropertyField(_propDefaultGeneratedMaxLevel, LabelDefaultGeneratedMaxLevel);

            EditorGUILayout.Space(SectionSpacingMedium);
            EditorGUILayout.LabelField("Edge Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_propEdgeColor, LabelEdgeColor);
            EditorGUILayout.PropertyField(_propEdgeThickness, LabelEdgeThickness);

            EditorGUILayout.Space(SectionSpacingMedium);
            EditorGUILayout.LabelField("Multi-Node Alignment", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            float newRadius = EditorGUILayout.Slider(
                AlignmentRadiusLabel,
                _branchPreviewSettings.alignmentRadiusUnits,
                MinAlignmentRadiusUnits,
                MaxAlignmentRadiusUnits);
            bool newVisible = EditorGUILayout.Toggle(
                AlignmentRadiusVisibleLabel,
                _branchPreviewSettings.alignmentRadiusVisible);
            if (EditorGUI.EndChangeCheck())
            {
                _branchPreviewSettings.alignmentRadiusUnits = newRadius;
                _branchPreviewSettings.alignmentRadiusVisible = newVisible;
                BranchPreviewSettingsPersistence.Save(_branchPreviewSettings);
                Repaint();
            }

            EditorGUILayout.Space(SectionSpacingLarge);

            EditorGUILayout.HelpBox(
                $"{_data.Nodes.Count} node(s). Select a node and use the Branch tab to add new nodes.",
                MessageType.Info);
        }

        private void DrawNodeTab()
        {
            EditorGUILayout.Space(SectionSpacingSmall);

            if (_selectedNodeIndex < 0 || _selectedNodeIndex >= _data.Nodes.Count)
            {
                EditorGUILayout.HelpBox("Select a node on the canvas to edit its properties.", MessageType.Info);
                return;
            }

            EditorGUILayout.LabelField($"Node {_selectedNodeIndex}", EditorStyles.boldLabel);

            var nodeForPos = _data.Nodes[_selectedNodeIndex];
            bool isRootNode = nodeForPos.id == SkillTreeData.CentralNodeId;
            EditorGUI.BeginDisabledGroup(isRootNode);
            EditorGUI.BeginChangeCheck();
            float newX = EditorGUILayout.FloatField("Position X", nodeForPos.position.x);
            float newY = EditorGUILayout.FloatField("Position Y", nodeForPos.position.y);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RegisterCompleteObjectUndo(_data, "Edit Node Position");
                var updatedPos = nodeForPos;
                updatedPos.position = new Vector2(newX, newY);
                _data.SetNode(_selectedNodeIndex, updatedPos);
                EditorUtility.SetDirty(_data);
            }
            EditorGUI.EndDisabledGroup();

            nodeForPos = _data.Nodes[_selectedNodeIndex];
            EditorGUI.BeginDisabledGroup(isRootNode);
            EditorGUI.BeginChangeCheck();
            bool newSnapEnabled = EditorGUILayout.Toggle(SnapEnabledLabel, nodeForPos.snapEnabled);
            float newSnapThreshold = nodeForPos.snapThresholdUnits;
            using (new EditorGUI.DisabledScope(!newSnapEnabled))
            {
                newSnapThreshold = EditorGUILayout.Slider(
                    SnapThresholdLabel,
                    newSnapThreshold,
                    MinSnapThresholdUnits,
                    MaxSnapThresholdUnits);
            }
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RegisterCompleteObjectUndo(_data, "Edit Node Snap");
                var updatedSnap = nodeForPos;
                updatedSnap.snapEnabled = newSnapEnabled;
                updatedSnap.snapThresholdUnits = newSnapThreshold;
                _data.SetNode(_selectedNodeIndex, updatedSnap);
                EditorUtility.SetDirty(_data);
            }
            EditorGUI.EndDisabledGroup();

            if (GUILayout.Button("Create Branch from Selected"))
            {
                BeginBranchPreview(_selectedNodeIndex);
            }

            EditorGUILayout.Space(SectionSpacingSmall);

            var node = _data.Nodes[_selectedNodeIndex];
            if (_cachedNodePalette == null)
                _cachedNodePalette = ActiveSkillNodePaletteResolver.GetActive();
            var palette = _cachedNodePalette;

            EditorGUI.BeginChangeCheck();

            var newCostType = (SkillTreeData.CostType)EditorGUILayout.EnumPopup("Cost Type", node.costType);
            int newMaxLevel = EditorGUILayout.IntField("Max Level (0=unlimited)", node.maxLevel);

            EditorGUILayout.Space(SectionSpacingSmall);
            EditorGUILayout.LabelField("Cost Formula", EditorStyles.boldLabel);
            int newBaseCost = EditorGUILayout.IntField("Base Cost", node.baseCost);
            float newMultOdd = EditorGUILayout.FloatField("Multiplier (Odd)", node.costMultiplierOdd);
            float newMultEven = EditorGUILayout.FloatField("Multiplier (Even)", node.costMultiplierEven);
            int newAdditive = EditorGUILayout.IntField("Additive / Level", node.costAdditivePerLevel);

            EditorGUILayout.Space(SectionSpacingSmall);
            EditorGUILayout.LabelField("Stat Modifier", EditorStyles.boldLabel);
            var newStatModType = (StatType)EditorGUILayout.EnumPopup("Stat", node.statModifierType);
            var newStatModMode = (SkillTreeData.StatModifierMode)EditorGUILayout.EnumPopup("Mode", node.statModifierMode);
            string statModValueLabel = newStatModMode == SkillTreeData.StatModifierMode.Percent
                ? "% / Level"
                : "Value / Level";
            float newStatModValue = EditorGUILayout.FloatField(statModValueLabel, node.statModifierValuePerLevel);
            if (newStatModMode == SkillTreeData.StatModifierMode.Percent)
                EditorGUILayout.HelpBox("Stored as percent (5 = +5%)", MessageType.Info);

            EditorGUILayout.Space(SectionSpacingSmall);
            EditorGUILayout.LabelField("Color Tag", EditorStyles.boldLabel);
            NodeColorTag newColorTag;
            using (new EditorGUILayout.HorizontalScope())
            {
                newColorTag = (NodeColorTag)EditorGUILayout.EnumPopup("Tag", node.colorTag);
                var swatchRect = GUILayoutUtility.GetRect(ColorSwatchWidth, ColorSwatchHeight, GUILayout.Width(ColorSwatchWidth));
                var swatchColor = palette != null ? palette.GetColor(newColorTag) : Color.white;
                EditorGUI.DrawRect(swatchRect, swatchColor);
            }

            if (EditorGUI.EndChangeCheck())
            {
                var updated = node;
                updated.costType = newCostType;
                updated.maxLevel = newMaxLevel;
                updated.baseCost = newBaseCost;
                updated.costMultiplierOdd = newMultOdd;
                updated.costMultiplierEven = newMultEven;
                updated.costAdditivePerLevel = newAdditive;
                updated.statModifierType = newStatModType;
                updated.statModifierMode = newStatModMode;
                updated.statModifierValuePerLevel = newStatModValue;
                updated.colorTag = newColorTag;

                _data.SetNode(_selectedNodeIndex, updated);
                EditorUtility.SetDirty(_data);
                AssetDatabase.SaveAssets();
            }

            EditorGUILayout.Space(SectionSpacingSmall);
            EditorGUILayout.LabelField("Cost Preview", EditorStyles.boldLabel);
            int previewLevels = node.maxLevel > 0 ? Mathf.Min(node.maxLevel, CostPreviewMaxRows) : CostPreviewMaxRows;
            for (int lvl = 0; lvl < previewLevels; lvl++)
            {
                int cost = SkillTreeData.ComputeNodeCost(node, lvl);
                EditorGUILayout.LabelField($"  Level {lvl} \u2192 {lvl + 1}", $"{cost} {node.costType}");
            }
            if (node.maxLevel == 0)
                EditorGUILayout.LabelField("  ...", "(unlimited)");

            EditorGUILayout.Space(SectionSpacingSmall);
            NodeConnectionsInspector.CollectConnections(_data, _selectedNodeIndex, _connectionsBuffer);
            _connectionsFoldoutOpen = EditorGUILayout.Foldout(
                _connectionsFoldoutOpen,
                $"Connections ({_connectionsBuffer.Count})",
                toggleOnLabelClick: true,
                EditorStyles.foldoutHeader);
            if (_connectionsFoldoutOpen)
            {
                if (_connectionsBuffer.Count == 0)
                {
                    EditorGUILayout.LabelField("No connections", EditorStyles.miniLabel);
                }
                else
                {
                    using (new EditorGUI.IndentLevelScope())
                    {
                        foreach (var row in _connectionsBuffer)
                        {
                            string arrow = row.IsOutgoing ? "→" : "←";
                            EditorGUILayout.LabelField(
                                $"{arrow} Node {row.OtherNodeId}",
                                $"{SkillTreeGrid.DistanceDisplayFromUnits(row.DistanceUnits)} u");
                        }
                    }
                }
            }

            EditorGUILayout.Space(SectionSpacingLarge);
            EditorGUILayout.LabelField("Danger Zone", EditorStyles.boldLabel);

            bool isRoot = node.id == SkillTreeData.CentralNodeId;
            EditorGUI.BeginDisabledGroup(isRoot);
            if (GUILayout.Button("Delete Node"))
            {
                Undo.RegisterCompleteObjectUndo(_data, "Delete Node");
                _data.RemoveNode(node.id);
                InvalidateMirrorSourceIfOutOfRange();
                EditorUtility.SetDirty(_data);
                AssetDatabase.SaveAssets();
                _selectedNodeIndex = -1;
                _activeTab = DesignerTab.SkillTree;
                RebuildNodeLabels();
                _serializedData.Update();
                Repaint();
            }
            EditorGUI.EndDisabledGroup();

            if (isRoot)
                EditorGUILayout.HelpBox("The root node cannot be deleted — it's the unlock seed.", MessageType.Info);
        }

        private void DrawBranchTab()
        {
            EditorGUILayout.Space(SectionSpacingSmall);

            if (!_branchPreviewActive || _branchPreviewParentIndex < 0 || _branchPreviewParentIndex >= _data.Nodes.Count)
            {
                EditorGUILayout.HelpBox("Branch preview not active.", MessageType.Info);
                return;
            }

            EditorGUILayout.LabelField($"Branch from Node {_branchPreviewParentIndex}", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            _branchPreviewSettings.distance = EditorGUILayout.Slider("Distance", _branchPreviewSettings.distance, MinBranchPreviewDistance, MaxBranchPreviewDistance);
            if (EditorGUI.EndChangeCheck())
            {
                BranchPreviewSettingsPersistence.Save(_branchPreviewSettings);
                Repaint();
            }

            EditorGUI.BeginChangeCheck();
            _branchPreviewSettings.angleDegrees = EditorGUILayout.Slider(AngleSliderLabel, _branchPreviewSettings.angleDegrees, MinBranchAngleDegrees, MaxBranchAngleDegrees);
            if (EditorGUI.EndChangeCheck())
            {
                BranchPreviewSettingsPersistence.Save(_branchPreviewSettings);
                Repaint();
            }

            EditorGUI.BeginChangeCheck();
            bool newMirrorEnabled = EditorGUILayout.Toggle(MirrorEnabledLabel, _branchPreviewSettings.mirrorEnabled);
            if (EditorGUI.EndChangeCheck())
            {
                SetMirrorEnabled(newMirrorEnabled);
                BranchPreviewSettingsPersistence.Save(_branchPreviewSettings);
                Repaint();
            }

            if (_branchPreviewSettings.mirrorEnabled)
            {
                EditorGUI.BeginChangeCheck();
                _branchPreviewSettings.mirrorAxisDegrees = EditorGUILayout.Slider(MirrorAxisSliderLabel, _branchPreviewSettings.mirrorAxisDegrees, MinMirrorAxisDegrees, MaxMirrorAxisDegrees);
                if (EditorGUI.EndChangeCheck())
                {
                    MirrorAxisPersistence.Save(_branchPreviewSettings.mirrorAxisDegrees);
                    Repaint();
                }

                DrawMirrorSourceControls();
            }

            if (!string.IsNullOrEmpty(_lastMirrorWarning))
            {
                EditorGUILayout.HelpBox(_lastMirrorWarning, MessageType.Warning);
            }

            EditorGUILayout.Space(SectionSpacingSmall);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Generate"))
                ExecuteGenerateBranch();
            if (GUILayout.Button("Cancel"))
                CancelBranchPreview();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawMirrorSourceControls()
        {
            if (GUILayout.Button(_pickMirrorSourceMode ? CancelPickLabel : PickMirrorSourceLabel))
                TogglePickMirrorSourceMode();

            string sourceStatus = _mirrorSourceNodeIndex == BranchPlacement.NoMirrorSourceOverride
                ? MirrorSourceStatusWorkingNode
                : string.Format(MirrorSourceStatusFormatNode, _mirrorSourceNodeIndex);
            EditorGUILayout.LabelField(sourceStatus);

            EditorGUI.BeginChangeCheck();
            bool newRel = EditorGUILayout.Toggle(AngleRelativeToggleLabel, _angleIsRelativeToMirrorAxis);
            if (EditorGUI.EndChangeCheck())
            {
                _angleIsRelativeToMirrorAxis = newRel;
                Repaint();
            }

            if (_angleIsRelativeToMirrorAxis)
            {
                float resolvedAbs = BranchPlacement.ResolveAbsoluteAngle(
                    _branchPreviewSettings.angleDegrees,
                    _branchPreviewSettings.mirrorAxisDegrees,
                    true);
                EditorGUILayout.LabelField(string.Format(ResolvedAbsoluteAngleFormat, resolvedAbs));
            }
        }

        internal void TogglePickMirrorSourceMode()
        {
            if (_pickMirrorSourceMode)
                CancelMirrorSourcePickMode();
            else
                StartMirrorSourcePickMode();
        }

        internal void BeginBranchPreview(int parentIndex)
        {
            _branchPreviewPreviousTab = _activeTab;
            _branchPreviewPreviousTopLevelTab = _topLevelTab;
            _topLevelTab = TopLevelTab.Designer;
            _branchPreviewActive = true;
            _branchPreviewParentIndex = parentIndex;
            _branchPreviewSettings = _lastBranchPreviewSettings;
            if (_data != null && parentIndex >= 0 && parentIndex < _data.Nodes.Count)
                _branchPreviewSettings.angleDegrees = BranchPreviewSettingsPersistence.ResolveInitialAngle(
                    _data.Nodes[parentIndex].position);
            _mirrorSourceNodeIndex = BranchPlacement.NoMirrorSourceOverride;
            _angleIsRelativeToMirrorAxis = false;
            _pickMirrorSourceMode = false;
            _lastMirrorWarning = null;
            _activeTab = DesignerTab.Branch;
            Repaint();
        }

        internal void EndBranchPreview()
        {
            _branchPreviewActive = false;
            _branchPreviewParentIndex = -1;
            _mirrorSourceNodeIndex = BranchPlacement.NoMirrorSourceOverride;
            _angleIsRelativeToMirrorAxis = false;
            _pickMirrorSourceMode = false;
        }

        internal void SetMirrorEnabled(bool enabled)
        {
            bool wasEnabled = _branchPreviewSettings.mirrorEnabled;
            _branchPreviewSettings.mirrorEnabled = enabled;
            if (!wasEnabled && enabled)
                _angleIsRelativeToMirrorAxis = false;
            if (!enabled)
                _pickMirrorSourceMode = false;
        }

        internal void StartMirrorSourcePickMode()
        {
            if (!_branchPreviewActive || !_branchPreviewSettings.mirrorEnabled) return;
            _pickMirrorSourceMode = true;
            Repaint();
        }

        internal void CancelMirrorSourcePickMode()
        {
            _pickMirrorSourceMode = false;
            Repaint();
        }

        internal bool IsMirrorSourcePickModeActiveForTests => _pickMirrorSourceMode;

        internal void InvalidateMirrorSourceIfOutOfRange()
        {
            int nodeCount = _data != null ? _data.Nodes.Count : 0;
            if (_mirrorSourceNodeIndex >= nodeCount)
                _mirrorSourceNodeIndex = BranchPlacement.NoMirrorSourceOverride;
        }

        internal (Vector2 parentPos, Vector2 mirrorSourcePos, float resolvedAngle, float mirrorBranchAngle) ComputeResolvedPreview()
        {
            if (!_branchPreviewActive || _data == null
                || _branchPreviewParentIndex < 0 || _branchPreviewParentIndex >= _data.Nodes.Count)
            {
                return (Vector2.zero, Vector2.zero, 0f, 0f);
            }

            return BranchPlacement.ResolveBranchPlan(
                _data.Nodes,
                _branchPreviewParentIndex,
                _mirrorSourceNodeIndex,
                _branchPreviewSettings.angleDegrees,
                _branchPreviewSettings.mirrorAxisDegrees,
                _angleIsRelativeToMirrorAxis,
                _branchPreviewSettings.mirrorEnabled);
        }

        internal int MirrorSourceNodeIndexForTests
        {
            get => _mirrorSourceNodeIndex;
            set => _mirrorSourceNodeIndex = value;
        }

        internal bool AngleIsRelativeToMirrorAxisForTests
        {
            get => _angleIsRelativeToMirrorAxis;
            set => _angleIsRelativeToMirrorAxis = value;
        }

        internal void SetDataForTests(SkillTreeData data)
        {
            _data = data;
        }

        internal void CancelBranchPreview()
        {
            EndBranchPreview();
            _activeTab = _branchPreviewPreviousTab;
            _topLevelTab = _branchPreviewPreviousTopLevelTab;
            _lastMirrorWarning = null;
            Repaint();
        }

        private void ExecuteGenerateBranch()
        {
            int parentIndex = _branchPreviewParentIndex;
            if (parentIndex < 0 || parentIndex >= _data.Nodes.Count) return;

            string undoLabel = _branchPreviewSettings.mirrorEnabled
                ? MirrorPairGenerator.UndoLabelMirroredPair
                : MirrorPairGenerator.UndoLabelSingleNode;
            Undo.RegisterCompleteObjectUndo(_data, undoLabel);

            var (_, mirrorSourcePos, resolvedAngle, mirrorBranchAngle) = BranchPlacement.ResolveBranchPlan(
                _data.Nodes,
                parentIndex,
                _mirrorSourceNodeIndex,
                _branchPreviewSettings.angleDegrees,
                _branchPreviewSettings.mirrorAxisDegrees,
                _angleIsRelativeToMirrorAxis,
                _branchPreviewSettings.mirrorEnabled);

            var result = MirrorPairGenerator.TryGenerate(
                _data,
                parentIndex,
                _branchPreviewSettings.distance,
                resolvedAngle,
                _branchPreviewSettings.mirrorEnabled,
                mirrorSourcePos,
                mirrorBranchAngle);

            EditorUtility.SetDirty(_data);
            AssetDatabase.SaveAssets();
            _serializedData.Update();
            RebuildNodeLabels();

            _lastMirrorWarning = result.WarningMessage;
            _lastBranchPreviewSettings = _branchPreviewSettings;
            BranchPreviewSettingsPersistence.Save(_branchPreviewSettings);
            _selectedNodeIndex = _data.Nodes.Count - 1;
            EndBranchPreview();
            InvalidateMirrorSourceIfOutOfRange();
            _activeTab = DesignerTab.Node;
            Repaint();
        }

        private Vector2 ComputeCanvasOrigin(Rect canvasRect)
        {
            return new Vector2(canvasRect.width * 0.5f, canvasRect.height * 0.5f) + _canvasOffset;
        }

        private int HitTestNodeAt(Vector2 mousePos, Rect canvasRect)
        {
            return HitTestNode(mousePos, ComputeCanvasOrigin(canvasRect), _data.Nodes, _data.UnitSize, _data.NodeSize, _canvasZoom);
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

        internal TopLevelTab TopLevelTabForTests
        {
            get => _topLevelTab;
            set => _topLevelTab = value;
        }

        internal bool BranchPreviewActiveForTests => _branchPreviewActive;

        internal void InvokeBeginBranchPreviewForTests(int parentNodeIndex) => BeginBranchPreview(parentNodeIndex);
        internal void InvokeCancelBranchPreviewForTests() => CancelBranchPreview();

        internal static IReadOnlyList<string> TopLevelTabLabelsForTests => TopLevelTabLabels;
        internal static string TopLevelTabEditorPrefKeyForTests => TopLevelTabEditorPrefKey;
        internal void InvokeLoadTopLevelTabFromPrefsForTests() => LoadTopLevelTabFromPrefs();
        internal void InvokeSaveTopLevelTabToPrefsForTests() => SaveTopLevelTabToPrefs();

        internal SerializedObject VisualSettingsSerializedObjectForTests => _visualSettingsSerializedObject;
        internal SkillTreeVisualSettings VisualSettingsForTests => _visualSettings;
        internal SkillTreePreviewPanel VisualTabPreviewPanelForTests => _visualTabPreviewPanel;
        internal VisualElement VisualTabRootForTests => _visualTabRoot;
        internal void InvokeOnVisualSettingsChangedForTests() => OnVisualSettingsChanged();
        internal void InvokeInitializeVisualTabRootForTests() => InitializeVisualTabRoot();
    }
}
