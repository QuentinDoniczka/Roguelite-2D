using NUnit.Framework;
using RogueliteAutoBattler.Editor.Tools;
using UnityEditor;

namespace RogueliteAutoBattler.Tests.EditMode
{
    public class MirrorAxisPersistenceTests
    {
        private const float Tolerance = 1e-4f;

        [SetUp]
        public void SetUp()
        {
            EditorPrefs.DeleteKey(MirrorAxisPersistence.EditorPrefKey);
        }

        [TearDown]
        public void TearDown()
        {
            EditorPrefs.DeleteKey(MirrorAxisPersistence.EditorPrefKey);
        }

        [Test]
        public void LoadMirrorAxisDegrees_KeyAbsent_ReturnsZero()
        {
            float result = MirrorAxisPersistence.Load();

            Assert.That(result, Is.EqualTo(0f).Within(Tolerance));
        }

        [Test]
        public void SaveThenLoad_RoundTripsValue()
        {
            MirrorAxisPersistence.Save(45f);

            float result = MirrorAxisPersistence.Load();

            Assert.That(result, Is.EqualTo(45f).Within(Tolerance));
        }

        [Test]
        public void SaveOverwrites_PreviousValue()
        {
            MirrorAxisPersistence.Save(45f);
            MirrorAxisPersistence.Save(90f);

            float result = MirrorAxisPersistence.Load();

            Assert.That(result, Is.EqualTo(90f).Within(Tolerance));
        }

        [Test]
        public void SaveLoad_NegativeValue_PreservedRaw()
        {
            MirrorAxisPersistence.Save(-15f);

            float result = MirrorAxisPersistence.Load();

            Assert.That(result, Is.EqualTo(-15f).Within(Tolerance));
        }
    }
}
