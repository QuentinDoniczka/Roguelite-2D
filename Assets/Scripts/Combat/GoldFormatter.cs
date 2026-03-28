using System.Globalization;

namespace RogueliteAutoBattler.Combat
{
    public static class GoldFormatter
    {
        private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

        public static string Format(int value)
        {
            if (value < 1000) return value.ToString();
            if (value < 10000) return (value / 1000f).ToString("0.#", Inv) + "K";
            if (value < 1000000) return (value / 1000).ToString() + "K";
            if (value < 10000000) return (value / 1000000f).ToString("0.#", Inv) + "M";
            if (value < 1000000000) return (value / 1000000).ToString() + "M";
            return (value / 1000000000f).ToString("0.#", Inv) + "B";
        }
    }
}
