using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using BackOffice.Models;
using Common;
using Core.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;

namespace BackOffice.Controllers
{
    public enum ErrorPlaceholder
    {
        Bottom,
        Top,
        Left,
        Right
    }


    public static class ControllerExtensions
    {
        public static ViewResult ErrorDialog(this Controller controller, string errorMessage)
        {
            return controller.View("~/Views/Shared/ErrorDialog.cshtml", new ErrorDialogViewModel(errorMessage));
        }

        public static JsonResult JsonFailResult(this Controller contr, string message, string div,
            ErrorPlaceholder placeholder = ErrorPlaceholder.Top)
        {
            if (placeholder == ErrorPlaceholder.Top)
                return new JsonResult(new {status = "Fail", msg = message, divError = div});

            return
                new JsonResult(
                    new {status = "Fail", msg = message, divError = div, placement = placeholder.ToString().ToLower()});

        }

        public static JsonResult JsonRequestResult(this Controller contr, string div, string url, object model = null,
            bool putToHistory = false)
        {

            if (model == null)
                return new JsonResult(new {div, refreshUrl = url, showLoading = true});

            var modelAsString = model as string ?? model.ToUrlParamString();
            return new JsonResult(new {div, refreshUrl = url, prms = modelAsString, showLoading = true});
        }

        public static JsonResult JsonResultShowDialog(this Controller contr, string url, object model = null)
        {
            return new JsonResult(new {status = "ShowDialog", url, prms = model});
        }

        public static JsonResult JsonResultReloadData(this Controller contr)
        {
            return new JsonResult(new {status = "Reload"});
        }

        public static JsonResult JsonRefreshLast(this Controller contr)
        {
            return new JsonResult(new {status = "refreshLast"});
        }



        private const string OwnershipCookie = "DataOwnership";

        public static void SetDataOwnership(this Controller contr, string ownership)
        {
            if (string.IsNullOrEmpty(ownership))
                return;
            contr.Response.Cookies.Append(OwnershipCookie, ownership,
                new CookieOptions() {Expires = DateTime.UtcNow.AddYears(5)});
        }

        public static string GetDataOwnership(this Controller contr)
        {
            return contr.Request.Cookies[OwnershipCookie];
        }




        public const string LangCookie = "Language";

        //public static ITranslation UserLanguage(this HttpRequestBase request)
        //{
        //    var langCookie = request.Cookies[LangCookie];
        //    var langId = langCookie == null ? DetectLangIdByHeader(request) : langCookie.Value;
        //    return PhraseList.GetTranslations(langId);

        //}


        //public static void SetThread(ITranslation translation)
        //{
        //    Thread.CurrentThread.CurrentCulture = translation.Culture;
        //    Thread.CurrentThread.CurrentUICulture = translation.Culture;
        //}




        public static string SessionCookie => "Session";

        public static string GetSession(this Controller contr)
        {
            var sessionCooke = contr.HttpContext.Request.Cookies[SessionCookie];
            if (sessionCooke != null)
                return sessionCooke;

            var sessionId = Guid.NewGuid().ToString();
            contr.Response.Cookies.Append(SessionCookie, sessionId,
                new CookieOptions {Expires = DateTime.UtcNow.AddYears(5)});
            return sessionId;
        }

        private static readonly Regex b =
            new Regex(
                @"(android|bb\d+|meego).+mobile|avantgo|bada\/|blackberry|blazer|compal|elaine|fennec|hiptop|iemobile|ip(hone|od)|iris|kindle|lge |maemo|midp|mmp|mobile.+firefox|netfront|opera m(ob|in)i|palm( os)?|phone|p(ixi|re)\/|plucker|pocket|psp|series(4|6)0|symbian|treo|up\.(browser|link)|vodafone|wap|windows ce|xda|xiino",
                RegexOptions.IgnoreCase | RegexOptions.Multiline);

        private static readonly Regex v =
            new Regex(
                @"1207|6310|6590|3gso|4thp|50[1-6]i|770s|802s|a wa|abac|ac(er|oo|s\-)|ai(ko|rn)|al(av|ca|co)|amoi|an(ex|ny|yw)|aptu|ar(ch|go)|as(te|us)|attw|au(di|\-m|r |s )|avan|be(ck|ll|nq)|bi(lb|rd)|bl(ac|az)|br(e|v)w|bumb|bw\-(n|u)|c55\/|capi|ccwa|cdm\-|cell|chtm|cldc|cmd\-|co(mp|nd)|craw|da(it|ll|ng)|dbte|dc\-s|devi|dica|dmob|do(c|p)o|ds(12|\-d)|el(49|ai)|em(l2|ul)|er(ic|k0)|esl8|ez([4-7]0|os|wa|ze)|fetc|fly(\-|_)|g1 u|g560|gene|gf\-5|g\-mo|go(\.w|od)|gr(ad|un)|haie|hcit|hd\-(m|p|t)|hei\-|hi(pt|ta)|hp( i|ip)|hs\-c|ht(c(\-| |_|a|g|p|s|t)|tp)|hu(aw|tc)|i\-(20|go|ma)|i230|iac( |\-|\/)|ibro|idea|ig01|ikom|im1k|inno|ipaq|iris|ja(t|v)a|jbro|jemu|jigs|kddi|keji|kgt( |\/)|klon|kpt |kwc\-|kyo(c|k)|le(no|xi)|lg( g|\/(k|l|u)|50|54|\-[a-w])|libw|lynx|m1\-w|m3ga|m50\/|ma(te|ui|xo)|mc(01|21|ca)|m\-cr|me(rc|ri)|mi(o8|oa|ts)|mmef|mo(01|02|bi|de|do|t(\-| |o|v)|zz)|mt(50|p1|v )|mwbp|mywa|n10[0-2]|n20[2-3]|n30(0|2)|n50(0|2|5)|n7(0(0|1)|10)|ne((c|m)\-|on|tf|wf|wg|wt)|nok(6|i)|nzph|o2im|op(ti|wv)|oran|owg1|p800|pan(a|d|t)|pdxg|pg(13|\-([1-8]|c))|phil|pire|pl(ay|uc)|pn\-2|po(ck|rt|se)|prox|psio|pt\-g|qa\-a|qc(07|12|21|32|60|\-[2-7]|i\-)|qtek|r380|r600|raks|rim9|ro(ve|zo)|s55\/|sa(ge|ma|mm|ms|ny|va)|sc(01|h\-|oo|p\-)|sdk\/|se(c(\-|0|1)|47|mc|nd|ri)|sgh\-|shar|sie(\-|m)|sk\-0|sl(45|id)|sm(al|ar|b3|it|t5)|so(ft|ny)|sp(01|h\-|v\-|v )|sy(01|mb)|t2(18|50)|t6(00|10|18)|ta(gt|lk)|tcl\-|tdg\-|tel(i|m)|tim\-|t\-mo|to(pl|sh)|ts(70|m\-|m3|m5)|tx\-9|up(\.b|g1|si)|utst|v400|v750|veri|vi(rg|te)|vk(40|5[0-3]|\-v)|vm40|voda|vulc|vx(52|53|60|61|70|80|81|83|85|98)|w3c(\-| )|webc|whit|wi(g |nc|nw)|wmlb|wonu|x700|yas\-|your|zeto|zte\-",
                RegexOptions.IgnoreCase | RegexOptions.Multiline);

        public static bool IsMobileBrowser(this HttpRequest request)
        {
            var userAgent = request.UserAgent();
            if ((b.IsMatch(userAgent) || v.IsMatch(userAgent.Substring(0, 4))))
            {
                return true;
            }

            return false;
        }

        public static string UserAgent(this HttpRequest request)
        {
            return request.Headers["User-Agent"];
        }



        public static async Task<IBackOfficeUser> GetBackofficeUserAsync(this Controller contr)
        {
            return await Dependencies.BackOfficeUsersRepository.GetAsync(contr.GetUserId());
        }


        public static string GetUserId(this Controller controller)
        {
            return controller.User.Identity.Name;
        }


        public static async Task<UserRolesPair> GetUserRolesPair(this Controller controller)
        {
            var userId = controller.GetUserId();

            var result = new UserRolesPair();

            var taskBackOfficeUser = Dependencies.BackOfficeUsersRepository.GetAsync(userId);
            var taskUserRoles = Dependencies.BackofficeUserRolesRepository.GetAllRolesAsync();

            result.User = await taskBackOfficeUser;
            result.Roles = (await taskUserRoles).ToArray();

            return result;
        }

        public static string GetIp(this Controller ctx)
        {
            string ip = string.Empty;

            // http://stackoverflow.com/a/43554000/538763
            var xForwardedForVal =
                GetHeaderValueAs<string>(ctx.HttpContext, "X-Forwarded-For").SplitCsv().FirstOrDefault();

            if (!string.IsNullOrEmpty(xForwardedForVal))
            {
                ip = xForwardedForVal.Split(':')[0];
            }

            // RemoteIpAddress is always null in DNX RC1 Update1 (bug).
            if (string.IsNullOrWhiteSpace(ip) && ctx.HttpContext?.Connection?.RemoteIpAddress != null)
                ip = ctx.HttpContext.Connection.RemoteIpAddress.ToString();

            if (string.IsNullOrWhiteSpace(ip))
                ip = GetHeaderValueAs<string>(ctx.HttpContext, "REMOTE_ADDR");

            return ip;
        }

        #region Tools

        private static T GetHeaderValueAs<T>(HttpContext httpContext, string headerName)
        {
            StringValues values;

            if (httpContext?.Request?.Headers?.TryGetValue(headerName, out values) ?? false)
            {
                string rawValues = values.ToString();   // writes out as Csv when there are multiple.

                if (!string.IsNullOrEmpty(rawValues))
                    return (T)Convert.ChangeType(values.ToString(), typeof(T));
            }
            return default(T);
        }

        private static List<string> SplitCsv(this string csvList, bool nullOrWhitespaceInputReturnsNull = false)
        {
            if (string.IsNullOrWhiteSpace(csvList))
                return nullOrWhitespaceInputReturnsNull ? null : new List<string>();

            return csvList
                .TrimEnd(',')
                .Split(',')
                .AsEnumerable<string>()
                .Select(s => s.Trim())
                .ToList();
        }

        #endregion
    }
}
