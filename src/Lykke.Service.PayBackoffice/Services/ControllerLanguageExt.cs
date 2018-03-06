using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.AspNetCore.Mvc;


namespace BackOffice.Services
{
    public static class ControllerLangExtention
    {
        public const string LangCookie = "Lang";
        private const string DefaultLang = "en";

        private static readonly Dictionary<string, string> Localizations = new Dictionary<string, string>
        {
            {"en", "en"},
            {"ru", "ru"},

        };


        public static IEnumerable<string> GetLanguages()
        {
            return Localizations.Keys;
        }



        public static void SetThread(string langId)
        {
            if (string.IsNullOrEmpty(langId))
                langId = DefaultLang;
            else
                langId = Localizations.ContainsKey(langId) ? Localizations[langId] : DefaultLang;

            var culture = new CultureInfo(langId);

            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;
        }

        public static void SetLanguage(this Controller ctx, string langId)
        {
            langId = langId.ToLower();
            if (!Localizations.ContainsKey(langId))
                langId = DefaultLang;
            ctx.Response.Cookies.Append(LangCookie, langId,
                new Microsoft.AspNetCore.Http.CookieOptions { Expires = DateTime.UtcNow.AddYears(5) });
            SetThread(langId);
        }
    }
}