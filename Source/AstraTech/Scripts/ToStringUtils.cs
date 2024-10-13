using UnityEngine;

namespace AstraTech
{
    public static class ToStringUtils
    {
        public static string ToPrettyString(this float value)
        {
            if (value >= 10)
            {
                return (Mathf.Ceil(value * 10) / 10f).ToString("F0");
            }
            else
            {
                return value.ToString("0.0");
            }
        }
    }
}
