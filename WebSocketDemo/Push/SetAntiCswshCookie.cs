using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WebSocketDemo.Push
{
    public class SetAntiCswshCookie : IResultFilter
    {
        public static string CookieName { get; } = "antiCswshToken";

        readonly AntiCswshTokenValidator _tokenValidator;

        public SetAntiCswshCookie(AntiCswshTokenValidator tokenValidator)
        {
            _tokenValidator = tokenValidator;
        }

        public void OnResultExecuted(ResultExecutedContext context)
        {
        }

        public void OnResultExecuting(ResultExecutingContext context)
        {
            if (string.IsNullOrEmpty(context.HttpContext.Request.Cookies[CookieName]))
            {
                var token = _tokenValidator.GenerateToken(
                    context.HttpContext.User.Identity.Name,
                    expires: DateTime.UtcNow.AddDays(7));

                context.HttpContext.Response.Cookies.Append(
                    CookieName,
                    token,
                    new CookieOptions { HttpOnly = false });
            }
        }
    }
}
