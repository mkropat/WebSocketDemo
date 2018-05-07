using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WebSocketDemo.Push
{
    public class SetAntiCswshCookie : IResultFilter
    {
        public static string CookieName { get; } = "antiCswshToken";

        public void OnResultExecuted(ResultExecutedContext context)
        {
        }

        public void OnResultExecuting(ResultExecutingContext context)
        {
            if (string.IsNullOrEmpty(context.HttpContext.Request.Cookies[CookieName]))
            {
                context.HttpContext.Response.Cookies.Append(
                    CookieName,
                    Guid.NewGuid().ToString(),
                    new CookieOptions { HttpOnly = false });
            }
        }
    }
}
