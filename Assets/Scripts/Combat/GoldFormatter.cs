using System.Globalization;

namespace RogueliteAutoBattler.Combat
{
    public static class GoldFormatter
    {
        private const int ThousandThreshold = 1_000;
        private const int TenThousandThreshold = 10_000;
        private const int MillionThreshold = 1_000_000;
        private const int TenMillionThreshold = 10_000_000;
        private const int BillionThreshold = 1_000_000_000;

        private const string OneDecimalFormat = "0.#";

        private static readonly CultureInfo InvariantCulture = CultureInfo.InvariantCulture;

        public static string Format(int value)
        {
            if (value < ThousandThreshold) return value.ToString();
            if (value < TenThousandThreshold) return (value / 1000f).ToString(OneDecimalFormat, InvariantCulture) + "K";
            if (value < MillionThreshold) return (value / 1000).ToString() + "K";
            if (value < TenMillionThreshold) return (value / 1000000f).ToString(OneDecimalFormat, InvariantCulture) + "M";
            if (value < BillionThreshold) return (value / 1000000).ToString() + "M";
            return (value / 1000000000f).ToString(OneDecimalFormat, InvariantCulture) + "B";
        }
    }
}
