using System.Collections.Generic;
using NUnit.Framework;
using RogueliteAutoBattler.Data;
using UnityEditor;
using UnityEngine;

namespace RogueliteAutoBattler.Tests.EditMode
{
    [TestFixture]
    public class LevelDesignerTabPropertiesTests
    {
        private LevelDatabase _db;

        [TearDown]
        public void TearDown()
        {
            if (_db != null)
                Object.DestroyImmediate(_db);
            _db = null;
        }

        [Test]
        public void LevelDatabase_HasSerializedDefaultBackgroundProperty()
        {
            _db = ScriptableObject.CreateInstance<LevelDatabase>();
            var so = new SerializedObject(_db);

            Assert.IsNotNull(so.FindProperty("defaultBackground"));
        }

        [Test]
        public void LevelData_HasSerializedBackgroundProperty()
        {
            _db = ScriptableObject.CreateInstance<LevelDatabase>();
            _db.Stages = new List<StageData>
            {
                new StageData("Stage 1", new List<LevelData>
                {
                    new LevelData("Level 1", new List<StepData>())
                })
            };

            var so = new SerializedObject(_db);
            var levelProp = so.FindProperty("stages.Array.data[0].levels.Array.data[0]");

            Assert.IsNotNull(levelProp, "Could not find SerializedProperty for level[0]");
            Assert.IsNotNull(levelProp.FindPropertyRelative("background"));
        }

        [Test]
        public void LevelData_HasSerializedFitProperty()
        {
            _db = ScriptableObject.CreateInstance<LevelDatabase>();
            _db.Stages = new List<StageData>
            {
                new StageData("Stage 1", new List<LevelData>
                {
                    new LevelData("Level 1", new List<StepData>())
                })
            };

            var so = new SerializedObject(_db);
            var levelProp = so.FindProperty("stages.Array.data[0].levels.Array.data[0]");

            Assert.IsNotNull(levelProp, "Could not find SerializedProperty for level[0]");
            var fitProp = levelProp.FindPropertyRelative("fit");
            Assert.IsNotNull(fitProp);
            Assert.AreEqual(SerializedPropertyType.Enum, fitProp.propertyType);
        }

        [Test]
        public void StageData_NoLongerHasSerializedTerrainProperty()
        {
            _db = ScriptableObject.CreateInstance<LevelDatabase>();
            _db.Stages = new List<StageData>
            {
                new StageData("Stage 1", new List<LevelData>())
            };

            var so = new SerializedObject(_db);
            var stageProp = so.FindProperty("stages.Array.data[0]");

            Assert.IsNotNull(stageProp, "Could not find SerializedProperty for stage[0]");
            Assert.IsNull(stageProp.FindPropertyRelative("terrain"),
                "StageData must not have a 'terrain' serialized field (removed in ST8)");
        }
    }
}
