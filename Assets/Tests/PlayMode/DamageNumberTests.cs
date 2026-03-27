using System.Collections;
using NUnit.Framework;
using RogueliteAutoBattler.Combat;
using RogueliteAutoBattler.Data;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;

namespace RogueliteAutoBattler.Tests.PlayMode
{
    public class DamageNumberTests : PlayModeTestBase
    {
        private GameObject _container;
        private GameObject _damageNumberGo;
        private DamageNumber _damageNumber;
        private TextMeshPro _tmp;
        private DamageNumberConfig _config;

        [SetUp]
        public void SetUp()
        {
            _container = Track(new GameObject("DamageNumberTestContainer"));

            _damageNumberGo = new GameObject("TestDamageNumber");
            _damageNumberGo.transform.SetParent(_container.transform, false);

            _tmp = _damageNumberGo.AddComponent<TextMeshPro>();
            _damageNumber = _damageNumberGo.AddComponent<DamageNumber>();

            _config = ScriptableObject.CreateInstance<DamageNumberConfig>();

            _damageNumber.Initialize(_ => { });
        }

        [UnityTest]
        public IEnumerator Play_SetsTextToValue()
        {
            yield return null;

            _damageNumber.Play(Vector3.zero, 42, Color.white, _config);
            yield return null;

            Assert.AreEqual("42", _tmp.text);
        }

        [UnityTest]
        public IEnumerator Play_SetsColor()
        {
            yield return null;

            _damageNumber.Play(Vector3.zero, 10, Color.red, _config);
            yield return null;

            Assert.AreEqual(Color.red.r, _tmp.color.r, 0.01f);
            Assert.AreEqual(Color.red.g, _tmp.color.g, 0.01f);
            Assert.AreEqual(Color.red.b, _tmp.color.b, 0.01f);
        }

        [UnityTest]
        public IEnumerator Play_DeactivatesAfterLifetime()
        {
            yield return null;

            _damageNumber.Play(Vector3.zero, 10, Color.white, _config);
            yield return null;

            Assert.IsTrue(_damageNumberGo.activeSelf);

            yield return new WaitForSeconds(_config.Lifetime + 0.2f);

            Assert.IsFalse(_damageNumberGo.activeSelf);
        }

        [UnityTest]
        public IEnumerator Play_InvokesReturnCallback()
        {
            bool callbackInvoked = false;
            _damageNumber.Initialize(dn => callbackInvoked = true);

            yield return null;

            _damageNumber.Play(Vector3.zero, 10, Color.white, _config);

            yield return new WaitForSeconds(_config.Lifetime + 0.2f);

            Assert.IsTrue(callbackInvoked);
        }

        [UnityTest]
        public IEnumerator Play_MovesInSlideDirection()
        {
            yield return null;

            _damageNumber.Play(Vector3.zero, 10, Color.white, _config);
            yield return null;

            float startLocalY = _damageNumberGo.transform.localPosition.y;

            yield return new WaitForSeconds(_config.Lifetime * 0.5f);

            Assert.Greater(_damageNumberGo.transform.localPosition.y, startLocalY);
        }

        public override void TearDown()
        {
            if (_config != null)
                Object.Destroy(_config);

            base.TearDown();
        }
    }
}
