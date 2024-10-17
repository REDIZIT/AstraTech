using UnityEngine;

namespace AstraTech
{
    public static class ToStringUtils
    {
        public enum RoundType
        {
            Round,
            Floor,
            Ceil
        }

        public static string ToPrettyString(this float value, RoundType type = RoundType.Round)
        {
            if (value >= 10)
            {
                if (type == RoundType.Round) value = Mathf.Ceil(value * 10) / 10f;
                else if (type == RoundType.Floor) value = Mathf.Floor(value * 10) / 10f;
                else value = Mathf.Ceil(value * 10) / 10f;

                return value.ToString("F0");
            }
            else
            {
                return value.ToString("0.0");
            }
        }
    }
}
