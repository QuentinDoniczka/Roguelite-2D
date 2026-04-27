using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using RogueliteAutoBattler.Data;

namespace RogueliteAutoBattler.Tests.EditMode
{
    [TestFixture]
    public class StageDataMigrationTests
    {
        [Test]
        public void StageData_HasNoTerrainProperty()
        {
            PropertyInfo terrainProperty = typeof(StageData).GetProperty(
                "Terrain",
                BindingFlags.Public | BindingFlags.Instance);

            Assert.IsNull(terrainProperty,
                "StageData.Terrain property must be removed; per-level Background replaces it.");
        }

        [Test]
        public void StageData_HasNoTerrainField()
        {
            FieldInfo terrainField = typeof(StageData).GetField(
                "terrain",
                BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.IsNull(terrainField,
                "StageData.terrain field must be removed; per-level Background replaces it.");
        }

        [Test]
        public void StageData_ConstructorTakesNameAndLevelsOnly()
        {
            ConstructorInfo[] constructors = typeof(StageData).GetConstructors(
                BindingFlags.Instance | BindingFlags.NonPublic);

            ConstructorInfo twoParamConstructor = constructors
                .FirstOrDefault(c => c.GetParameters().Length == 2);

            Assert.IsNotNull(twoParamConstructor,
                "StageData must expose a non-public instance constructor with exactly 2 parameters.");

            ParameterInfo[] parameters = twoParamConstructor.GetParameters();
            Assert.AreEqual(typeof(string), parameters[0].ParameterType,
                "First constructor parameter must be string (stageName).");
            Assert.AreEqual(typeof(List<LevelData>), parameters[1].ParameterType,
                "Second constructor parameter must be List<LevelData> (levels).");
        }
    }
}
