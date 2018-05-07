using System.Security.Claims;

namespace WebSocketDemo.Push
{
    public interface IAuthorizableMessage
    {
        string Message { get; }
        bool IsAuthorized(ClaimsPrincipal principal);
    }
}
