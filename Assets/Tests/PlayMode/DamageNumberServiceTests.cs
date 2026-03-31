using System.Collections;
using NUnit.Framework;
using RogueliteAutoBattler.Combat.Visuals;
using RogueliteAutoBattler.Data;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;

namespace RogueliteAutoBattler.Tests.PlayMode
{
    public class DamageNumberServiceTests : PlayModeTestBase
    {
        private GameObject _container;
        private DamageNumberConfig _config;

        [SetUp]
        public void SetUp()
        {
            DamageNumberService.ResetForTest();

            _container = Track(new GameObject("DamageNumberServiceTestContainer"));
            _config = ScriptableObject.CreateInstance<DamageNumberConfig>();

            DamageNumberService.Initialize(_container.transform, _config);
        }

        [UnityTest]
        public IEnumerator Initialize_CreatesPoolInstances()
        {
            yield return null;

            Assert.AreEqual(20, _container.transform.childCount);

            for (int i = 0; i < _container.transform.childCount; i++)
            {
                Assert.IsFalse(_container.transform.GetChild(i).gameObject.activeSelf);
            }
        }

        [UnityTest]
        public IEnumerator Show_ActivatesOneInstance()
        {
            yield return null;

            DamageNumberService.Show(Vector3.zero, 50, true);
            yield return null;

            int activeCount = CountActiveChildren(_container.transform);
            Assert.AreEqual(1, activeCount);
        }

        [UnityTest]
        public IEnumerator Show_SetsCorrectText()
        {
            yield return null;

            DamageNumberService.Show(Vector3.zero, 99, false);
            yield return null;

            TextMeshPro activeTmp = FindActiveTmp(_container.transform);
            Assert.IsNotNull(activeTmp);
            Assert.AreEqual("99", activeTmp.text);
        }

        [UnityTest]
        public IEnumerator Show_ReturnsToPoolAfterLifetime()
        {
            yield return null;

            DamageNumberService.Show(Vector3.zero, 10, true);
            yield return null;

            Assert.AreEqual(1, CountActiveChildren(_container.transform));

            yield return new WaitForSeconds(_config.Lifetime + 0.2f);

            Assert.AreEqual(0, CountActiveChildren(_container.transform));
        }

        [UnityTest]
        public IEnumerator Show_AutoExpandsPool()
        {
            yield return null;

            for (int i = 0; i < 25; i++)
            {
                DamageNumberService.Show(new Vector3(i * 0.1f, 0, 0), i, true);
            }

            yield return null;

            Assert.AreEqual(25, _container.transform.childCount);
        }

        [UnityTest]
        public IEnumerator Show_AllyUsesAllyColor()
        {
            yield return null;

            DamageNumberService.Show(Vector3.zero, 10, true);
            yield return null;

            TextMeshPro activeTmp = FindActiveTmp(_container.transform);
            Assert.IsNotNull(activeTmp);
            Assert.AreEqual(_config.AllyDamageColor.r, activeTmp.color.r, 0.01f);
            Assert.AreEqual(_config.AllyDamageColor.g, activeTmp.color.g, 0.01f);
            Assert.AreEqual(_config.AllyDamageColor.b, activeTmp.color.b, 0.01f);
        }

        [UnityTest]
        public IEnumerator Show_EnemyUsesEnemyColor()
        {
            yield return null;

            DamageNumberService.Show(Vector3.zero, 10, false);
            yield return null;

            TextMeshPro activeTmp = FindActiveTmp(_container.transform);
            Assert.IsNotNull(activeTmp);
            Assert.AreEqual(_config.EnemyDamageColor.r, activeTmp.color.r, 0.01f);
            Assert.AreEqual(_config.EnemyDamageColor.g, activeTmp.color.g, 0.01f);
            Assert.AreEqual(_config.EnemyDamageColor.b, activeTmp.color.b, 0.01f);
        }

        public override void TearDown()
        {
            DamageNumberService.ResetForTest();

            if (_config != null)
                Object.Destroy(_config);

            base.TearDown();
        }

        private static int CountActiveChildren(Transform parent)
        {
            int count = 0;
            for (int i = 0; i < parent.childCount; i++)
            {
                if (parent.GetChild(i).gameObject.activeSelf)
                    count++;
            }
            return count;
        }

        private static TextMeshPro FindActiveTmp(Transform parent)
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                var child = parent.GetChild(i);
                if (child.gameObject.activeSelf)
                    return child.GetComponent<TextMeshPro>();
            }
            return null;
        }
    }
}
