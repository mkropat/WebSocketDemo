using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.WebSockets;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace WebSocketDemo.Push
{
    public class MessagePushHandler
    {
        static readonly TimeSpan pushMessagePollingInterval = TimeSpan.FromMilliseconds(100);

        readonly ILogger _log;
        readonly IMessageSource _messageSource;
        readonly RequestDelegate _next;
        readonly CancellationToken _shutdown;
        readonly string _url;

        public MessagePushHandler(RequestDelegate next, IApplicationLifetime appLifetime, ILoggerFactory logFactory, IMessageSource messageSource, string url)
        {
            _log = logFactory.CreateLogger<MessagePushHandler>();
            _messageSource = messageSource;
            _next = next;
            _shutdown = appLifetime.ApplicationStopping;
            _url = url;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path != _url)
            {
                await _next.Invoke(context);
                return;
            }

            var expectedCswshToken = context.Request.Cookies[SetAntiCswshCookie.CookieName];
            if (string.IsNullOrEmpty(expectedCswshToken) || context.Request.Query[SetAntiCswshCookie.CookieName] != expectedCswshToken)
            {
                // We could check the Origin header instead of a token. The token approach is slightly preferred because:
                // 1. It follows the canonical OWASP anti-CSRF pattern
                // 2. It doesn't require the application to maintain a whitelist config of allowed Origin values

                await WriteJsonResponse(context.Response, HttpStatusCode.Forbidden, new
                {
                    message = "A valid antiCswshToken query string parameter must be passed",
                });
                return;
            }

            if (!context.User.Identity.IsAuthenticated)
            {
                // If the demo were using authentication, we could require authentication like so:

                //await WriteJsonResponse(context.Response, HttpStatusCode.Forbidden, new
                //{
                //    message = "User must authenticate",
                //});
                //return;
            }

            if (!context.WebSockets.IsWebSocketRequest)
            {
                await WriteJsonResponse(context.Response, HttpStatusCode.BadRequest, new
                {
                    message = "Expected WebSocket request"
                });
                return;
            }

            try
            {
                using (var socket = await context.WebSockets.AcceptWebSocketAsync())
                {
                    var requestClosing = new CancellationTokenSource();
                    var anyCancel = CancellationTokenSource.CreateLinkedTokenSource(_shutdown, requestClosing.Token).Token;

                    var pushTask = Task.Run(async () => {
                        try
                        {
                            await PushMessages(_messageSource, socket, context.User, anyCancel);
                        }
                        finally
                        {
                            requestClosing.Cancel(); // Notify the receive task to shut down
                        }
                    });

                    try
                    {
                        await ReceiveMessages(socket, anyCancel);
                    }
                    finally
                    {
                        requestClosing.Cancel(); // Notify the push task to shut down
                    }

                    await pushTask; // Wait for any pending send operations to complete before sending the close message
                                    // (since the WebSocket class is not thread-safe in general)

                    await socket.CloseAsync(WebSocketCloseStatus.EndpointUnavailable, "Server is stopping", CancellationToken.None);
                }
            }
            catch (WebSocketException ex)
            {
                switch (ex.WebSocketErrorCode)
                {
                    case WebSocketError.ConnectionClosedPrematurely:
                        _log.LogInformation(ex, "Client connection closed prematurely");
                        break;

                    default:
                        _log.LogError(ex, "Unexpected WebSocket error");
                        break;
                }
            }

        }

        static async Task ReceiveMessages(WebSocket socket, CancellationToken cancelToken)
        {
            const int maxMessageSize = 4096;
            var buffer = new byte[maxMessageSize];

            while (!cancelToken.IsCancellationRequested)
            {
                try
                {
                    var (response, message) = await ReceiveFullMessage(socket, cancelToken);
                    if (response.MessageType == WebSocketMessageType.Close)
                    {
                        break;
                    }
                }
                catch (OperationCanceledException)
                {
                    // Exit normally
                }
            }
        }

        static async Task PushMessages(IMessageSource source, WebSocket socket, ClaimsPrincipal principal, CancellationToken cancelToken)
        {
            var messages = new ConcurrentQueue<string>();

            source.OnMessage += enqueueAuthorizedMessages;
            try
            {
                while (!cancelToken.IsCancellationRequested)
                {
                    if (!messages.TryDequeue(out var message))
                    {
                        await Task.Delay(pushMessagePollingInterval, cancelToken).ContinueWith(_ => Task.CompletedTask);
                        continue;
                    }

                    try
                    {
                        var buffer = Encoding.UTF8.GetBytes(message);
                        await socket.SendAsync(buffer, WebSocketMessageType.Text, endOfMessage: true, cancellationToken: cancelToken);
                    }
                    catch (OperationCanceledException)
                    {
                        // Exit normally
                    }
                }
            }
            finally
            {
                source.OnMessage -= enqueueAuthorizedMessages;
            }

            void enqueueAuthorizedMessages(IAuthorizableMessage authorizableMessage)
            {
                if (authorizableMessage.IsAuthorized(principal))
                    messages.Enqueue(authorizableMessage.Message);
            }
        }

        static async Task WriteJsonResponse(HttpResponse response, HttpStatusCode statusCode, object body)
        {
            response.StatusCode = (int)statusCode;
            response.ContentType = "application/json";
            await response.WriteAsync(JsonConvert.SerializeObject(body));
        }

        static async Task<(WebSocketReceiveResult, IEnumerable<byte>)> ReceiveFullMessage(WebSocket socket, CancellationToken cancelToken)
        {
            WebSocketReceiveResult response;
            var message = new List<byte>();

            var buffer = new byte[4096];
            do
            {
                response = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), cancelToken);
                message.AddRange(new ArraySegment<byte>(buffer, 0, response.Count));
            }
            while (!response.EndOfMessage);

            return (response, message);
        }
    }

    public static class MessagePushHandlerExtensions
    {
        public static IApplicationBuilder UseMessagePushHandler(this IApplicationBuilder builder, string url)
        {
            return builder.UseMiddleware<MessagePushHandler>(url);
        }
    }
}
