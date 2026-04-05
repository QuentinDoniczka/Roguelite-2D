using RogueliteAutoBattler.Data;
using RogueliteAutoBattler.UI.Screens.SkillTree;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace RogueliteAutoBattler.Editor
{
    internal static class SkillTreeBuilder
    {
        internal static void BuildSkillTreeContent(GameObject skillTreePanel)
        {
            Transform existingLabel = skillTreePanel.transform.Find("Label");
            if (existingLabel != null)
                Object.DestroyImmediate(existingLabel.gameObject);

            var viewportGo = new GameObject("SkillTreeViewport");
            GameObjectUtility.SetParentAndAlign(viewportGo, skillTreePanel);
            EditorUIFactory.Stretch(viewportGo.AddComponent<RectTransform>());

            Image viewportImage = viewportGo.AddComponent<Image>();
            viewportImage.color = new Color(0, 0, 0, 0);
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
                EditorUIFactory.SetObj(nodeManagerSO, "_data", skillTreeData);
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
    }
}
