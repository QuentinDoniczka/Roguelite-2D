using RogueliteAutoBattler.Data;
using RogueliteAutoBattler.UI.Screens.SkillTree;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace RogueliteAutoBattler.Editor
{
    internal static class SkillTreeBuilder
    {
        internal const string CircleSpritePath = "Assets/Sprites/UI/circle_white.png";

        internal static void BuildSkillTreeContent(GameObject skillTreePanel)
        {
            Transform existingLabel = skillTreePanel.transform.Find("Label");
            if (existingLabel != null)
                Object.DestroyImmediate(existingLabel.gameObject);

            var viewportGo = new GameObject("SkillTreeViewport");
            GameObjectUtility.SetParentAndAlign(viewportGo, skillTreePanel);
            EditorUIFactory.Stretch(viewportGo.AddComponent<RectTransform>());

            Image viewportImage = viewportGo.AddComponent<Image>();
            viewportImage.color = Color.clear;
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
