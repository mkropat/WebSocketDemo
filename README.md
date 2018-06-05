# WebSocketDemo

Demo code for a blog post I wrote: [Things I Wish Someone Told Me About ASP.NET Core WebSockets](https://www.codetinkerer.com/2018/06/05/aspnet-core-websockets.html)

The demo doesn't look pretty:

![screenshot](https://www.codetinkerer.com/assets/aspnet-core-websockets/demo-screenshot.png)

But it might help you if you're trying to write a WebSocket handler and you want to compare your code to some running code.

## Starting Points

Take a look at [MessagePushHandler.cs](/WebSocketDemo/Push/MessagePushHandler.cs).

Other files of interest:

- [app.js](/WebSocketDemo/wwwroot/app.js)
- [AntiCswshTokenValidator.cs](/WebSocketDemo/Push/AntiCswshTokenValidator.cs)
- [SetAntiCswshCookie.cs](/WebSocketDemo/Push/SetAntiCswshCookie.cs)
