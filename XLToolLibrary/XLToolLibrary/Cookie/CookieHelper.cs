using System;
using System.Web;

using Newtonsoft.Json;

namespace XLToolLibrary.Utilities
{
    public class CookieHelper
    {
        /// <summary>
        /// 增加cookie
        /// </summary>
        /// <param name="response"></param>
        /// <param name="userObject"></param>
        /// <param name="cookieName"></param>
        /// <param name="days"></param>
        public static void AddCookie(HttpResponseBase response, object userObject, string cookieName, int days)
        {
            var json = JsonConvert.SerializeObject(userObject);
            var userCookie = new HttpCookie(cookieName, json);

            userCookie.Expires.AddDays(days);
            response.Cookies.Add(userCookie);
        }
        /// <summary>
        /// 移除cookie
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cookieName"></param>
        public static void RemoveCookie(HttpContextBase context,  string cookieName)
        {
            if (context.Request.Cookies[cookieName] != null)
            {
                var user = new HttpCookie(cookieName)
                {
                    Expires = DateTime.Now.AddDays(-1),
                    Value = null
                };
                context.Response.Cookies.Add(user);
            }
        }
    }
}
