using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackOffice.Helpers
{
    public static class StringExtensions
    {
        public static string ShortenText(this string str, int len)
        {
            if (str != null && str.Length > len)
            {
                return str.Substring(0, len) + "...";
            }
            return str;
        }

        public static string ShortenText(this string str, int symbolsStart, int symbolsEnd)
        {
            string res = string.Empty;
            if (str != null && str.Length > symbolsStart)
            {
                res += str.Substring(0, symbolsStart) + " ... ";
                if (str.Length >= symbolsStart + symbolsEnd)
                {
                    res += str.Substring(str.Length - symbolsEnd, symbolsEnd);
                }
                return res;
            }
            return str;
        }
    }
}
