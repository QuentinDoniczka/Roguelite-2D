using RogueliteAutoBattler.UI.Core;
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
    public static class SetupNavigationSceneEditor
    {
        private const int CanvasWidth = 1080;
        private const int CanvasHeight = 1920;
        private const float CanvasMatch = 0.5f;

        private const float NavRatio = 0.08f;
        private const float InfoTop = 0.40f;

        private const int TabCount = 5;

        private const int NavFontSize = 22;
        private const int PanelFontSize = 56;
        private const int InfoFontSize = 32;

        private const string InputAssetPath = "Assets/Settings/InputSystem_Actions.inputactions";

        private static readonly Color NavBarBg = (Color)new Color32(25, 25, 25, 255);
        private static readonly Color NavBtnNormal = (Color)new Color32(40, 40, 40, 255);
        private static readonly Color NavBtnSelected = (Color)new Color32(80, 80, 80, 255);
        private static readonly Color InfoBg = (Color)new Color32(30, 30, 40, 240);
        private static readonly Color BtnHighlighted = (Color)new Color32(220, 220, 220, 255);
        private static readonly Color BtnPressed = (Color)new Color32(180, 180, 180, 255);

        [MenuItem("Roguelite/Setup Navigation UI")]
        private static void SetupNavigationUI()
        {
            Canvas existingCanvas =
                Object.FindFirstObjectByType<Canvas>(FindObjectsInactive.Include);
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

            EventSystem es = Object.FindFirstObjectByType<EventSystem>(FindObjectsInactive.Include);
            if (es != null)
                Undo.DestroyObjectImmediate(es.gameObject);

            NavigationManager oldNav = Object.FindFirstObjectByType<NavigationManager>(FindObjectsInactive.Include);
            if (oldNav != null)
                Undo.DestroyObjectImmediate(oldNav.gameObject);

            GameObject oldWorld = GameObject.Find("CombatWorld");
            if (oldWorld != null)
                Undo.DestroyObjectImmediate(oldWorld);

            Camera mainCam = CombatWorldBuilder.ConfigureMainCamera();

            GameObject combatWorldGo = CombatWorldBuilder.CreateCombatWorld();
            Undo.RegisterCreatedObjectUndo(combatWorldGo, "CombatWorld");

            GameObject esGo = CreateEventSystem();
            GameObject canvasGo = CreateCanvas(mainCam);

            GameObject gameArea = EditorUIFactory.CreateArea(canvasGo.transform, "GameArea", InfoTop, 1f, Color.clear);
            UIScreen combatScreen = CombatHudBuilder.CreateCombatPanel(gameArea.transform);
            UIScreen[] tabScreens = CreateTabPanels(gameArea.transform);

            GameObject infoArea = EditorUIFactory.CreateArea(canvasGo.transform, "InfoArea", NavRatio, InfoTop, Color.clear);
            UIScreen defaultInfoScreen = CreateInfoPanel(infoArea.transform, "CombatInfo", "INVENTAIRE / STATS", InfoBg, true);
            UIScreen[] infoScreens = CreateTabInfoPanels(infoArea.transform);

            GameObject navBar = CreateNavBar(canvasGo.transform);
            TabButton[] tabButtons = CreateTabButtons(navBar.transform);

            GameObject navGo = CreateNavigationManager(canvasGo.transform, combatScreen, defaultInfoScreen, tabButtons, tabScreens, infoScreens);
            WireCancelAction(navGo.GetComponent<NavigationManager>());

            Undo.RegisterCreatedObjectUndo(esGo, "EventSystem");
            Undo.RegisterCreatedObjectUndo(canvasGo, "UICanvas");
            Undo.CollapseUndoOperations(undoGroup);

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Selection.activeGameObject = canvasGo;
            Debug.Log("[SetupNavigationUI] Done. Press Play — click tabs to switch panels.");
        }

        private static GameObject CreateEventSystem()
        {
            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            go.AddComponent<InputSystemUIInputModule>();
            return go;
        }

        private static GameObject CreateCanvas(Camera cam)
        {
            var go = new GameObject("UICanvas");
            Canvas c = go.AddComponent<Canvas>();
            c.renderMode = RenderMode.ScreenSpaceCamera;
            c.worldCamera = cam;
            c.planeDistance = 100f;
            c.sortingLayerName = "UI";
            c.sortingOrder = 0;

            CanvasScaler s = go.AddComponent<CanvasScaler>();
            s.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            s.referenceResolution = new Vector2(CanvasWidth, CanvasHeight);
            s.matchWidthOrHeight = CanvasMatch;

            go.AddComponent<GraphicRaycaster>();
            return go;
        }

        private static UIScreen[] CreateTabPanels(Transform parent)
        {
            var configs = new[]
            {
                new PanelCfg("VillagePanel", "VILLAGE", "2D6A4F", typeof(VillageScreen)),
                new PanelCfg("SkillTreePanel", "ARBRE", "7B2D8E", typeof(SkillTreeScreen)),
                new PanelCfg("AutrePanel", "AUTRE", "555555", typeof(GuildScreen)),
                new PanelCfg("GuildePanel", "GUILDE", "1D3557", typeof(GuildScreen)),
                new PanelCfg("ShopPanel", "SHOP", "E9C46A", typeof(ShopScreen)),
            };

            var screens = new UIScreen[TabCount];
            for (int i = 0; i < configs.Length; i++)
            {
                PanelCfg c = configs[i];
                var go = new GameObject(c.Name);
                GameObjectUtility.SetParentAndAlign(go, parent.gameObject);
                EditorUIFactory.Stretch(go.AddComponent<RectTransform>());

                go.AddComponent<Image>().color = EditorUIFactory.HexToColor(c.Hex);

                EditorUIFactory.SetupCanvasGroup(go, false);

                screens[i] = (UIScreen)go.AddComponent(c.ScreenType);
                EditorUIFactory.CreateLabel(go.transform, "Label", c.Label, PanelFontSize, Color.white);
            }
            return screens;
        }

        private static UIScreen CreateInfoPanel(Transform parent, string name, string label, Color bg, bool visible = false)
        {
            var go = new GameObject(name);
            GameObjectUtility.SetParentAndAlign(go, parent.gameObject);
            EditorUIFactory.Stretch(go.AddComponent<RectTransform>());
            go.AddComponent<Image>().color = bg;

            EditorUIFactory.SetupCanvasGroup(go, visible);

            UIScreen screen = go.AddComponent<UIScreen>();
            EditorUIFactory.CreateLabel(go.transform, "Label", label, InfoFontSize, Color.white);
            return screen;
        }

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
                screens[i] = CreateInfoPanel(parent, c.Name, c.Label, EditorUIFactory.HexToColor(c.Hex));
            }
            return screens;
        }

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

                Image img = go.AddComponent<Image>();
                img.color = NavBtnNormal;

                Button btn = go.AddComponent<Button>();
                ColorBlock cb = btn.colors;
                cb.normalColor = Color.white;
                cb.highlightedColor = BtnHighlighted;
                cb.pressedColor = BtnPressed;
                cb.selectedColor = Color.white;
                btn.colors = cb;

                LayoutElement le = go.AddComponent<LayoutElement>();
                le.flexibleWidth = 1f;
                le.flexibleHeight = 1f;

                TabButton tb = go.AddComponent<TabButton>();
                buttons[i] = tb;

                TextMeshProUGUI tmp = EditorUIFactory.CreateLabel(go.transform, "Label", c.Label, NavFontSize, Color.white);
                tmp.fontStyle = FontStyles.Bold;

                var so = new SerializedObject(tb);
                EditorUIFactory.SetInt(so, "_tabIndex", c.Index);
                EditorUIFactory.SetObj(so, "_icon", img);
                EditorUIFactory.SetColor(so, "_normalColor", NavBtnNormal);
                EditorUIFactory.SetColor(so, "_selectedColor", NavBtnSelected);
                so.ApplyModifiedProperties();
            }
            return buttons;
        }

        private static GameObject CreateNavigationManager(Transform parent, UIScreen defaultScreen, UIScreen defaultInfoScreen,
            TabButton[] tabButtons, UIScreen[] tabScreens, UIScreen[] infoScreens)
        {
            var go = new GameObject("NavigationManager");
            GameObjectUtility.SetParentAndAlign(go, parent.gameObject);
            NavigationManager nav = go.AddComponent<NavigationManager>();
            var so = new SerializedObject(nav);

            EditorUIFactory.SetObj(so, "_defaultScreen", defaultScreen);
            EditorUIFactory.SetObj(so, "_defaultInfoScreen", defaultInfoScreen);

            EditorUIFactory.WireArray(so, "_tabButtons", tabButtons, TabCount);
            EditorUIFactory.WireArray(so, "_rootScreens", tabScreens, TabCount);
            EditorUIFactory.WireArray(so, "_infoScreens", infoScreens, TabCount);

            so.ApplyModifiedProperties();
            return go;
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
            EditorUIFactory.SetObj(so, "_cancelAction", cancelRef);
            so.ApplyModifiedProperties();
        }

        private static InputActionReference FindActionRef(string path, string map, string action)
        {
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
            foreach (Object asset in assets)
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
