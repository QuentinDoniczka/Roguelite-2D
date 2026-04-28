using NUnit.Framework;
using RogueliteAutoBattler.Editor.Tools;
using UnityEditor;
using UnityEngine;

namespace RogueliteAutoBattler.Tests.EditMode
{
    [TestFixture]
    public class HudIconAssetTests
    {
        [Test]
        public void HudIcons_AllSpritesExist_AsSpriteType()
        {
            foreach (string slug in HudIconsImporter.HudIconSlugs)
            {
                string path = HudIconsImporter.GetHudIconAssetPath(slug);

                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                Assert.That(sprite, Is.Not.Null, $"sprite {slug}");

                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                Assert.That(importer, Is.Not.Null, $"TextureImporter not found for '{path}'.");
                Assert.That(importer.textureType, Is.EqualTo(TextureImporterType.Sprite),
                    $"Texture at '{path}' must have textureType == Sprite.");
                Assert.That(importer.spriteImportMode, Is.EqualTo(SpriteImportMode.Single),
                    $"Texture at '{path}' must have spriteImportMode == Single.");
                Assert.That(importer.alphaIsTransparency, Is.True,
                    $"Texture at '{path}' must have alphaIsTransparency == true.");
            }
        }

        [Test]
        public void HudIcons_OldSpritesRootPaths_AreEmpty()
        {
            foreach (string slug in HudIconsImporter.HudIconSlugs)
            {
                var texture = AssetDatabase.LoadAssetAtPath<Texture2D>($"Assets/Sprites/{slug}.png");
                Assert.That(texture, Is.Null, $"old root {slug} should have moved");
            }
        }
    }
}
