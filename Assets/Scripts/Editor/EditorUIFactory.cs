using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace RogueliteAutoBattler.Editor
{
    internal static class EditorUIFactory
    {
        internal static void Stretch(RectTransform r)
        {
            r.anchorMin = Vector2.zero;
            r.anchorMax = Vector2.one;
            r.offsetMin = Vector2.zero;
            r.offsetMax = Vector2.zero;
        }

        internal static GameObject CreateArea(Transform parent, string name, float anchorBottom, float anchorTop, Color bg)
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

        internal static TextMeshProUGUI CreateLabel(Transform parent, string name, string text, int size, Color color)
        {
            var go = new GameObject(name);
            GameObjectUtility.SetParentAndAlign(go, parent.gameObject);
            Stretch(go.AddComponent<RectTransform>());
            return ConfigureText(go, text, size, color);
        }

        internal static TextMeshProUGUI ConfigureText(GameObject go, string text, int size, Color color)
        {
            TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Center;
            return tmp;
        }

        internal static CanvasGroup SetupCanvasGroup(GameObject go, bool visible)
        {
            CanvasGroup cg = go.AddComponent<CanvasGroup>();
            cg.alpha = visible ? 1f : 0f;
            cg.blocksRaycasts = visible;
            cg.interactable = visible;
            return cg;
        }

        internal static Color HexToColor(string hex)
        {
            if (ColorUtility.TryParseHtmlString("#" + hex, out Color c)) return c;
            return Color.magenta;
        }

        internal static SerializedProperty FindProp(SerializedObject so, string name)
        {
            SerializedProperty p = so.FindProperty(name);
            if (p == null) Debug.LogError($"[{nameof(EditorUIFactory)}] Property '{name}' not found on {so.targetObject.GetType().Name}.");
            return p;
        }

        internal static void SetInt(SerializedObject so, string name, int v)
        {
            SerializedProperty p = FindProp(so, name);
            if (p != null) p.intValue = v;
        }

        internal static void SetObj(SerializedObject so, string name, Object v)
        {
            SerializedProperty p = FindProp(so, name);
            if (p != null) p.objectReferenceValue = v;
        }

        internal static void SetColor(SerializedObject so, string name, Color v)
        {
            SerializedProperty p = FindProp(so, name);
            if (p != null) p.colorValue = v;
        }

        internal static void EnsureDirectoryExists(string assetPath)
        {
            string dir = System.IO.Path.GetDirectoryName(assetPath);
            if (!string.IsNullOrEmpty(dir) && !System.IO.Directory.Exists(dir))
                System.IO.Directory.CreateDirectory(dir);
        }

        internal static void WireArray(SerializedObject so, string name, Component[] items, int count)
        {
            SerializedProperty prop = FindProp(so, name);
            if (prop == null) return;
            prop.arraySize = count;
            for (int i = 0; i < count; i++)
                prop.GetArrayElementAtIndex(i).objectReferenceValue = items[i];
        }

        private static readonly GUIContent AppearanceLabelHead   = new GUIContent("Head");
        private static readonly GUIContent AppearanceLabelHat    = new GUIContent("Hat / Armor");
        private static readonly GUIContent AppearanceLabelWeapon = new GUIContent("Weapon");
        private static readonly GUIContent AppearanceLabelShield = new GUIContent("Shield");

        internal static void DrawAppearanceFields(SerializedProperty parentProp)
        {
            var appearanceProp = parentProp.FindPropertyRelative("appearance");
            if (appearanceProp == null)
            {
                Debug.LogError("[GameDesigner] Property 'appearance' not found.");
                return;
            }

            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("Appearance", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(
                appearanceProp.FindPropertyRelative("headSprite"), AppearanceLabelHead);
            EditorGUILayout.PropertyField(
                appearanceProp.FindPropertyRelative("hatSprite"), AppearanceLabelHat);
            EditorGUILayout.PropertyField(
                appearanceProp.FindPropertyRelative("weaponSprite"), AppearanceLabelWeapon);
            EditorGUILayout.PropertyField(
                appearanceProp.FindPropertyRelative("shieldSprite"), AppearanceLabelShield);
        }
    }
}
