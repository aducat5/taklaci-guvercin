using System;
using UnityEngine;
using TaklaciGuvercin.Shared.Enums;

namespace TaklaciGuvercin.Utils
{
    /// <summary>
    /// Extension methods for common operations.
    /// </summary>
    public static class Extensions
    {
        #region String Extensions

        public static bool IsNullOrEmpty(this string value)
        {
            return string.IsNullOrEmpty(value);
        }

        public static bool IsNotNullOrEmpty(this string value)
        {
            return !string.IsNullOrEmpty(value);
        }

        public static string TruncateWithEllipsis(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value) || value.Length <= maxLength)
                return value;

            return value.Substring(0, maxLength - 3) + "...";
        }

        #endregion

        #region Color Extensions

        public static Color WithAlpha(this Color color, float alpha)
        {
            return new Color(color.r, color.g, color.b, alpha);
        }

        #endregion

        #region DateTime Extensions

        public static string ToRelativeTime(this DateTime dateTime)
        {
            var timeSpan = DateTime.UtcNow - dateTime;

            if (timeSpan.TotalSeconds < 60)
                return "Just now";
            if (timeSpan.TotalMinutes < 60)
                return $"{(int)timeSpan.TotalMinutes}m ago";
            if (timeSpan.TotalHours < 24)
                return $"{(int)timeSpan.TotalHours}h ago";
            if (timeSpan.TotalDays < 7)
                return $"{(int)timeSpan.TotalDays}d ago";

            return dateTime.ToString("MMM dd");
        }

        public static string ToFlightDuration(this TimeSpan timeSpan)
        {
            if (timeSpan.TotalHours >= 1)
                return $"{(int)timeSpan.TotalHours}h {timeSpan.Minutes}m";
            return $"{timeSpan.Minutes}m";
        }

        #endregion

        #region Enum Extensions

        public static string ToDisplayString(this BirdRarity rarity)
        {
            return rarity switch
            {
                BirdRarity.Common => "Common",
                BirdRarity.Uncommon => "Uncommon",
                BirdRarity.Rare => "Rare",
                BirdRarity.Epic => "Epic",
                BirdRarity.Legendary => "Legendary",
                BirdRarity.Mythical => "Mythical",
                _ => rarity.ToString()
            };
        }

        public static Color ToColor(this BirdRarity rarity)
        {
            return rarity switch
            {
                BirdRarity.Common => new Color(0.6f, 0.6f, 0.6f),
                BirdRarity.Uncommon => new Color(0.2f, 0.8f, 0.2f),
                BirdRarity.Rare => new Color(0.2f, 0.4f, 0.9f),
                BirdRarity.Epic => new Color(0.6f, 0.2f, 0.8f),
                BirdRarity.Legendary => new Color(0.9f, 0.7f, 0.1f),
                BirdRarity.Mythical => new Color(0.9f, 0.2f, 0.2f),
                _ => Color.white
            };
        }

        public static string ToDisplayString(this Element element)
        {
            return element switch
            {
                Element.None => "None",
                Element.Fire => "Fire",
                Element.Ice => "Ice",
                Element.Wind => "Wind",
                Element.Emerald => "Emerald",
                _ => element.ToString()
            };
        }

        public static Color ToColor(this Element element)
        {
            return element switch
            {
                Element.Fire => new Color(0.9f, 0.3f, 0.1f),
                Element.Ice => new Color(0.3f, 0.7f, 0.9f),
                Element.Wind => new Color(0.5f, 0.9f, 0.5f),
                Element.Emerald => new Color(0.1f, 0.8f, 0.3f),
                _ => Color.white
            };
        }

        public static string ToDisplayString(this BirdState state)
        {
            return state switch
            {
                BirdState.InCoop => "In Coop",
                BirdState.Flying => "Flying",
                BirdState.Sick => "Sick",
                BirdState.Resting => "Resting",
                _ => state.ToString()
            };
        }

        #endregion

        #region Transform Extensions

        public static void DestroyAllChildren(this Transform transform)
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                UnityEngine.Object.Destroy(transform.GetChild(i).gameObject);
            }
        }

        public static void DestroyAllChildrenImmediate(this Transform transform)
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                UnityEngine.Object.DestroyImmediate(transform.GetChild(i).gameObject);
            }
        }

        #endregion

        #region Vector Extensions

        public static Vector2 ToVector2(this Vector3 v)
        {
            return new Vector2(v.x, v.y);
        }

        public static Vector3 ToVector3(this Vector2 v, float z = 0)
        {
            return new Vector3(v.x, v.y, z);
        }

        #endregion
    }
}
