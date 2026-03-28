using NUnit.Framework;
using RogueliteAutoBattler.Combat;

namespace RogueliteAutoBattler.Tests.EditMode
{
    [TestFixture]
    public class GoldFormatterTests
    {
        [TestCase(0, "0")]
        [TestCase(1, "1")]
        [TestCase(999, "999")]
        [TestCase(1000, "1K")]
        [TestCase(1500, "1.5K")]
        [TestCase(9999, "10K")]
        [TestCase(10000, "10K")]
        [TestCase(15000, "15K")]
        [TestCase(999999, "999K")]
        [TestCase(1000000, "1M")]
        [TestCase(1500000, "1.5M")]
        [TestCase(9999999, "10M")]
        [TestCase(10000000, "10M")]
        [TestCase(1000000000, "1B")]
        public void Format_ReturnsExpectedString(int value, string expected)
        {
            Assert.AreEqual(expected, GoldFormatter.Format(value));
        }

        [Test]
        public void Format_BelowThousand_ReturnsExactNumber()
        {
            Assert.AreEqual("500", GoldFormatter.Format(500));
        }

        [Test]
        public void Format_ThousandsWithDecimal_ShowsOneDecimal()
        {
            Assert.AreEqual("2.5K", GoldFormatter.Format(2500));
        }

        [Test]
        public void Format_ThousandsWithoutDecimal_OmitsDecimal()
        {
            Assert.AreEqual("3K", GoldFormatter.Format(3000));
        }

        [Test]
        public void Format_TenThousandsAndAbove_UsesIntegerDivision()
        {
            Assert.AreEqual("99K", GoldFormatter.Format(99999));
        }

        [Test]
        public void Format_MillionsWithDecimal_ShowsOneDecimal()
        {
            Assert.AreEqual("5.5M", GoldFormatter.Format(5500000));
        }

        [Test]
        public void Format_TenMillionsAndAbove_UsesIntegerDivision()
        {
            Assert.AreEqual("500M", GoldFormatter.Format(500000000));
        }
    }
}
