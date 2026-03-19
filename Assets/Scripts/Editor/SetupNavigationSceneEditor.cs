using RogueliteAutoBattler.UI.Core;
using RogueliteAutoBattler.UI.Screens.Combat;
using RogueliteAutoBattler.UI.Screens.Guild;
using RogueliteAutoBattler.UI.Screens.Shop;
using RogueliteAutoBattler.UI.Screens.SkillTree;
using RogueliteAutoBattler.UI.Screens.Village;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace RogueliteAutoBattler.Editor
{
    /// <summary>
    /// Creates the navigation UI hierarchy in one click.
    /// Layout: 60% game (top) | 30% info (middle) | 10% nav bar (bottom, edge to edge).
    /// Combat is the default screen (no tab selected).
    /// 5 tabs: Village, Arbre, Autre, Guilde, Shop.
    /// </summary>
    public static class SetupNavigationSceneEditor
    {
        // Canvas
        private const int CanvasWidth = 1080;
        private const int CanvasHeight = 1920;
        private const float CanvasMatch = 0.5f;

        // Layout ratios (from bottom)
        private const float NavRatio = 0.08f;   // bottom 8%
        private const float InfoTop = 0.40f;     // info ends at 40% (30% tall)
        // Game area = 40% to 100% (top 60%)

        // Tab count
        private const int TabCount = 5;

        // Font sizes
        private const int NavFontSize = 22;
        private const int PanelFontSize = 56;
        private const int InfoFontSize = 32;
        private const int CombatFontSize = 56;

        // Input
        private const string InputAssetPath = "Assets/Settings/InputSystem_Actions.inputactions";

        // Colors
        private static readonly Color NavBarBg = (Color)new Color32(25, 25, 25, 255);
        private static readonly Color NavBtnNormal = (Color)new Color32(40, 40, 40, 255);
        private static readonly Color NavBtnSelected = (Color)new Color32(80, 80, 80, 255);
        private static readonly Color InfoBg = (Color)new Color32(30, 30, 40, 240);
        private static readonly Color ModalBg = (Color)new Color32(0, 0, 0, 150);

        [MenuItem("Roguelite/Setup Navigation UI")]
        private static void SetupNavigationUI()
        {
            Canvas existingCanvas =
                UnityEngine.Object.FindFirstObjectByType<Canvas>(FindObjectsInactive.Include);
            if (existingCanvas != null)
            {
                if (!EditorUtility.DisplayDialog("Canvas Exists", "Replace existing Canvas?", "Replace", "Cancel"))
                    return;
            }

            Undo.IncrementCurrentGroup();
            int undoGroup = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Setup Navigation UI");

            if (existingCanvas != null)
                Undo.DestroyObjectImmediate(existingCanvas.gameObject);

            EventSystem es = UnityEngine.Object.FindFirstObjectByType<EventSystem>(FindObjectsInactive.Include);
            if (es != null)
                Undo.DestroyObjectImmediate(es.gameObject);

            // Also remove old NavigationManager
            NavigationManager oldNav = UnityEngine.Object.FindFirstObjectByType<NavigationManager>(FindObjectsInactive.Include);
            if (oldNav != null)
                Undo.DestroyObjectImmediate(oldNav.gameObject);

            // --- Build ---
            GameObject esGo = CreateEventSystem();
            GameObject canvasGo = CreateCanvas();

            // 1. Game area (behind) — contains combat + tab panels
            GameObject gameArea = CreateArea(canvasGo.transform, "GameArea", InfoTop, 1f, Color.clear);
            UIScreen combatScreen = CreateCombatPanel(gameArea.transform);
            UIScreen[] tabScreens = CreateTabPanels(gameArea.transform);

            // 2. Info area — contains default info + per-tab info panels
            GameObject infoArea = CreateArea(canvasGo.transform, "InfoArea", NavRatio, InfoTop, Color.clear);
            UIScreen defaultInfoScreen = CreateInfoPanel(infoArea.transform, "CombatInfo", "INVENTAIRE / STATS", InfoBg, true);
            UIScreen[] infoScreens = CreateTabInfoPanels(infoArea.transform);

            // 3. Nav bar (edge to edge, no padding)
            GameObject navBar = CreateNavBar(canvasGo.transform);
            TabButton[] tabButtons = CreateTabButtons(navBar.transform);

            // 4. Modal layer
            CreateModalLayer(canvasGo.transform);

            // 5. NavigationManager (child of UICanvas for clean hierarchy)
            GameObject navGo = CreateNavigationManager(canvasGo.transform, combatScreen, defaultInfoScreen, tabButtons, tabScreens, infoScreens);
            WireCancelAction(navGo.GetComponent<NavigationManager>());

            // Undo
            Undo.RegisterCreatedObjectUndo(esGo, "EventSystem");
            Undo.RegisterCreatedObjectUndo(canvasGo, "UICanvas");
            Undo.CollapseUndoOperations(undoGroup);

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Selection.activeGameObject = canvasGo;
            Debug.Log("[SetupNavigationUI] Done. Press Play — click tabs to switch panels.");
        }

        // =============================================================
        // Core objects
        // =============================================================

        private static GameObject CreateEventSystem()
        {
            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            go.AddComponent<InputSystemUIInputModule>();
            return go;
        }

        private static GameObject CreateCanvas()
        {
            var go = new GameObject("UICanvas");
            Canvas c = go.AddComponent<Canvas>();
            c.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler s = go.AddComponent<CanvasScaler>();
            s.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            s.referenceResolution = new Vector2(CanvasWidth, CanvasHeight);
            s.matchWidthOrHeight = CanvasMatch;

            go.AddComponent<GraphicRaycaster>();
            return go;
        }

        /// <summary>Creates a rectangular area between two vertical anchor ratios.</summary>
        private static GameObject CreateArea(Transform parent, string name, float anchorBottom, float anchorTop, Color bg)
        {
            var go = new GameObject(name);
            GameObjectUtility.SetParentAndAlign(go, parent.gameObject);
            RectTransform r = go.AddComponent<RectTransform>();
            r.anchorMin = new Vector2(0, anchorBottom);
            r.anchorMax = new Vector2(1, anchorTop);
            r.offsetMin = Vector2.zero;
            r.offsetMax = Vector2.zero;

            if (bg.a > 0)
                go.AddComponent<Image>().color = bg;

            return go;
        }

        // =============================================================
        // Combat (default screen, always behind)
        // =============================================================

        private static UIScreen CreateCombatPanel(Transform parent)
        {
            var go = new GameObject("CombatPanel");
            GameObjectUtility.SetParentAndAlign(go, parent.gameObject);
            Stretch(go.AddComponent<RectTransform>());

            go.AddComponent<Image>().color = HexToColor("A4161A");

            CanvasGroup cg = go.AddComponent<CanvasGroup>();
            cg.alpha = 1f;
            cg.blocksRaycasts = true;
            cg.interactable = true;

            UIScreen screen = go.AddComponent<CombatScreen>();
            CreateLabel(go.transform, "Label", "COMBAT", CombatFontSize, Color.white);
            return screen;
        }

        // =============================================================
        // Tab panels (overlay on top of combat)
        // =============================================================

        private static UIScreen[] CreateTabPanels(Transform parent)
        {
            var configs = new[]
            {
                new PanelCfg("VillagePanel", "VILLAGE", "2D6A4F", typeof(VillageScreen)),
                new PanelCfg("SkillTreePanel", "ARBRE", "7B2D8E", typeof(SkillTreeScreen)),
                new PanelCfg("AutrePanel", "AUTRE", "555555", typeof(GuildScreen)), // placeholder
                new PanelCfg("GuildePanel", "GUILDE", "1D3557", typeof(ShopScreen)), // placeholder
                new PanelCfg("ShopPanel", "SHOP", "E9C46A", typeof(VillageScreen)), // placeholder
            };

            var screens = new UIScreen[TabCount];
            for (int i = 0; i < configs.Length; i++)
            {
                PanelCfg c = configs[i];
                var go = new GameObject(c.Name);
                GameObjectUtility.SetParentAndAlign(go, parent.gameObject);
                Stretch(go.AddComponent<RectTransform>());

                go.AddComponent<Image>().color = HexToColor(c.Hex);

                // Start hidden — combat is visible by default
                CanvasGroup cg = go.AddComponent<CanvasGroup>();
                cg.alpha = 0f;
                cg.blocksRaycasts = false;
                cg.interactable = false;

                screens[i] = (UIScreen)go.AddComponent(c.ScreenType);
                CreateLabel(go.transform, "Label", c.Label, PanelFontSize, Color.white);
            }
            return screens;
        }

        // =============================================================
        // Info panels (inside InfoArea)
        // =============================================================

        /// <summary>Creates a single info panel with background and label.</summary>
        private static UIScreen CreateInfoPanel(Transform parent, string name, string label, Color bg, bool visible = false)
        {
            var go = new GameObject(name);
            GameObjectUtility.SetParentAndAlign(go, parent.gameObject);
            Stretch(go.AddComponent<RectTransform>());
            go.AddComponent<Image>().color = bg;

            CanvasGroup cg = go.AddComponent<CanvasGroup>();
            cg.alpha = visible ? 1f : 0f;
            cg.blocksRaycasts = visible;
            cg.interactable = visible;

            UIScreen screen = go.AddComponent<UIScreen>();
            CreateLabel(go.transform, "Label", label, InfoFontSize, Color.white);
            return screen;
        }

        /// <summary>Creates one info panel per tab, all hidden by default.</summary>
        private static UIScreen[] CreateTabInfoPanels(Transform parent)
        {
            var configs = new[]
            {
                new InfoCfg("VillageInfo", "VILLAGE", "1B4332"),
                new InfoCfg("ArbreInfo", "ARBRE", "4A1D6B"),
                new InfoCfg("AutreInfo", "AUTRE", "333333"),
                new InfoCfg("GuildeInfo", "GUILDE", "0D2137"),
                new InfoCfg("ShopInfo", "SHOP", "B8962E"),
            };

            var screens = new UIScreen[TabCount];
            for (int i = 0; i < configs.Length; i++)
            {
                InfoCfg c = configs[i];
                screens[i] = CreateInfoPanel(parent, c.Name, c.Label, HexToColor(c.Hex));
            }
            return screens;
        }

        // =============================================================
        // Nav bar — bottom 8%, edge to edge, no padding
        // =============================================================

        private static GameObject CreateNavBar(Transform parent)
        {
            var go = new GameObject("NavBar");
            GameObjectUtility.SetParentAndAlign(go, parent.gameObject);

            RectTransform r = go.AddComponent<RectTransform>();
            r.anchorMin = Vector2.zero;
            r.anchorMax = new Vector2(1, NavRatio);
            r.offsetMin = Vector2.zero;
            r.offsetMax = Vector2.zero;

            go.AddComponent<Image>().color = NavBarBg;

            HorizontalLayoutGroup layout = go.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 0;
            layout.padding = new RectOffset(0, 0, 0, 0);
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;

            return go;
        }

        private static TabButton[] CreateTabButtons(Transform parent)
        {
            var configs = new[]
            {
                new TabCfg("Tab_Village", "Village", 0),
                new TabCfg("Tab_Arbre", "Arbre", 1),
                new TabCfg("Tab_Autre", "Autre", 2),
                new TabCfg("Tab_Guilde", "Guilde", 3),
                new TabCfg("Tab_Shop", "Shop", 4),
            };

            var buttons = new TabButton[TabCount];
            for (int i = 0; i < configs.Length; i++)
            {
                TabCfg c = configs[i];
                var go = new GameObject(c.Name);
                GameObjectUtility.SetParentAndAlign(go, parent.gameObject);
                go.AddComponent<RectTransform>();

                // Button background — fills its cell, no border
                Image img = go.AddComponent<Image>();
                img.color = NavBtnNormal;

                // Button component with flat color transition
                Button btn = go.AddComponent<Button>();
                ColorBlock cb = btn.colors;
                cb.normalColor = Color.white;
                cb.highlightedColor = (Color)new Color32(220, 220, 220, 255);
                cb.pressedColor = (Color)new Color32(180, 180, 180, 255);
                cb.selectedColor = Color.white;
                btn.colors = cb;

                // Force each button to take equal space (flex like CSS)
                LayoutElement le = go.AddComponent<LayoutElement>();
                le.flexibleWidth = 1f;
                le.flexibleHeight = 1f;

                TabButton tb = go.AddComponent<TabButton>();
                buttons[i] = tb;

                // Label — centered text, stays white always
                TextMeshProUGUI tmp = CreateLabel(go.transform, "Label", c.Label, NavFontSize, Color.white);
                tmp.fontStyle = FontStyles.Bold;

                // Wire SerializedFields
                // NOTE: _label is NOT wired — keeps text white. Only _icon (background) changes color.
                var so = new SerializedObject(tb);
                SetInt(so, "_tabIndex", c.Index);
                SetObj(so, "_icon", img);
                SetColor(so, "_normalColor", NavBtnNormal);
                SetColor(so, "_selectedColor", NavBtnSelected);
                so.ApplyModifiedProperties();
            }
            return buttons;
        }

        // =============================================================
        // Modal layer
        // =============================================================

        private static void CreateModalLayer(Transform parent)
        {
            var go = new GameObject("ModalLayer");
            GameObjectUtility.SetParentAndAlign(go, parent.gameObject);
            Stretch(go.AddComponent<RectTransform>());
            go.AddComponent<Image>().color = ModalBg;
            CanvasGroup cg = go.AddComponent<CanvasGroup>();
            cg.alpha = 0f;
            cg.blocksRaycasts = false;
            cg.interactable = false;
        }

        // =============================================================
        // NavigationManager wiring
        // =============================================================

        private static GameObject CreateNavigationManager(Transform parent, UIScreen defaultScreen, UIScreen defaultInfoScreen,
            TabButton[] tabButtons, UIScreen[] tabScreens, UIScreen[] infoScreens)
        {
            var go = new GameObject("NavigationManager");
            GameObjectUtility.SetParentAndAlign(go, parent.gameObject);
            NavigationManager nav = go.AddComponent<NavigationManager>();
            var so = new SerializedObject(nav);

            // Wire default screens
            SetObj(so, "_defaultScreen", defaultScreen);
            SetObj(so, "_defaultInfoScreen", defaultInfoScreen);

            // Wire arrays
            WireArray(so, "_tabButtons", tabButtons, TabCount);
            WireArray(so, "_rootScreens", tabScreens, TabCount);
            WireArray(so, "_infoScreens", infoScreens, TabCount);

            so.ApplyModifiedProperties();
            return go;
        }

        private static void WireArray(SerializedObject so, string name, Component[] items, int count)
        {
            SerializedProperty prop = FindProp(so, name);
            if (prop == null) return;
            prop.arraySize = count;
            for (int i = 0; i < count; i++)
                prop.GetArrayElementAtIndex(i).objectReferenceValue = items[i];
        }

        private static void WireCancelAction(NavigationManager nav)
        {
            InputActionReference cancelRef = FindActionRef(InputAssetPath, "UI", "Cancel");
            if (cancelRef == null)
            {
                Debug.LogWarning("[SetupNavigationUI] UI/Cancel action not found.");
                return;
            }
            var so = new SerializedObject(nav);
            SetObj(so, "_cancelAction", cancelRef);
            so.ApplyModifiedProperties();
        }

        private static InputActionReference FindActionRef(string path, string map, string action)
        {
            UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
            foreach (UnityEngine.Object asset in assets)
            {
                if (asset is InputActionReference r &&
                    r.action != null &&
                    r.action.actionMap != null &&
                    r.action.actionMap.name == map &&
                    r.action.name == action)
                {
                    return r;
                }
            }
            return null;
        }

        // =============================================================
        // Helpers
        // =============================================================

        private static void Stretch(RectTransform r)
        {
            r.anchorMin = Vector2.zero;
            r.anchorMax = Vector2.one;
            r.offsetMin = Vector2.zero;
            r.offsetMax = Vector2.zero;
        }

        private static TextMeshProUGUI CreateLabel(Transform parent, string name, string text, int size, Color color)
        {
            var go = new GameObject(name);
            GameObjectUtility.SetParentAndAlign(go, parent.gameObject);
            Stretch(go.AddComponent<RectTransform>());
            TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Center;
            return tmp;
        }

        private static Color HexToColor(string hex)
        {
            if (ColorUtility.TryParseHtmlString("#" + hex, out Color c)) return c;
            return Color.magenta;
        }

        private static SerializedProperty FindProp(SerializedObject so, string name)
        {
            SerializedProperty p = so.FindProperty(name);
            if (p == null) Debug.LogError($"[SetupNavigationUI] Property '{name}' not found on {so.targetObject.GetType().Name}.");
            return p;
        }

        private static void SetInt(SerializedObject so, string name, int v)
        { SerializedProperty p = FindProp(so, name); if (p != null) p.intValue = v; }

        private static void SetObj(SerializedObject so, string name, UnityEngine.Object v)
        { SerializedProperty p = FindProp(so, name); if (p != null) p.objectReferenceValue = v; }

        private static void SetColor(SerializedObject so, string name, Color v)
        { SerializedProperty p = FindProp(so, name); if (p != null) p.colorValue = v; }

        // =============================================================
        // Config structs
        // =============================================================

        private readonly struct PanelCfg
        {
            public readonly string Name, Label, Hex;
            public readonly System.Type ScreenType;
            public PanelCfg(string name, string label, string hex, System.Type type)
            { Name = name; Label = label; Hex = hex; ScreenType = type; }
        }

        private readonly struct InfoCfg
        {
            public readonly string Name, Label, Hex;
            public InfoCfg(string name, string label, string hex)
            { Name = name; Label = label; Hex = hex; }
        }

        private readonly struct TabCfg
        {
            public readonly string Name, Label;
            public readonly int Index;
            public TabCfg(string name, string label, int index)
            { Name = name; Label = label; Index = index; }
        }
    }
}
