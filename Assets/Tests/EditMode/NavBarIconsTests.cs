using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RogueliteAutoBattler.Tests.EditMode
{
    [TestFixture]
    public class NavBarIconsTests
    {
        private const string MainLayoutAssetPath = "Assets/UI/Layouts/MainLayout.uxml";

        private static readonly string[] NavIconSlugs =
        {
            "village",
            "skilltree",
            "map",
            "guilde",
            "shop"
        };

        private static string IconAssetPath(string slug) => $"Assets/UI/Icons/Nav/{slug}.png";

        private static string TabButtonName(string slug) => $"tab-{slug}";

        [Test]
        public void NavIcons_AllFiveSpritesExist_AsSpriteType()
        {
            foreach (var slug in NavIconSlugs)
            {
                var path = IconAssetPath(slug);

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
            var tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(MainLayoutAssetPath);
            Assert.IsNotNull(tree, $"VisualTreeAsset not found at '{MainLayoutAssetPath}'.");

            var root = tree.Instantiate();
            Assert.IsNotNull(root, "tree.Instantiate() returned null.");

            foreach (var slug in NavIconSlugs)
            {
                var buttonName = TabButtonName(slug);
                var button = root.Q<Button>(buttonName);
                Assert.IsNotNull(button, $"Button '{buttonName}' not found in '{MainLayoutAssetPath}'.");

                var iconChild = button.Q<VisualElement>(className: "tab-icon");
                Assert.IsNotNull(iconChild,
                    $"Button '{buttonName}' has no descendant with class 'tab-icon'.");

                var modifierClass = $"tab-icon--{slug}";
                Assert.IsTrue(iconChild.ClassListContains(modifierClass),
                    $"Icon child of '{buttonName}' is missing modifier class '{modifierClass}'.");

                Assert.AreEqual(PickingMode.Ignore, iconChild.pickingMode,
                    $"Icon child of '{buttonName}' must have pickingMode == Ignore.");
            }
        }

        [Test]
        public void NavBarUxml_TabButtons_HaveNoTextLabel()
        {
            var tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(MainLayoutAssetPath);
            Assert.IsNotNull(tree, $"VisualTreeAsset not found at '{MainLayoutAssetPath}'.");

            var root = tree.Instantiate();
            Assert.IsNotNull(root, "tree.Instantiate() returned null.");

            foreach (var slug in NavIconSlugs)
            {
                var buttonName = TabButtonName(slug);
                var button = root.Q<Button>(buttonName);
                Assert.IsNotNull(button, $"Button '{buttonName}' not found in '{MainLayoutAssetPath}'.");

                Assert.IsTrue(string.IsNullOrEmpty(button.text),
                    $"Button '{buttonName}' must not display text (got '{button.text}').");
            }
        }
    }
}
