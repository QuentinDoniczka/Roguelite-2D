using System;
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
    /// Editor utility that creates the full navigation UI hierarchy and wires all references.
    /// </summary>
    public static class SetupNavigationSceneEditor
    {
        private const int CanvasWidth = 1080;
        private const int CanvasHeight = 1920;
        private const float CanvasMatch = 0.5f;
        private const int TopBarHeight = 100;
        private const int BottomNavHeight = 160;
        private const int BottomNavPadding = 10;
        private const int TabCount = 5;
        private const int DefaultTabIndex = 2;
        private const float CombatTabWidthMultiplier = 1.3f;
        private const int TopBarFontSize = 24;
        private const int PanelFontSizeLarge = 60;
        private const int PanelFontSizeMedium = 48;
        private const int TabFontSize = 20;
        private const int CombatTabFontSize = 24;
        private const string InputAssetPath = "Assets/Settings/InputSystem_Actions.inputactions";

        private static readonly Color TopBarColor = (Color)new Color32(30, 30, 30, 220);
        private static readonly Color BottomNavColor = (Color)new Color32(20, 20, 20, 240);
        private static readonly Color ModalOverlayColor = (Color)new Color32(0, 0, 0, 150);
        private static readonly Color TransparentWhite = new(1f, 1f, 1f, 0f);

        [MenuItem("GameObject/UI/Setup Navigation UI")]
        private static void SetupNavigationUI()
        {
            Canvas existingCanvas = Object.FindFirstObjectByType<Canvas>(FindObjectsInactive.Include);
            if (existingCanvas != null)
            {
                bool shouldReplace = EditorUtility.DisplayDialog(
                    "Canvas Already Exists",
                    "A Canvas already exists in the scene. Do you want to replace it?",
                    "Replace",
                    "Cancel");

                if (!shouldReplace)
                {
                    return;
                }
            }

            Undo.IncrementCurrentGroup();
            int undoGroup = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Setup Navigation UI");

            if (existingCanvas != null)
            {
                Undo.DestroyObjectImmediate(existingCanvas.gameObject);
            }

            EventSystem existingEventSystem = Object.FindFirstObjectByType<EventSystem>(FindObjectsInactive.Include);
            if (existingEventSystem != null)
            {
                Undo.DestroyObjectImmediate(existingEventSystem.gameObject);
            }

            GameObject eventSystemGo = CreateEventSystem();
            GameObject canvasGo = CreateCanvas();
            CreateTopBar(canvasGo.transform);
            GameObject contentArea = CreateContentArea(canvasGo.transform);

            UIScreen[] rootScreens = CreatePanels(contentArea.transform);

            GameObject bottomNav = CreateBottomNav(canvasGo.transform);
            TabButton[] tabButtons = CreateTabButtons(bottomNav.transform);

            CreateModalLayer(canvasGo.transform);

            GameObject navManagerGo = CreateNavigationManager(tabButtons, rootScreens);

            WireCancelAction(navManagerGo.GetComponent<NavigationManager>());

            Undo.RegisterCreatedObjectUndo(eventSystemGo, "Create EventSystem");
            Undo.RegisterCreatedObjectUndo(canvasGo, "Create UICanvas");
            Undo.RegisterCreatedObjectUndo(navManagerGo, "Create NavigationManager");

            Undo.CollapseUndoOperations(undoGroup);

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            Selection.activeGameObject = canvasGo;
            Debug.Log("[SetupNavigationUI] Navigation UI created and wired successfully.");
        }

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

            Canvas canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(CanvasWidth, CanvasHeight);
            scaler.matchWidthOrHeight = CanvasMatch;

            go.AddComponent<GraphicRaycaster>();

            return go;
        }

        private static GameObject CreateTopBar(Transform parent)
        {
            var go = new GameObject("TopBar");
            GameObjectUtility.SetParentAndAlign(go, parent.gameObject);

            RectTransform rect = go.AddComponent<RectTransform>();
            SetAnchorStretchTop(rect, TopBarHeight);

            go.AddComponent<Image>().color = TopBarColor;

            CreateLabel(go.transform, "TopBarText", "Palier -- | Or: 0 | Reset: --",
                TopBarFontSize, Color.white);

            return go;
        }

        private static GameObject CreateContentArea(Transform parent)
        {
            var go = new GameObject("ContentArea");
            GameObjectUtility.SetParentAndAlign(go, parent.gameObject);

            RectTransform rect = go.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(0, BottomNavHeight);
            rect.offsetMax = new Vector2(0, -TopBarHeight);

            return go;
        }

        private static UIScreen[] CreatePanels(Transform parent)
        {
            var panelConfigs = new[]
            {
                new PanelConfig("VillagePanel", "VILLAGE", HexToColor("2D6A4F"), PanelFontSizeLarge, false, typeof(VillageScreen)),
                new PanelConfig("SkillTreePanel", "ARBRE DE COMPETENCES", HexToColor("7B2D8E"), PanelFontSizeMedium, false, typeof(SkillTreeScreen)),
                new PanelConfig("CombatPanel", "COMBAT", HexToColor("A4161A"), PanelFontSizeLarge, true, typeof(CombatScreen)),
                new PanelConfig("GuildPanel", "GUILDE", HexToColor("1D3557"), PanelFontSizeLarge, false, typeof(GuildScreen)),
                new PanelConfig("ShopPanel", "SHOP", HexToColor("E9C46A"), PanelFontSizeLarge, false, typeof(ShopScreen)),
            };

            var screens = new UIScreen[TabCount];

            for (int i = 0; i < panelConfigs.Length; i++)
            {
                PanelConfig config = panelConfigs[i];
                var panelGo = new GameObject(config.Name);
                GameObjectUtility.SetParentAndAlign(panelGo, parent.gameObject);

                RectTransform rect = panelGo.AddComponent<RectTransform>();
                SetAnchorStretchFill(rect);

                panelGo.AddComponent<Image>().color = config.BackgroundColor;

                CanvasGroup canvasGroup = panelGo.AddComponent<CanvasGroup>();
                canvasGroup.alpha = config.IsDefault ? 1f : 0f;
                canvasGroup.blocksRaycasts = config.IsDefault;
                canvasGroup.interactable = config.IsDefault;

                screens[i] = (UIScreen)panelGo.AddComponent(config.ScreenType);

                CreateLabel(panelGo.transform, "Label", config.LabelText,
                    config.FontSize, Color.white);
            }

            return screens;
        }

        private static GameObject CreateBottomNav(Transform parent)
        {
            var go = new GameObject("BottomNav");
            GameObjectUtility.SetParentAndAlign(go, parent.gameObject);

            RectTransform rect = go.AddComponent<RectTransform>();
            SetAnchorStretchBottom(rect, BottomNavHeight);

            go.AddComponent<Image>().color = BottomNavColor;

            HorizontalLayoutGroup layout = go.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 0;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;
            layout.padding = new RectOffset(BottomNavPadding, BottomNavPadding, BottomNavPadding, BottomNavPadding);

            return go;
        }

        private static TabButton[] CreateTabButtons(Transform parent)
        {
            // NOTE: Tab labels use emoji characters. These require a TMP font asset that includes
            // the corresponding glyphs. Replace with plain text (e.g. "Village", "Combat") if the
            // font does not support them.
            var tabConfigs = new[]
            {
                new TabConfig("Tab_Village", "\ud83c\udfe0\nVillage", 0, TabFontSize, false),
                new TabConfig("Tab_SkillTree", "\ud83c\udf33\nArbre", 1, TabFontSize, false),
                new TabConfig("Tab_Combat", "\u2694\nCOMBAT", 2, CombatTabFontSize, true),
                new TabConfig("Tab_Guild", "\ud83d\udc65\nGuilde", 3, TabFontSize, false),
                new TabConfig("Tab_Shop", "\ud83d\udcb0\nShop", 4, TabFontSize, false),
            };

            var tabButtons = new TabButton[TabCount];

            for (int i = 0; i < tabConfigs.Length; i++)
            {
                TabConfig config = tabConfigs[i];
                var tabGo = new GameObject(config.Name);
                GameObjectUtility.SetParentAndAlign(tabGo, parent.gameObject);

                tabGo.AddComponent<RectTransform>();

                Image image = tabGo.AddComponent<Image>();
                image.color = TransparentWhite;

                tabGo.AddComponent<Button>();

                TabButton tabButton = tabGo.AddComponent<TabButton>();
                tabButtons[i] = tabButton;

                if (config.IsCombat)
                {
                    LayoutElement layoutElement = tabGo.AddComponent<LayoutElement>();
                    layoutElement.flexibleWidth = CombatTabWidthMultiplier;
                }

                TextMeshProUGUI tmp = CreateLabel(tabGo.transform, "Label", config.LabelText,
                    config.FontSize, Color.white);

                if (config.IsCombat)
                {
                    tmp.fontStyle = FontStyles.Bold;
                }

                var tabSo = new SerializedObject(tabButton);
                SetIntProperty(tabSo, "_tabIndex", config.Index);
                SetObjectProperty(tabSo, "_icon", image);
                SetObjectProperty(tabSo, "_label", tmp);
                SetColorProperty(tabSo, "_normalColor", Color.gray);
                SetColorProperty(tabSo, "_selectedColor", Color.white);
                tabSo.ApplyModifiedProperties();
            }

            return tabButtons;
        }

        private static void CreateModalLayer(Transform parent)
        {
            var go = new GameObject("ModalLayer");
            GameObjectUtility.SetParentAndAlign(go, parent.gameObject);

            RectTransform rect = go.AddComponent<RectTransform>();
            SetAnchorStretchFill(rect);

            go.AddComponent<Image>().color = ModalOverlayColor;

            CanvasGroup canvasGroup = go.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }

        private static GameObject CreateNavigationManager(TabButton[] tabButtons, UIScreen[] rootScreens)
        {
            var go = new GameObject("NavigationManager");

            NavigationManager navManager = go.AddComponent<NavigationManager>();

            var so = new SerializedObject(navManager);

            SerializedProperty tabButtonsProp = FindPropertyChecked(so, "_tabButtons");
            if (tabButtonsProp != null)
            {
                tabButtonsProp.arraySize = TabCount;
                for (int i = 0; i < TabCount; i++)
                {
                    tabButtonsProp.GetArrayElementAtIndex(i).objectReferenceValue = tabButtons[i];
                }
            }

            SerializedProperty rootScreensProp = FindPropertyChecked(so, "_rootScreens");
            if (rootScreensProp != null)
            {
                rootScreensProp.arraySize = TabCount;
                for (int i = 0; i < TabCount; i++)
                {
                    rootScreensProp.GetArrayElementAtIndex(i).objectReferenceValue = rootScreens[i];
                }
            }

            SetIntProperty(so, "_defaultTabIndex", DefaultTabIndex);

            so.ApplyModifiedProperties();

            return go;
        }

        private static void WireCancelAction(NavigationManager navManager)
        {
            InputActionReference cancelRef = FindActionReference(InputAssetPath, "UI", "Cancel");

            if (cancelRef == null)
            {
                Debug.LogWarning("[SetupNavigationUI] Could not find InputActionReference for UI/Cancel. " +
                                 "Check that InputSystem_Actions.inputactions exists at Assets/Settings/.");
                return;
            }

            var so = new SerializedObject(navManager);
            SetObjectProperty(so, "_cancelAction", cancelRef);
            so.ApplyModifiedProperties();
        }

        private static InputActionReference FindActionReference(string assetPath, string mapName, string actionName)
        {
            Object[] subAssets = AssetDatabase.LoadAllAssetsAtPath(assetPath);

            foreach (Object subAsset in subAssets)
            {
                if (subAsset is not InputActionReference actionRef)
                {
                    continue;
                }

                if (actionRef.action != null &&
                    actionRef.action.actionMap != null &&
                    actionRef.action.actionMap.name == mapName &&
                    actionRef.action.name == actionName)
                {
                    return actionRef;
                }
            }

            return null;
        }

        private static SerializedProperty FindPropertyChecked(SerializedObject so, string propertyName)
        {
            SerializedProperty prop = so.FindProperty(propertyName);
            if (prop == null)
            {
                Debug.LogError($"[SetupNavigationUI] SerializedProperty '{propertyName}' not found on {so.targetObject.GetType().Name}. " +
                               "Field may have been renamed or removed.");
            }

            return prop;
        }

        private static void SetIntProperty(SerializedObject so, string propertyName, int value)
        {
            SerializedProperty prop = FindPropertyChecked(so, propertyName);
            if (prop != null) prop.intValue = value;
        }

        private static void SetObjectProperty(SerializedObject so, string propertyName, Object value)
        {
            SerializedProperty prop = FindPropertyChecked(so, propertyName);
            if (prop != null) prop.objectReferenceValue = value;
        }

        private static void SetColorProperty(SerializedObject so, string propertyName, Color value)
        {
            SerializedProperty prop = FindPropertyChecked(so, propertyName);
            if (prop != null) prop.colorValue = value;
        }

        // ---------------------------------------------------------------
        // Helpers
        // ---------------------------------------------------------------

        private static TextMeshProUGUI CreateLabel(Transform parent, string name, string text,
            int fontSize, Color color)
        {
            var textGo = new GameObject(name);
            GameObjectUtility.SetParentAndAlign(textGo, parent.gameObject);

            RectTransform textRect = textGo.AddComponent<RectTransform>();
            SetAnchorStretchFill(textRect);

            TextMeshProUGUI tmp = textGo.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Center;

            return tmp;
        }

        private static void SetAnchorStretchTop(RectTransform rect, float height)
        {
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(0, -height);
            rect.offsetMax = Vector2.zero;
            rect.pivot = new Vector2(0.5f, 1);
        }

        private static void SetAnchorStretchBottom(RectTransform rect, float height)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = new Vector2(1, 0);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = new Vector2(0, height);
            rect.pivot = new Vector2(0.5f, 0);
        }

        private static void SetAnchorStretchFill(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static Color HexToColor(string hex)
        {
            if (!ColorUtility.TryParseHtmlString("#" + hex, out Color color))
            {
                Debug.LogWarning($"[SetupNavigationUI] Failed to parse hex color: #{hex}. Defaulting to magenta.");
                return Color.magenta;
            }

            return color;
        }

        // ---------------------------------------------------------------
        // Config structs
        // ---------------------------------------------------------------

        private readonly struct PanelConfig
        {
            public readonly string Name;
            public readonly string LabelText;
            public readonly Color BackgroundColor;
            public readonly int FontSize;
            public readonly bool IsDefault;
            public readonly Type ScreenType;

            public PanelConfig(string name, string labelText, Color backgroundColor, int fontSize, bool isDefault, Type screenType)
            {
                Name = name;
                LabelText = labelText;
                BackgroundColor = backgroundColor;
                FontSize = fontSize;
                IsDefault = isDefault;
                ScreenType = screenType;
            }
        }

        private readonly struct TabConfig
        {
            public readonly string Name;
            public readonly string LabelText;
            public readonly int Index;
            public readonly int FontSize;
            public readonly bool IsCombat;

            public TabConfig(string name, string labelText, int index, int fontSize, bool isCombat)
            {
                Name = name;
                LabelText = labelText;
                Index = index;
                FontSize = fontSize;
                IsCombat = isCombat;
            }
        }
    }
}
