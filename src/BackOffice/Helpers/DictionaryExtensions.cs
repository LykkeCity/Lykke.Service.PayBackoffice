using Common;
using System.Collections.Generic;

namespace BackOffice.Helpers
{
    public static class DictionaryExtensions
    {
        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> source, TKey key, TValue defaultValue = default(TValue))
        {
            if (key != null && source?.ContainsKey(key) == true)
            {
                return source[key];
            }

            return defaultValue;
        }

        public static string GetCountryIso3ByIso2(string code2)
        {
            return code2 == null ? "" : CountryManager.Iso2ToIso3(code2);
        }

    }
}
