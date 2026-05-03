using System.Collections.Generic;
using NUnit.Framework;
using RogueliteAutoBattler.Combat.Core;
using RogueliteAutoBattler.Data;
using RogueliteAutoBattler.Editor.Tools;
using UnityEngine;

namespace RogueliteAutoBattler.Tests.EditMode
{
    public class MirrorPartnerFinderTests
    {
        private const float Tolerance = 1e-4f;

        private static SkillTreeData.SkillNodeEntry MakeNode(int id, Vector2 position)
        {
            return new SkillTreeData.SkillNodeEntry
            {
                id = id,
                position = position,
                connectedNodeIds = new List<int>(),
                costType = SkillTreeData.CostType.Gold,
                maxLevel = 1,
                baseCost = 1,
                costMultiplierOdd = 1f,
                costMultiplierEven = 1f,
                costAdditivePerLevel = 0,
                statModifierType = StatType.Hp,
                statModifierMode = SkillTreeData.StatModifierMode.Flat,
                statModifierValuePerLevel = 5f
            };
        }

        [Test]
        public void FindPartnerIndex_NoMirrorPair_ReturnsMinusOne()
        {
            // t=90: R(3,0) = (-3,0) — no node exists at (-3,0)
            var nodes = new List<SkillTreeData.SkillNodeEntry>
            {
                MakeNode(0, new Vector2(0f, 0f)),
                MakeNode(1, new Vector2(3f, 0f))
            };

            int result = MirrorPartnerFinder.FindPartnerIndex(nodes, 1, 90f, 0.5f);

            Assert.That(result, Is.EqualTo(-1));
        }

        [Test]
        public void FindPartnerIndex_PerfectReflection_ReturnsPartnerIndex()
        {
            // t=90: R(x,y) = (-x, y)  →  R(3,0) = (-3,0)
            var nodes = new List<SkillTreeData.SkillNodeEntry>
            {
                MakeNode(0, new Vector2(3f, 0f)),
                MakeNode(1, new Vector2(-3f, 0f))
            };

            int result = MirrorPartnerFinder.FindPartnerIndex(nodes, 0, 90f, 0.5f);

            Assert.That(result, Is.EqualTo(1));
        }

        [Test]
        public void FindPartnerIndex_WithinTolerance_ReturnsPartnerIndex()
        {
            // t=90: R(3,0) = (-3,0); partner at (-3.3,0) → distance 0.3 < tolerance 0.5
            var nodes = new List<SkillTreeData.SkillNodeEntry>
            {
                MakeNode(0, new Vector2(3f, 0f)),
                MakeNode(1, new Vector2(-3.3f, 0f))
            };

            int result = MirrorPartnerFinder.FindPartnerIndex(nodes, 0, 90f, 0.5f);

            Assert.That(result, Is.EqualTo(1));
        }

        [Test]
        public void FindPartnerIndex_OutsideTolerance_ReturnsMinusOne()
        {
            // t=90: R(3,0) = (-3,0); partner at (-3.6,0) → distance 0.6 > tolerance 0.5
            var nodes = new List<SkillTreeData.SkillNodeEntry>
            {
                MakeNode(0, new Vector2(3f, 0f)),
                MakeNode(1, new Vector2(-3.6f, 0f))
            };

            int result = MirrorPartnerFinder.FindPartnerIndex(nodes, 0, 90f, 0.5f);

            Assert.That(result, Is.EqualTo(-1));
        }

        [Test]
        public void FindPartnerIndex_VerticalAxis90Deg_DetectsHorizontallyMirroredPair()
        {
            // t=90: cos(180)=-1, sin(180)=0 → R(x,y)=(-x, y)
            // R(2,3) = (-2,3)
            var nodes = new List<SkillTreeData.SkillNodeEntry>
            {
                MakeNode(0, new Vector2(2f, 3f)),
                MakeNode(1, new Vector2(-2f, 3f))
            };

            int result = MirrorPartnerFinder.FindPartnerIndex(nodes, 0, 90f, 0.5f);

            Assert.That(result, Is.EqualTo(1));
        }

        [Test]
        public void FindPartnerIndex_HorizontalAxis0Deg_DetectsVerticallyMirroredPair()
        {
            // t=0: cos(0)=1, sin(0)=0 → R(x,y)=(x, -y)
            // R(2,3) = (2,-3)
            var nodes = new List<SkillTreeData.SkillNodeEntry>
            {
                MakeNode(0, new Vector2(2f, 3f)),
                MakeNode(1, new Vector2(2f, -3f))
            };

            int result = MirrorPartnerFinder.FindPartnerIndex(nodes, 0, 0f, 0.5f);

            Assert.That(result, Is.EqualTo(1));
        }

        [Test]
        public void FindPartnerIndex_ExcludesSourceIndex()
        {
            var nodes = new List<SkillTreeData.SkillNodeEntry>
            {
                MakeNode(0, new Vector2(0f, 0f))
            };

            int result = MirrorPartnerFinder.FindPartnerIndex(nodes, 0, 0f, 1f);

            Assert.That(result, Is.EqualTo(-1));
        }

        [Test]
        public void FindPartnerIndex_NullNodes_ReturnsMinusOne()
        {
            int result = MirrorPartnerFinder.FindPartnerIndex(null, 0, 0f, 0.5f);

            Assert.That(result, Is.EqualTo(-1));
        }

        [Test]
        public void FindPartnerIndex_OutOfRangeIndex_ReturnsMinusOne()
        {
            var nodes = new List<SkillTreeData.SkillNodeEntry>
            {
                MakeNode(0, new Vector2(1f, 0f))
            };

            int result = MirrorPartnerFinder.FindPartnerIndex(nodes, 5, 0f, 0.5f);

            Assert.That(result, Is.EqualTo(-1));
        }
    }
}
