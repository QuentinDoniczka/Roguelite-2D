using System;
using NUnit.Framework;
using RogueliteAutoBattler.Data;
using UnityEngine;

namespace RogueliteAutoBattler.Tests.EditMode
{
    public class SkillTreeProgressTests
    {
        private SkillTreeProgress _progress;

        [SetUp]
        public void SetUp()
        {
            _progress = ScriptableObject.CreateInstance<SkillTreeProgress>();
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(_progress);
        }

        [Test]
        public void GetLevel_DefaultIsZero()
        {
            Assert.AreEqual(0, _progress.GetLevel(0));
            Assert.AreEqual(0, _progress.GetLevel(5));
        }

        [Test]
        public void SetLevel_ThenGetLevel_ReturnsValue()
        {
            _progress.SetLevel(0, 3);
            Assert.AreEqual(3, _progress.GetLevel(0));
        }

        [Test]
        public void SetLevel_ExpandsList()
        {
            _progress.SetLevel(5, 2);
            Assert.AreEqual(2, _progress.GetLevel(5));
            Assert.AreEqual(0, _progress.GetLevel(3));
        }

        [Test]
        public void GetLevel_NegativeIndex_ReturnsZero()
        {
            Assert.AreEqual(0, _progress.GetLevel(-1));
        }

        [Test]
        public void ResetAll_ClearsAllLevels()
        {
            _progress.SetLevel(0, 5);
            _progress.SetLevel(2, 3);
            _progress.ResetAll();
            Assert.AreEqual(0, _progress.GetLevel(0));
            Assert.AreEqual(0, _progress.GetLevel(2));
        }

        [Test]
        public void SetLevel_FiresEvent_WithNodeIndexAndNewLevel()
        {
            var recorder = new EventRecorder();
            _progress.OnLevelChanged += recorder.Handler;

            _progress.SetLevel(2, 4);

            Assert.AreEqual(1, recorder.Count);
            Assert.AreEqual(2, recorder.LastNodeIndex);
            Assert.AreEqual(4, recorder.LastNewLevel);
        }

        [Test]
        public void SetLevel_SameValue_DoesNotFire()
        {
            _progress.SetLevel(0, 3);

            int invocationCount = 0;
            Action<int, int> handler = (nodeIndex, level) => invocationCount++;
            _progress.OnLevelChanged += handler;

            _progress.SetLevel(0, 3);

            Assert.AreEqual(0, invocationCount);
        }

        [Test]
        public void ResetAll_FiresOnce_WithSentinelMinusOne()
        {
            _progress.SetLevel(0, 5);
            _progress.SetLevel(2, 3);

            var recorder = new EventRecorder();
            _progress.OnLevelChanged += recorder.Handler;

            _progress.ResetAll();

            Assert.AreEqual(1, recorder.Count);
            Assert.AreEqual(-1, recorder.LastNodeIndex);
            Assert.AreEqual(0, recorder.LastNewLevel);
        }

        [Test]
        public void ResetAll_WhenAlreadyEmpty_StillFires()
        {
            var recorder = new EventRecorder();
            _progress.OnLevelChanged += recorder.Handler;

            _progress.ResetAll();

            Assert.AreEqual(1, recorder.Count);
            Assert.AreEqual(-1, recorder.LastNodeIndex);
            Assert.AreEqual(0, recorder.LastNewLevel);
        }

        [Test]
        public void OnEnable_DoesNotFire()
        {
            var freshProgress = ScriptableObject.CreateInstance<SkillTreeProgress>();
            try
            {
                int invocationCount = 0;
                Action<int, int> handler = (nodeIndex, level) => invocationCount++;
                freshProgress.OnLevelChanged += handler;

                Assert.AreEqual(0, invocationCount);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(freshProgress);
            }
        }

        private sealed class EventRecorder
        {
            public int Count;
            public int LastNodeIndex = int.MinValue;
            public int LastNewLevel = int.MinValue;

            public Action<int, int> Handler => (nodeIndex, level) =>
            {
                Count++;
                LastNodeIndex = nodeIndex;
                LastNewLevel = level;
            };
        }
    }
}
