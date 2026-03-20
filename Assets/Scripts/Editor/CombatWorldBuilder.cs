using UnityEditor;
using UnityEngine;

namespace RogueliteAutoBattler.Editor
{
    /// <summary>
    /// Builds the CombatWorld 2D scene hierarchy and configures the main camera.
    /// </summary>
    internal static class CombatWorldBuilder
    {
        private const float CameraOrthoSize = 5.4f;
        private const float CameraZPosition = -10f;
        private const int BattlefieldSortingOrder = 0;
        private static readonly Color BattlefieldBg = (Color)new Color32(106, 45, 47, 255);

        /// <summary>
        /// Configures the main camera as orthographic with the correct settings for 2D mobile.
        /// Creates one if none exists.
        /// </summary>
        internal static Camera ConfigureMainCamera()
        {
            Camera cam = Camera.main;
            if (cam == null)
                cam = Object.FindFirstObjectByType<Camera>(FindObjectsInactive.Include);

            if (cam == null)
            {
                var camGo = new GameObject("Main Camera");
                cam = camGo.AddComponent<Camera>();
                camGo.tag = "MainCamera";
                Undo.RegisterCreatedObjectUndo(camGo, "Main Camera");
            }
            else
            {
                Undo.RecordObject(cam, "Configure Camera");
                Undo.RecordObject(cam.transform, "Configure Camera");
            }

            cam.orthographic = true;
            cam.orthographicSize = CameraOrthoSize;
            cam.transform.position = new Vector3(0, 0, CameraZPosition);
            cam.backgroundColor = Color.black;
            cam.clearFlags = CameraClearFlags.SolidColor;

            return cam;
        }

        /// <summary>
        /// Creates a CombatWorld container with a background and containers for characters/effects.
        /// Place character prefabs as children of Characters/.
        /// </summary>
        internal static GameObject CreateCombatWorld()
        {
            var root = new GameObject("CombatWorld");
            root.transform.position = Vector3.zero;

            // Background — red sprite covering the visible camera area
            var bgGo = new GameObject("Background");
            bgGo.transform.SetParent(root.transform, false);
            SpriteRenderer bgRenderer = bgGo.AddComponent<SpriteRenderer>();
            bgRenderer.sprite = CreateOrLoadPlaceholderSprite();
            bgRenderer.color = BattlefieldBg;
            bgRenderer.sortingOrder = BattlefieldSortingOrder;
            // Camera ortho size 5.4 → visible height ~10.8, width ~6 (9:16). Scale to cover.
            bgGo.transform.localScale = new Vector3(12f, 12f, 1f);

            var charsGo = new GameObject("Characters");
            charsGo.transform.SetParent(root.transform, false);

            var fxGo = new GameObject("Effects");
            fxGo.transform.SetParent(root.transform, false);

            return root;
        }

        /// <summary>
        /// Creates a 4x4 white texture saved as asset, or loads it if it already exists.
        /// Used as a placeholder sprite for the battlefield background.
        /// </summary>
        internal static Sprite CreateOrLoadPlaceholderSprite()
        {
            const string path = "Assets/Sprites/Environment/placeholder_white.png";

            Sprite existing = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (existing != null)
                return existing;

            string directory = System.IO.Path.GetDirectoryName(path);
            if (!System.IO.Directory.Exists(directory))
                System.IO.Directory.CreateDirectory(directory);

            var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
            var pixels = new Color[16];
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = Color.white;
            tex.SetPixels(pixels);
            tex.Apply();

            System.IO.File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);

            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spritePixelsPerUnit = 4;
                importer.filterMode = FilterMode.Point;
                importer.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
    }
}
