using RogueliteAutoBattler.Data;
using RogueliteAutoBattler.Economy;
using RogueliteAutoBattler.UI.Screens.SkillTree;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace RogueliteAutoBattler.Editor
{
    internal static class SkillTreeBuilder
    {
        internal const string CircleSpritePath = "Assets/Sprites/UI/circle_white.png";

        private static readonly Color32 PanelBg         = new Color32(30,  30,  58,  255);
        private static readonly Color32 HeaderBg        = new Color32(34,  34,  64,  255);
        private static readonly Color32 DetailBg        = new Color32(26,  26,  53,  255);
        private static readonly Color32 ActionBg        = new Color32(30,  30,  58,  255);
        private static readonly Color32 GoldColor       = new Color32(255, 215,   0, 255);
        private static readonly Color32 SubtextColor    = new Color32(136, 136, 136, 255);
        private static readonly Color32 DimColor        = new Color32( 85,  85,  85, 255);
        private static readonly Color32 ButtonLabelColor = new Color32( 17,  17,  17, 255);
        private static readonly Color32 ErrorColor      = new Color32(220,  50,  50, 255);

        private const float TopBorderHeight     =   3f;
        private const float HeaderSectionHeight  =  52f;
        private const float DetailSectionHeight  =  30f;
        private const float ActionSectionHeight  =  72f;
        private const float TotalPanelHeight     = TopBorderHeight + HeaderSectionHeight + DetailSectionHeight + ActionSectionHeight;
        private const float LevelBlockWidth      =  48f;
        private const float SectionSpacing       =   8f;
        private const float SubItemSpacing       =   2f;
        private const int   StatNameFontSize     =  14;
        private const int   StatDescFontSize     =   9;
        private const int   LevelFontSize        =  22;
        private const int   LevelCaptionSize     =   7;
        private const int   BonusFontSize        =  10;
        private const int   CostFontSize         =  11;
        private const int   CostMultFontSize     =   8;
        private const int   DeficitFontSize      =   9;
        private const int   UpgradeBtnFontSize   =  11;
        private const int   IconSize             =  36;
        private const int   UpgradeBtnMinWidth   = 100;
        private const int   HeaderPad            =   8;
        private const int   DetailPadV           =   6;
        private const int   DetailPadH           =  10;
        private const int   ActionPadV           =   8;
        private const int   ActionPadH           =  10;

        internal static void BuildSkillTreeContent(GameObject skillTreePanel)
        {
            Transform existingLabel = skillTreePanel.transform.Find("Label");
            if (existingLabel != null)
                Object.DestroyImmediate(existingLabel.gameObject);

            var viewportGo = new GameObject("SkillTreeViewport");
            GameObjectUtility.SetParentAndAlign(viewportGo, skillTreePanel);
            EditorUIFactory.Stretch(viewportGo.AddComponent<RectTransform>());

            Image viewportImage = viewportGo.AddComponent<Image>();
            viewportImage.color = Color.black;
            viewportImage.raycastTarget = true;

            viewportGo.AddComponent<RectMask2D>();

            SkillTreeInputHandler inputHandler = viewportGo.AddComponent<SkillTreeInputHandler>();

            var contentGo = new GameObject("Content");
            GameObjectUtility.SetParentAndAlign(contentGo, viewportGo);
            RectTransform contentRect = contentGo.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0.5f, 0.5f);
            contentRect.anchorMax = new Vector2(0.5f, 0.5f);
            contentRect.pivot = new Vector2(0.5f, 0.5f);
            contentRect.sizeDelta = Vector2.zero;
            contentRect.localScale = Vector3.one;

            SkillTreeNodeManager nodeManager = contentGo.AddComponent<SkillTreeNodeManager>();

            var inputHandlerSO = new SerializedObject(inputHandler);
            EditorUIFactory.SetObj(inputHandlerSO, "_content", contentRect);
            inputHandlerSO.ApplyModifiedProperties();

            var nodeManagerSO = new SerializedObject(nodeManager);
            EditorUIFactory.SetObj(nodeManagerSO, "_content", contentRect);
            var skillTreeData = AssetDatabase.LoadAssetAtPath<SkillTreeData>(SkillTreeData.DefaultAssetPath);
            if (skillTreeData != null)
            {
                EditorUIFactory.SetObj(nodeManagerSO, "_data", skillTreeData);
                EditorUIFactory.SetColor(nodeManagerSO, "_edgeColor", skillTreeData.EdgeColor);
                EditorUIFactory.SetFloat(nodeManagerSO, "_edgeThickness", skillTreeData.EdgeThickness);
            }

            var circleSprite = EnsureCircleSprite();
            if (circleSprite != null)
                EditorUIFactory.SetObj(nodeManagerSO, "_circleSprite", circleSprite);

            nodeManagerSO.ApplyModifiedProperties();

            SkillTreeScreen screen = skillTreePanel.GetComponent<SkillTreeScreen>();
            if (screen != null)
            {
                var screenSO = new SerializedObject(screen);
                EditorUIFactory.SetObj(screenSO, "_inputHandler", inputHandler);
                EditorUIFactory.SetObj(screenSO, "_nodeManager", nodeManager);
                screenSO.ApplyModifiedProperties();
            }
            else
            {
                Debug.LogError("[SkillTreeBuilder] SkillTreeScreen component not found on skillTreePanel.");
            }
        }

        internal static void BuildDetailPanel(GameObject skillTreePanel)
        {
            Transform existing = skillTreePanel.transform.Find("NodeDetailPanel");
            if (existing != null)
                Object.DestroyImmediate(existing.gameObject);

            if (skillTreePanel.GetComponent<RectMask2D>() == null)
                skillTreePanel.AddComponent<RectMask2D>();

            TMP_FontAsset bangersFont = EditorUIFactory.LoadBangersFont(nameof(SkillTreeBuilder));

            var panelGo = new GameObject("NodeDetailPanel");
            GameObjectUtility.SetParentAndAlign(panelGo, skillTreePanel);

            RectTransform panelRect = panelGo.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0, 0);
            panelRect.anchorMax = new Vector2(1, 0);
            panelRect.pivot = new Vector2(0.5f, 0);
            panelRect.sizeDelta = new Vector2(0, TotalPanelHeight);

            Image panelBgImg = panelGo.AddComponent<Image>();
            panelBgImg.color = PanelBg;

            CanvasGroup canvasGroup = EditorUIFactory.SetupCanvasGroup(panelGo, false);

            VerticalLayoutGroup panelVlg = panelGo.AddComponent<VerticalLayoutGroup>();
            panelVlg.childControlWidth      = true;
            panelVlg.childControlHeight     = false;
            panelVlg.childForceExpandWidth  = true;
            panelVlg.childForceExpandHeight = false;
            panelVlg.spacing = 0f;

            Image topBorderImg = BuildTopBorder(panelGo.transform);

            (Image iconImage, TMP_Text statNameLabel, TMP_Text statDescLabel,
             TMP_Text levelLabel, TMP_Text levelCaptionLabel) = BuildHeaderSection(panelGo.transform, bangersFont);

            (TMP_Text currentBonusLabel, TMP_Text nextBonusLabel) = BuildDetailSection(panelGo.transform, bangersFont);

            (TMP_Text costLabel, TMP_Text costMultiplierLabel, TMP_Text deficitLabel,
             Button upgradeButton, TMP_Text upgradeButtonLabel, Image upgradeButtonImage) = BuildActionSection(panelGo.transform, bangersFont);

            SkillTreeDetailPanel detailPanel = panelGo.AddComponent<SkillTreeDetailPanel>();
            var detailPanelSO = new SerializedObject(detailPanel);
            EditorUIFactory.SetObj(detailPanelSO, "_canvasGroup",         canvasGroup);
            EditorUIFactory.SetObj(detailPanelSO, "_panelRect",           panelRect);
            EditorUIFactory.SetObj(detailPanelSO, "_topBorder",           topBorderImg);
            EditorUIFactory.SetObj(detailPanelSO, "_iconImage",           iconImage);
            EditorUIFactory.SetObj(detailPanelSO, "_statNameLabel",       statNameLabel);
            EditorUIFactory.SetObj(detailPanelSO, "_statDescLabel",       statDescLabel);
            EditorUIFactory.SetObj(detailPanelSO, "_levelLabel",          levelLabel);
            EditorUIFactory.SetObj(detailPanelSO, "_levelCaptionLabel",   levelCaptionLabel);
            EditorUIFactory.SetObj(detailPanelSO, "_currentBonusLabel",   currentBonusLabel);
            EditorUIFactory.SetObj(detailPanelSO, "_nextBonusLabel",      nextBonusLabel);
            EditorUIFactory.SetObj(detailPanelSO, "_costLabel",           costLabel);
            EditorUIFactory.SetObj(detailPanelSO, "_upgradeButton",       upgradeButton);
            EditorUIFactory.SetObj(detailPanelSO, "_upgradeButtonLabel",  upgradeButtonLabel);
            EditorUIFactory.SetObj(detailPanelSO, "_upgradeButtonImage",  upgradeButtonImage);
            EditorUIFactory.SetObj(detailPanelSO, "_deficitLabel",        deficitLabel);
            EditorUIFactory.SetObj(detailPanelSO, "_costMultiplierLabel", costMultiplierLabel);
            detailPanelSO.ApplyModifiedProperties();

            SkillTreeProgress progress = EnsureSkillTreeProgressAsset();
            GoldWallet goldWallet = FindOrCreateGoldWallet();
            SkillPointWallet skillPointWallet = FindOrCreateSkillPointWallet();

            SkillTreeScreen screen = skillTreePanel.GetComponent<SkillTreeScreen>();
            if (screen != null)
            {
                var screenSO = new SerializedObject(screen);
                EditorUIFactory.SetObj(screenSO, "_detailPanel",      detailPanel);
                EditorUIFactory.SetObj(screenSO, "_progress",         progress);
                EditorUIFactory.SetObj(screenSO, "_skillPointWallet", skillPointWallet);
                EditorUIFactory.SetObj(screenSO, "_goldWallet",       goldWallet);

                var skillTreeData = AssetDatabase.LoadAssetAtPath<SkillTreeData>(SkillTreeData.DefaultAssetPath);
                if (skillTreeData != null)
                    EditorUIFactory.SetObj(screenSO, "_data", skillTreeData);

                screenSO.ApplyModifiedProperties();
            }
            else
            {
                Debug.LogError("[SkillTreeBuilder] SkillTreeScreen component not found — cannot wire detail panel references.");
            }
        }

        private static Image BuildTopBorder(Transform parent)
        {
            var go = new GameObject("TopBorder");
            GameObjectUtility.SetParentAndAlign(go, parent.gameObject);
            RectTransform r = go.AddComponent<RectTransform>();
            r.sizeDelta = new Vector2(0f, TopBorderHeight);

            LayoutElement le = go.AddComponent<LayoutElement>();
            le.minHeight       = TopBorderHeight;
            le.preferredHeight = TopBorderHeight;
            le.flexibleHeight  = 0f;

            Image img = go.AddComponent<Image>();
            img.color = GoldColor;
            return img;
        }

        private static (Image icon, TMP_Text statName, TMP_Text statDesc, TMP_Text level, TMP_Text levelCaption)
            BuildHeaderSection(Transform parent, TMP_FontAsset font)
        {
            var sectionGo = new GameObject("HeaderSection");
            GameObjectUtility.SetParentAndAlign(sectionGo, parent.gameObject);
            RectTransform sectionRect = sectionGo.AddComponent<RectTransform>();
            sectionRect.sizeDelta = new Vector2(0f, HeaderSectionHeight);

            Image sectionBg = sectionGo.AddComponent<Image>();
            sectionBg.color = HeaderBg;

            HorizontalLayoutGroup hlg = sectionGo.AddComponent<HorizontalLayoutGroup>();
            hlg.padding              = new RectOffset(HeaderPad, HeaderPad, HeaderPad, HeaderPad);
            hlg.spacing              = SectionSpacing;
            hlg.childAlignment       = TextAnchor.MiddleLeft;
            hlg.childControlWidth    = true;
            hlg.childControlHeight   = true;
            hlg.childForceExpandWidth  = false;
            hlg.childForceExpandHeight = false;

            LayoutElement sectionLe = sectionGo.AddComponent<LayoutElement>();
            sectionLe.minHeight       = HeaderSectionHeight;
            sectionLe.preferredHeight = HeaderSectionHeight;
            sectionLe.flexibleHeight  = 0f;

            Image iconImage = BuildStatIcon(sectionGo.transform);
            (TMP_Text statName, TMP_Text statDesc) = BuildHeaderInfo(sectionGo.transform, font);
            (TMP_Text level, TMP_Text levelCaption) = BuildLevelBlock(sectionGo.transform, font);

            return (iconImage, statName, statDesc, level, levelCaption);
        }

        private static Image BuildStatIcon(Transform parent)
        {
            var go = new GameObject("StatIcon");
            GameObjectUtility.SetParentAndAlign(go, parent.gameObject);
            go.AddComponent<RectTransform>();

            Image img = go.AddComponent<Image>();
            img.color = Color.white;
            img.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");

            LayoutElement le = go.AddComponent<LayoutElement>();
            le.minWidth       = IconSize;
            le.minHeight      = IconSize;
            le.preferredWidth = IconSize;
            le.preferredHeight= IconSize;
            le.flexibleWidth  = 0f;
            le.flexibleHeight = 0f;

            return img;
        }

        private static (TMP_Text statName, TMP_Text statDesc) BuildHeaderInfo(Transform parent, TMP_FontAsset font)
        {
            var go = new GameObject("HeaderInfo");
            GameObjectUtility.SetParentAndAlign(go, parent.gameObject);
            go.AddComponent<RectTransform>();

            VerticalLayoutGroup vlg = go.AddComponent<VerticalLayoutGroup>();
            vlg.childAlignment       = TextAnchor.MiddleLeft;
            vlg.childControlWidth    = true;
            vlg.childControlHeight   = true;
            vlg.childForceExpandWidth  = true;
            vlg.childForceExpandHeight = false;
            vlg.spacing = SubItemSpacing;

            LayoutElement le = go.AddComponent<LayoutElement>();
            le.flexibleWidth = 1f;

            TMP_Text statName = CreateLayoutLabel(go.transform, "StatNameLabel", "Attack",
                StatNameFontSize, Color.white, font, FontStyles.Bold, TextAlignmentOptions.Left);

            TMP_Text statDesc = CreateLayoutLabel(go.transform, "StatDescLabel", "+5 ATK per level (Flat)",
                StatDescFontSize, SubtextColor, font, FontStyles.Normal, TextAlignmentOptions.Left);

            return (statName, statDesc);
        }

        private static (TMP_Text level, TMP_Text levelCaption) BuildLevelBlock(Transform parent, TMP_FontAsset font)
        {
            var go = new GameObject("LevelBlock");
            GameObjectUtility.SetParentAndAlign(go, parent.gameObject);
            go.AddComponent<RectTransform>();

            VerticalLayoutGroup vlg = go.AddComponent<VerticalLayoutGroup>();
            vlg.childAlignment       = TextAnchor.MiddleRight;
            vlg.childControlWidth    = true;
            vlg.childControlHeight   = true;
            vlg.childForceExpandWidth  = false;
            vlg.childForceExpandHeight = false;
            vlg.spacing = 0f;

            LayoutElement le = go.AddComponent<LayoutElement>();
            le.minWidth      = LevelBlockWidth;
            le.preferredWidth= LevelBlockWidth;
            le.flexibleWidth = 0f;

            TMP_Text levelLabel = CreateLayoutLabel(go.transform, "LevelLabel", "0",
                LevelFontSize, GoldColor, font, FontStyles.Bold, TextAlignmentOptions.Right);

            TMP_Text levelCaption = CreateLayoutLabel(go.transform, "LevelCaptionLabel", "LEVEL",
                LevelCaptionSize, SubtextColor, font, FontStyles.Normal, TextAlignmentOptions.Right);

            return (levelLabel, levelCaption);
        }

        private static (TMP_Text currentBonus, TMP_Text nextBonus) BuildDetailSection(Transform parent, TMP_FontAsset font)
        {
            var go = new GameObject("DetailSection");
            GameObjectUtility.SetParentAndAlign(go, parent.gameObject);
            RectTransform r = go.AddComponent<RectTransform>();
            r.sizeDelta = new Vector2(0f, DetailSectionHeight);

            Image bg = go.AddComponent<Image>();
            bg.color = DetailBg;

            HorizontalLayoutGroup hlg = go.AddComponent<HorizontalLayoutGroup>();
            hlg.padding              = new RectOffset(DetailPadH, DetailPadH, DetailPadV, DetailPadV);
            hlg.spacing              = SectionSpacing;
            hlg.childAlignment       = TextAnchor.MiddleLeft;
            hlg.childControlWidth    = true;
            hlg.childControlHeight   = true;
            hlg.childForceExpandWidth  = false;
            hlg.childForceExpandHeight = false;

            LayoutElement le = go.AddComponent<LayoutElement>();
            le.minHeight       = DetailSectionHeight;
            le.preferredHeight = DetailSectionHeight;
            le.flexibleHeight  = 0f;

            TMP_Text currentBonus = CreateLayoutLabel(go.transform, "CurrentBonusLabel", "Current: +0",
                BonusFontSize, SubtextColor, font, FontStyles.Normal, TextAlignmentOptions.Left);
            SetFlexibleWidth(go.transform, "CurrentBonusLabel", 1f);

            TMP_Text nextBonus = CreateLayoutLabel(go.transform, "NextBonusLabel", "Next: +5 (+5)",
                BonusFontSize, GoldColor, font, FontStyles.Normal, TextAlignmentOptions.Right);
            SetFlexibleWidth(go.transform, "NextBonusLabel", 1f);

            return (currentBonus, nextBonus);
        }

        private static (TMP_Text cost, TMP_Text costMult, TMP_Text deficit,
                        Button upgradeBtn, TMP_Text upgradeBtnLabel, Image upgradeBtnImage)
            BuildActionSection(Transform parent, TMP_FontAsset font)
        {
            var go = new GameObject("ActionSection");
            GameObjectUtility.SetParentAndAlign(go, parent.gameObject);
            RectTransform r = go.AddComponent<RectTransform>();
            r.sizeDelta = new Vector2(0f, ActionSectionHeight);

            Image bg = go.AddComponent<Image>();
            bg.color = ActionBg;

            HorizontalLayoutGroup hlg = go.AddComponent<HorizontalLayoutGroup>();
            hlg.padding              = new RectOffset(ActionPadH, ActionPadH, ActionPadV, ActionPadV);
            hlg.spacing              = SectionSpacing;
            hlg.childAlignment       = TextAnchor.MiddleLeft;
            hlg.childControlWidth    = true;
            hlg.childControlHeight   = true;
            hlg.childForceExpandWidth  = false;
            hlg.childForceExpandHeight = true;

            LayoutElement sectionLe = go.AddComponent<LayoutElement>();
            sectionLe.minHeight       = ActionSectionHeight;
            sectionLe.preferredHeight = ActionSectionHeight;
            sectionLe.flexibleHeight  = 1f;

            (TMP_Text cost, TMP_Text costMult, TMP_Text deficit) = BuildCostBlock(go.transform, font);
            (Button upgradeBtn, TMP_Text upgradeBtnLabel, Image upgradeBtnImage) = BuildUpgradeButton(go.transform, font);

            return (cost, costMult, deficit, upgradeBtn, upgradeBtnLabel, upgradeBtnImage);
        }

        private static (TMP_Text cost, TMP_Text costMult, TMP_Text deficit) BuildCostBlock(Transform parent, TMP_FontAsset font)
        {
            var go = new GameObject("CostBlock");
            GameObjectUtility.SetParentAndAlign(go, parent.gameObject);
            go.AddComponent<RectTransform>();

            VerticalLayoutGroup vlg = go.AddComponent<VerticalLayoutGroup>();
            vlg.childAlignment       = TextAnchor.MiddleLeft;
            vlg.childControlWidth    = true;
            vlg.childControlHeight   = true;
            vlg.childForceExpandWidth  = true;
            vlg.childForceExpandHeight = false;
            vlg.spacing = SubItemSpacing;

            LayoutElement le = go.AddComponent<LayoutElement>();
            le.flexibleWidth = 1f;

            TMP_Text cost = CreateLayoutLabel(go.transform, "CostLabel", "50",
                CostFontSize, GoldColor, font, FontStyles.Bold, TextAlignmentOptions.Left);

            TMP_Text costMult = CreateLayoutLabel(go.transform, "CostMultiplierLabel", "cost x1.5",
                CostMultFontSize, DimColor, font, FontStyles.Normal, TextAlignmentOptions.Left);

            TMP_Text deficit = CreateLayoutLabel(go.transform, "DeficitLabel", "",
                DeficitFontSize, ErrorColor, font, FontStyles.Normal, TextAlignmentOptions.Left);
            deficit.gameObject.SetActive(false);

            return (cost, costMult, deficit);
        }

        private static (Button btn, TMP_Text label, Image btnImage) BuildUpgradeButton(Transform parent, TMP_FontAsset font)
        {
            var go = new GameObject("UpgradeButton");
            GameObjectUtility.SetParentAndAlign(go, parent.gameObject);
            go.AddComponent<RectTransform>();

            Image btnImage = go.AddComponent<Image>();
            btnImage.color = GoldColor;

            Button btn = go.AddComponent<Button>();

            LayoutElement le = go.AddComponent<LayoutElement>();
            le.minWidth      = UpgradeBtnMinWidth;
            le.preferredWidth= UpgradeBtnMinWidth;
            le.flexibleWidth = 0f;

            var labelGo = new GameObject("UpgradeButtonLabel");
            GameObjectUtility.SetParentAndAlign(labelGo, go);
            EditorUIFactory.Stretch(labelGo.AddComponent<RectTransform>());

            TMP_Text labelTmp = labelGo.AddComponent<TextMeshProUGUI>();
            labelTmp.text      = "UNLOCK";
            labelTmp.fontSize  = UpgradeBtnFontSize;
            labelTmp.color     = ButtonLabelColor;
            labelTmp.fontStyle = FontStyles.Bold;
            labelTmp.alignment = TextAlignmentOptions.Center;
            EditorUIFactory.ApplyFont(labelTmp, font);

            return (btn, labelTmp, btnImage);
        }

        private static TMP_Text CreateLayoutLabel(Transform parent, string name, string text,
            int fontSize, Color color, TMP_FontAsset font,
            FontStyles style, TextAlignmentOptions alignment)
        {
            var go = new GameObject(name);
            GameObjectUtility.SetParentAndAlign(go, parent.gameObject);
            go.AddComponent<RectTransform>();
            go.AddComponent<LayoutElement>();

            TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text      = text;
            tmp.fontSize  = fontSize;
            tmp.color     = color;
            tmp.fontStyle = style;
            tmp.alignment = alignment;
            EditorUIFactory.ApplyFont(tmp, font);
            return tmp;
        }

        private static void SetFlexibleWidth(Transform parent, string childName, float value)
        {
            Transform child = parent.Find(childName);
            if (child == null) return;
            LayoutElement le = child.GetComponent<LayoutElement>();
            if (le == null) le = child.gameObject.AddComponent<LayoutElement>();
            le.flexibleWidth = value;
        }

        private static SkillTreeProgress EnsureSkillTreeProgressAsset()
        {
            var existing = AssetDatabase.LoadAssetAtPath<SkillTreeProgress>(SkillTreeProgress.DefaultAssetPath);
            if (existing != null) return existing;

            EditorUIFactory.EnsureDirectoryExists(SkillTreeProgress.DefaultAssetPath);
            var asset = ScriptableObject.CreateInstance<SkillTreeProgress>();
            AssetDatabase.CreateAsset(asset, SkillTreeProgress.DefaultAssetPath);
            AssetDatabase.SaveAssets();
            return AssetDatabase.LoadAssetAtPath<SkillTreeProgress>(SkillTreeProgress.DefaultAssetPath);
        }

        private static GoldWallet FindOrCreateGoldWallet()
        {
            GoldWallet found = Object.FindFirstObjectByType<GoldWallet>(FindObjectsInactive.Include);
            if (found != null) return found;

            var walletGo = new GameObject("GoldWallet");
            return walletGo.AddComponent<GoldWallet>();
        }

        private static SkillPointWallet FindOrCreateSkillPointWallet()
        {
            SkillPointWallet found = Object.FindFirstObjectByType<SkillPointWallet>(FindObjectsInactive.Include);
            if (found != null) return found;

            var walletGo = new GameObject("SkillPointWallet");
            return walletGo.AddComponent<SkillPointWallet>();
        }

        internal static Sprite EnsureCircleSprite()
        {
            var existing = AssetDatabase.LoadAssetAtPath<Sprite>(CircleSpritePath);
            if (existing != null) return existing;

            const int size = 128;
            const float center = size * 0.5f;
            const float radius = center - 1f;

            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color[size * size];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(center, center));
                    float alpha = Mathf.Clamp01(radius - dist + 0.5f);
                    pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();

            EditorUIFactory.EnsureDirectoryExists(CircleSpritePath);

            byte[] pngData = tex.EncodeToPNG();
            Object.DestroyImmediate(tex);
            System.IO.File.WriteAllBytes(CircleSpritePath, pngData);
            AssetDatabase.ImportAsset(CircleSpritePath);

            var importer = (TextureImporter)AssetImporter.GetAtPath(CircleSpritePath);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.isReadable = false;
            importer.SaveAndReimport();

            return AssetDatabase.LoadAssetAtPath<Sprite>(CircleSpritePath);
        }
    }
}
