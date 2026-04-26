using NUnit.Framework;
using RogueliteAutoBattler.Data;

namespace RogueliteAutoBattler.Tests.EditMode
{
    public class SkillTreeDesignerViewportFitTests
    {
        [Test]
        public void DefaultRingRadiusAndUnitSize_FitWithinMinimumWindowViewport()
        {
            const float MinWindowWidth = 800f;
            const float ConfigPanelWidthRatio = 0.3f;
            const float MarginFactor = 0.9f;

            float canvasViewportWidth = MinWindowWidth * (1f - ConfigPanelWidthRatio);
            float ringDiameterPx = SkillTreeData.DefaultRingRadius * SkillTreeData.DefaultUnitSize * 2f;

            Assert.Less(ringDiameterPx, canvasViewportWidth * MarginFactor,
                "Default ring diameter must fit inside the minimum designer viewport with 10% margin.");
        }
    }
}
