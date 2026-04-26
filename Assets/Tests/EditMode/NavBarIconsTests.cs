using NUnit.Framework;
using RogueliteAutoBattler.Editor.Tools;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RogueliteAutoBattler.Tests.EditMode
{
    [TestFixture]
    public class NavBarIconsTests
    {
        private const string MainLayoutAssetPath = "Assets/UI/Layouts/MainLayout.uxml";

        private static string TabButtonName(string slug) => $"tab-{slug}";

        private static string TabIconModifierClass(string slug) => $"tab-icon--{slug}";

        [Test]
        public void NavIcons_AllSpritesExist_AsSpriteType()
        {
            foreach (string slug in NavIconsImporter.NavIconSlugs)
            {
                string path = NavIconsImporter.GetNavIconAssetPath(slug);

                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                Assert.IsNotNull(sprite, $"Sprite not found at '{path}'. Ensure the PNG is imported as Sprite.");

                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                Assert.IsNotNull(importer, $"TextureImporter not found for '{path}'.");
                Assert.AreEqual(TextureImporterType.Sprite, importer.textureType,
                    $"Texture at '{path}' must have textureType == Sprite.");
                Assert.IsTrue(importer.alphaIsTransparency,
                    $"Texture at '{path}' must have alphaIsTransparency == true.");
            }
        }

        [Test]
        public void NavBarUxml_EachTabButtonHasIconChild()
        {
            VisualElement root = LoadMainLayoutRoot();

            foreach (string slug in NavIconsImporter.NavIconSlugs)
            {
                string buttonName = TabButtonName(slug);
                var button = root.Q<Button>(buttonName);
                Assert.IsNotNull(button, $"Button '{buttonName}' not found in '{MainLayoutAssetPath}'.");

                var iconChild = button.Q<VisualElement>(className: "tab-icon");
                Assert.IsNotNull(iconChild,
                    $"Button '{buttonName}' has no descendant with class 'tab-icon'.");

                string modifierClass = TabIconModifierClass(slug);
                Assert.IsTrue(iconChild.ClassListContains(modifierClass),
                    $"Icon child of '{buttonName}' is missing modifier class '{modifierClass}'.");

                Assert.AreEqual(PickingMode.Ignore, iconChild.pickingMode,
                    $"Icon child of '{buttonName}' must have pickingMode == Ignore.");
            }
        }

        [Test]
        public void NavBarUxml_TabButtons_HaveNoTextLabel()
        {
            VisualElement root = LoadMainLayoutRoot();

            foreach (string slug in NavIconsImporter.NavIconSlugs)
            {
                string buttonName = TabButtonName(slug);
                var button = root.Q<Button>(buttonName);
                Assert.IsNotNull(button, $"Button '{buttonName}' not found in '{MainLayoutAssetPath}'.");

                Assert.IsTrue(string.IsNullOrEmpty(button.text),
                    $"Button '{buttonName}' must not display text (got '{button.text}').");
            }
        }

        private static VisualElement LoadMainLayoutRoot()
        {
            var tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(MainLayoutAssetPath);
            Assert.IsNotNull(tree, $"VisualTreeAsset not found at '{MainLayoutAssetPath}'.");

            VisualElement root = tree.Instantiate();
            Assert.IsNotNull(root, "tree.Instantiate() returned null.");
            return root;
        }
    }
}
