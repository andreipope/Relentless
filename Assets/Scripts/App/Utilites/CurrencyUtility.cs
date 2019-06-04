using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public static class CurrencyUtility
    {
        private static readonly Dictionary<string, CultureInfo> IsoCurrencyCodeToCultureMap =
            CultureInfo
                .GetCultures(CultureTypes.SpecificCultures)
                .Select(c => new {c, new RegionInfo(c.LCID).ISOCurrencySymbol})
                .GroupBy(x => x.ISOCurrencySymbol)
                .ToDictionary(g => g.Key, g => g.First().c, StringComparer.OrdinalIgnoreCase);

        public static CultureInfo GetCultureFromIsoCurrencyCode(string currencyCode)
        {
            IsoCurrencyCodeToCultureMap.TryGetValue(currencyCode, out CultureInfo culture);
            return culture;
        }

        public static string FormatCurrency(this decimal amount, string currencyCode)
        {
            if (IsoCurrencyCodeToCultureMap.TryGetValue(currencyCode, out CultureInfo culture))
            {
                Debug.Log(culture.Name);
                Debug.Log(culture.NumberFormat.CurrencySymbol);
                return String.Format(culture, "{0:C}", amount);
            }

            return $"{amount:0.00} {currencyCode}";
        }
    }
}
